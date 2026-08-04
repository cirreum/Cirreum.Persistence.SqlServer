namespace Cirreum.Persistence.Internal;

using Cirreum.Providers.Configuration;

/// <summary>
/// Options for SQL Server connection.
/// </summary>
internal sealed class SqlServerOptions {

	/// <summary>
	/// The connection string.
	/// </summary>
	public string? ConnectionString { get; set; }

	/// <summary>
	/// Whether to use Entra (Azure AD) token authentication.
	/// </summary>
	public bool UseAzureAuthentication { get; set; }

	/// <summary>
	/// How the Entra credential is selected. Null uses the default chain.
	/// Only applies when <see cref="UseAzureAuthentication"/> is <see langword="true"/>.
	/// </summary>
	public CredentialSettings? Credential { get; set; }

	/// <summary>
	/// The Entra tenant to authenticate against, mapped from the instance's
	/// <c>Identifier</c>. Null uses the credential's default tenant.
	/// </summary>
	public string? TenantId { get; set; }

	/// <summary>
	/// Command timeout in seconds. Default is 30.
	/// </summary>
	public int CommandTimeoutSeconds { get; set; } = 30;

}
