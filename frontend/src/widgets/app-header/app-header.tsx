"use client";

import { Bell, Menu, Search } from "lucide-react";

export function AppHeader() {
  return (
    <header className="topbar">
      <button className="icon-button" aria-label="Меню"><Menu size={18}/></button>
      <div className="topbar-search"><Search size={16}/><span>Поиск по системе</span></div>
      <div className="topbar-actions">
        <button className="notification" aria-label="Уведомления"><Bell size={18}/><span>3</span></button>
        <div className="user"><div className="avatar">ДС</div><div><strong>Даниил</strong><small>Администратор</small></div></div>
      </div>
    </header>
  );
}
