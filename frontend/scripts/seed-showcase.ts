import { bootstrap } from '../src/server/bootstrap';
import { pool, one, rows, transaction, insert, update } from '../src/server/db';
import { customers } from '../src/server/modules/customers';
import { users } from '../src/server/modules/users';
import { roles } from '../src/server/modules/permissions';
import { contracts } from '../src/server/modules/contracts';
import { createTicket, mutateTicket } from '../src/server/modules/tickets';
import { uploadFiles } from '../src/server/modules/files';
import { events } from '../src/server/modules/events';
import type { Session } from '../src/server/auth/session';

// Explicit opt-in: this command writes synthetic showcase data, never run on a working database.
async function main(){
  if(process.env.SHOWCASE_SEED!=='true')throw new Error('Set SHOWCASE_SEED=true only for a dedicated showcase database');
  const passwords=JSON.parse(process.env.SHOWCASE_PASSWORDS??'{}') as Record<string,string>;
  const logins=['demo.admin','demo.head','demo.manager','demo.specialist','demo.client','demo.other'];
  for(const login of logins)if(!passwords[login]||passwords[login].length<12)throw new Error('Provide a unique password for '+login);
  const q=new URLSearchParams();
  if(!await one(`SELECT "Id" FROM "AbpUsers" WHERE "NormalizedUserName"='DEMO.ADMIN' AND "TenantId" IS NULL`))await bootstrap({userName:'demo.admin',email:'admin@example.test',password:passwords['demo.admin']});
  const adminRow=await one(`SELECT "Id" FROM "AbpUsers" WHERE "NormalizedUserName"='DEMO.ADMIN' AND "TenantId" IS NULL`);
  const admin:Session={userId:String(adminRow!.Id),userName:'demo.admin',tenantId:null,roles:['admin'],permissions:[],specialistId:null};
  return transaction(async db=>{
    await db.query(`SELECT pg_advisory_xact_lock(728441024)`);
    if(await one(`SELECT "Key" FROM "NextCache" WHERE "Key"='showcase.seed.v1'`,[],db))return {alreadySeeded:true};
    const groups=['Ticket','TicketHistory','TicketAttachments','TicketHoursSpentHistory','SpecialistManagement','Contract','ContractStatus','CustomerManagement','UserManagement','EmployeeProfileManagement','AnswerTemplate','TicketSection','Calendar','Event','StoriesManagement'];
    const all=groups.flatMap(group=>[group,...['Get','Create','Edit','Delete'].map(action=>group+'.'+action)]);
    const specialist=['Ticket','Ticket.Get','Ticket.Edit','TicketHistory.Get','TicketHistory.Create','TicketHistory.Edit','TicketAttachments.Get','TicketAttachments.Create','TicketHoursSpentHistory.Create','SpecialistManagement.Get','SpecialistManagement.Create','SpecialistManagement.Delete','Contract','Contract.Get','TicketSection.Get','AnswerTemplate.Get','Calendar.Get','Calendar.Create','Calendar.Edit','Event.Get'];
    const rolePlans=[['Руководитель отдела продаж',all],['Менеджер Отдела Продаж',all.filter(p=>!p.startsWith('EmployeeProfileManagement')&&!p.endsWith('.Delete'))],['Специалист',specialist]] as const;
    for(const [name,permissions] of rolePlans){const role=await roles(admin,undefined,undefined,'POST',{name,tenantId:null},q) as {Id:string};await roles(admin,role.Id,'permissions','POST',{permissions},q);}
    const employees=[];
    for(const [login,firstName,lastName,role] of [['demo.head','Анна','Волкова','Руководитель отдела продаж'],['demo.manager','Иван','Соколов','Менеджер Отдела Продаж'],['demo.specialist','Елена','Морозова','Специалист']]){
      const user=await users(admin,undefined,undefined,'POST',{userName:login,password:passwords[login],email:login+'@example.test',firstName,lastName,jobPost:role,roles:[role]},q,true) as {id:string;profileId:string};employees.push({...user,login,role});
    }
    const ticketTypes=await rows(`SELECT "Id","Name" FROM "TicketTypes" ORDER BY "Name"`,[],db),sections=await rows(`SELECT "Id","Name" FROM "TicketSections" ORDER BY "Name"`,[],db),constraints=await rows(`SELECT "Id" FROM "ConstraintTypes"`,[],db);
    const pkg=await insert('ServicePackages',{Name:'Консультационное сопровождение — ДЕМО',Style:'label-info'},db,admin.userId);
    await insert('AnswerTemplates',{Title:'Запрос уточняющих документов',Description:'Добрый день! Для подготовки консультации просим приложить договор и уточнить период операции. После получения документов продолжим рассмотрение.'},db,admin.userId);
    await insert('AnswerTemplates',{Title:'Консультация подготовлена',Description:'Добрый день! Консультация подготовлена. Рекомендации приведены в ответе ниже. Если останутся вопросы, напишите в этой заявке.'},db,admin.userId);
    const orgs=[];
    const subjects=['Учёт расходов на доставку','Проверка условий договора поставки','Порядок оформления отпуска','Корректировка НДС за прошлый квартал','Подготовка ответа на требование ФНС','Страховые взносы по договору ГПХ','Применение электронной подписи','Закрывающие документы по услугам','Изменение реквизитов организации','Продление консультационного сопровождения','Уточнение налоговой базы','Учёт командировочных расходов'];
    for(let orgIndex=0;orgIndex<2;orgIndex++){
      const short=orgIndex?'Вектор':'Альфа',login=orgIndex?'demo.other':'demo.client';
      const customer=await customers(admin,undefined,undefined,'POST',{ShortName:short,LongName:'ООО «'+short+'» — демонстрационная организация',Address:orgIndex?'г. Москва, ул. Примерная, д. 10':'г. Волгоград, ул. Демонстрационная, д. 5',Email:short==='Альфа'?'alpha@example.test':'vector@example.test',Description:'Синтетические данные для демонстрации переноса. Не является реальной организацией.'},q) as {Id:string};
      await update('TenantProfiles',customer.Id,{ResponsibleManagerId:employees[1].profileId,IsActive:true},db,admin.userId);
      const owner=await users(admin,undefined,undefined,'POST',{userName:login,password:passwords[login],email:login+'@example.test',firstName:orgIndex?'Павел':'Мария',lastName:orgIndex?'Орлов':'Кузнецова',jobPost:'Главный бухгалтер',tenantId:customer.Id,phone:'+79000000000'},q) as {id:string};
      const client:Session={userId:owner.id,userName:login,tenantId:customer.Id,roles:['Клиент'],permissions:['TicketAttachments.Create','Calendar.Create','Event.Get'],specialistId:null};
      const status=await one(`SELECT "Id" FROM "ContractStatuses" WHERE "TenantId"=$1 AND "Code"='in progress'`,[customer.Id],db);
      const starts=new Date(new Date().getFullYear(),0,1).toISOString(),ends=new Date(new Date().getFullYear()+1,11,31).toISOString();
      const contract=await contracts(admin,undefined,undefined,'POST',{name:'ДЕМО-'+(orgIndex?'202':'101')+'/К',tenantId:customer.Id,startsAt:starts,endsAt:ends,statusId:status!.Id},q) as {Id:string};
      await contracts(admin,contract.Id,'packages','POST',{servicePackageId:pkg.Id,startsAt:starts,endsAt:ends},q);
      for(const constraint of constraints)await contracts(admin,contract.Id,'limits','POST',{constraintTypeId:constraint.Id,max:20},q);
      const ids=[];
      for(let index=0;index<(orgIndex?4:12);index++){
        const draft=!orgIndex&&index===11,type=ticketTypes[index%ticketTypes.length],section=sections[index%sections.length];
        const ticket=await createTicket(client,{subject:subjects[index],description:'Демонстрационная заявка. Просим предоставить консультацию по вопросу: '+subjects[index]+'. Все сведения и документы на стенде являются тестовыми.',ticketTypeId:type.Id,ticketSectionId:section.Id,contractId:contract.Id,draft});
        const id=String(ticket.Id);ids.push(id);
        await update('Tickets',ticket.Id,{CreationTime:new Date(Date.now()-(index+1)*86400_000),DueDate:new Date(Date.now()+(index%3-1)*86400_000)},db,admin.userId);
        if(!draft){
          await mutateTicket(admin,id,'assign',{specialistId:employees[2].profileId});
          await mutateTicket(client,id,'reply',{description:'Добрый день! Уточняющие сведения готовы, просим рассмотреть обращение.'});
          await mutateTicket({...admin,userId:employees[2].id},id,'reply',{description:'Здравствуйте! Обращение принято в работу. Проверяем документы и готовим рекомендации.'});
          await mutateTicket(admin,id,'reply',{description:'Внутренняя заметка для сотрудников: сверить рекомендации перед отправкой клиенту.',internal:true});
          if(index<3&&constraints[0])await mutateTicket(admin,id,'constraint',{constraintTypeId:constraints[0].Id,value:1});
          const statusName=['New','InProgress','MoreDetails','Answered','WaitingForSpecialistResponse','Renewed'][index%6];
          const st=await one(`SELECT "Id" FROM "TicketStatuses" WHERE "Name"=$1`,[statusName],db);await mutateTicket(admin,id,'status',{statusId:st!.Id});
          if(statusName==='Answered')await mutateTicket(client,id,'rating',{rating:5});
          if(index===0){const form=new FormData();form.append('files',new File(['Демонстрационное приложение к заявке. Реальных сведений не содержит.'],'Описание-вопроса.txt',{type:'text/plain'}));await uploadFiles(client,id,form);await mutateTicket(client,id,'favorite',{favorite:true});}
        }
      }
      for(let n=0;n<3;n++)await events(client,undefined,'POST',{title:['Сверить документы с консультантом','Проверить ответ по заявке','Обсудить продление договора'][n],date:new Date(Date.now()+(n+1)*86400_000).toISOString(),type:n===0?1:0},q);
      for(const recipient of [owner.id,...employees.map(e=>e.id),admin.userId])await insert('UserNotifications',{IdentityUserId:recipient,Title:'Демонстрационная заявка №'+ids[0]+' обновлена',Url:'/tickets/'+ids[0]},db,admin.userId);
      orgs.push({name:short,tenantId:customer.Id,login,ticketIds:ids,contractId:contract.Id});
    }
    await db.query(`INSERT INTO "NextCache"("Key","Value","ExpiresAt") VALUES('showcase.seed.v1',$1::jsonb,'2099-01-01')`,[JSON.stringify({orgs,employees})]);
    return {seeded:true,orgs,employees};
  });
}
main().then(result=>console.log(JSON.stringify(result))).catch(error=>{console.error(error);process.exitCode=1;}).finally(()=>pool().end());
