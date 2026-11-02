"use client";
import { useEffect, useId, useRef, type ReactNode } from "react";
export function PortalDialog({ open, title, children, onClose, className = "" }: { open: boolean; title: string; children: ReactNode; onClose: () => void; className?: string }) {
  const dialog = useRef<HTMLDialogElement>(null); const id = useId();
  useEffect(() => { if (open && !dialog.current?.open) dialog.current?.showModal(); if (!open && dialog.current?.open) dialog.current.close(); }, [open]);
  return <dialog ref={dialog} className={`portal-dialog ${className}`} aria-labelledby={id} onCancel={e => { e.preventDefault(); onClose(); }}><header><h2 id={id}>{title}</h2><button type="button" aria-label="Закрыть окно" onClick={onClose}>×</button></header>{children}</dialog>;
}
