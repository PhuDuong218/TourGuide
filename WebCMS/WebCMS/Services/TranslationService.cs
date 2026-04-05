using WebCMS.Models;
using System.Net.Http.Json;
public class TranslationService
{
    private readonly HttpClient _http;

    public TranslationService(HttpClient http)
    {
        _http = http;
        _http.BaseAddress = new Uri("https://localhost:5001/api/");
    }

    public List<POITranslation> GetByPOI(int id)
    {
        return _http.GetFromJsonAsync<List<POITranslation>>($"Translation/{id}").Result;
    }

    public void Create(POITranslation t)
    {
        _http.PostAsJsonAsync("Translation", t).Wait();
    }

    public void Delete(int poiId, string lang)
    {
        _http.DeleteAsync($"Translation/{poiId}/{lang}").Wait();
    }
}