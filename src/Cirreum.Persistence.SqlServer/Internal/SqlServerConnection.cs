namespace Cirreum.Persistence.Internal;

using Dapper;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

internal class SqlServerConnection(
	SqlConnection connection,
	int commandTimeoutSeconds = 30
) : ISqlConnection {

	public async Task<T?> QuerySingleOrDefaultAsync<T>(
	string sql,
	object? parameters,
	IDbTransaction? transaction,
	CancellationToken cancellationToken) {

		var command = new CommandDefinition(
			sql,
			parameters,
			transaction: transaction,
			commandTimeout: commandTimeoutSeconds,
			cancellationToken: cancellationToken);

		return await connection.QuerySingleOrDefaultAsync<T>(command);
	}

	public async Task<T?> QueryFirstOrDefaultAsync<T>(
		string sql,
		object? parameters,
		IDbTransaction? transaction,
		CancellationToken cancellationToken) {

		var command = new CommandDefinition(
			sql,
			parameters,
			transaction: transaction,
			commandTimeout: commandTimeoutSeconds,
			cancellationToken: cancellationToken);

		return await connection.QueryFirstOrDefaultAsync<T>(command);
	}

	public async Task<IEnumerable<T>> QueryAsync<T>(
		string sql,
		object? parameters,
		IDbTransaction? transaction,
		CancellationToken cancellationToken) {

		var command = new CommandDefinition(
			sql,
			parameters,
			transaction: transaction,
			commandTimeout: commandTimeoutSeconds,
			cancellationToken: cancellationToken);

		return await connection.QueryAsync<T>(command);
	}

	public async Task<T?> ExecuteScalarAsync<T>(
		string sql,
		object? parameters,
		IDbTransaction? transaction,
		CancellationToken cancellationToken) {

		var command = new CommandDefinition(
			sql,
			parameters,
			transaction: transaction,
			commandTimeout: commandTimeoutSeconds,
			cancellationToken: cancellationToken);

		return await connection.ExecuteScalarAsync<T>(command);
	}

	public async Task<int> ExecuteAsync(
		string sql,
		object? parameters,
		IDbTransaction? transaction,
		CancellationToken cancellationToken) {

		var command = new CommandDefinition(
			sql,
			parameters,
			transaction: transaction,
			commandTimeout: commandTimeoutSeconds,
			cancellationToken: cancellationToken);

		return await connection.ExecuteAsync(command);
	}

	public async Task<IMultipleResult> QueryMultipleAsync(
		string sql,
		object? parameters,
		IDbTransaction? transaction,
		CancellationToken cancellationToken) {

		var command = new CommandDefinition(
			sql,
			parameters,
			transaction: transaction,
			commandTimeout: commandTimeoutSeconds,
			cancellationToken: cancellationToken);

		var gridReader = await connection.QueryMultipleAsync(command);
		return new SqlServerMultipleResult(gridReader);
	}

	public IDbTransaction BeginTransaction() => connection.BeginTransaction();

	public ValueTask DisposeAsync() => connection.DisposeAsync();

}