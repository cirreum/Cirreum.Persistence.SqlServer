namespace Cirreum.Persistence.Extensions.Hosting;

using Cirreum.Persistence.Configuration;
using Cirreum.Persistence.Health;
using Microsoft.Extensions.Hosting;

/// <summary>
/// Extension methods for registering SQL Server persistence with <see cref="IHostApplicationBuilder"/>.
/// </summary>
public static class HostingExtensions {

	/// <summary>
	/// Adds a SQL Server database connection factory to the application.
	/// </summary>
	/// <param name="builder">The host application builder.</param>
	/// <param name="serviceKey">
	/// The service key for keyed DI registration. Use <c>ServiceProviderSettings.DefaultKey</c>
	/// to register as the default/primary (non-keyed) service.
	/// </param>
	/// <param name="settings">The instance settings containing connection string and authentication options.</param>
	/// <param name="configureHealthCheckOptions">Optional action to configure health check options.</param>
	/// <returns>The builder instance for chaining.</returns>
	public static IHostApplicationBuilder AddSqlServer(
		this IHostApplicationBuilder builder,
		string serviceKey,
		SqlServerInstanceSettings settings,
		Action<SqlServerHealthCheckOptions>? configureHealthCheckOptions = null) {

		ArgumentNullException.ThrowIfNull(builder);

		// Configure health options
		settings.HealthOptions ??= new SqlServerHealthCheckOptions();
		configureHealthCheckOptions?.Invoke(settings.HealthOptions);

		// Reuse our Registrar...
		var registrar = new SqlServerRegistrar();
		registrar.RegisterInstance(
			serviceKey,
			settings,
			builder.Services,
			builder.Configuration);

		return builder;

	}

	/// <summary>
	/// Adds a SQL Server database connection factory to the application using an action-based configuration.
	/// </summary>
	/// <param name="builder">The host application builder.</param>
	/// <param name="serviceKey">
	/// The service key for keyed DI registration. Use <c>ServiceProviderSettings.DefaultKey</c>
	/// to register as the default/primary (non-keyed) service.
	/// </param>
	/// <param name="configure">An action to configure the instance settings.</param>
	/// <param name="configureHealthCheckOptions">Optional action to configure health check options.</param>
	/// <returns>The builder instance for chaining.</returns>
	public static IHostApplicationBuilder AddSqlServer(
		this IHostApplicationBuilder builder,
		string serviceKey,
		Action<SqlServerInstanceSettings> configure,
		Action<SqlServerHealthCheckOptions>? configureHealthCheckOptions = null) {

		ArgumentNullException.ThrowIfNull(builder);

		var settings = new SqlServerInstanceSettings();
		configure?.Invoke(settings);
		if (string.IsNullOrWhiteSpace(settings.Name)) {
			settings.Name = serviceKey;
		}

		return AddSqlServer(builder, serviceKey, settings, configureHealthCheckOptions);

	}

	/// <summary>
	/// Adds a SQL Server database connection factory to the application using a connection string.
	/// </summary>
	/// <param name="builder">The host application builder.</param>
	/// <param name="serviceKey">
	/// The service key for keyed DI registration. Use <c>ServiceProviderSettings.DefaultKey</c>
	/// to register as the defaul/primary (non-keyed) service.
	/// </param>
	/// <param name="connectionString">The SQL Server connection string.</param>
	/// <param name="configureHealthCheckOptions">Optional action to configure health check options.</param>
	/// <returns>The builder instance for chaining.</returns>
	public static IHostApplicationBuilder AddSqlServer(
		this IHostApplicationBuilder builder,
		string serviceKey,
		string connectionString,
		Action<SqlServerHealthCheckOptions>? configureHealthCheckOptions = null) {

		ArgumentNullException.ThrowIfNull(builder);

		var settings = new SqlServerInstanceSettings() {
			ConnectionString = connectionString,
			Name = serviceKey
		};

		return AddSqlServer(builder, serviceKey, settings, configureHealthCheckOptions);

	}

}