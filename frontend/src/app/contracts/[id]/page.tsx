import { LiveContractDetails } from '@/views/backend/ui/live-directories';
export default async function Page({params}:{params:Promise<{id:string}>}){const {id}=await params;return <LiveContractDetails id={id}/>;}
