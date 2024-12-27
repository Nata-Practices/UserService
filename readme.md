# UserService

## Описание
`UserService` — это 2-ой микросервис для 4-ой практической работы.

---

## Структура проекта

```plaintext
UserService/
├── Dependencies/                      # Зависимости проекта, подключенные через NuGet
├── Properties/                        # Конфигурационные файлы и параметры запуска
│   ├── appsettings.json               # Основные настройки приложения (MongoDB, Kafka, логирование)
│   ├── appsettings.Development.json   # Настройки для режима разработки
│   └── launchSettings.json            # Настройки запуска приложения
├── Controllers/                       # Контроллеры API
│   └── UsersController.cs             # Обработчик запросов для работы с пользователями
├── Models/                            # Модели данных приложения
│   ├── ApiResponse.cs                 # Модель данных для ответа от API
│   ├── MessageModel.cs                # Модель данных для сообщений Kafka
│   ├── MongoDBSettings.cs             # Модель для конфигурации подключения к MongoDB
│   └── UserModel.cs                   # Модель данных для пользователей
├── Services/                          # Сервисы и обработка данных
│   └── UserService.cs                 # Сервис для работы с пользователями
├── Utils/                             # Утилиты приложения
│   ├── KafkaProducer.cs               # Реализация Kafka Producer для отправки сообщений
│   └── KafkaConsumer.cs               # Реализация Kafka Consumer для обработки сообщений
└── Program.cs                         # Точка входа в приложение
```

---

## Установка и запуск

### Требования:
- .NET 8.0 (или новее)
- IDE с поддержкой .NET
- MongoDB (локальная или удалённая база данных)
- Kafka (локальный или удалённый сервер)

### Шаги установки:
1. Склонируй репозиторий:
   ```bash
   git clone https://github.com/Nata-Practices/UserService.git
   ```
2. Перейди в папку проекта:
   ```bash
   cd UserService
   ```
3. Установи зависимости:
   ```bash
   dotnet restore
   ```

### Запуск приложения:
1. Собери проект:
   ```bash
   dotnet build
   ```
2. Запусти сервер:
   ```bash
   dotnet run
   ```
3. Приложение будет доступно по адресу `http://localhost:5137/swagger/index.html`.

---

## API Роуты

### Users
- `GET /api/users` — получить всех пользователей
- `POST /api/users` — создать нового пользователя
- `GET /api/users/{id}` — получить конкретного пользователя по id
- `PUT /api/users/{id}` — обновить конкретного пользователя по id
- `DELETE /api/users/{id}` — удалить конкретного пользователя по id

---

## Kafka

- **Producer**:
  - Отправляет подтверждение объекта в тему `ObjectConfirmations`.

- **Consumer**:
  - Принимает сообщения из темы `ObjectRequests`:
    - Сообщение содержит `Id` объекта (ключ) и `UserId` (значение).
    - Увеличивает поле `RegisteredObjects` для соответствующего пользователя.

---