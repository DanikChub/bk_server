import { PageHeading } from "@/shared/ui/page-heading/page-heading";
import { Plus } from "lucide-react";
import { StatusBadge } from "@/shared/ui/status-badge/status-badge";

const employees = [
 ["Екатерина Смирнова","Ведущий специалист","24","Онлайн"],
 ["Анна Кузнецова","Специалист","17","Онлайн"],
 ["Михаил Орлов","Юрист","11","Не в сети"],
 ["Ольга Соколова","Бухгалтер-консультант","8","Онлайн"],
];

export default function Page() {
 return <div><PageHeading title={`Сотрудники (${employees.length})`} breadcrumbs={[{ label: "Администрирование" }, { label: "Сотрудники" }]} actions={<button className="primary-button"><Plus size={16}/>Новый сотрудник</button>} /><section className="panel"><table className="data-table"><thead><tr><th>Сотрудник</th><th>Должность</th><th>Обращений</th><th>Статус</th></tr></thead><tbody>{employees.map(e=><tr key={e[0]}><td><strong>{e[0]}</strong></td><td>{e[1]}</td><td>{e[2]}</td><td><StatusBadge tone={e[3]==="Онлайн"?"green":"gray"}>{e[3]}</StatusBadge></td></tr>)}</tbody></table></section></div>
}
