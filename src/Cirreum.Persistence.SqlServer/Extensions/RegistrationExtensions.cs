namespace Cirreum.Persistence.Extensions;

using Cirreum.Persistence.Configuration;
using Cirreum.Persistence.Health;
using Cirreum.Persistence.Internal;
using Cirreum.ServiceProvider.Configuration;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Internal extension methods for registering Dapper SQL services with the DI container.
/// </summary>
internal static class RegistrationExtensions {

	/// <summary>
	/// Registers <see cref="ISqlConnectionFactory"/> as a singleton with the service collection.
	/// </summary>
	/// <param name="services">The service collection.</param>
	/// <param name="serviceKey">The service key for keyed registration.</param>
	/// <param name="settings">The instance settings.</param>
	/// <remarks>
	/// <para>
	/// The factory is registered as a singleton because it holds only configuration state
	/// (connection string, auth settings). Individual connections are short-lived and
	/// managed by ADO.NET connection pooling.
	/// </para>
	/// <para>
	/// When <paramref name="serviceKey"/> equals <see cref="ServiceProviderSettings.DefaultKey"/>,
	/// the factory is registered as both a primary (non-keyed) and keyed service.
	/// Otherwise, only a keyed service is registered.
	/// </para>
	/// </remarks>
	public static void AddDbFactories(
		this IServiceCollection services,
		string serviceKey,
		SqlServerInstanceSettings settings) {

		// Create options from settings
		var options = new SqlServerOptions {
			ConnectionString = settings.ConnectionString,
			UseAzureAuthentication = settings.UseAzureAuthentication,
			CommandTimeoutSeconds = settings.CommandTimeoutSeconds
		};

		var factory = new SqlServerConnectionFactory(options);

		// Always register as keyed service for explicit access
		services.AddKeyedSingleton<ISqlConnectionFactory>(serviceKey, factory);

		// Determine if this should be the default (non-keyed) service
		var isDefault = serviceKey.Equals(ServiceProviderSettings.DefaultKey, StringComparison.OrdinalIgnoreCase);
		if (isDefault) {
			services.AddSingleton<ISqlConnectionFactory>(factory);
		}

	}

	/// <summary>
	/// Creates a health check instance for monitoring SQL Server connectivity.
	/// </summary>
	/// <param name="_">The service provider (unused, provided for extension method pattern).</param>
	/// <param name="settings">The instance settings containing connection configuration.</param>
	/// <returns>A new <see cref="SqlServerHealthCheck"/> instance.</returns>
	public static SqlServerHealthCheck CreateDapperSqlHealthCheck(
		this IServiceProvider _,
		SqlServerInstanceSettings settings) {

		var options = new SqlServerOptions {
			ConnectionString = settings.ConnectionString,
			UseAzureAuthentication = settings.UseAzureAuthentication,
			CommandTimeoutSeconds = settings.CommandTimeoutSeconds
		};

		return new SqlServerHealthCheck(options, settings.HealthOptions ?? new SqlServerHealthCheckOptions());

	}

}