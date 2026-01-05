namespace Cirreum.Persistence.Internal;

using Dapper;

internal sealed class SqlServerMultipleResult(
	SqlMapper.GridReader gridReader
) : IMultipleResult {

	public bool IsConsumed => gridReader.IsConsumed;

	public async Task<T?> ReadSingleOrDefaultAsync<T>()
		=> await gridReader.ReadSingleOrDefaultAsync<T>();

	public async Task<T?> ReadFirstOrDefaultAsync<T>()
		=> await gridReader.ReadFirstOrDefaultAsync<T>();

	public async Task<IEnumerable<T>> ReadAsync<T>(bool buffered = true)
		=> await gridReader.ReadAsync<T>(buffered);

	public ValueTask DisposeAsync() {
		gridReader.Dispose();
		return ValueTask.CompletedTask;
	}

}