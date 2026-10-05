import Link from "next/link";
import type { ReactNode } from "react";

type Breadcrumb = { label: string; href?: string };
export function PageHeading({ title, breadcrumbs = [], actions }: { title: ReactNode; breadcrumbs?: Breadcrumb[]; actions?: ReactNode }) {
  return <header className="page-heading"><div><h1>{title}</h1><nav className="breadcrumbs" aria-label="Навигационная цепочка"><Link href="/">Главная</Link>{breadcrumbs.map((item, index) => <span className="breadcrumb-item" key={`${item.label}-${index}`}><span aria-hidden="true">/</span>{item.href ? <Link href={item.href}>{item.label}</Link> : <span aria-current={index === breadcrumbs.length - 1 ? "page" : undefined}>{item.label}</span>}</span>)}</nav></div>{actions && <div className="page-heading-actions">{actions}</div>}</header>;
}
