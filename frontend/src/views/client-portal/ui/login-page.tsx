"use client";
/* eslint-disable @next/next/no-img-element */
import { useState } from "react";
import { PortalLink, usePortalDesign } from "@/shared/lib/client-design";
export function ClientLoginPage() {
  const [error, setError] = useState(""); const adapted = usePortalDesign() === "adapted";
  return <main className="portal-login"><form onSubmit={e => { e.preventDefault(); setError("Сервис авторизации ещё не подключён. Для просмотра интерфейса используйте демонстрационный кабинет."); }}><h1><img src={`/design/client/${adapted ? "0252a.png" : "52de5.png"}`} width={30} height={30} alt="" />ИПС Консалтинг</h1><label className="sr-only" htmlFor="client-login">Логин</label><input id="client-login" name="username" placeholder="Логин" autoComplete="username" required /><label className="sr-only" htmlFor="client-password">Пароль</label><input id="client-password" name="password" type="password" placeholder="Пароль" autoComplete="current-password" required /><button className="primary-button">Войти</button><p>Вход в систему доступен только авторизованным пользователям. Для регистрации свяжитесь с нами по телефону <a href="tel:88005505690">8 800 550 5690</a></p>{error && <p role="alert" className="portal-error">{error}</p>}<PortalLink className="portal-login-demo" href="/client">Открыть демонстрационный кабинет</PortalLink></form></main>;
}
