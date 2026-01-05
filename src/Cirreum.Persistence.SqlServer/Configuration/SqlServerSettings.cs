namespace Cirreum.Persistence.Configuration;

using Cirreum.Persistence.Health;
using Cirreum.ServiceProvider.Configuration;

/// <summary>
/// Configuration settings container for Dapper SQL Server persistence provider.
/// </summary>
/// <remarks>
/// This class serves as the root configuration type for the Dapper SQL provider,
/// containing a collection of <see cref="SqlServerInstanceSettings"/> for configuring
/// multiple database instances.
/// </remarks>
/// <seealso cref="SqlServerInstanceSettings"/>
/// <seealso cref="SqlServerHealthCheckOptions"/>
public sealed class SqlServerSettings :
	ServiceProviderSettings<
		SqlServerInstanceSettings,
		SqlServerHealthCheckOptions>;
