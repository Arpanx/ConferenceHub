# ConferenceHub

REST API для керування конференц-залами, додатковими послугами та бронюваннями.

Проєкт виконано як тестове завдання з використанням ASP.NET Core Web API, Entity Framework Core та SQL Server LocalDB.

## Основні можливості

* створення конференц-залів;
* редагування залів;
* м'яке видалення залів;
* отримання списку залів;
* отримання інформації про конкретний зал;
* отримання доступних додаткових послуг залу;
* пошук вільних залів за часом та місткістю;
* бронювання залу;
* перевірка перетину бронювань;
* вибір додаткових послуг під час бронювання;
* автоматичний розрахунок вартості з урахуванням тарифних періодів;
* централізована обробка бізнес-помилок;
* автоматизовані unit-тести.

## Технології

### Backend

* .NET 10
* ASP.NET Core Web API
* Entity Framework Core 10
* SQL Server / SQL Server LocalDB
* Swagger / OpenAPI
* C#
* xUnit
* Moq

### Frontend

У проєкті також присутній Angular-клієнт у каталозі `ClientApp`.

## Архітектура

Проєкт розділений на такі основні компоненти:

```text
ConferenceHub
│
├── Controllers
│   ├── HallController
│   └── BookingController
│
├── Services
│   ├── HallService
│   ├── BookingService
│   └── PricingService
│
├── Models
│   ├── Hall
│   ├── Booking
│   ├── AdditionalService
│   ├── HallAdditionalService
│   └── BookingAdditionalService
│
├── Models/Dtos
│
├── Data
│   ├── ConferenceHubDbContext
│   ├── ConferenceHubDbContextFactory
│   └── DatabaseSeeder
│
├── Exceptions
│   ├── BusinessException
│   └── GlobalExceptionHandler
│
└── Migrations
```

Основна бізнес-логіка знаходиться у сервісах, а контролери відповідають за HTTP API.

`PricingService` винесений в окремий сервіс, щоб розрахунок вартості не залежав від контролерів або логіки бронювання.

## Модель даних

Основні сутності:

### Hall

Конференц-зал:

* `Id`
* `Name`
* `Capacity`
* `BaseHourlyRate`
* `IsActive`

### AdditionalService

Додаткова послуга:

* `Id`
* `Name`
* `Price`
* `IsActive`

### Booking

Бронювання:

* `Id`
* `HallId`
* `StartTime`
* `EndTime`
* `TotalCost`
* `CreatedAt`

### HallAdditionalService

Зв'язок між залом та доступними послугами.

Складений ключ:

```text
HallId + AdditionalServiceId
```

### BookingAdditionalService

Послуги, вибрані для конкретного бронювання.

Зберігається ціна послуги на момент бронювання:

```text
BookingId + AdditionalServiceId + Price
```

Це дозволяє зберігати історичну вартість послуги навіть після зміни її поточної ціни.

## Початкові дані

Під час першого запуску застосунок створює такі зали:

| Зал   | Місткість |  Базова ціна |
| ----- | --------: | -----------: |
| Зал А |        50 | 2000 грн/год |
| Зал B |       100 | 3500 грн/год |
| Зал C |        30 | 1500 грн/год |

Додаткові послуги:

| Послуга  |    Ціна |
| -------- | ------: |
| Проєктор | 500 грн |
| Wi-Fi    | 300 грн |
| Звук     | 700 грн |

## Тарифи

Вартість оренди розраховується окремо для кожного часового інтервалу.

| Час         |       Тариф |
| ----------- | ----------: |
| 06:00–09:00 |        -10% |
| 09:00–18:00 | базова ціна |
| 12:00–14:00 |        +15% |
| 18:00–23:00 |        -20% |
| 23:00–06:00 | базова ціна |

У разі перетину кількох тарифних періодів бронювання автоматично розбивається на відповідні інтервали.

Пріоритет тарифів:

1. 12:00–14:00 — +15%;
2. 06:00–09:00 — -10%;
3. 18:00–23:00 — -20%;
4. інші періоди — базова ціна.

Наприклад, бронювання з 11:00 до 15:00 розраховується так:

```text
11:00–12:00 → 100%
12:00–14:00 → 115%
14:00–15:00 → 100%
```

## API

### Зали

Отримати список залів:

```http
GET /api/Hall
```

Отримати зал:

```http
GET /api/Hall/{id}
```

Отримати послуги залу:

```http
GET /api/Hall/{id}/services
```

Створити зал:

```http
POST /api/Hall
```

Приклад:

```json
{
  "name": "Зал D",
  "capacity": 80,
  "baseHourlyRate": 2800,
  "serviceIds": [1, 2]
}
```

Змінити зал:

```http
PUT /api/Hall/{id}
```

Видалити зал:

```http
DELETE /api/Hall/{id}
```

Видалення реалізовано як soft delete — зал позначається як неактивний.

### Пошук вільних залів

