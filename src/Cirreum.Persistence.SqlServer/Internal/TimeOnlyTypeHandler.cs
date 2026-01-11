namespace Cirreum.Persistence.Internal;

using Dapper;
using System.Data;

/// <summary>
/// Dapper type handler for <see cref="TimeOnly"/> values.
/// </summary>
internal sealed class TimeOnlyTypeHandler : SqlMapper.TypeHandler<TimeOnly> {

	public override void SetValue(IDbDataParameter parameter, TimeOnly value) {
		parameter.DbType = DbType.Time;
		parameter.Value = value.ToTimeSpan();
	}

	public override TimeOnly Parse(object value) {
		return value switch {
			TimeSpan timeSpan => TimeOnly.FromTimeSpan(timeSpan),
			DateTime dateTime => TimeOnly.FromDateTime(dateTime),
			TimeOnly timeOnly => timeOnly,
			_ => throw new InvalidCastException($"Cannot convert {value.GetType()} to TimeOnly.")
		};
	}

}
