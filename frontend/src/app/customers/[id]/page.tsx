import { notFound } from "next/navigation";
import { customers } from "@/entities/customer/model/mock";
import { CustomerDetailsPage } from "@/views/customers/ui/customer-details-page";

export function generateStaticParams() {
  return customers.map(({ id }) => ({ id }));
}

export default async function Page({ params }: { params: Promise<{ id: string }> }) {
  const { id } = await params;
  const customer = customers.find((item) => item.id === id);
  if (!customer) notFound();
  return <CustomerDetailsPage customer={customer} />;
}
