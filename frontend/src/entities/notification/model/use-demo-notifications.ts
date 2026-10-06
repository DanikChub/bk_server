"use client";
import { useStoredValue } from "@/shared/lib/use-stored-value";
import { demoNotifications } from "./mock";
import { isNotificationReadState, markNotificationsRead, notificationCounts, notificationsForUser, type NotificationReadState } from "./notifications";
const initial: NotificationReadState = {};
export function useDemoNotifications(identityUserId: string) {
  const [seen, saveSeen] = useStoredValue(`notifications:seen:demo:${identityUserId}:v1`, initial, isNotificationReadState);
  const rows = notificationsForUser(demoNotifications, identityUserId, seen);
  return {
    rows, counts: notificationCounts(rows, identityUserId),
    markRead: (ids: readonly string[]) => saveSeen(markNotificationsRead(demoNotifications, identityUserId, seen, ids, new Date().toISOString())),
  };
}
