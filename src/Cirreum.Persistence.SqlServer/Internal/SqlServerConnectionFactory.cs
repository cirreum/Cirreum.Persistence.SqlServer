namespace Cirreum.Persistence.Internal;

using Azure.Core;
using Azure.Identity;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Threading;

/// <summary>
/// SQL Server connection factory with Azure authentication support.
/// </summary>
internal sealed class SqlServerConnectionFactory : ISqlConnectionFactory {

	static SqlServerConnectionFactory() {
		SqlMapper.AddTypeHandler(new DateOnlyTypeHandler());
		SqlMapper.AddTypeHandler(new TimeOnlyTypeHandler());
	}

	private readonly string _connectionString;
	private readonly bool _useAzureAdAuth;
	private readonly int _commandTimeoutSeconds;

	public SqlServerConnectionFactory(SqlServerOptions options) {
		_connectionString = options.ConnectionString
			?? throw new InvalidOperationException("ConnectionString is required.");
		_useAzureAdAuth = options.UseAzureAuthentication;
		_commandTimeoutSeconds = options.CommandTimeoutSeconds;

		if (_useAzureAdAuth) {
			// Ensure Integrated Security is not set when using Azure AD
			var builder = new SqlConnectionStringBuilder(_connectionString) {
				IntegratedSecurity = false
			};
			_connectionString = builder.ConnectionString;
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
		if (_useAzureAdAuth) {
			var credential = new DefaultAzureCredential();
			var token = await credential.GetTokenAsync(
				new TokenRequestContext(["https://database.windows.net/.default"]),
				cancellationToken);
			connection = new SqlConnection(_connectionString) {
				AccessToken = token.Token
			};
		} else {
			connection = new SqlConnection(_connectionString);
		}
		await connection.OpenAsync(cancellationToken);
		return connection;
	}

}