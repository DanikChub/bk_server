import { Fragment } from "react";
export function MessageText({text}:{text:string}){
  return <>{text.split("\n").map((line,index)=><Fragment key={index}>{index>0&&<br/>}{line.split(/(\*\*[^*]+\*\*|__[^_]+__|\*[^*]+\*|`[^`]+`)/g).map((part,i)=>part.startsWith("**")?<strong key={i}>{part.slice(2,-2)}</strong>:part.startsWith("__")?<u key={i}>{part.slice(2,-2)}</u>:part.startsWith("*")?<em key={i}>{part.slice(1,-1)}</em>:part.startsWith("`")?<code key={i}>{part.slice(1,-1)}</code>:<Fragment key={i}>{part}</Fragment>)}</Fragment>)}</>;
}
