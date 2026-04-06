using Mapsui;
using Mapsui.Layers;
using Mapsui.Projections;
using Mapsui.Styles;
using TourGuideMauiApp.Models;
using TourGuideMauiApp.Services;
using System;
using System.Linq;
using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.ApplicationModel;

// Alias tránh ambiguous reference (CS0104)
using MapsuiBrush = Mapsui.Styles.Brush;
using MapsuiColor = Mapsui.Styles.Color;
using MapsuiPen = Mapsui.Styles.Pen;
using MapsuiFont = Mapsui.Styles.Font;
using MapsuiSymbol = Mapsui.Styles.SymbolStyle;
using MapsuiLabel = Mapsui.Styles.LabelStyle;
using MapsuiOffset = Mapsui.Styles.Offset;

// Alias cho MAUI
using MauiColor = Microsoft.Maui.Graphics.Color;
using MauiImage = Microsoft.Maui.Controls.Image;

namespace TourGuideMauiApp.Views;

public partial class MapPage : ContentPage
{
    private readonly DatabaseService _dbService = new();
    private readonly TTSService _ttsService = new();
    private readonly GeofenceService _geofenceService = new();

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
        await LoadPoisOnMap();
        await ZoomToMyLocation();
        StartLocationTracking();
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

        var mapControl = this.FindByName<Mapsui.UI.Maui.MapControl>("mapView");
        if (mapControl != null) mapControl.RefreshGraphics();
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

    private async void OnNearestPoiTapped(object sender, TappedEventArgs e)
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

        var mapControl = this.FindByName<Mapsui.UI.Maui.MapControl>("mapView");
        if (mapControl == null) return;

        var map = new Mapsui.Map();
        map.Layers.Add(Mapsui.Tiling.OpenStreetMap.CreateTileLayer());

        _poiLayer = new MemoryLayer { Name = "PoiLayer" };
        _myLocationLayer = new MemoryLayer { Name = "MyLocationLayer" };
        map.Layers.Add(_poiLayer);
        map.Layers.Add(_myLocationLayer);

        mapControl.Map = map;
        mapControl.Info += OnMapInfo;
        _mapInitialized = true;
    }

    private async void OnRefreshMapClicked(object sender, EventArgs e) => await LoadPoisOnMap();
    private async void OnMyLocationClicked(object sender, EventArgs e) => await ZoomToMyLocation();

    // ─── HÀM TẠO STYLE HÌNH GHIM (PIN) GIỐNG GOOGLE MAPS ───────────────────
    private static List<IStyle> CreatePinStyle(MapsuiColor pinColor, string labelText = "")
    {
        var styles = new List<IStyle>();

        // 1. Đuôi ghim (Hình tam giác nhọn phía dưới)
        styles.Add(new MapsuiSymbol
        {
            SymbolType = SymbolType.Triangle,
            SymbolScale = 0.4,
            Fill = new MapsuiBrush { Color = pinColor },
            Outline = new MapsuiPen { Color = MapsuiColor.White, Width = 1 },
            Offset = new MapsuiOffset(0, -12),
            RotateWithMap = true,
            SymbolRotation = 180
        });

        // 2. Đầu ghim (Hình tròn lớn)
        styles.Add(new MapsuiSymbol
        {
            SymbolType = SymbolType.Ellipse,
            SymbolScale = 0.6,
            Fill = new MapsuiBrush { Color = pinColor },
            Outline = new MapsuiPen { Color = MapsuiColor.White, Width = 2 },
            Offset = new MapsuiOffset(0, 4)
        });

        // 3. Chấm trắng ở giữa đầu ghim
        styles.Add(new MapsuiSymbol
        {
            SymbolType = SymbolType.Ellipse,
            SymbolScale = 0.2,
            Fill = new MapsuiBrush { Color = MapsuiColor.White },
            Offset = new MapsuiOffset(0, 4)
        });

        // 4. Nhãn tên
        if (!string.IsNullOrEmpty(labelText))
        {
            styles.Add(new MapsuiLabel
            {
                Text = labelText,
                ForeColor = MapsuiColor.Black,
                BackColor = new MapsuiBrush { Color = MapsuiColor.FromArgb(180, 255, 255, 255) },
                Font = new MapsuiFont { Size = 11, FontFamily = "sans-serif" },
                Offset = new MapsuiOffset(0, 28),
                HorizontalAlignment = MapsuiLabel.HorizontalAlignmentEnum.Center
            });
        }

        return styles;
    }

    // ─── Load POI + vẽ ghim lên bản đồ ──────────────────────────────────────
    private async Task LoadPoisOnMap()
    {
        ShowLoading("Đang tải địa điểm...");
        try
        {
            var pois = await _dbService.GetPointsOfInterestAsync();
            if (pois == null || pois.Count == 0)
            {
                await DisplayAlert("Thông báo",
                    "Không có dữ liệu địa điểm từ Server.\nKiểm tra lại IP và port!", "OK");
                return;
            }

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

                // Áp dụng style GHIM ĐỎ
                foreach (var style in CreatePinStyle(googleRed, poi.Name))
                {
                    feature.Styles.Add(style);
                }

                features.Add(feature);
            }

            _poiLayer.Features = features;
            _allPois = pois;
            _geofenceService.SetPois(pois); // Cập nhật danh sách POI cho Geofence

            var mapControl = this.FindByName<Mapsui.UI.Maui.MapControl>("mapView");
            if (mapControl != null) mapControl.RefreshGraphics();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Lỗi", $"Không thể tải địa điểm: {ex.Message}", "OK");
        }
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

            var mapControl = this.FindByName<Mapsui.UI.Maui.MapControl>("mapView");
            if (mapControl != null && mapControl.Map != null)
            {
                mapControl.Map.Navigator.CenterOnAndZoomTo(
                    new MPoint(x, y),
                    mapControl.Map.Navigator.Resolutions[15]);
                mapControl.RefreshGraphics();
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
    private async void OnLangViTapped(object sender, TappedEventArgs e) => await ChangeLanguage("vi");
    private async void OnLangEnTapped(object sender, TappedEventArgs e) => await ChangeLanguage("en");
    private async void OnLangFrTapped(object sender, TappedEventArgs e) => await ChangeLanguage("fr");
    private async void OnLangJaTapped(object sender, TappedEventArgs e) => await ChangeLanguage("ja");

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
    private async void OnPlayPauseTapped(object sender, TappedEventArgs e)
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

    private async void OnSeekBackward(object sender, TappedEventArgs e)
    {
        await StopAudio();
        await ToggleAudioAsync();
    }

    private async void OnSeekForward(object sender, TappedEventArgs e)
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
