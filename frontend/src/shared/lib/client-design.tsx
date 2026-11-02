"use client";
import Link from "next/link";
import { createContext, useContext, type ComponentProps } from "react";
import { useSearchParams } from "next/navigation";
export type PortalDesign = "original" | "adapted";
export const PortalDesignContext = createContext<PortalDesign>("original");
export function usePortalDesign() { return useContext(PortalDesignContext); }
export function usePortalHref() { const design = usePortalDesign(); return (href: string) => `${href}${href.includes("?") ? "&" : "?"}design=${design}`; }
export function PortalLink({ href, ...props }: Omit<ComponentProps<typeof Link>, "href"> & { href: string }) { const resolve = usePortalHref(); return <Link href={resolve(href)} {...props} />; }
export function useDesignParam(): PortalDesign { return useSearchParams().get("design") === "adapted" ? "adapted" : "original"; }
