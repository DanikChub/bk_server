"use client";
import { useCallback, useSyncExternalStore } from "react";
import { readStoredSnapshot, subscribeStoredValue, writeStoredValue } from "./stored-value-store";
export function useStoredValue<T>(key: string, fallback: T, isValid: (value: unknown) => value is T) {
  const snapshot = useCallback(() => readStoredSnapshot(key), [key]);
  const raw = useSyncExternalStore(subscribeStoredValue, snapshot, () => null);
  let value = fallback;
  try { if (raw) { const parsed: unknown = JSON.parse(raw);if (isValid(parsed)) value = parsed; } } catch { /* Ignore invalid or unavailable storage. */ }
  return [value, (next: T) => writeStoredValue(key, JSON.stringify(next))] as const;
}
