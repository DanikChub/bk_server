"use client";
import { createContext, useCallback, useContext, useEffect, useState, type ReactNode } from "react";
import { demoPortalRepository } from "../api/demo-repository";
import type { PortalRepository, PortalSnapshot } from "./types";

type PortalContext = { data: PortalSnapshot | null; loading: boolean; error: string; reload: () => Promise<void>; repository: PortalRepository };
const Context = createContext<PortalContext | null>(null);
export function PortalProvider({ children, repository = demoPortalRepository }: { children: ReactNode; repository?: PortalRepository }) {
  const [data, setData] = useState<PortalSnapshot | null>(null);
  const [loading, setLoading] = useState(true); const [error, setError] = useState("");
  const reload = useCallback(async () => { try { setData(await repository.load()); setError(""); } catch (e) { setError(e instanceof Error ? e.message : "Не удалось загрузить данные."); } }, [repository]);
  useEffect(() => { const controller = new AbortController(); repository.load(controller.signal).then(value => { if (!controller.signal.aborted) { setData(value); setLoading(false); } }).catch(e => { if (!controller.signal.aborted) { setError(e instanceof Error ? e.message : "Не удалось загрузить данные."); setLoading(false); } }); return () => controller.abort(); }, [repository]);
  return <Context.Provider value={{ data, loading, error, reload, repository }}>{children}</Context.Provider>;
}
export function usePortal() { const context = useContext(Context); if (!context) throw new Error("PortalProvider is required"); return context; }
export function PortalState({ children }: { children: ReactNode }) {
  const { data, loading, error, reload } = usePortal();
  if (loading) return <div className="panel" role="status">Загрузка…</div>;
  if (!data) return <div className="panel" role="alert">{error || "Данные недоступны."}<button className="secondary-button" onClick={reload}>Повторить</button></div>;
  return <>{error && <p className="portal-error" role="alert">{error}</p>}{children}</>;
}
