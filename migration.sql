BEGIN TRANSACTION;
GO

-- Mark RequireEmail migration as applied (tables already exist)
IF NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20260406134518_RequireEmail')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260406134518_RequireEmail', N'8.0.1');
END
GO

DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Customers]') AND [c].[name] = N'Email');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [Customers] DROP CONSTRAINT [' + @var0 + '];');
UPDATE [Customers] SET [Email] = N'' WHERE [Email] IS NULL;
ALTER TABLE [Customers] ALTER COLUMN [Email] nvarchar(max) NOT NULL;
ALTER TABLE [Customers] ADD DEFAULT N'' FOR [Email];
GO

DECLARE @var1 sysname;
SELECT @var1 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Customers]') AND [c].[name] = N'CompanyName');
IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [Customers] DROP CONSTRAINT [' + @var1 + '];');
UPDATE [Customers] SET [CompanyName] = N'' WHERE [CompanyName] IS NULL;
ALTER TABLE [Customers] ALTER COLUMN [CompanyName] nvarchar(max) NOT NULL;
ALTER TABLE [Customers] ADD DEFAULT N'' FOR [CompanyName];
GO

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'CartItems')
BEGIN
    CREATE TABLE [CartItems] (
        [CartItemId] int NOT NULL IDENTITY,
        [ProductId] int NOT NULL,
        [CustomerId] int NOT NULL,
        [Quantity] int NOT NULL,
        CONSTRAINT [PK_CartItems] PRIMARY KEY ([CartItemId]),
        CONSTRAINT [FK_CartItems_Customers_CustomerId] FOREIGN KEY ([CustomerId]) REFERENCES [Customers] ([CustomerId]) ON DELETE CASCADE,
        CONSTRAINT [FK_CartItems_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([ProductId]) ON DELETE CASCADE
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_CartItems_CustomerId')
    CREATE INDEX [IX_CartItems_CustomerId] ON [CartItems] ([CustomerId]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_CartItems_ProductId')
    CREATE INDEX [IX_CartItems_ProductId] ON [CartItems] ([ProductId]);
GO

IF NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20260420131411_CartItem')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260420131411_CartItem', N'8.0.1');
END
GO

COMMIT;
GO

