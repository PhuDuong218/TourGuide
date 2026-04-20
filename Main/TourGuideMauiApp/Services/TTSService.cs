using Microsoft.Maui.Media;

namespace TourGuideMauiApp.Services;

public class TTSService
{
    private CancellationTokenSource? _cts;

    // Thêm tham số speed, mặc định là 1.0f
    public async Task SpeakAsync(string text, float speed = 1.0f)
    {
        if (string.IsNullOrEmpty(text)) return;

        _cts?.Cancel();
        _cts = new CancellationTokenSource();

        try
        {
            // Cấu hình âm thanh (Điều chỉnh Pitch để mô phỏng sự thay đổi tốc độ)
            var options = new SpeechOptions
            {
                Volume = 1.0f,
                Pitch = speed
            };

            await TextToSpeech.Default.SpeakAsync(text, options, cancelToken: _cts.Token);
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