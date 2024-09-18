using Dwt.Api.Helpers;
using Dwt.Shared.Cache;
using Dwt.Shared.Models;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Options;

namespace Dwt.Api.Bootstrap;

[Bootstrapper]
public class CacheBootstrapper
{
	public static void ConfigureBuilder(WebApplicationBuilder appBuilder)
	{
		var logger = LoggerFactory.Create(b => b.AddConsole()).CreateLogger<CacheBootstrapper>();
		logger.LogInformation("Configuring cache services...");

		var confKeyBase = "Caches:Application";
		var keyedServiceName = "CACHE_APPLICATION";
		var cacheConf = SetupCache(appBuilder, confKeyBase, keyedServiceName, logger);
		if (cacheConf != null)
		{
			appBuilder.Services.AddSingleton<ICacheFacade<IApplicationRepository>>(sp =>
			{
				var options = new CacheFacadeOptions()
				{
					CompressionLevel = cacheConf.CompressionLevel,
					KeyPrefix = cacheConf.KeyPrefix,
					DefaultDistributedCacheEntryOptions = new DistributedCacheEntryOptions()
					{
						AbsoluteExpirationRelativeToNow = cacheConf.ExpirationAfterWrite > 0 ? TimeSpan.FromSeconds(cacheConf.ExpirationAfterWrite) : null,
						SlidingExpiration = cacheConf.ExpirationAfterAccess > 0 ? TimeSpan.FromSeconds(cacheConf.ExpirationAfterAccess) : null,
					},
				};
				var cacheService = sp.GetRequiredKeyedService<IDistributedCache>(keyedServiceName);
				return new CacheFacade<IApplicationRepository>(cacheService, options);
			});
		}

		logger.LogInformation("Cache services configured.");
	}

	private static CacheConf? SetupCache(WebApplicationBuilder appBuilder, string confKeyBase, string keyedServiceName, ILogger logger)
	{
		var cacheConf = appBuilder.Configuration.GetSection(confKeyBase).Get<CacheConf>()
			?? throw new InvalidDataException($"No configuration found at key {confKeyBase} in the configurations.");
		switch (cacheConf.Type)
		{
			case CacheType.INMEMORY or CacheType.MEMORY:
				var cacheSizeLimit = cacheConf.SizeLimit > 0 ? cacheConf.SizeLimit : CacheConf.DEFAULT_SIZE_LIMIT;
				logger.LogInformation("Using in-memory cache for {domain}, with SizeLimit = {SizeLimit}...", keyedServiceName, cacheSizeLimit);
				var memoryCache = new MemoryDistributedCache(Options.Create(new MemoryDistributedCacheOptions()
				{
					SizeLimit = cacheSizeLimit
				}));
				appBuilder.Services.AddKeyedSingleton<IDistributedCache>(keyedServiceName, memoryCache);
				return cacheConf;
			case CacheType.REDIS:
				if (string.IsNullOrWhiteSpace(cacheConf.ConnectionString))
				{
					throw new InvalidDataException($"No connection string name found at key {confKeyBase}:ConnectionString in the configurations.");
				}
				var connStr = appBuilder.Configuration.GetConnectionString(cacheConf.ConnectionString) ?? "";
				if (string.IsNullOrWhiteSpace(connStr))
				{
					throw new InvalidDataException($"No connection string {cacheConf.ConnectionString} defined in the ConnectionStrings section in the configurations.");
				}
				logger.LogInformation("Using Redis cache for {domain}...", keyedServiceName);
				var redisCache = new RedisCache(Options.Create(new RedisCacheOptions()
				{
					Configuration = connStr
				}));
				appBuilder.Services.AddKeyedSingleton<IDistributedCache>(keyedServiceName, redisCache);
				return cacheConf;
			default:
				logger.LogInformation("No cache configured for {domain}, or invalid cache type '{cacheType}'.", keyedServiceName, cacheConf.Type);
				return null;
		}
	}
}
