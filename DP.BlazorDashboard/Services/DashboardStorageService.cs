using Microsoft.JSInterop;
using System.Text.Json;
using DP.BlazorDashboard.Models;

namespace DP.BlazorDashboard.Services;

public class DashboardStorageService
{
    private readonly IJSRuntime _jsRuntime;

    public DashboardStorageService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task<List<DashboardCard>> LoadLayoutAsync(string storageKey)
    {
        try
        {
            var json = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", storageKey);
            if (!string.IsNullOrEmpty(json))
            {
                return JsonSerializer.Deserialize<List<DashboardCard>>(json) ?? new List<DashboardCard>();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading layout: {ex.Message}");
        }

        return new List<DashboardCard>();
    }

    public async Task SaveLayoutAsync(string storageKey, List<DashboardCard> cards)
    {
        try
        {
            var json = JsonSerializer.Serialize(cards);
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", storageKey, json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving layout: {ex.Message}");
        }
    }
}