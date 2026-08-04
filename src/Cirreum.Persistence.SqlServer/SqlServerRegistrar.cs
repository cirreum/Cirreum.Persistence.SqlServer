namespace Cirreum.Persistence;

using Cirreum.Persistence.Configuration;
using Cirreum.Persistence.Extensions;
using Cirreum.Persistence.Health;
using Cirreum.Providers;
using Cirreum.ServiceProvider;
using Cirreum.ServiceProvider.Health;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Service provider registrar for SQL Server persistence.
/// </summary>
/// <remarks>
/// <para>
/// This registrar integrates SQL Server database connections into the Cirreum service provider framework,
/// enabling dependency injection of <see cref="ISqlConnectionFactory"/> instances with support for:
/// </para>
/// <list type="bullet">
///   <item><description>Entra (Azure AD) token authentication, with the identity selected by the instance's <c>Credential</c> block and the tenant by its <c>Identifier</c></description></item>
///   <item><description>Multiple named instances using keyed DI services</description></item>
///   <item><description>Configurable service lifetimes (Singleton, Scoped, Transient)</description></item>
///   <item><description>Health check integration with customizable queries</description></item>
/// </list>
/// </remarks>
/// <seealso cref="ISqlConnectionFactory"/>
/// <seealso cref="SqlServerSettings"/>
/// <seealso cref="SqlServerInstanceSettings"/>
public sealed class SqlServerRegistrar() :
	ServiceProviderRegistrar<
		SqlServerSettings,
		SqlServerInstanceSettings,
		SqlServerHealthCheckOptions> {

	/// <inheritdoc/>
	public override ProviderType ProviderType { get; } = ProviderType.Persistence;

	/// <summary>
	/// Gets the name of the data provider associated with this implementation.
	/// </summary>
	public override string ProviderName { get; } = "SqlServer";

	/// <inheritdoc/>
	public override string[] ActivitySourceNames { get; } = [
		"Microsoft.Data.SqlClient",
		"Cirreum.Persistence.SqlServer"
	];

	/// <inheritdoc/>
	protected override void AddServiceProviderInstance(
		IServiceCollection services,
		string serviceKey,
		SqlServerInstanceSettings settings) {
		services.AddDbFactories(serviceKey, settings);
	}

	/// <inheritdoc/>
	protected override IServiceProviderHealthCheck<SqlServerHealthCheckOptions> CreateHealthCheck(
		IServiceProvider serviceProvider,
		string serviceKey,
		SqlServerInstanceSettings settings) {
		return serviceProvider.CreateDapperSqlHealthCheck(settings);
	}

}