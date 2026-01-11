namespace Cirreum.Persistence.Internal;

using Dapper;
using System.Data;

/// <summary>
/// Dapper type handler for <see cref="DateOnly"/> values.
/// </summary>
internal sealed class DateOnlyTypeHandler : SqlMapper.TypeHandler<DateOnly> {

	public override void SetValue(IDbDataParameter parameter, DateOnly value) {
		parameter.DbType = DbType.Date;
		parameter.Value = value.ToDateTime(TimeOnly.MinValue);
	}

	public override DateOnly Parse(object value) {
		return value switch {
			DateTime dateTime => DateOnly.FromDateTime(dateTime),
			DateOnly dateOnly => dateOnly,
			_ => throw new InvalidCastException($"Cannot convert {value.GetType()} to DateOnly.")
		};
	}

}
