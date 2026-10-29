using System.Net;

namespace FabuRobotics.Orders.Api;

public class CatalogClient(HttpClient httpClient)
{
    public async Task<CatalogProduct?> GetProductAsync(string sku, CancellationToken cancellationToken)
    {
        using var response = await httpClient.GetAsync($"/api/catalog/products/{sku}", cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<CatalogProduct>(cancellationToken);
    }
}