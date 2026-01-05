namespace Cirreum.Persistence.Health;

using Cirreum.Persistence.Internal;
using Cirreum.ServiceProvider.Health;
using Microsoft.Extensions.Diagnostics.HealthChecks;

/// <summary>
/// Health check for SQL Server connections.
/// </summary>
internal sealed class SqlServerHealthCheck(
	SqlServerOptions options,
	SqlServerHealthCheckOptions healthOptions
) : IServiceProviderHealthCheck<SqlServerHealthCheckOptions> {

	/// <inheritdoc/>
	public SqlServerHealthCheckOptions HealthOptions => healthOptions;

	/// <inheritdoc/>
	public async Task<HealthCheckResult> CheckHealthAsync(
		HealthCheckContext context,
		CancellationToken cancellationToken = default) {

		try {
			var factory = new SqlServerConnectionFactory(options);
			await using var connection = await factory.CreateSqlConnectionAsync(cancellationToken);

			await using var command = connection.CreateCommand();
			command.CommandText = healthOptions.Query;
			command.CommandTimeout = (int)(healthOptions.Timeout?.TotalSeconds ?? 5);

			await command.ExecuteScalarAsync(cancellationToken);

			return HealthCheckResult.Healthy("SQL Server connection is healthy.");

		} catch (Exception ex) {
			return HealthCheckResult.Unhealthy(
				"SQL Server connection failed.",
				exception: ex);
		}

	}

}