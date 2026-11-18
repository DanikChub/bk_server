import { CustomerEditorPage } from '@/views/customers/ui/customer-editor-page';
export default async function Page({params}:{params:Promise<{id:string}>}){const {id}=await params;return <CustomerEditorPage id={id}/>;}
