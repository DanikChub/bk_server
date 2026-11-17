"use client";
import { createContext, useCallback, useContext, useEffect, useRef, useState, type ReactNode } from "react";
import Link from 'next/link';
import { demoPortalRepository } from "../api/demo-repository";
import { httpPortalRepository } from "../api/http-repository";
import type { PortalRepository, PortalSnapshot } from "./types";

type PortalContext = { data: PortalSnapshot | null; loading: boolean; error: string; reload: () => Promise<void>; clear:()=>void; repository: PortalRepository };
const Context = createContext<PortalContext | null>(null);
export function PortalProvider({ children, repository = process.env.NEXT_PUBLIC_DATA_MODE === 'demo' ? demoPortalRepository : httpPortalRepository }: { children: ReactNode; repository?: PortalRepository }) {
  const [data, setData] = useState<PortalSnapshot | null>(null);
  const generation=useRef(0);
  const [loading, setLoading] = useState(true); const [error, setError] = useState("");
  const clear=useCallback(()=>{generation.current++;setData(null);setError('');setLoading(false);},[]);
  const reload = useCallback(async () => { const version=generation.current;try { const value=await repository.load();if(generation.current===version){setData(value);setError('');setLoading(false);} } catch (e) { if(generation.current===version){setData(null);setError(e instanceof Error ? e.message : 'Не удалось загрузить данные.');setLoading(false);} } }, [repository]);
  useEffect(() => { const controller = new AbortController(),version=generation.current; repository.load(controller.signal).then(value => { if (!controller.signal.aborted && generation.current===version) { setData(value); setLoading(false); } }).catch(e => { if (!controller.signal.aborted && generation.current===version) { setData(null);setError(e instanceof Error ? e.message : "Не удалось загрузить данные."); setLoading(false); } }); return () => controller.abort(); }, [repository]);
  return <Context.Provider value={{ data, loading, error, reload, clear, repository }}>{children}</Context.Provider>;
}
export function usePortal() { const context = useContext(Context); if (!context) throw new Error("PortalProvider is required"); return context; }
export function PortalState({ children }: { children: ReactNode }) {
  const { data, loading, error, reload } = usePortal();
  if (loading) return <div className="panel" role="status">Загрузка…</div>;
  if (!data) return <div className="panel" role="alert">{error || "Данные недоступны."}<button className="secondary-button" onClick={reload}>Повторить</button><Link href="/client/login">Войти</Link></div>;
  return <>{error && <p className="portal-error" role="alert">{error}</p>}{children}</>;
}
