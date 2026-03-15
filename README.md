# PeripheralsStore — учебный ASP.NET Core MVC проект

## Описание проекта
Информационная система интернет-магазина компьютерной периферии с пользовательской и административной частью.

## Предметная область
Продажа компьютерной периферии: клавиатуры, мыши, наушники и др.

## Технологии
- ASP.NET Core MVC (.NET 8)
- Entity Framework Core
- MSSQL LocalDB
- Cookie Authentication (без ASP.NET Identity)
- Bootstrap 5

## Структура проекта
- `Controllers` — контроллеры пользовательской и админской логики
- `Models` — сущности БД (`User`, `Role`, `Category`, `Brand`, `Product`, `Review`, `CartItem`)
- `ViewModels` — модели для форм
- `Data` — `AppDbContext`
- `Services` — хеширование паролей и авторизация
- `Helpers` — нормализация ввода
- `Views` — Razor-представления
- `wwwroot` — статика и стили

## Как запустить
1. `dotnet restore`
2. `dotnet ef migrations add InitialCreate --project PeripheralsStore`
3. `dotnet ef database update --project PeripheralsStore`
4. `dotnet run --project PeripheralsStore`

## Тестовые аккаунты
- Админ: `admin@peripherals.local` / `Admin123!`

## Какие требования реализованы
- Регистрация и вход через собственные таблицы БД
- Хранение пароля в виде хеша
- Роли `User`/`Admin` и доступ по `[Authorize]`
- Каталог: поиск, фильтры, сортировка, пагинация
- CRUD для товаров, категорий и брендов
- Отзывы с модерацией
- Корзина (`CartItem`) с добавлением, изменением количества и удалением
- Бан/разбан и повышение роли пользователя
- Валидация на клиенте/сервере и ограничения полей
- Глобальная обработка ошибок и страница Error
- Единый интерфейс и адаптивный дизайн
