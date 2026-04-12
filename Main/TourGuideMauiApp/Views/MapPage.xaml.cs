using Mapsui;
using Mapsui.Layers;
using Mapsui.Projections;
using Mapsui.Styles;
using SkiaSharp;
using TourGuideMauiApp.Models;
using TourGuideMauiApp.Services;

using MapsuiColor = Mapsui.Styles.Color;
using MapsuiSymbol = Mapsui.Styles.SymbolStyle;
using MapsuiOffset = Mapsui.Styles.Offset;

// Alias cho MAUI
using MauiColor = Microsoft.Maui.Graphics.Color;
using MauiImage = Microsoft.Maui.Controls.Image;
using MauiTappedEventArgs = Microsoft.Maui.Controls.TappedEventArgs;

namespace TourGuideMauiApp.Views;

[QueryProperty(nameof(SelectedPoiId), "poiId")]
public partial class MapPage : ContentPage
{
    public string? SelectedPoiId { get; set; } // Nhận ID từ QR Scanner gửi qua

    private readonly DatabaseService _dbService = new();
    private readonly TTSService _ttsService = new();
    private readonly GeofenceService _geofenceService = new();

    private Mapsui.UI.Maui.MapControl? _mapControl;
    private MemoryLayer _poiLayer = new() { Name = "PoiLayer" };
    private MemoryLayer _myLocationLayer = new() { Name = "MyLocationLayer" };
    private bool _mapInitialized = false;
    private List<POIDTO> _allPois = new();
    private Location? _lastLocation;

    // Trạng thái audio
    private bool _isPlaying = false;
    private string _currentNarration = "";
    private POIDTO? _currentPOI = null;

    public MapPage()
    {
        InitializeComponent();
        _geofenceService.OnGeofenceTriggered += OnGeofenceTriggered;
        _geofenceService.OnNearestPoiFound += OnNearestPoiFound;
    }

    // ─── Khởi tạo bản đồ 1 lần ───────────────────────────────────────────────
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        InitMapIfNeeded();

        // Tải POI nếu chưa có hoặc cần làm mới
        if (_allPois.Count == 0)
        {
            await LoadPoisOnMap();
        }

        // Đợi một chút để MapControl ổn định layout
        await Task.Delay(300);

        // Kiểm tra nếu có POI được truyền từ trang quét QR hoặc danh sách
        if (!string.IsNullOrEmpty(SelectedPoiId) && int.TryParse(SelectedPoiId, out int id))
        {
            System.Diagnostics.Debug.WriteLine($"[MapPage] Đang focus vào POI ID: {id}");
            var poi = _allPois.FirstOrDefault(p => p.POIID == id);
            if (poi != null)
            {
                await FocusOnPoi(poi);
                SelectedPoiId = null; // Xóa ID sau khi đã xử lý
            }
        }
        else if (_lastLocation == null)
        {
            await ZoomToMyLocation();
        }

