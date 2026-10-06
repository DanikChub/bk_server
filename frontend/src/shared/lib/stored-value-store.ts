const eventName = "app-preference-change";
// Synchronize independently mounted consumers even when storage is unavailable.
const memoryValues = new Map<string, string>();
export function subscribeStoredValue(callback: () => void) {
  const onStorage = (event: StorageEvent) => { if (event.key) memoryValues.delete(event.key);else memoryValues.clear();callback(); };
  window.addEventListener("storage", onStorage);
  window.addEventListener(eventName, callback);
  return () => { window.removeEventListener("storage", onStorage);window.removeEventListener(eventName, callback); };
}
export function readStoredSnapshot(key: string): string | null {
  const memory = memoryValues.get(key);
  if (memory !== undefined) return memory;
  try { return window.localStorage.getItem(key); } catch { return null; }
}
export function writeStoredValue(key: string, serialized: string): boolean {
  let persisted = true;
  try { window.localStorage.setItem(key, serialized);memoryValues.delete(key); } catch { memoryValues.set(key, serialized);persisted = false; }
  window.dispatchEvent(new Event(eventName));
  return persisted;
}
