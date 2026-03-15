using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace PeripheralsStore.Data;

/// <summary>
/// Простая и понятная инициализация БД для учебного проекта.
/// </summary>
public static class DbInitializer
{
    public static async Task InitializeAsync(IServiceProvider services, ILogger logger)
    {
        await using var scope = services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        try
        {
            // Основной путь: применяем миграции.
            await dbContext.Database.MigrateAsync();
            logger.LogInformation("Миграции базы данных успешно применены.");
        }
        catch (SqlException ex) when (
            ex.Message.Contains("Invalid column name", StringComparison.OrdinalIgnoreCase) ||
            ex.Message.Contains("Cannot find the object", StringComparison.OrdinalIgnoreCase))
        {
            logger.LogWarning(ex,
                "Обнаружена несовместимая старая схема БД. Выполняется пересоздание учебной базы данных.");

            // Учебный fallback: если схема устарела, пересоздаём БД,
            // чтобы приложение гарантированно запускалось у преподавателя/студента.
            await dbContext.Database.EnsureDeletedAsync();
            await dbContext.Database.EnsureCreatedAsync();
            logger.LogInformation("База данных пересоздана с актуальной схемой.");
        }
    }
}