```http
GET /api/Hall/available?startTime=2026-09-10T12:00:00&duration=02:00:00&capacity=40
```

Під час пошуку враховуються:

* активність залу;
* місткість;
* наявні бронювання;
* перетин часових інтервалів.

### Бронювання

Створити бронювання:

```http
POST /api/Booking
```

Приклад:

```json
{
  "hallId": 1,
  "startTime": "2026-09-10T12:00:00",
  "duration": "02:00:00",
  "serviceIds": [1, 2]
}
```

У відповіді повертаються:

* інформація про зал;
* час початку та завершення;
* вартість оренди;
* вартість додаткових послуг;
* загальна вартість;
* вибрані послуги.

## Перевірка перетину бронювань

Для визначення конфлікту використовується стандартна перевірка перетину часових інтервалів:

```text
existing.StartTime < requested.EndTime
AND
existing.EndTime > requested.StartTime
```

Бронювання, які стикаються лише межею часу, не вважаються такими, що перетинаються.

Наприклад:

```text
10:00–12:00
12:00–14:00
```

є допустимими послідовними бронюваннями.

## Обробка помилок

Бізнес-помилки обробляються централізовано через `GlobalExceptionHandler`.

Для повернення помилок використовується `ProblemDetails` з кодом помилки.

Приклади кодів:

```text
HALL_NOT_FOUND
HALL_NOT_ACTIVE
HALL_ALREADY_BOOKED
INVALID_DURATION
INVALID_TIME_RANGE
INVALID_CAPACITY
INVALID_HALL_NAME
INVALID_HALL_CAPACITY
INVALID_HALL_PRICE
INVALID_SERVICES
SERVICE_NOT_AVAILABLE
```

HTTP status codes:

```text
400 Bad Request
404 Not Found
409 Conflict
500 Internal Server Error
```

## Вимоги

Для запуску backend необхідні:

* .NET 10 SDK;
* SQL Server LocalDB;
* Visual Studio 2022 або VS Code.

Використовується екземпляр SQL Server:

```text
(localdb)\MSSQLLocalDB
```

## Запуск backend

Перейти до каталогу backend:

```powershell
cd ConferenceHub
```

Відновити залежності:

```powershell
dotnet restore
```

Застосувати міграції:

```powershell
dotnet ef database update
```

Запустити застосунок:

```powershell
dotnet run
```

Після запуску Swagger доступний за адресою:

```text
https://localhost:7154/swagger
```

Фактичний порт може відрізнятися залежно від налаштувань запуску.

## Міграції бази даних

Створення нової міграції:

```powershell
dotnet ef migrations add MigrationName
```

Застосування міграцій:

```powershell
dotnet ef database update
```

Пересоздання локальної бази даних:

```powershell
dotnet ef database drop --force
dotnet ef database update
```

## Конфігурація бази даних

Налаштування підключення до SQL Server знаходяться у:

```text
ConferenceHub/appsettings.json
```

Тип бази даних:

```json
{
  "DatabaseProvider": "SqlServer"
}
```

Для автоматизованого тестування може використовуватися InMemory database.

## Тестування

Тестовий проєкт:

```text
ConferenceHub.Tests
```

Запуск усіх тестів із кореня solution:

```powershell
dotnet test
```

Тестами покриті:

* розрахунок вартості;
* тарифні періоди;
* перехід через тарифні межі;
* створення та редагування залів;
* видалення залів;
* пошук доступних залів;
* перевірка місткості;
* перевірка зайнятості;
* перевірка додаткових послуг;
* створення бронювань;
* перевірка перетину бронювань;
* перевірка помилок;
* контролери та HTTP-результати.

## Принципи реалізації

Під час розробки використовувалися такі підходи:

* розділення HTTP-рівня та бізнес-логіки;
* Dependency Injection;
* DTO замість безпосереднього використання entity в API;
* Entity Framework Core;
* async/await;
* CancellationToken;
* централізована обробка винятків;
* soft delete для залів та послуг;
* `AsNoTracking()` для read-only запитів;
* збереження історичної ціни додаткових послуг;
* валідація вхідних даних;
* unit-тестування бізнес-логіки та контролерів.

## Можливості подальшого розвитку

Для використання у production-середовищі проєкт можна додатково розширити:

* автентифікацією та авторизацією;
* ролями користувачів;
* пагінацією;
* аудитом змін;
* оптимізацією пошуку при великій кількості бронювань;
* захистом від конкурентного створення двох бронювань одного залу;
* інтеграційними тестами з реальною базою даних;
* повноцінним frontend-інтерфейсом для керування залами та бронюваннями.

## Структура solution

```text
ConferenceHub.slnx
│
├── ConferenceHub
│   └── ASP.NET Core Web API
│
├── ConferenceHub.Tests
│   └── Unit tests
│
└── ClientApp
    └── Angular application
```

## Автор

Олександр Пісєцький

Test assignment — ConferenceHub.
