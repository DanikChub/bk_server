# PostgreSQL: точный перенос схемы из KV.Server

`init.sql` создаёт **69 таблиц, 608 столбцов, 168 индексов, 76 внешних ключей,
69 первичных ключей, 3 именованных UNIQUE-ограничения, 4 identity-последовательности,
6 PL/pgSQL-функций и 6 триггеров**. Инициализатор — обычный SQL; для его применения
не нужны .NET, Entity Framework, Prisma или Node.js.

Источник — исходники `KV.Server`, включая 67 миграций от
`20211017123156_Initial` до `20240826092427_TenantProfileContractsServicePackagesTrigger`.
Имена, типы, обязательность полей, значения по умолчанию, ключи, правила удаления,
индексы и тела функций сохранены. Старые файлы приложения не изменены.
Проверено на настоящем PostgreSQL 18.4.

## Запуск

Создайте **новую пустую** базу и выполните из корня репозитория:

```sh
createdb -h localhost -U postgres consulting_lk
psql -h localhost -U postgres -d consulting_lk -X -v ON_ERROR_STOP=1 -f database/init.sql
```

На Windows, если PostgreSQL не добавлен в PATH:

```powershell
& 'C:/Program Files/PostgreSQL/18/bin/createdb.exe' -h localhost -U postgres consulting_lk
& 'C:/Program Files/PostgreSQL/18/bin/psql.exe' -h localhost -U postgres -d consulting_lk -X -v ON_ERROR_STOP=1 -f database/init.sql
```

Пароль вводится средствами PostgreSQL. В скриптах нет паролей или подключения
к базе заказчика. Также можно открыть `init.sql` в SQL-редакторе, подключённом
к новой базе, и выполнить файл целиком. Скрипт заключён в транзакцию.
Повторный запуск на непустой схеме `public` отклоняется, существующие таблицы
не удаляются и не пересоздаются. Это начальный SQL-baseline, а не обновление
существующей базы.

## Файлы

- `init.sql` — единый файл для применения; включает три части ниже.
- `migrations/001_legacy_schema.sql` — итоговая структура PostgreSQL.
- `migrations/002_legacy_sql_objects.sql` — последние оригинальные функции и триггеры.
- `migrations/003_model_reference_data.sql` — явные `HasData` из последней модели:
  5 категорий лимитов, 18 разделов и 6 типов заявок с исходными UUID.
- `source-manifest.json` — происхождение объектов и SHA-256 135 исходных файлов
  модели/миграций; перед хешированием нормализованы окончания строк.
- `model-migration-differences.json` — отличия сохранённой модели EF от реальных
  SQL-эффектов миграций.
- `tests/expected-catalog.json` — независимый эталон, полученный прогоном миграций,
  со справочниками из последней модели.
- `tests/behavior.sql` — 30 проверок поведения на транзакционных fixtures.

Не редактируйте сгенерированные SQL-файлы по отдельности: `init.sql` собирается
из частей инструментом `tools/build-init.mjs`. Дальнейшие изменения базы следует
оформлять отдельными SQL-миграциями после этого baseline.

## Границы точности и обнаруженные расхождения исходников

Доступа к рабочей базе нет. Это воспроизводимое описание БД **по репозиторию**,
а не дамп неизвестной установленной базы. Известные расхождения не скрыты:

1. В `20240426182006_UserMails_Part3.cs` две команды временно отключают/включают
   `ContractSettings.upsert_constraint_monthly_stats_trigger`. Создания такого
   триггера нет ни в одной миграции. Немодифицированный прогон всех миграций
   останавливается на этом месте. Для диагностического прогона пропущены **только
   эти две команды**, которые не создают постоянных объектов. Отсутствующий
   триггер не придуман; исходный `.cs` не исправлен.
2. Один `ServerDbContextModelSnapshot.cs` недостаточен: в нём отсутствуют
   исторически оставшиеся defaults, пять `timestamptz`-полей `TicketHistory`,
   `ON DELETE RESTRICT` для родительского подразделения и три raw SQL UNIQUE.
   Инициализатор сохраняет **итоговые SQL-эффекты миграций**, а не теряет их при
   генерации только из модели. Подробности — в JSON-отчёте.
3. В последней модели есть тип «Письменная» с UUID
   `a736e1b2-3ad6-471f-971f-d2a27f86a417`, но его вставки нет в исторических
   миграциях. Bootstrap-справочники взяты из явного `HasData` последней модели
   (6 типов), а не из неполной истории seed-операций (5 типов).
