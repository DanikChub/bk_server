import type { Metadata } from "next";
import "./globals.css";
import { AppShell } from "@/widgets/app-shell/app-shell";

export const metadata: Metadata = {
  title: "Библиотека консультаций",
  description: "Новый интерфейс Библиотеки консультаций",
};

export default function RootLayout({ children }: { children: React.ReactNode }) {
  return <html lang="ru"><body><AppShell>{children}</AppShell></body></html>;
}
