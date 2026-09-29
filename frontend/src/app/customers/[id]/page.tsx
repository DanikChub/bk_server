import { notFound } from "next/navigation";
import { customers } from "@/entities/customer/model/mock";
import { CustomerDetailsPage } from "@/pages/customers/ui/customer-details-page";

export default async function Page({ params }: { params: Promise<{ id: string }> }) {
 const { id } = await params;
 const customer = customers.find(c => c.id === id);
 if (!customer) notFound();
 return <CustomerDetailsPage customer={customer}/>;
}
