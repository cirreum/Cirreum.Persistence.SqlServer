# Cirreum.Persistence.SqlServer

[![NuGet Version](https://img.shields.io/nuget/v/Cirreum.Persistence.SqlServer.svg?style=flat-square&labelColor=1F1F1F&color=003D8F)](https://www.nuget.org/packages/Cirreum.Persistence.SqlServer/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Cirreum.Persistence.SqlServer.svg?style=flat-square&labelColor=1F1F1F&color=003D8F)](https://www.nuget.org/packages/Cirreum.Persistence.SqlServer/)
[![GitHub Release](https://img.shields.io/github/v/release/cirreum/Cirreum.Persistence.SqlServer?style=flat-square&labelColor=1F1F1F&color=FF3B2E)](https://github.com/cirreum/Cirreum.Persistence.SqlServer/releases)
[![License](https://img.shields.io/github/license/cirreum/Cirreum.Persistence.SqlServer?style=flat-square&labelColor=1F1F1F&color=F2F2F2)](https://github.com/cirreum/Cirreum.Persistence.SqlServer/blob/main/LICENSE)
[![.NET](https://img.shields.io/badge/.NET-10.0-003D8F?style=flat-square&labelColor=1F1F1F)](https://dotnet.microsoft.com/)

**SQL Server provider for [Cirreum.Persistence.Sql](https://github.com/cirreum/Cirreum.Persistence.Sql) with Azure Entra ID authentication support**

## Overview

**Cirreum.Persistence.SqlServer** is a SQL Server-specific implementation of the `ISqlConnectionFactory` interface from the [Cirreum.Persistence.Sql](https://github.com/cirreum/Cirreum.Persistence.Sql) abstraction layer. It provides:

- SQL Server connection factory with Dapper integration
- Azure Entra ID (Azure AD) authentication via `DefaultAzureCredential`
- Health check support for service monitoring
- Integration with the Cirreum Service Provider framework

For query extensions, pagination, transaction chaining, and constraint handling, see the [Cirreum.Persistence.Sql documentation](https://github.com/cirreum/Cirreum.Persistence.Sql).

## Installation

```bash
dotnet add package Cirreum.Persistence.SqlServer
```

## Quick Start

### Basic Registration

```csharp
builder.AddSqlServer("default", "Server=localhost;Database=MyDb;Trusted_Connection=True;");
```

### With Azure Entra ID Authentication

```csharp
builder.AddSqlServer("default", settings =>
{
    settings.ConnectionString = "Server=myserver.database.windows.net;Database=MyDb;";
    settings.UseAzureAuthentication = true;
});
```

### With Full Configuration

```csharp
builder.AddSqlServer("default", settings =>
{
    settings.ConnectionString = configuration.GetConnectionString("SqlServer")!;
    settings.UseAzureAuthentication = true;
    settings.CommandTimeoutSeconds = 60;
}, healthOptions =>
{
    healthOptions.Query = "SELECT 1";
    healthOptions.Timeout = TimeSpan.FromSeconds(5);
});
```

## Configuration Options

### SqlServerInstanceSettings

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `ConnectionString` | `string` | - | SQL Server connection string |
| `UseAzureAuthentication` | `bool` | `false` | Enable Azure Entra ID authentication |
| `CommandTimeoutSeconds` | `int` | `30` | Default command timeout in seconds |

### SqlServerHealthCheckOptions

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Query` | `string` | `"SELECT 1"` | SQL query to execute for health checks |
| `Timeout` | `TimeSpan` | `5s` | Health check timeout |

## Usage

Inject `ISqlConnectionFactory` and use the query/command extensions from Cirreum.Persistence.Sql:

```csharp
public class OrderRepository(ISqlConnectionFactory db)
{
    public async Task<Result<OrderDto>> GetOrderAsync(Guid orderId, CancellationToken ct)
    {
        await using var conn = await db.CreateConnectionAsync(ct);

        return await conn.GetAsync<OrderDto>(
            "SELECT * FROM Orders WHERE OrderId = @Id",
            new { Id = orderId },
            key: orderId,
            ct);
    }

    public async Task<Result<Guid>> CreateOrderAsync(CreateOrder cmd, CancellationToken ct)
    {
        await using var conn = await db.CreateConnectionAsync(ct);
        var orderId = Guid.CreateVersion7();

        return await conn.InsertAndReturnAsync(
            """
            INSERT INTO Orders (OrderId, CustomerId, Amount, CreatedAt)
            VALUES (@OrderId, @CustomerId, @Amount, @CreatedAt)
            """,
            new { OrderId = orderId, cmd.CustomerId, cmd.Amount, CreatedAt = DateTime.UtcNow },
            () => orderId,
            uniqueConstraintMessage: "Order already exists",
            ct: ct);
    }
}
```

### Multiple Instances

Register multiple SQL Server instances with different keys:

```csharp
builder.AddSqlServer("primary", primaryConnectionString);
builder.AddSqlServer("reporting", reportingConnectionString);

// Inject with [FromKeyedServices]
public class ReportService([FromKeyedServices("reporting")] ISqlConnectionFactory db)
{
    // ...
}
```

## DateOnly and TimeOnly Support

This package includes built-in Dapper type handlers for `DateOnly` and `TimeOnly`, allowing you to use these types directly in your models and queries:

```csharp
public record Appointment(int Id, DateOnly Date, TimeOnly StartTime, TimeOnly EndTime);

// Query with DateOnly/TimeOnly parameters
var appointments = await conn.QueryAsync<Appointment>(
    "SELECT * FROM Appointments WHERE Date = @Date AND StartTime > @MinTime",
    new { Date = new DateOnly(2026, 1, 15), MinTime = new TimeOnly(9, 0) });

// Insert with DateOnly/TimeOnly values
await conn.ExecuteAsync(
    "INSERT INTO Appointments (Date, StartTime, EndTime) VALUES (@Date, @StartTime, @EndTime)",
    new { Date = DateOnly.FromDateTime(DateTime.Today), StartTime = new TimeOnly(9, 30), EndTime = new TimeOnly(10, 0) });
```

The type handlers are registered automatically when the package is loaded.

## Azure Entra ID Authentication

When `UseAzureAuthentication` is enabled, the connection factory uses `DefaultAzureCredential` to acquire tokens for `https://database.windows.net/.default`. This supports:

- Managed Identity (Azure VMs, App Service, AKS)
- Azure CLI credentials (local development)
- Visual Studio / VS Code credentials
- Environment variables

Ensure your Azure SQL Database is configured for Azure AD authentication and the appropriate identity has been granted access.

## Related Packages

| Package | Description |
|---------|-------------|
| [Cirreum.Persistence.Sql](https://github.com/cirreum/Cirreum.Persistence.Sql) | Database-agnostic SQL abstraction layer |
| Cirreum.Persistence.Sqlite | SQLite provider (coming soon) |
| Cirreum.Persistence.MySql | MySQL provider (coming soon) |
| Cirreum.Persistence.PostgreSql | PostgreSQL provider (coming soon) |

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

**Cirreum Foundation Framework**
*Layered simplicity for modern .NET*