        StartLocationTracking();
    }

    private async Task FocusOnPoi(POIDTO poi)
    {
        var (x, y) = SphericalMercator.FromLonLat(poi.Longitude, poi.Latitude);

        if (_mapControl != null && _mapControl.Map != null)
        {
            // Di chuyển camera đến vị trí POI
            _mapControl.Map.Navigator.CenterOnAndZoomTo(
                new MPoint(x, y),
                _mapControl.Map.Navigator.Resolutions[15]);

            // Đợi 1 chút để bản đồ ổn định rồi hiện Bottom Sheet
            await Task.Delay(500);
            await ShowPoiDetails(poi);

            _mapControl.RefreshGraphics();
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        StopLocationTracking();
    }

    private async void StartLocationTracking()
    {
        try
        {
            var status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
            if (status != PermissionStatus.Granted) return;

            // Theo dõi vị trí liên tục
            Geolocation.LocationChanged += OnLocationChanged;
            var request = new GeolocationListeningRequest(GeolocationAccuracy.High, TimeSpan.FromSeconds(5));
            await Geolocation.StartListeningForegroundAsync(request);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Lỗi tracking: {ex.Message}");
        }
    }

    private void StopLocationTracking()
    {
        try
        {
            Geolocation.LocationChanged -= OnLocationChanged;
            Geolocation.StopListeningForeground();
        }
        catch { }
    }

    private void OnLocationChanged(object? sender, GeolocationLocationChangedEventArgs e)
    {
        _lastLocation = e.Location;
        MainThread.BeginInvokeOnMainThread(() =>
        {
            UpdateMyLocationOnMap(e.Location);
            _geofenceService.CheckProximity(e.Location);
        });
    }

    private void UpdateMyLocationOnMap(Location location)
    {
        var (x, y) = SphericalMercator.FromLonLat(location.Longitude, location.Latitude);
        var myFeature = new PointFeature(x, y);
        var googleBlue = MapsuiColor.FromArgb(255, 66, 133, 244);

        foreach (var style in CreatePinStyle(googleBlue, "Vị trí của tôi"))
        {
            myFeature.Styles.Add(style);
        }

        _myLocationLayer.Features = new List<IFeature> { myFeature };

        if (_mapControl != null) _mapControl.RefreshGraphics();
    }

    private async void OnGeofenceTriggered(POIDTO poi)
    {
        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            var panel = this.FindByName<Border>("nearestPoiPanel");
            if (panel != null) panel.IsVisible = false;

            // Tự động hiển thị và thuyết minh
            await ShowPoiDetails(poi);
            await ToggleAudioAsync();
        });
    }

    private void OnNearestPoiFound(POIDTO poi, double distance)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            var sheet = this.FindByName<Border>("poiBottomSheet");
            if (sheet != null && sheet.IsVisible) return;

            var label = this.FindByName<Label>("nearestPoiLabel");
            var panel = this.FindByName<Border>("nearestPoiPanel");

            if (label != null) label.Text = $"Gần bạn: {poi.Name} ({Math.Round(distance)}m)";
            if (panel != null) panel.IsVisible = true;
        });
    }

    private async void OnNearestPoiTapped(object sender, MauiTappedEventArgs e)
    {
        try
        {
            var label = this.FindByName<Label>("nearestPoiLabel");
            var panel = this.FindByName<Border>("nearestPoiPanel");
            if (label == null) return;

            var text = label.Text;
            if (string.IsNullOrEmpty(text) || !text.Contains(":") || !text.Contains("(")) return;

            var poiNameStr = text.Split(':')[1].Split('(')[0].Trim();
            var poi = _allPois.FirstOrDefault(p => p.Name == poiNameStr);

            if (poi != null)
            {
                if (panel != null) panel.IsVisible = false;
                await ShowPoiDetails(poi);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Lỗi OnNearestPoiTapped: {ex.Message}");
        }
    }

    private async Task ShowPoiDetails(POIDTO poi)
    {
        await StopAudio();

        _currentPOI = poi;
        _currentNarration = string.IsNullOrEmpty(poi.Narration)
            ? poi.Description
            : poi.Narration;

        var nameLabel = this.FindByName<Label>("poiName");
        var descLabel = this.FindByName<Label>("poiDescription");
        var addrLabel = this.FindByName<Label>("poiAddress");
        var distLabel = this.FindByName<Label>("poiDistance");
        var image = this.FindByName<MauiImage>("poiImage");
        var placeholder = this.FindByName<Label>("imgPlaceholder");
        var icon = this.FindByName<Label>("playPauseIcon");
        var sheet = this.FindByName<Border>("poiBottomSheet");

        if (nameLabel != null) nameLabel.Text = poi.Name;
        if (descLabel != null) descLabel.Text = poi.Description;

        if (addrLabel != null)
        {
            if (!string.IsNullOrEmpty(poi.Address))
            {
                addrLabel.Text = "📍 " + poi.Address;
                addrLabel.IsVisible = true;
            }
            else
            {
                addrLabel.IsVisible = false;
            }
        }

        // Hiển thị khoảng cách
        if (distLabel != null && _lastLocation != null && poi.Latitude != 0)
        {
            double distance = Location.CalculateDistance(
                _lastLocation.Latitude, _lastLocation.Longitude,
                poi.Latitude, poi.Longitude,
                DistanceUnits.Kilometers) * 1000; // Đổi sang mét

            distLabel.Text = $"📏 Cách bạn: {Math.Round(distance)}m";
            distLabel.IsVisible = true;
        }
        else if (distLabel != null)
        {
            distLabel.IsVisible = false;
        }

        if (image != null && placeholder != null)
        {
            if (!string.IsNullOrEmpty(poi.ImageUrl))
            {
                image.Source = ImageSource.FromUri(new Uri(poi.ImageUrl));
                image.IsVisible = true;
                placeholder.IsVisible = false;
            }
            else
            {
                image.IsVisible = false;
                placeholder.IsVisible = true;
            }
        }

        if (icon != null) icon.Text = "▶";
        _isPlaying = false;

        UpdateLanguageUI("VI");

        if (sheet != null)
        {
            sheet.IsVisible = true;
            sheet.TranslationY = 400;
            await sheet.TranslateTo(0, 0, 280, Easing.CubicOut);
        }
    }

    private void InitMapIfNeeded()
    {
        if (_mapInitialized) return;

        _mapControl = this.FindByName<Mapsui.UI.Maui.MapControl>("mapView");
        if (_mapControl == null) return;

        var map = new Mapsui.Map();
        map.Layers.Add(Mapsui.Tiling.OpenStreetMap.CreateTileLayer());

        _poiLayer = new MemoryLayer { Name = "PoiLayer" };
        _myLocationLayer = new MemoryLayer { Name = "MyLocationLayer" };
        map.Layers.Add(_poiLayer);
        map.Layers.Add(_myLocationLayer);

        _mapControl.Map = map;
        _mapControl.Info += OnMapInfo;
        _mapInitialized = true;
    }

    private async void OnRefreshMapClicked(object sender, EventArgs e) => await LoadPoisOnMap();
    private async void OnMyLocationClicked(object sender, EventArgs e) => await ZoomToMyLocation();

    // ─── VẼ ICON GHIM PHẲNG (GIỐNG HÌNH BẠN GỬI) ───────────────────────────
    private static byte[] CreatePinBitmap(SkiaSharp.SKColor bodyColor)
    {
        const int W = 80, H = 100;
        using var surface = SKSurface.Create(new SKImageInfo(W, H, SKColorType.Rgba8888));
        var canvas = surface.Canvas;
        canvas.Clear(SKColors.Transparent);

        using var paint = new SKPaint { IsAntialias = true, Color = bodyColor, Style = SKPaintStyle.Fill };
        float cx = W / 2f;
        float r = W / 2.5f;

        using var path = new SKPath();
        path.MoveTo(cx, H - 5);
        path.ArcTo(new SKRect(cx - r, 5, cx + r, 5 + 2 * r), 135, 270, false);
        path.Close();
        canvas.DrawPath(path, paint);

        using var whitePaint = new SKPaint { IsAntialias = true, Color = SKColors.White, Style = SKPaintStyle.Fill };
        canvas.DrawCircle(cx, r + 5, r * 0.4f, whitePaint);

        using var image = surface.Snapshot();
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        return data.ToArray();
    }

    private static List<IStyle> CreatePinStyle(Mapsui.Styles.Color pinColor, string labelText = "")
    {
        var styles = new List<IStyle>();

        styles.Add(new SymbolStyle
        {
            SymbolScale = 0.7,

            Fill = new Mapsui.Styles.Brush
            {
                Color = pinColor
            },

            Outline = new Pen
            {
                Color = MapsuiColor.White,
                Width = 3
            }
        });

        if (!string.IsNullOrEmpty(labelText))
        {
            styles.Add(new LabelStyle
            {
                Text = labelText,

                ForeColor = MapsuiColor.Black,

                BackColor = new Mapsui.Styles.Brush
                {
                    Color = MapsuiColor.FromArgb(180, 255, 255, 255)
                },

                Font = new Mapsui.Styles.Font
                {
                    Size = 11
                },

                Offset = new Offset(0, 20),

                HorizontalAlignment = LabelStyle.HorizontalAlignmentEnum.Center
            });
        }

        return styles;
    }

    private async Task LoadPoisOnMap()
    {
        ShowLoading("Đang tải địa điểm...");
        try
        {
            var pois = await _dbService.GetPointsOfInterestAsync();
            if (pois == null || pois.Count == 0) return;

            var features = new List<IFeature>();
            var googleRed = MapsuiColor.FromArgb(255, 219, 68, 55);

            foreach (var poi in pois)
            {
                var (x, y) = SphericalMercator.FromLonLat(poi.Longitude, poi.Latitude);
                var feature = new PointFeature(x, y);
                feature["POIID"] = poi.POIID;
                feature["Name"] = poi.Name;
                feature["Description"] = poi.Description;
                feature["Narration"] = poi.Narration;
                feature["Address"] = poi.Address ?? "";
                feature["ImageUrl"] = poi.ImageUrl ?? "";
                feature["Latitude"] = poi.Latitude;
                feature["Longitude"] = poi.Longitude;

                foreach (var style in CreatePinStyle(googleRed, poi.Name)) feature.Styles.Add(style);
                features.Add(feature);
            }
            _poiLayer.Features = features;
            _allPois = pois;
            _geofenceService.SetPois(pois);
            if (_mapControl != null) _mapControl.RefreshGraphics();
        }
        catch (Exception ex) { System.Diagnostics.Debug.WriteLine(ex.Message); }
        finally { HideLoading(); }
    }

    // ─── Zoom về vị trí hiện tại ──────────────────────────────────────────────
    private async Task ZoomToMyLocation()
    {
        ShowLoading("Đang lấy vị trí...");
        try
        {
            var status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
            if (status != PermissionStatus.Granted)
            {
                await DisplayAlert("Thông báo", "Cần quyền truy cập vị trí.", "OK");
                return;
            }

            var location = await Geolocation.GetLocationAsync(new GeolocationRequest
            {
                DesiredAccuracy = GeolocationAccuracy.Medium,
                Timeout = TimeSpan.FromSeconds(10)
            });
            if (location == null) return;

            _lastLocation = location;
            var (x, y) = SphericalMercator.FromLonLat(location.Longitude, location.Latitude);
            var myFeature = new PointFeature(x, y);
            var googleBlue = MapsuiColor.FromArgb(255, 66, 133, 244);

            // Áp dụng style GHIM XANH
            foreach (var style in CreatePinStyle(googleBlue, "Vị trí của tôi"))
            {
                myFeature.Styles.Add(style);
            }

            _myLocationLayer.Features = new List<IFeature> { myFeature };

            if (_mapControl != null && _mapControl.Map != null)
            {
                _mapControl.Map.Navigator.CenterOnAndZoomTo(
                    new MPoint(x, y),
                    _mapControl.Map.Navigator.Resolutions[15]);
                _mapControl.RefreshGraphics();
            }
        }
        catch (FeatureNotSupportedException)
        {
            await DisplayAlert("Thông báo", "Thiết bị không hỗ trợ GPS.", "OK");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Lỗi vị trí: {ex.Message}");
        }
        finally { HideLoading(); }
    }

    // ─── Tap vào marker → mở Bottom Sheet ────────────────────────────────────
    private async void OnMapInfo(object? sender, MapInfoEventArgs e)
    {
        var feature = e.GetMapInfo(new[] { _poiLayer })?.Feature;
        if (feature == null) return;

        var poi = new POIDTO
        {
            POIID = feature["POIID"] is int id ? id : 0,
            Name = feature["Name"]?.ToString() ?? "",
            Description = feature["Description"]?.ToString() ?? "",
            Narration = feature["Narration"]?.ToString() ?? "",
            Address = feature["Address"]?.ToString(),
            ImageUrl = feature["ImageUrl"]?.ToString(),
            Latitude = feature["Latitude"] is double lat ? lat : 0,
            Longitude = feature["Longitude"] is double lon ? lon : 0
        };

        await ShowPoiDetails(poi);
    }

    // ─── Language selection handlers ──────────────────────────────────────────
    private async void OnLangViTapped(object sender, MauiTappedEventArgs e) => await ChangeLanguage("vi");
    private async void OnLangEnTapped(object sender, MauiTappedEventArgs e) => await ChangeLanguage("en");
    private async void OnLangFrTapped(object sender, MauiTappedEventArgs e) => await ChangeLanguage("fr");
    private async void OnLangJaTapped(object sender, MauiTappedEventArgs e) => await ChangeLanguage("ja");

    private async Task ChangeLanguage(string langCode)
    {
        if (_currentPOI == null) return;

        UpdateLanguageUI(langCode.ToUpper());
        ShowLoading("Đang chuyển ngôn ngữ...");

        try
        {
            var updatedPoi = await _dbService.GetPOIByIdAsync(_currentPOI.POIID, langCode.ToLower());

            if (updatedPoi != null)
            {
                await StopAudio();

                _currentPOI = updatedPoi;
                _currentNarration = string.IsNullOrEmpty(updatedPoi.Narration)
                                    ? updatedPoi.Description
                                    : updatedPoi.Narration;

                var nameLabel = this.FindByName<Label>("poiName");
                var descLabel = this.FindByName<Label>("poiDescription");
                var addrLabel = this.FindByName<Label>("poiAddress");

                if (nameLabel != null) nameLabel.Text = updatedPoi.Name;
                if (descLabel != null) descLabel.Text = updatedPoi.Description;
                if (addrLabel != null && !string.IsNullOrEmpty(updatedPoi.Address))
                    addrLabel.Text = "📍 " + updatedPoi.Address;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Lỗi chuyển ngôn ngữ: {ex.Message}");
        }
        finally
        {
            HideLoading();
        }
    }

    private void UpdateLanguageUI(string lang)
    {
        var inactiveBg = MauiColor.FromArgb("#EEEEEE");
        var activeBg = MauiColor.FromArgb("#6200EE");
        var inactiveText = MauiColor.FromArgb("#555555");
        var activeText = Microsoft.Maui.Graphics.Colors.White;

        var bVI = this.FindByName<Border>("btnLangVI");
        var bEN = this.FindByName<Border>("btnLangEN");
        var bFR = this.FindByName<Border>("btnLangFR");
        var bJA = this.FindByName<Border>("btnLangJA");
        var lVI = this.FindByName<Label>("lblVI");
        var lEN = this.FindByName<Label>("lblEN");
        var lFR = this.FindByName<Label>("lblFR");
        var lJA = this.FindByName<Label>("lblJA");

        // Reset UI
        if (bVI != null) bVI.BackgroundColor = inactiveBg; if (lVI != null) lVI.TextColor = inactiveText;
        if (bEN != null) bEN.BackgroundColor = inactiveBg; if (lEN != null) lEN.TextColor = inactiveText;
        if (bFR != null) bFR.BackgroundColor = inactiveBg; if (lFR != null) lFR.TextColor = inactiveText;
        if (bJA != null) bJA.BackgroundColor = inactiveBg; if (lJA != null) lJA.TextColor = inactiveText;

        switch (lang)
        {
            case "VI": if (bVI != null) bVI.BackgroundColor = activeBg; if (lVI != null) lVI.TextColor = activeText; break;
            case "EN": if (bEN != null) bEN.BackgroundColor = activeBg; if (lEN != null) lEN.TextColor = activeText; break;
            case "FR": if (bFR != null) bFR.BackgroundColor = activeBg; if (lFR != null) lFR.TextColor = activeText; break;
            case "JA": if (bJA != null) bJA.BackgroundColor = activeBg; if (lJA != null) lJA.TextColor = activeText; break;
        }
    }

    // ─── Đóng Bottom Sheet ────────────────────────────────────────────────────
    private async void OnCloseBottomSheet(object sender, EventArgs e)
    {
        await StopAudio();
        var sheet = this.FindByName<Border>("poiBottomSheet");
        if (sheet != null)
        {
            await sheet.TranslateTo(0, 400, 220, Easing.CubicIn);
            sheet.IsVisible = false;
        }
    }

    // ─── Play / Pause ─────────────────────────────────────────────────────────
    private async void OnPlayPauseTapped(object sender, MauiTappedEventArgs e)
    {
        await ToggleAudioAsync();
    }

    private async Task ToggleAudioAsync()
    {
        var icon = this.FindByName<Label>("playPauseIcon");
        if (_isPlaying)
        {
            await _ttsService.StopAsync();
            _isPlaying = false;
            if (icon != null) icon.Text = "▶";
        }
        else
        {
            _isPlaying = true;
            if (icon != null) icon.Text = "⏸";
            await _ttsService.SpeakAsync(_currentNarration);
            _isPlaying = false;
            if (icon != null) icon.Text = "▶";
        }
    }

    private async void OnSeekBackward(object sender, MauiTappedEventArgs e)
    {
        await StopAudio();
        await ToggleAudioAsync();
    }

    private async void OnSeekForward(object sender, MauiTappedEventArgs e)
    {
        await StopAudio();
        await ToggleAudioAsync();
    }

    private async Task StopAudio()
    {
        if (_isPlaying)
        {
            await _ttsService.StopAsync();
            _isPlaying = false;
            var icon = this.FindByName<Label>("playPauseIcon");
            if (icon != null) icon.Text = "▶";
        }
    }

    private void ShowLoading(string message)
    {
        var label = this.FindByName<Label>("loadingLabel");
        var indicator = this.FindByName<ActivityIndicator>("loadingIndicator");
        var overlay = this.FindByName<Border>("loadingOverlay");

        if (label != null) label.Text = message;
        if (indicator != null) indicator.IsRunning = true;
        if (overlay != null) overlay.IsVisible = true;
    }

    private void HideLoading()
    {
        var indicator = this.FindByName<ActivityIndicator>("loadingIndicator");
        var overlay = this.FindByName<Border>("loadingOverlay");

        if (indicator != null) indicator.IsRunning = false;
        if (overlay != null) overlay.IsVisible = false;
    }
}
