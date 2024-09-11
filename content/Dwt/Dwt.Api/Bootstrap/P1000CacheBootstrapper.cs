using Dwt.Api.Helpers;
using Dwt.Shared.Cache;
using Dwt.Shared.Models;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace Dwt.Api.Bootstrap;

[Bootstrapper]
public class CacheBootstrapper
{
	public static void ConfigureBuilder(WebApplicationBuilder appBuilder)
	{
		var logger = LoggerFactory.Create(b => b.AddConsole()).CreateLogger<CacheBootstrapper>();
		logger.LogInformation("Configuring cache services...");

		if (SetupCache(appBuilder, "Caches:Application", "CACHE_APPLICATION", logger))
		{
			appBuilder.Services.AddSingleton<ICacheFacade<IApplicationRepository>>(sp =>
			{
				var expiration = appBuilder.Configuration.GetValue<int>("Caches:Application:Expiration");
				var defaultOptions = new DistributedCacheEntryOptions()
				{
					SlidingExpiration = expiration > 0 ? TimeSpan.FromSeconds(expiration) : null
				};
				var cacheService = sp.GetRequiredKeyedService<IDistributedCache>("CACHE_APPLICATION");
				return new CacheFacade<IApplicationRepository>(defaultOptions, cacheService);
			});
		}

		logger.LogInformation("Cache services configured.");
	}

	private static bool SetupCache(WebApplicationBuilder appBuilder, string confKey, string keyedServiceName, ILogger logger)
	{
		Enum.TryParse<CacheType>(appBuilder.Configuration[$"{confKey}:Type"], true, out var cacheType);
		var cacheEnabled = false;
		switch (cacheType)
		{
			case CacheType.INMEMORY or CacheType.MEMORY:
				var cacheSizeLimit = appBuilder.Configuration.GetValue<int>($"{confKey}:SizeLimit");
				cacheSizeLimit = cacheSizeLimit > 0 ? cacheSizeLimit : 100 * 1024 * 1024; // ~100mb
				logger.LogInformation("Using in-memory cache for {domain}, with SizeLimit = {SizeLimit}...", keyedServiceName, cacheSizeLimit);
				var cache = new MemoryDistributedCache(Options.Create(new MemoryDistributedCacheOptions()
				{
					SizeLimit = cacheSizeLimit
				}));
				appBuilder.Services.AddKeyedSingleton<IDistributedCache>(keyedServiceName, cache);
				cacheEnabled = true;
				break;
		}
		return cacheEnabled;
	}
}
