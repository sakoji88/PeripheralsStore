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

            // Если миграций нет, старая БД может остаться со старой схемой.
            // Проверяем критичные колонки и при несовместимости пересоздаём БД.
            if (!await HasRequiredProductColumnsAsync(dbContext))
            {
                logger.LogWarning(
                    "Обнаружена устаревшая схема таблицы Products (не хватает обязательных колонок). Выполняется пересоздание БД.");
                await RecreateDatabaseAsync(dbContext, logger);
                return;
            }

            logger.LogInformation("Миграции базы данных успешно применены.");
        }
        catch (SqlException ex) when (
            ex.Message.Contains("Invalid column name", StringComparison.OrdinalIgnoreCase) ||
            ex.Message.Contains("Cannot find the object", StringComparison.OrdinalIgnoreCase))
        {
            logger.LogWarning(ex,
                "Обнаружена несовместимая старая схема БД. Выполняется пересоздание учебной базы данных.");
            await RecreateDatabaseAsync(dbContext, logger);
        }
    }

    private static async Task RecreateDatabaseAsync(AppDbContext dbContext, ILogger logger)
    {
        await dbContext.Database.EnsureDeletedAsync();
        await dbContext.Database.EnsureCreatedAsync();
        logger.LogInformation("База данных пересоздана с актуальной схемой.");
    }

    private static async Task<bool> HasRequiredProductColumnsAsync(AppDbContext dbContext)
    {
        var connectionString = dbContext.Database.GetConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return false;
        }

        var requiredColumns = new[] { "Article", "ConnectionType", "ImageUrl", "StockQuantity" };

        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();

        const string sql = """
                           SELECT COUNT(*)
                           FROM INFORMATION_SCHEMA.COLUMNS
                           WHERE TABLE_NAME = 'Products' AND COLUMN_NAME = @ColumnName
                           """;

        foreach (var columnName in requiredColumns)
        {
            await using var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@ColumnName", columnName);
            var exists = (int)(await command.ExecuteScalarAsync() ?? 0) > 0;
            if (!exists)
            {
                return false;
            }
        }

        return true;
    }
}
