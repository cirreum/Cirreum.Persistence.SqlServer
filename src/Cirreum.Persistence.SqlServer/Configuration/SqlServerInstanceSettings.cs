namespace Cirreum.Persistence.Configuration;

using Cirreum.Persistence.Health;
using Cirreum.ServiceProvider.Configuration;

/// <summary>
/// Instance-specific settings for a Dapper SQL Server database connection.
/// </summary>
/// <remarks>
/// <para>
/// Each instance represents a single database connection configuration, allowing for multiple
/// database connections within the same application. Settings include authentication mode
/// and command timeout.
/// </para>
/// <para>
/// For Entra (Azure AD) token authentication, set <see cref="UseAzureAuthentication"/> to
/// <c>true</c>. The inherited <c>Credential</c> block selects whose identity acquires tokens
/// (default chain, a managed identity, or the developer's tooling identity), and the inherited
/// <c>Identifier</c> names the Entra tenant to authenticate against. Both are optional: with
/// neither set, the default credential chain against its default tenant applies. Configuring
/// a <c>Credential</c> block while <see cref="UseAzureAuthentication"/> is <c>false</c> is
/// rejected at registration as a contradiction.
/// </para>
/// <para>
/// The connection factory is registered as a singleton. Individual connections are short-lived
/// and managed by ADO.NET connection pooling.
/// </para>
/// </remarks>
/// <seealso cref="SqlServerSettings"/>
public sealed class SqlServerInstanceSettings :
	ServiceProviderInstanceSettings<SqlServerHealthCheckOptions> {

	/// <summary>
	/// Whether to use Entra (Azure AD) token authentication. When enabled, the factory
	/// acquires access tokens using the identity selected by the inherited <c>Credential</c>
	/// block, against the tenant named by the inherited <c>Identifier</c>.
	/// </summary>
	public bool UseAzureAuthentication { get; set; }

	/// <summary>
	/// Command timeout in seconds. Default is 30.
	/// </summary>
	public int CommandTimeoutSeconds { get; set; } = 30;

	/// <inheritdoc/>
	public override SqlServerHealthCheckOptions? HealthOptions { get; set; }

}
