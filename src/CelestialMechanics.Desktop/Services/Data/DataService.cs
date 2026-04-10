using System.Net.Http;

namespace CelestialMechanics.Desktop.Services.Data;

public sealed class DataService
{
    private readonly HttpClient _httpClient = new();

    public async Task<string> FetchHorizonsSummaryAsync()
    {
        var url = "https://ssd.jpl.nasa.gov/api/horizons.api?format=text&COMMAND='399'&MAKE_EPHEM='YES'&EPHEM_TYPE='VECTORS'&CENTER='500@10'&START_TIME='2026-01-01'&STOP_TIME='2026-01-02'&STEP_SIZE='1d'";
        var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();
        var text = await response.Content.ReadAsStringAsync();
        return $"Fetched {text.Length} chars from Horizons";
    }
}
