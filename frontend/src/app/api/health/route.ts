export const runtime='nodejs';
export function GET() {return Response.json({status:'ok',service:'consulting-lk'},{headers:{'Cache-Control':'no-store'}});}
