"use client";
import { demoUser } from "@/shared/config/demo-user";
import { DesignIcon } from "@/shared/ui/design-icon/design-icon";

export function AppHeader({ collapsed, onToggleMenu }: { collapsed: boolean; onToggleMenu: () => void }) {
  return <header className="topbar">
    <button className="sidebar-toggle" onClick={onToggleMenu} aria-label={collapsed ? "Открыть меню" : "Свернуть меню"} aria-controls="app-sidebar" aria-expanded={!collapsed}><DesignIcon name="collapse" /></button>
    <div className="topbar-actions">
      <span className="topbar-greeting">Здравствуйте, {demoUser.firstName} {demoUser.lastName}!</span>
      <button className="notification" disabled title="Уведомления будут подключены на этапе 5" aria-label={`Уведомления: ${demoUser.unreadNotifications} непрочитанных`}><DesignIcon name="bell" /><span>{demoUser.unreadNotifications}</span></button>
      <button className="logout-button" disabled title="Выход будет доступен после подключения авторизации"><DesignIcon name="logout" /><span>Выйти</span></button>
    </div>
  </header>;
}
