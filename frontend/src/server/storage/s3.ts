import 'server-only';
import { S3Client, PutObjectCommand, GetObjectCommand, DeleteObjectCommand } from '@aws-sdk/client-s3';
import { Readable } from 'node:stream';
import { assert } from '../errors';

export type Container='attachments'|'stories'|'public'|'employee-avatar';
let client:S3Client|undefined;
function s3() {
  assert(process.env.S3_ENDPOINT && process.env.S3_ACCESS_KEY_ID && process.env.S3_SECRET_ACCESS_KEY,503,'S3 не настроен');
  return client??=new S3Client({endpoint:process.env.S3_ENDPOINT,region:process.env.S3_REGION??'ru-central1',forcePathStyle:process.env.S3_FORCE_PATH_STYLE==='true',credentials:{accessKeyId:process.env.S3_ACCESS_KEY_ID,secretAccessKey:process.env.S3_SECRET_ACCESS_KEY}});
}
export function objectKey(tenantId:string|null,fileId:string) {return tenantId?`tenants/${tenantId}/${fileId}`:`host/${fileId}`;}
function bucket(container:Container) {
  const name=process.env[{'attachments':'S3_ATTACHMENTS_BUCKET','stories':'S3_STORIES_BUCKET','public':'S3_PUBLIC_BUCKET','employee-avatar':'S3_EMPLOYEE_AVATAR_BUCKET'}[container]];
  assert(name,503,'Контейнер S3 не настроен');return name;
}
export async function putObject(container:Container,tenantId:string|null,id:string,body:Buffer,contentType:string) {
  await s3().send(new PutObjectCommand({Bucket:bucket(container),Key:objectKey(tenantId,id),Body:body,ContentType:contentType}));
}
export async function getObject(container:Container,tenantId:string|null,id:string) {
  const object=await s3().send(new GetObjectCommand({Bucket:bucket(container),Key:objectKey(tenantId,id)}));
  assert(object.Body,404,'Файл не найден');return object.Body as Readable;
}
export async function deleteObject(container:Container,tenantId:string|null,id:string) {
  await s3().send(new DeleteObjectCommand({Bucket:bucket(container),Key:objectKey(tenantId,id)}));
}
