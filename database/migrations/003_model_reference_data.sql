-- Explicit HasData reference rows from the latest saved model. No customer data or generated account IDs.

INSERT INTO "ConstraintTypes" ("Id", "Name")
VALUES ('5575ad4c-45c3-4bfb-88c1-8d0e3ff6ae65', 'НПА');

INSERT INTO "ConstraintTypes" ("Id", "Name")
VALUES ('5ef7dd65-e63f-4d94-a270-4a2d4c74728e', 'Закупки');

INSERT INTO "ConstraintTypes" ("Id", "Name")
VALUES ('63aa36f1-5185-4d77-9fd8-02f78c062808', 'Торги');

INSERT INTO "ConstraintTypes" ("Id", "Name")
VALUES ('6d562221-4437-48d7-b17c-ba4c21328a86', 'БУХУЧЕТ');

INSERT INTO "ConstraintTypes" ("Id", "Name")
VALUES ('8b1fe2cb-165c-449d-a005-71870f37085b', 'Претензии');

INSERT INTO "TicketSections" ("Id", "Name")
VALUES ('1736e1b2-3ad6-471f-971f-d2a27f86a417', 'Бухгалтерский учет');

INSERT INTO "TicketSections" ("Id", "Name")
VALUES ('2736e1b2-3ad6-471f-971f-d2a27f86a417', 'Гражданское законодательство');

INSERT INTO "TicketSections" ("Id", "Name")
VALUES ('3736e1b2-3ad6-471f-971f-d2a27f86a417', 'Административное законодательство');

INSERT INTO "TicketSections" ("Id", "Name")
VALUES ('4736e1b2-3ad6-471f-971f-d2a27f86a417', 'Бюджетное законодательство');

INSERT INTO "TicketSections" ("Id", "Name")
VALUES ('5736e1b2-3ad6-471f-971f-d2a27f86a417', 'Полномочия органов власти(ОМСУ)');

INSERT INTO "TicketSections" ("Id", "Name")
VALUES ('6736e1b2-3ad6-471f-971f-d2a27f86a417', 'Трудовое законодательство');

INSERT INTO "TicketSections" ("Id", "Name")
VALUES ('7736e1b2-3ad6-471f-971f-d2a27f86a417', 'Земельное законодательство');

INSERT INTO "TicketSections" ("Id", "Name")
VALUES ('8736e1b2-3ad6-471f-971f-d2a27f86a417', 'Муниципальное имущество');

INSERT INTO "TicketSections" ("Id", "Name")
VALUES ('9736e1b2-3ad6-471f-971f-d2a27f86a417', 'Закупки(44 - ФЗ, 223 - ФЗ)');

INSERT INTO "TicketSections" ("Id", "Name")
VALUES ('a036e1b2-3ad6-471f-971f-d2a27f86a417', 'Налоговое законодательство');

INSERT INTO "TicketSections" ("Id", "Name")
VALUES ('b136e1b2-3ad6-471f-971f-d2a27f86a417', 'Противодействие коррупции');

INSERT INTO "TicketSections" ("Id", "Name")
VALUES ('c236e1b2-3ad6-471f-971f-d2a27f86a417', 'Гражданский / Арбитражный процесс, КАС');

INSERT INTO "TicketSections" ("Id", "Name")
VALUES ('d336e1b2-3ad6-471f-971f-d2a27f86a417', 'Торги(Аренда, Продажа, Отбор УК)');

INSERT INTO "TicketSections" ("Id", "Name")
VALUES ('e736e1b2-3ad6-471f-971f-d2a27f86a417', 'Антимонопольное законодательство');

INSERT INTO "TicketSections" ("Id", "Name")
VALUES ('f736e1b2-3ad6-471f-971f-d2a27f86a417', 'Гражданская / Муниципальная служба');

INSERT INTO "TicketSections" ("Id", "Name")
VALUES ('fa36e1b2-3ad6-471f-971f-d2a27f86a417', 'НТО');

INSERT INTO "TicketSections" ("Id", "Name")
VALUES ('fb36e1b2-3ad6-471f-971f-d2a27f86a417', 'Корпоративное право');

INSERT INTO "TicketSections" ("Id", "Name")
VALUES ('fc36e1b2-3ad6-471f-971f-d2a27f86a417', 'Иное');

INSERT INTO "TicketTypes" ("Id", "Name", "SLATime")
VALUES ('1736e1b2-3ad6-471f-971f-d2a27f86a416', 'Размещение', INTERVAL '7 00:00:00');

INSERT INTO "TicketTypes" ("Id", "Name", "SLATime")
VALUES ('2736e1b2-3ad6-471f-971f-d2a27f86a416', 'Разработка', INTERVAL '7 00:00:00');

INSERT INTO "TicketTypes" ("Id", "Name", "SLATime")
VALUES ('3736e1b2-3ad6-471f-971f-d2a27f86a416', 'Сайт', INTERVAL '2 00:00:00');

INSERT INTO "TicketTypes" ("Id", "Name", "SLATime")
VALUES ('4736e1b2-3ad6-471f-971f-d2a27f86a416', 'ИТ', INTERVAL '2 00:00:00');

INSERT INTO "TicketTypes" ("Id", "Name", "SLATime")
VALUES ('a736e1b2-3ad6-471f-971f-d2a27f86a416', 'Устная', INTERVAL '2 00:00:00');

INSERT INTO "TicketTypes" ("Id", "Name", "SLATime")
VALUES ('a736e1b2-3ad6-471f-971f-d2a27f86a417', 'Письменная', INTERVAL '2 00:00:00');
