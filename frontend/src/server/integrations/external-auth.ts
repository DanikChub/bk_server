import 'server-only';
import { createHmac } from 'node:crypto';
import { assert } from '../errors';
import type { Session } from '../auth/session';
export function externalResource(s:Session,source:string,resourceId:string) {
  assert(process.env.EXTERNAL_API_KEY && resourceId===process.env.EXTERNAL_API_KEY,403,'Ресурс недоступен');
  const secret=process.env.EXTERNAL_JWT_SECRET;assert(secret && secret.length>=32,503,'Внешняя авторизация не настроена');
  const target=new URL(source),allowed=(process.env.EXTERNAL_ALLOWED_ORIGINS??'').split(',').filter(Boolean);
  assert(target.protocol==='https:' && allowed.includes(target.origin),403,'Адрес ресурса недоступен');
  const encode=(value:unknown)=>Buffer.from(JSON.stringify(value)).toString('base64url');
  const header=encode({alg:'HS512',typ:'JWT'}),payload=encode({UserName:s.userName,nbf:Math.floor(Date.now()/1000),exp:Math.floor(Date.now()/1000)+6*3600,iat:Math.floor(Date.now()/1000)});
  const unsigned=header+'.'+payload;const signature=createHmac('sha512',secret).update(unsigned).digest('base64url');
  target.searchParams.set('token',unsigned+'.'+signature);return target.toString();
}
