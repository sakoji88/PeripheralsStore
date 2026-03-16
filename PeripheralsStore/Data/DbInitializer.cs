using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace PeripheralsStore.Data;

/// <summary>
/// Инициализация схемы БД без потери пользовательских данных по умолчанию.
/// </summary>
public static class DbInitializer
{
    public static async Task InitializeAsync(IServiceProvider services, IConfiguration configuration, ILogger logger)
    {
        await using var scope = services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var allowDestructiveReset = configuration.GetValue<bool>("DatabaseSettings:AllowDestructiveReset");

        try
        {
            await dbContext.Database.MigrateAsync();

            var compatible = await HasRequiredProductColumnsAsync(dbContext);
            if (!compatible)
            {
                if (!allowDestructiveReset)
                {
                    throw new InvalidOperationException(
                        "Схема БД не соответствует текущим моделям (в таблице Products нет обязательных колонок). " +
                        "Обновите БД через миграции или установите DatabaseSettings:AllowDestructiveReset=true для принудительного пересоздания в учебном режиме.");
                }

                logger.LogWarning("Обнаружена старая схема Products. Выполняется пересоздание БД по конфигу.");
                await RecreateDatabaseAsync(dbContext, logger);
            }

            // Для существующих учебных БД без миграций: безопасно создаем недостающую таблицу корзины.
            await EnsureCartItemsTableAsync(dbContext, logger);

            logger.LogInformation("Схема базы данных готова к работе.");
        }
        catch (SqlException ex) when (
            ex.Message.Contains("Invalid column name", StringComparison.OrdinalIgnoreCase) ||
            ex.Message.Contains("Cannot find the object", StringComparison.OrdinalIgnoreCase))
        {
            if (!allowDestructiveReset)
            {
                throw new InvalidOperationException(
                    "Обнаружена несовместимая схема БД. Выполните миграции или включите DatabaseSettings:AllowDestructiveReset=true.", ex);
            }

            logger.LogWarning(ex, "Несовместимая схема БД. Пересоздание включено в конфиге.");
            await RecreateDatabaseAsync(dbContext, logger);
            await EnsureCartItemsTableAsync(dbContext, logger);
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

    private static async Task EnsureCartItemsTableAsync(AppDbContext dbContext, ILogger logger)
    {
        var connectionString = dbContext.Database.GetConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();

        const string sql = """
                           IF OBJECT_ID(N'[dbo].[CartItems]', N'U') IS NULL
                           BEGIN
                               CREATE TABLE [dbo].[CartItems] (
                                   [Id] INT IDENTITY(1,1) NOT NULL,
                                   [UserId] INT NOT NULL,
                                   [ProductId] INT NOT NULL,
                                   [Quantity] INT NOT NULL,
                                   CONSTRAINT [PK_CartItems] PRIMARY KEY ([Id]),
                                   CONSTRAINT [FK_CartItems_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users]([Id]) ON DELETE CASCADE,
                                   CONSTRAINT [FK_CartItems_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [dbo].[Products]([Id]) ON DELETE CASCADE
                               );

                               CREATE UNIQUE INDEX [IX_CartItems_UserId_ProductId] ON [dbo].[CartItems]([UserId], [ProductId]);
                           END
                           """;

        await using var command = new SqlCommand(sql, connection);
        await command.ExecuteNonQueryAsync();
        logger.LogInformation("Проверка таблицы CartItems завершена.");
    }
}
