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

		// A credential block only applies to Entra token auth. Rejecting the
		// contradiction at registration stops the block from binding and silently
		// doing nothing on a connection-string-authenticated instance.
		if (settings.Credential is not null && !settings.UseAzureAuthentication) {
			throw new InvalidOperationException(
				$"SQL Server instance '{serviceKey}' configures a Credential block but " +
				"UseAzureAuthentication is false. The credential block selects the Entra " +
				"identity used for token authentication — set UseAzureAuthentication to " +
				"true, or remove the Credential block to authenticate via the connection " +
				"string alone.");
		}

		var factory = new SqlServerConnectionFactory(settings.ToConnectionOptions());

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

		return new SqlServerHealthCheck(
			settings.ToConnectionOptions(),
			settings.HealthOptions ?? new SqlServerHealthCheckOptions());

	}

	/// <summary>
	/// Maps instance settings onto the internal connection options, including the
	/// credential selection (<c>Credential</c> block, <c>Identifier</c> ⇒ Entra tenant).
	/// </summary>
	/// <param name="settings">The instance settings.</param>
	/// <returns>The mapped <see cref="SqlServerOptions"/>.</returns>
	private static SqlServerOptions ToConnectionOptions(this SqlServerInstanceSettings settings) {
		return new SqlServerOptions {
			ConnectionString = settings.ConnectionString,
			UseAzureAuthentication = settings.UseAzureAuthentication,
			Credential = settings.Credential,
			TenantId = settings.Identifier,
			CommandTimeoutSeconds = settings.CommandTimeoutSeconds
		};
	}

}