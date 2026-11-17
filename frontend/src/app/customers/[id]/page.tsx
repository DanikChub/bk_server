import { notFound } from "next/navigation";
import { customers } from "@/entities/customer/model/mock";
import { CustomerDetailsPage } from "@/views/customers/ui/customer-details-page";
import { LiveCustomerDetails } from '@/views/backend/ui/live-directories';

export function generateStaticParams() {
  return process.env.NEXT_PUBLIC_DATA_MODE==='demo'?customers.map(({ id }) => ({ id })):[];
}

export default async function Page({ params }: { params: Promise<{ id: string }> }) {
  const { id } = await params;
  if(process.env.NEXT_PUBLIC_DATA_MODE!=='demo')return <LiveCustomerDetails id={id}/>;
  const customer = customers.find((item) => item.id === id);
  if (!customer) notFound();
  return <CustomerDetailsPage customer={customer} />;
}
