import { createHmac, timingSafeEqual } from 'node:crypto';
function decodeBase32(input:string):Buffer {
  const alphabet='ABCDEFGHIJKLMNOPQRSTUVWXYZ234567',text=input.toUpperCase().replace(/[\s=]/g,'');let bits=0,value=0;const bytes:number[]=[];
  for(const char of text){const digit=alphabet.indexOf(char);if(digit<0)throw new Error('Invalid authenticator key');value=(value<<5)|digit;bits+=5;if(bits>=8){bits-=8;bytes.push((value>>>bits)&255);}}
  if(bytes.length<10)throw new Error('Invalid authenticator key');return Buffer.from(bytes);
}
// ASP.NET Core Identity AuthenticatorTokenProvider: SHA1, 30 seconds, 6 digits, +/-2 steps.
export function verifyAuthenticatorCode(secret:string,code:string,now=Date.now()):boolean {
  if(!/^\d{6}$/.test(code))return false;
  try{
    const key=decodeBase32(secret),counter=Math.floor(now/30_000);
    for(let offset=-2;offset<=2;offset++){
      if(counter+offset<0)continue;
      const bytes=Buffer.alloc(8);bytes.writeBigUInt64BE(BigInt(counter+offset));const hmac=createHmac('sha1',key).update(bytes).digest(),index=hmac[hmac.length-1]&15;
      const actual=String((hmac.readUInt32BE(index)&0x7fffffff)%1_000_000).padStart(6,'0');
      if(timingSafeEqual(Buffer.from(actual),Buffer.from(code)))return true;
    }
  }catch{return false;}return false;
}
