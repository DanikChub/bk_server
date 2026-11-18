import 'server-only';
import nodemailer from 'nodemailer';
import { z } from 'zod';
import { assert } from '../errors';
export function emailConfigured(){return Boolean(process.env.SMTP_HOST&&process.env.SMTP_FROM);}
export async function sendEmail(body:unknown){
  assert(emailConfigured(),503,'Отправка писем не настроена');
  const input=z.object({to:z.string().email(),subject:z.string().max(500),text:z.string().max(10000)}).strict().parse(body);
  const port=Number(process.env.SMTP_PORT??587);assert(Number.isInteger(port)&&port>0&&port<=65535,503,'Некорректный SMTP-порт');
  const transport=nodemailer.createTransport({host:process.env.SMTP_HOST,port,secure:process.env.SMTP_SECURE==='true'||port===465,requireTLS:process.env.SMTP_REQUIRE_TLS!=='false',auth:process.env.SMTP_USER?{user:process.env.SMTP_USER,pass:process.env.SMTP_PASSWORD}:undefined,connectionTimeout:15000,greetingTimeout:15000,socketTimeout:30000});
  try{await transport.sendMail({from:process.env.SMTP_FROM,to:input.to,subject:input.subject,text:input.text,disableFileAccess:true,disableUrlAccess:true});}finally{transport.close();}
}
