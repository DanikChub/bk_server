import 'server-only';
import { assert } from '../errors';
import { one, rows } from '../db';
type CalendarDay={date:string;workingHours:number};
const cache=new Map<number,{expires:number;promise:Promise<CalendarDay[]>}>();
function parseDate(input:string):string {
  const [day,month,year]=input.split('.');
  assert(day && month && year,502,'Неожиданный формат производственного календаря');return `${year}-${month}-${day}`;
}
async function yearCalendar(year:number):Promise<CalendarDay[]> {
  const saved=cache.get(year);if(saved && saved.expires>Date.now())return saved.promise;
  const promise=(async()=>{
    const stored=await one(`SELECT "Value" FROM "NextCache" WHERE "Key"=$1 AND "ExpiresAt">now()`,['production-calendar:'+year]);
    if(stored)return stored.Value as CalendarDay[];
    assert(process.env.WORK_CALENDAR_TOKEN,503,'Производственный календарь не настроен');
    const url=`https://production-calendar.ru/get-period/${encodeURIComponent(process.env.WORK_CALENDAR_TOKEN!)}/ru/${year}/json`;
    const response=await fetch(url,{signal:AbortSignal.timeout(30000),cache:'no-store'});assert(response.ok,502,'Производственный календарь недоступен');
    const result=await response.json();const days=result.days??result.Days;assert(Array.isArray(days),502,'Некорректный ответ производственного календаря');
    const parsed=days.map((day:Record<string,unknown>)=>({date:parseDate(String(day.date??day.Date)),workingHours:Number(day.working_hours??day.workingHours??day.WorkingHours)}));
    assert(parsed.every((day:CalendarDay)=>Number.isFinite(day.workingHours)),502,'Некорректные рабочие часы календаря');
    await rows(`INSERT INTO "NextCache"("Key","Value","ExpiresAt") VALUES($1,$2,now()+interval '1 day') ON CONFLICT("Key") DO UPDATE SET "Value"=excluded."Value","ExpiresAt"=excluded."ExpiresAt"`,['production-calendar:'+year,JSON.stringify(parsed)]);
    return parsed;
  })();
  cache.set(year,{expires:Date.now()+86400_000,promise});
  try{return await promise;}catch(error){cache.delete(year);throw error;}
}
export async function productionCalendar(start:string,end:string) {
  const first=new Date(start),last=new Date(end);assert(Number.isFinite(+first)&&Number.isFinite(+last)&&last>=first,400,'Некорректный период');
  assert(last.getUTCFullYear()-first.getUTCFullYear()<=3,400,'Слишком большой период');
  const days:CalendarDay[]=[];
  for(let year=first.getUTCFullYear();year<=last.getUTCFullYear();year++)days.push(...await yearCalendar(year));
  const a=first.toISOString().slice(0,10),b=last.toISOString().slice(0,10),period=days.filter(d=>d.date>=a&&d.date<=b);
  const holidays=period.filter(d=>d.workingHours===0).map(d=>d.date);
  const weekends=holidays.filter(d=>[0,6].includes(new Date(d+'T00:00:00Z').getUTCDay()));
  return {holidays,countHolidays:holidays.length-weekends.length,countWeekends:weekends.length,countWorkdays:period.filter(d=>d.workingHours>0).length,countDays:Math.floor((Date.parse(b)-Date.parse(a))/86400_000)+1};
}
