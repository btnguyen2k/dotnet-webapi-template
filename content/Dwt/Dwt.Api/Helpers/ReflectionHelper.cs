using Ddth.Utilities;
using System.Reflection;

namespace Dwt.Api.Helpers;

/// <summary>
/// Helper to create instances of classes with constructor-based DI.
/// </summary>
public static class ReflectionHelper
{
	//private static IServiceProvider? serviceProvider;

	private static async Task InvokeAsyncMethod(IServiceProvider? serviceProvider, IEnumerable<object?>? services, Type typeInfo, MethodInfo methodInfo)
	{
		var paramsInfo = methodInfo.GetParameters();
		var parameters = ReflectionDIHelper.BuildDIParams(serviceProvider, services, paramsInfo);
		object? instance = null;
		if (!methodInfo.IsStatic)
		{
			instance = ReflectionDIHelper.CreateInstance<object>(serviceProvider, services, typeInfo);
		}
		await (dynamic)methodInfo.Invoke(instance, parameters)!;
	}

	private static void InvokeMethod(IServiceProvider? serviceProvider, IEnumerable<object?>? services, Type typeInfo, MethodInfo methodInfo)
	{
		var paramsInfo = methodInfo.GetParameters();
		var parameters = ReflectionDIHelper.BuildDIParams(serviceProvider, services, paramsInfo);
		object? instance = null;
		if (!methodInfo.IsStatic)
		{
			instance = ReflectionDIHelper.CreateInstance<object>(serviceProvider, services, typeInfo);
		}
		methodInfo.Invoke(instance, parameters);
	}

	public static async Task InvokeAsyncMethod(WebApplicationBuilder appBuilder, Type typeInfo, MethodInfo methodInfo)
	{
		// TODO: ASP0000 - calling IServiceCollection.BuildServiceProvider results in more than one copy of singleton
		// services being created which might result in incorrect application behavior.
		// Proposed workaround/fix: special treat for IOptions<T>, ILoggerFactory and ILogger<T>?
		var serviceProvider = appBuilder.Services.BuildServiceProvider();
		await InvokeAsyncMethod(serviceProvider, [appBuilder], typeInfo, methodInfo);
	}

	public static void InvokeMethod(WebApplicationBuilder appBuilder, Type typeInfo, MethodInfo methodInfo)
	{
		// TODO: ASP0000 - calling IServiceCollection.BuildServiceProvider results in more than one copy of singleton
		// services being created which might result in incorrect application behavior.
		// Proposed workaround/fix: special treat for IOptions<T>, ILoggerFactory and ILogger<T>?
		var serviceProvider = appBuilder.Services.BuildServiceProvider();
		InvokeMethod(serviceProvider, [appBuilder], typeInfo, methodInfo);
	}

	public static async Task InvokeAsyncMethod(WebApplication app, Type typeInfo, MethodInfo methodInfo)
	{
		await InvokeAsyncMethod(app.Services, [app], typeInfo, methodInfo);
	}

	public static void InvokeMethod(WebApplication app, Type typeInfo, MethodInfo methodInfo)
	{
		InvokeMethod(app.Services, [app], typeInfo, methodInfo);
	}
}
