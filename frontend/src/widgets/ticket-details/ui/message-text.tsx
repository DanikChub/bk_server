import Markdown from 'react-markdown';
import remarkGfm from 'remark-gfm';
import rehypeRaw from 'rehype-raw';
import rehypeSanitize, { defaultSchema } from 'rehype-sanitize';
const schema={...defaultSchema,tagNames:[...(defaultSchema.tagNames??[]),'u','mark'],attributes:{...defaultSchema.attributes,p:[...(defaultSchema.attributes?.p??[]),['align','left','center','right']]}};
export function MessageText({text}:{text:string}){
  // Legacy messages contain HTML. Sanitize the parsed tree before rendering any element.
  const source=text.replace(/__([^_]+)__/g,'<u>$1</u>').replace(/([^\n])\n(?!\n)/g,'$1  \n');
  return <Markdown remarkPlugins={[remarkGfm]} rehypePlugins={[rehypeRaw,[rehypeSanitize,schema]]} components={{p:({children,node})=><span data-align={String(node?.properties?.align??'left')} className="message-paragraph">{children}</span>,img:({src,alt})=><a href={typeof src==='string'?src:undefined} rel="noreferrer" target="_blank">{alt||'Изображение'}</a>,a:({children,href})=><a href={href} rel="noreferrer" target="_blank">{children}</a>}}>{source}</Markdown>;
}
