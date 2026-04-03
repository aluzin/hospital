# Hospital

REST API приложение на `.NET 6` с CRUD-методами для сущности `Patient`, где `Patient` рассматривается как ребенок, рожденный в роддоме.

Реализовано:

- REST API с CRUD для `Patient`
- поиск по `birthDate` по правилам FHIR `date`
- Swagger
- консольный `Seeder`, который загружает 100 пациентов через API
- Docker Compose для запуска API и PostgreSQL
- unit и integration tests
- Postman collection для демонстрации методов

## Стек

- `.NET 6`
- `ASP.NET Core Web API`
- `Entity Framework Core 6`
- `PostgreSQL`
- `Swagger / Swashbuckle`
- `FluentValidation`
- `Serilog`

## Структура решения

- `src/Hospital.Api` - HTTP API
- `src/Hospital.Application` - use cases
- `src/Hospital.Domain` - доменные модели
- `src/Hospital.Infrastructure` - EF Core, PostgreSQL, миграции
- `src/Hospital.Seeder` - консольное приложение для загрузки тестовых данных
- `tests/Hospital.UnitTests` - unit tests
- `tests/Hospital.IntegrationTests` - integration tests
- `postman/Hospital.postman_collection.json` - Postman collection

## Что умеет API

Основные маршруты:

- `POST /api/patients`
- `GET /api/patients/{id}`
- `PUT /api/patients/{id}`
- `DELETE /api/patients/{id}`
- `GET /api/patients` - поиск и пагинация по `birthDate`

Swagger:

- `http://localhost:8080/swagger`

## Формат Patient

Пример:

```json
{
  "name": {
    "id": "d8ff176f-bd0a-4b8e-b329-871952e32e1f",
    "use": "official",
    "family": "Иванов",
    "given": [
      "Иван",
      "Иванович"
    ]
  },
  "gender": "male",
  "birthDate": "2024-01-13T18:25:43Z",
  "active": true
}
```

Обязательные поля:

- `name.family`
- `birthDate`

Справочники:

- `gender`: `male | female | other | unknown`
- `active`: `true | false`

## Поиск по birthDate

Поиск реализован через `GET /api/patients`.

Поддерживается:

- FHIR-префиксы: `eq`, `ne`, `gt`, `lt`, `ge`, `le`, `sa`, `eb`, `ap`
- точности даты: `yyyy`, `yyyy-MM`, `yyyy-MM-dd`, `dateTime`
- повтор параметра `birthDate` как `AND`
- значения через запятую внутри одного `birthDate` как `OR`
- пагинация через `skip` и `take`

Примеры:

```http
GET /api/patients?birthDate=eq2024-01-13
```

```http
GET /api/patients?birthDate=ge2024-01-13&birthDate=lt2024-01-14
```

```http
GET /api/patients?birthDate=eq2024-01-12,eq2024-01-14
```

```http
GET /api/patients?birthDate=eq2024-01-12,eq2024-01-13&birthDate=eq2024-01-13,eq2024-01-14&skip=0&take=1
```

## Запуск через Docker

Это основной и рекомендуемый способ запуска, так как в задании требуется запуск разработанного ПО вместе с БД в виде docker-контейнеров.

### Что нужно

- установленный и запущенный Docker Desktop

### Запуск API и PostgreSQL

Из корня проекта:

```powershell
docker compose up --build
```

После запуска будут доступны:

- API: `http://localhost:8080`
- Swagger: `http://localhost:8080/swagger`
- PostgreSQL: `localhost:5432`

Compose поднимает:

- `db` - PostgreSQL
- `api` - ASP.NET Core API

Миграции БД применяются автоматически при старте API.

### Запуск Seeder в Docker

Seeder вынесен в отдельный профиль `tools`.

Запуск:

```powershell
docker compose --profile tools up --build seeder
```

По умолчанию `Seeder`:

- ждет доступности API
- создает `100` пациентов

Если нужно сначала поднять API и БД, а потом отдельно запустить сидер:

```powershell
docker compose up --build -d
docker compose --profile tools up --build seeder
```

### Остановка контейнеров

```powershell
docker compose down
```

Если нужно удалить volume с БД:

```powershell
docker compose down -v
```

## Swagger

Swagger доступен по адресу:

- `http://localhost:8080/swagger`

В нем описаны CRUD-методы и search endpoint.

## Postman

Коллекция лежит в файле:

- `postman/Hospital.postman_collection.json`

### Как использовать

1. Импортировать файл в Postman.
2. Убедиться, что переменная `baseUrl` равна `http://localhost:8080`.
3. Запускать запросы в таком порядке:

- `Create Patient`
- `Get Patient By Id`
- `Update Patient`
- `Delete Patient`

После `Create Patient` автоматически сохраняются:

- `patientId`
- `nameId`

Они используются в `Get`, `Update`, `Delete`.

### Что есть в коллекции

- создание пациента
- получение по идентификатору
- обновление
- удаление
- поиск `birthDate = eq`
- поиск `birthDate` через `AND`
- поиск `birthDate` через `OR`
- поиск `birthDate` через `AND + OR + pagination`
- поиск `birthDate = ap`

## Тесты

### Unit tests

```powershell
dotnet test tests/Hospital.UnitTests/Hospital.UnitTests.csproj
```

### Integration tests

Integration tests используют `Testcontainers.PostgreSql`, поэтому требуют запущенный Docker.

Запуск:

```powershell
dotnet test tests/Hospital.IntegrationTests/Hospital.IntegrationTests.csproj
```

Что проверяется:

- CRUD через HTTP
- validation errors
- `404 Not Found`
- поиск по `birthDate`
- `AND/OR`
- пагинация

### Все тесты

```powershell
dotnet test Hospital.sln
```

## Сборка

Сборка всего решения:

```powershell
dotnet build Hospital.sln
```

## Демонстрационный сценарий

1. Поднять API и БД:

```powershell
docker compose up --build
```

2. Открыть Swagger:

```text
http://localhost:8080/swagger
```

3. В отдельной консоли загрузить тестовые данные:

```powershell
docker compose --profile tools up --build seeder
```

4. Импортировать `postman/Hospital.postman_collection.json` в Postman и прогнать запросы.

## Примечания

- Все проекты в решении используют `.NET 6`, как требуется в задании.
- `Hospital.Seeder` использует те же request-контракты, что и API, через `ProjectReference` на `Hospital.Api`.
- Integration tests требуют рабочий Docker daemon.
