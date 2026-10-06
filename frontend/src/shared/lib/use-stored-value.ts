"use client";
import { useCallback, useState, useSyncExternalStore } from "react";
const eventName = "app-preference-change";
function subscribe(callback: () => void) {
  window.addEventListener("storage", callback);
  window.addEventListener(eventName, callback);
  return () => { window.removeEventListener("storage", callback); window.removeEventListener(eventName, callback); };
}
export function useStoredValue<T>(key: string, fallback: T, isValid: (value: unknown) => value is T) {
  const [memory, setMemory] = useState<{ key: string; value: T } | null>(null);
  const snapshot = useCallback(() => { try { return window.localStorage.getItem(key); } catch { return null; } }, [key]);
  const raw = useSyncExternalStore(subscribe, snapshot, () => null);
  let value = fallback;
  try { if (raw) { const parsed: unknown = JSON.parse(raw); if (isValid(parsed)) value = parsed; } } catch { /* Ignore invalid or unavailable storage. */ }
  function setValue(next: T) {
    try { window.localStorage.setItem(key, JSON.stringify(next)); setMemory(null); window.dispatchEvent(new Event(eventName)); return true; } catch { setMemory({ key, value: next }); return false; }
  }
  return [memory?.key === key ? memory.value : value, setValue] as const;
}
