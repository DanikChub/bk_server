import type { ReactNode } from "react";
import { AppHeader } from "@/widgets/app-header/app-header";
import { AppSidebar } from "@/widgets/app-sidebar/app-sidebar";

export function AppShell({ children }: { children: ReactNode }) {
  return <div className="app-shell"><AppSidebar/><div className="app-main"><AppHeader/><main className="page-content">{children}</main></div></div>;
}
