import { test } from 'node:test';
import assert from 'node:assert/strict';
import { pbkdf2Sync } from 'node:crypto';
import { verifyPassword, hashPassword } from '../src/server/auth/password';
import { verifyAuthenticatorCode } from '../src/server/auth/totp';
import { legacyDeadline } from '../src/server/modules/deadline';

test('ASP.NET Identity V2 SHA1 fixture authenticates without rehashing',async()=>{
  const salt=Buffer.alloc(16,17),key=pbkdf2Sync('Legacy-Password',salt,1000,32,'sha1');
  const hash=Buffer.concat([Buffer.from([0]),salt,key]).toString('base64');
  assert.equal(await verifyPassword('Legacy-Password',hash),true);
  assert.equal(await verifyPassword('wrong',hash),false);
});
test('ASP.NET Identity V3 supports all original PRF values',async()=>{
  for(const [prf,digest]of ['sha1','sha256','sha512'].entries()){
    const salt=Buffer.alloc(16,23),header=Buffer.alloc(13);header[0]=1;header.writeUInt32BE(prf,1);header.writeUInt32BE(10000,5);header.writeUInt32BE(16,9);
    const hash=Buffer.concat([header,salt,pbkdf2Sync('Пароль',salt,10000,32,digest)]).toString('base64');
    assert.equal(await verifyPassword('Пароль',hash),true);
    assert.equal(await verifyPassword('Другой пароль',hash),false);
  }
});
test('new hashes use Identity V3 and reject damaged or excessive-cost hashes',async()=>{
  const hash=await hashPassword('Новый-пароль!');assert.equal(await verifyPassword('Новый-пароль!',hash),true);
  const bytes=Buffer.from(hash,'base64');bytes.writeUInt32BE(0xffffffff,5);
  assert.equal(await verifyPassword('Новый-пароль!',bytes.toString('base64')),false);
  assert.equal(await verifyPassword('password','invalid'),false);
  assert.equal(await verifyPassword('password',null),false);
});
test('authenticator codes match the RFC 6238 SHA1 vector and reject invalid input',()=>{
  const secret='GEZDGNBVGY3TQOJQGEZDGNBVGY3TQOJQ';
  assert.equal(verifyAuthenticatorCode(secret,'287082',59000),true);
  assert.equal(verifyAuthenticatorCode(secret,'000000',59000),false);
  assert.equal(verifyAuthenticatorCode(secret,'94287082',59000),false);
  assert.equal(verifyAuthenticatorCode('bad secret','123456',59000),false);
});
test('deadline preserves the original bounded weekend and holiday adjustment',()=>{
  assert.equal(legacyDeadline(new Date('2026-11-13T10:00:00Z'),2*86400,[],'UTC').toISOString(),'2026-11-16T10:00:00.000Z');
  assert.equal(legacyDeadline(new Date('2026-11-09T10:00:00Z'),2*86400,['2026-11-09'],'UTC').toISOString(),'2026-11-12T10:00:00.000Z');
});