4. `__EFMigrationsHistory` не создаётся: не утверждаем, что новая база прошла
   проблемную историю .NET-миграций. Не запускайте старый DbMigrator поверх
   этого SQL-baseline.

Тела функций сохранены без исправления логики (нормализованы только CRLF/LF):

- `upsert_constraint_monthly_stats_trigger_fn` — месячные списания.
- `upsert_monthly_stats_section_counters_trigger_fn` — счётчики по разделам.
- `upsert_monthly_stats_types_counters_trigger_fn` — счётчики по типам.
- `update_ticket_service_packages_trigger_fn` — кеш пакетов в заявке.
- `update_contract_service_packages_trigger_fn` — кеш пакетов после INSERT/UPDATE связи.
- `update_contract_service_packages_trigger_delete_fn` — кеш пакетов после DELETE связи.

Исходные особенности намеренно сохранены и проверены:

- Функции обновления кеша `TenantProfiles.ServicePackagesData` не ограничивают
  внешний UPDATE организацией и изменяют кеш **всех** организаций.
- Месячные функции используют `NEW` даже для DELETE: после удаления счётчики
  и суммы могут остаться устаревшими. Обработка переноса записи в другой месяц
  или другую группу также не добавлена.
- Кеш заявки использует даты договора, кеш договора — даты подключённого пакета.
- Не добавлены отсутствующие в исходнике CHECK, FK или автоматические фильтры
  `TenantId`/`IsDeleted`. Это поведение приложения, а не новые правила БД.

Клиенты, сотрудники, логины, пароли, старые заявки и файлы не копируются.
Runtime-seeders ABP, права, настройки авторизации и OpenIddict-приложения не
выдумываются. В частности, `TicketStatuses` заполнялся C#-seeder'ом, а не `HasData`;
его данные относятся к следующему этапу переноса приложения. Содержимое вложений
хранится вне этих таблиц и также не входит в SQL-инициализацию.

## Проверка без .NET

Windows: одна команда запускает отдельный кластер в `.local/`, создаёт базу,
применяет SQL, проверяет её и останавливает кластер:

```powershell
./database/tools/verify-local.ps1
# При необходимости:
./database/tools/verify-local.ps1 -PostgresBin 'C:/Program Files/PostgreSQL/18/bin' -Port 55440
```

Требуются установленные PostgreSQL и Node.js. Кластер слушает только loopback,
содержит исключительно тестовые данные; существующие сервисы и базы не используются.
Логи остаются в игнорируемой `.local/`, рекурсивное удаление каталогов не выполняется.

Для другой ОС или заранее созданной **тестовой** базы примените `init.sql`, задайте
стандартные `PGHOST`, `PGPORT`, `PGUSER`, `PGDATABASE` и запустите:

```sh
node database/tests/verify.mjs
```

Проверяются все столбцы/defaults/ключи/индексы/последовательности/функции/триггеры
по эталонному каталогу PostgreSQL, справочники и исходные SHA. Поведенческие
fixtures всегда откатываются. Дополнительно проверяется безопасный отказ от
повторной инициализации без изменений в БД.

## Воспроизведение генерации (только для аудита/разработки)

Для экспорта нужен .NET SDK 8, для обычного запуска `init.sql` он не требуется.
Экспортёр компилирует оригинальные `.cs` напрямую. Вместо всего ABP используется
только enum-shim для не влияющей на DDL аннотации `_Abp_DatabaseProvider`;
сущности и бизнес-правила им не заменяются. Настройки старого приложения не читаются.

```sh
dotnet restore database/tools/LegacySchemaExport/LegacySchemaExport.csproj --configfile database/tools/NuGet.config
dotnet run --no-restore --project database/tools/LegacySchemaExport/LegacySchemaExport.csproj -- <absolute-repository-path>
node database/tools/build-init.mjs --prepare-replay
```

Создайте две пустые диагностические базы. В первую примените
`.local/export/replay-audit.sql`, во вторую — `.local/export/snapshot.sql`.
Из первой выполните `pg_dump --schema-only --no-owner --no-privileges` в
`database/.local/export/replayed-schema.sql` (EF history исключается сборщиком).
Затем:

```sh
node database/tools/build-init.mjs
node database/tools/capture-reference.mjs <diagnostic-replay-db> <original-model-db>
```

Не обновляйте эталон из базы, созданной самим `init.sql`: эталон должен оставаться
независимым результатом старых миграций. Весь SQL проверяется на новой пустой базе
после генерации.
