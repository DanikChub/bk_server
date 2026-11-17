// Exact algorithm used by TicketsAppService.CalculateDeadline in KV.Server.
// Kept separately so its historical weekend behavior is explicit and testable.
export function legacyDeadline(start:Date,seconds:number,holidays:readonly string[],timeZone='Europe/Moscow'):Date {
  let deadline=start.getTime()+seconds*1000;const days=Math.floor(seconds/86400),closed=new Set(holidays);
  const format=new Intl.DateTimeFormat('en-CA',{timeZone,year:'numeric',month:'2-digit',day:'2-digit'});
  for(let day=0;day<days;day++) {
    const current=new Date(start.getTime()+day*86400_000),parts=format.formatToParts(current);
    const part=(type:string)=>parts.find(p=>p.type===type)!.value;
    const key=`${part('year')}-${part('month')}-${part('day')}`;
    const weekday=new Date(key+'T00:00:00Z').getUTCDay();
    if(closed.has(key)||weekday===0||weekday===6)deadline+=86400_000;
  }
  return new Date(deadline);
}
