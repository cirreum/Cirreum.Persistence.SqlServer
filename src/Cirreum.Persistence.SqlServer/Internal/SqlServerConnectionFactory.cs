namespace Cirreum.Persistence.Internal;

using Azure.Core;
using Azure.Identity;
using Cirreum.Providers.Configuration;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Threading;

/// <summary>
/// SQL Server connection factory with Entra (Azure AD) token authentication support.
/// </summary>
internal sealed class SqlServerConnectionFactory : ISqlConnectionFactory {

	private static readonly TokenRequestContext SqlTokenRequest =
		new(["https://database.windows.net/.default"]);

	static SqlServerConnectionFactory() {
		SqlMapper.AddTypeHandler(new DateOnlyTypeHandler());
		SqlMapper.AddTypeHandler(new TimeOnlyTypeHandler());
	}

	private readonly string _connectionString;
	private readonly int _commandTimeoutSeconds;
	private readonly TokenCredential? _credential;

	public SqlServerConnectionFactory(SqlServerOptions options) {
		_connectionString = options.ConnectionString
			?? throw new InvalidOperationException("ConnectionString is required.");
		_commandTimeoutSeconds = options.CommandTimeoutSeconds;

		if (options.UseAzureAuthentication) {
			// Ensure Integrated Security is not set when using Entra token auth
			var builder = new SqlConnectionStringBuilder(_connectionString) {
				IntegratedSecurity = false
			};
			_connectionString = builder.ConnectionString;

			// One credential per factory: Azure.Identity credentials cache tokens
			// internally, so per-connection acquisition below is a cache read until
			// the token nears expiry.
			_credential = CreateCredential(options);
		}
	}

	/// <inheritdoc />
	public int CommandTimeoutSeconds => _commandTimeoutSeconds;

	/// <inheritdoc />
	public async Task<ISqlConnection> CreateConnectionAsync(CancellationToken cancellationToken = default) {
		var conn = await this.CreateSqlConnectionAsync(cancellationToken);
		return new SqlServerConnection(conn, _commandTimeoutSeconds);
	}

	internal async Task<SqlConnection> CreateSqlConnectionAsync(CancellationToken cancellationToken = default) {
		SqlConnection connection;
		if (_credential is not null) {
			var token = await _credential.GetTokenAsync(SqlTokenRequest, cancellationToken);
			connection = new SqlConnection(_connectionString) {
				AccessToken = token.Token
			};
		} else {
			connection = new SqlConnection(_connectionString);
		}
		await connection.OpenAsync(cancellationToken);
		return connection;
	}

	private static TokenCredential CreateCredential(SqlServerOptions options) {

		var tenantId = string.IsNullOrWhiteSpace(options.TenantId) ? null : options.TenantId;
		var credential = options.Credential ?? new CredentialSettings();
		var identityId = string.IsNullOrWhiteSpace(credential.IdentityId) ? null : credential.IdentityId;

		return credential.Mode switch {

			CredentialMode.Default => new DefaultAzureCredential(new DefaultAzureCredentialOptions {
				TenantId = tenantId,
				ManagedIdentityClientId = identityId,
			}),

			CredentialMode.ManagedIdentity => new ManagedIdentityCredential(
				identityId is null
					? ManagedIdentityId.SystemAssigned
					: ManagedIdentityId.FromUserAssignedClientId(identityId)),

			CredentialMode.Developer => new ChainedTokenCredential(
				new VisualStudioCredential(new VisualStudioCredentialOptions { TenantId = tenantId }),
				new AzureCliCredential(new AzureCliCredentialOptions { TenantId = tenantId }),
				new AzurePowerShellCredential(new AzurePowerShellCredentialOptions { TenantId = tenantId })),

			_ => throw new InvalidOperationException(
				$"CredentialMode '{credential.Mode}' is not supported by the SQL Server persistence provider."),

		};

	}

}
