using System.Text.Json;
using System.Text;
using Microsoft.AspNetCore.Http;

namespace veritheia.Web.Services;

/// <summary>
/// Base HTTP client for API service communication
/// </summary>
public class ApiClient
{
    private readonly HttpClient _httpClient;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly JsonSerializerOptions _jsonOptions;

    public ApiClient(HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
    {
        _httpClient = httpClient;
        _httpContextAccessor = httpContextAccessor;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };
    }

    private void AddUserContext(HttpRequestMessage request)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext?.User?.Identity?.IsAuthenticated == true)
        {
            var userIdClaim = httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userIdClaim))
            {
                request.Headers.Add("X-User-Id", userIdClaim);
            }
            
            var userEmailClaim = httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
            if (!string.IsNullOrEmpty(userEmailClaim))
            {
                request.Headers.Add("X-User-Email", userEmailClaim);
            }
        }
    }

    public async Task<T?> GetAsync<T>(string endpoint)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
        AddUserContext(request);
        
        var response = await _httpClient.SendAsync(request);
        
        if (!response.IsSuccessStatusCode)
        {
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return default(T);
            
            response.EnsureSuccessStatusCode();
        }

        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<T>(json, _jsonOptions);
    }

    public async Task<List<T>> GetListAsync<T>(string endpoint)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
        AddUserContext(request);
        
        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<List<T>>(json, _jsonOptions) ?? new List<T>();
    }

    public async Task<T> PostAsync<T>(string endpoint, object data)
    {
        var json = JsonSerializer.Serialize(data, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
        {
            Content = content
        };
        AddUserContext(request);

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var responseJson = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<T>(responseJson, _jsonOptions)!;
    }

    public async Task<T> PutAsync<T>(string endpoint, object data)
    {
        var json = JsonSerializer.Serialize(data, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        var request = new HttpRequestMessage(HttpMethod.Put, endpoint)
        {
            Content = content
        };
        AddUserContext(request);

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var responseJson = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<T>(responseJson, _jsonOptions)!;
    }

    public async Task DeleteAsync(string endpoint)
    {
        var request = new HttpRequestMessage(HttpMethod.Delete, endpoint);
        AddUserContext(request);
        
        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
    }

    public async Task PutAsync(string endpoint)
    {
        var request = new HttpRequestMessage(HttpMethod.Put, endpoint);
        AddUserContext(request);
        
        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
    }
}
