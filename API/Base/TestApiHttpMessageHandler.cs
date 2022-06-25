namespace BlazorWasmTest.API.Base;

public class TestApiHttpMessageHandler : DelegatingHandler
{
    private readonly IHttpClientFactory _httpClientFactory;

    public TestApiHttpMessageHandler(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }
}