namespace TourGuideMauiApp.Services;

public class TTSService
{
    private CancellationTokenSource? _cts;

    // Phát v?n b?n
    public async Task SpeakAsync(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return;

        // H?y l?n phát tr??c n?u còn ?ang ch?y
        _cts?.Cancel();
        _cts = new CancellationTokenSource();

        try
        {
            await TextToSpeech.Default.SpeakAsync(text, new SpeechOptions
            {
                Locale = await GetLocaleAsync(),
                Pitch = 1.0f,
                Volume = 1.0f
            }, _cts.Token);
        }
        catch (OperationCanceledException)
        {
            // B? cancel bình th??ng — không throw
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[TTS] L?i: {ex.Message}");
        }
    }

    // D?ng phát
    public Task StopAsync()
    {
        _cts?.Cancel();
        _cts = null;
        return Task.CompletedTask;
    }

    // L?y locale kh?p v?i ngôn ng? thi?t b?
    private static async Task<Locale?> GetLocaleAsync()
    {
        var locales = await TextToSpeech.Default.GetLocalesAsync();
        var lang = System.Globalization.CultureInfo.CurrentCulture.TwoLetterISOLanguageName;

        // ?u tiên locale kh?p ngôn ng? thi?t b?, fallback v? null (m?c ??nh)
        return locales.FirstOrDefault(l =>
            l.Language.StartsWith(lang, StringComparison.OrdinalIgnoreCase));
    }
}