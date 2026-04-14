namespace TourGuideMauiApp.Services;

public class TTSService
{
    private CancellationTokenSource? _cts;

    public async Task SpeakAsync(string text)
    {
        if (string.IsNullOrEmpty(text)) return;

        _cts?.Cancel();
        _cts = new CancellationTokenSource();

        try
        {
            await TextToSpeech.Default.SpeakAsync(text, cancelToken: _cts.Token);
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"TTS Error: {ex.Message}");
        }
    }

    public async Task StopAsync()
    {
        _cts?.Cancel();
        try
        {
            await TextToSpeech.Default.SpeakAsync(string.Empty); 
        }
        catch { }
    }
}
