using System.Net;
using System.Net.Http.Json;
using ProductClient.Models;

namespace ProductClient.Services;

public class ProductApiService : IProductApiService
{
    private readonly IHttpClientFactory _httpClientFactory;

    public ProductApiService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<IEnumerable<Product>> GetAllAsync()
    {
        var client = _httpClientFactory.CreateClient("ProductApi");
        return await client.GetFromJsonAsync<List<Product>>("api/products") ?? [];
    }

    public async Task<Product?> GetByIdAsync(int id)
    {
        var client = _httpClientFactory.CreateClient("ProductApi");

        try
        {
            return await client.GetFromJsonAsync<Product>($"api/products/{id}");
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task<bool> CreateAsync(Product product)
    {
        var client = _httpClientFactory.CreateClient("ProductApi");
        using var response = await client.PostAsJsonAsync("api/products", product);
        response.EnsureSuccessStatusCode();
        return true;
    }

    public async Task<bool> UpdateAsync(Product product)
    {
        var client = _httpClientFactory.CreateClient("ProductApi");
        using var response = await client.PutAsJsonAsync($"api/products/{product.Id}", product);
        response.EnsureSuccessStatusCode();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var client = _httpClientFactory.CreateClient("ProductApi");
        using var response = await client.DeleteAsync($"api/products/{id}");
        response.EnsureSuccessStatusCode();
        return true;
    }
}
