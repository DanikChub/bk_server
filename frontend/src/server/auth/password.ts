import { pbkdf2, randomBytes, timingSafeEqual } from 'node:crypto';
import { promisify } from 'node:util';

const derive = promisify(pbkdf2);
// ASP.NET Identity PasswordHasher V2 and V3. Existing hashes need no reset.
export async function verifyPassword(password: string, encoded: string | null): Promise<boolean> {
  if (!encoded || encoded.length > 1024 || password.length > 1024) return false;
  try {
    const data = Buffer.from(encoded, 'base64');
    let salt: Buffer, expected: Buffer, iterations: number, digest: string;
    if (data[0] === 0 && data.length === 49) {
      salt = data.subarray(1,17); expected = data.subarray(17); iterations = 1000; digest = 'sha1';
    } else if (data[0] === 1 && data.length >= 45) {
      digest = ['sha1', 'sha256', 'sha512'][data.readUInt32BE(1)];
      iterations = data.readUInt32BE(5);
      const length = data.readUInt32BE(9);
      if (!digest || iterations < 1 || iterations > 2_000_000 || length < 16 || length > 128) return false;
      salt = data.subarray(13,13+length); expected = data.subarray(13+length);
    } else return false;
    if (expected.length < 16 || expected.length > 128) return false;
    const actual = await derive(password, salt, iterations, expected.length, digest);
    return timingSafeEqual(actual, expected);
  } catch { return false; }
}
export async function hashPassword(password: string): Promise<string> {
  const salt = randomBytes(16), iterations = 100_000;
  const key = await derive(password, salt, iterations, 32, 'sha512');
  const header = Buffer.alloc(13); header[0] = 1;
  header.writeUInt32BE(2,1); header.writeUInt32BE(iterations,5); header.writeUInt32BE(salt.length,9);
  return Buffer.concat([header,salt,key]).toString('base64');
}
