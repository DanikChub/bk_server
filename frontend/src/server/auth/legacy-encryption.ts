import { createCipheriv, createDecipheriv, pbkdf2Sync } from 'node:crypto';
// Compatibility with ABP 8 StringEncryptionService (AES-CBC / PBKDF2-SHA1 / 1000).
// Salt and IV must match the previous installation; never guess production configuration.
function parameters() {
  const secret=process.env.LEGACY_PASSWORD_ENCRYPTION_KEY,salt=process.env.LEGACY_ENCRYPTION_SALT_BASE64,iv=process.env.LEGACY_ENCRYPTION_IV_BASE64;
  if(!secret || !salt || !iv)return null;
  const ivBytes=Buffer.from(iv,'base64');if(ivBytes.length!==16)throw new Error('Legacy encryption IV must have 16 bytes');
  return {key:pbkdf2Sync(secret,Buffer.from(salt,'base64'),1000,32,'sha1'),iv:ivBytes};
}
export function encryptLegacyPassword(plain:string):string|undefined {
  if(process.env.LEGACY_PASSWORD_ENCRYPTION_ENABLED!=='true')return undefined;
  const p=parameters();if(!p)throw new Error('Legacy encryption configuration is missing');
  const cipher=createCipheriv('aes-256-cbc',p.key,p.iv);return Buffer.concat([cipher.update(plain,'utf8'),cipher.final()]).toString('base64');
}
export function decryptLegacyPassword(cipherText:string):string {
  const p=parameters();if(!p)throw new Error('Legacy encryption configuration is missing');
  const decipher=createDecipheriv('aes-256-cbc',p.key,p.iv);return Buffer.concat([decipher.update(Buffer.from(cipherText,'base64')),decipher.final()]).toString('utf8');
}
