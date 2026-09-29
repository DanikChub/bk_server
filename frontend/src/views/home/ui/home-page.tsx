import Link from "next/link";
import { ArrowRight, BookOpen, Calculator, CalendarDays, FileSearch } from "lucide-react";

const apps = [
  { title: "Калькуляторы", icon: Calculator },
  { title: "Классификаторы", icon: FileSearch },
  { title: "Справки", icon: BookOpen },
  { title: "Мой календарь", icon: CalendarDays },
];

export function HomePage() {
  return (
    <div>
      <div className="page-heading"><div><h1>Главная</h1><p>Рабочее пространство Библиотеки консультаций</p></div></div>
      <div className="hero-grid">
        <section className="hero-card"><span className="eyebrow">Библиотека консультаций</span><h2>Вся работа с клиентами и обращениями в одном месте</h2><p>Демонстрационная версия нового интерфейса. Данные пока работают на локальных fixtures.</p><Link href="/tickets" className="primary-button">Перейти к обращениям <ArrowRight size={16}/></Link></section>
        <section className="stats-card"><div><strong>24</strong><span>новых обращения</span></div><div><strong>8</strong><span>на сегодня</span></div><div><strong>142</strong><span>активных клиента</span></div></section>
      </div>
      <section className="quick-apps">{apps.map(({title,icon:Icon})=><div className="quick-app" key={title}><Icon size={22}/><span>{title}</span></div>)}</section>
      <div className="dashboard-grid">
        <section className="panel"><div className="panel-title"><h2>Главное</h2><Link href="/tickets">Все обращения</Link></div><div className="news-list"><article><span>29 сентября</span><strong>Обновлена база консультаций по налоговому законодательству</strong></article><article><span>28 сентября</span><strong>Добавлены новые формы документов для кадрового учёта</strong></article><article><span>26 сентября</span><strong>Изменения в разделе договорной работы</strong></article></div></section>
        <section className="panel library-card"><BookOpen size={28}/><h2>Библиотека консультаций</h2><p>Быстрый доступ к материалам, справкам и внутренней базе знаний.</p><button className="secondary-button">Открыть библиотеку</button></section>
      </div>
    </div>
  );
}
