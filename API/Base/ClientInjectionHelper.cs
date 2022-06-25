namespace BlazorWasmTest.API.Base;

using System.Reflection;

public static class ClientInjectionHelper
{
    public static void AddApiClients(this IServiceCollection serviceCollection, string baseUrl)
    {
        serviceCollection.AddTransient<TestApiHttpMessageHandler>();
        
        var thisAssembly = Assembly.GetExecutingAssembly();

        var apiClients = thisAssembly.GetTypes().Where(t => !t.IsInterface && typeof(IApiBase).IsAssignableFrom(t));

        foreach (var client in apiClients)
        {
            var clientInterface = client.GetInterfaces().FirstOrDefault(i => i.Name != nameof(IApiBase));
            serviceCollection.AddTypedHttpService(clientInterface, client, GetHttpClientGenericMethod(), baseUrl);
        }
    }

    private static void AddTypedHttpService(this IServiceCollection serviceCollection, Type? clientInterface, Type clientImplementation, MethodInfo? info,
        string baseUrl)
    {
        if (clientInterface == null) return;
        var addHttpClientMethod = info?.MakeGenericMethod(clientInterface, clientImplementation);
        var t = addHttpClientMethod?.Invoke(null, new object[] {serviceCollection, GetHttpClientConfig(baseUrl)});
        (t as IHttpClientBuilder ?? throw new InvalidOperationException()).AddHttpMessageHandler<TestApiHttpMessageHandler>();
    }

    private static Action<HttpClient> GetHttpClientConfig(string baseUrl)
    {
        return h =>
        {
            h.BaseAddress = new Uri(baseUrl);
            h.DefaultRequestHeaders.Add("User-Agent", "BlazorWasmTest");
        };
    }

    private static MethodInfo? GetHttpClientGenericMethod()
    {
        return typeof(HttpClientFactoryServiceCollectionExtensions).GetMethods().FirstOrDefault(m => m.Name == "AddHttpClient" && m.IsGenericMethod
            && m.GetParameters().Length == 2 && m.GetParameters()[0].ParameterType == typeof(IServiceCollection) &&
            m.GetParameters()[1].ParameterType == typeof(Action<HttpClient>)
            && m.GetGenericArguments().Length == 2);
    }
}