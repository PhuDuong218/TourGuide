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

        if (_allPois == null || _allPois.Count == 0)
        {
            await LoadPoisOnMap();
        }

        await Task.Delay(300);

        if (!string.IsNullOrEmpty(SelectedPoiId))
        {
            string id = SelectedPoiId;
            System.Diagnostics.Debug.WriteLine($"[MapPage] Đang focus vào POI ID: {id}");

            var poi = _allPois?.FirstOrDefault(p => p.POIID == id);
            if (poi != null)
            {
                await FocusOnPoi(poi);
                SelectedPoiId = null;
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

            if (label != null) label.Text = $"Gần bạn: {poi.RestaurantName} ({Math.Round(distance)}m)";
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
            var poi = _allPois.FirstOrDefault(p => p.RestaurantName == poiNameStr);

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

    // Đổi từ private void thành private async Task
    private async Task ShowPoiDetails(POIDTO poi)
    {
        await StopAudio();

        _currentPOI = poi;

        // 1. Cập nhật tên biến NarrationText và ShortDescription
        _currentNarration = string.IsNullOrEmpty(poi.NarrationText) ? poi.ShortDescription : poi.NarrationText;

        // Khai báo các biến UI MỘT LẦN DUY NHẤT
        var nameLabel = this.FindByName<Label>("poiName");
        var descLabel = this.FindByName<Label>("poiDescription");
        var addrLabel = this.FindByName<Label>("poiAddress");
        var distLabel = this.FindByName<Label>("poiDistance");
        var image = this.FindByName<Microsoft.Maui.Controls.Image>("poiImage");
        var icon = this.FindByName<Label>("playPauseIcon");
        var statusText = this.FindByName<Label>("playStatusText");
        var sheet = this.FindByName<Border>("poiBottomSheet");

        // 2. Gán dữ liệu vào UI
        if (nameLabel != null) nameLabel.Text = poi.RestaurantName;
        if (descLabel != null) descLabel.Text = poi.ShortDescription; // Gán mô tả ngắn

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

        if (distLabel != null && _lastLocation != null && poi.Latitude != 0)
        {
            double distance = Location.CalculateDistance(_lastLocation.Latitude, _lastLocation.Longitude, poi.Latitude, poi.Longitude, DistanceUnits.Kilometers) * 1000;
            distLabel.Text = $"📏 Cách bạn: {Math.Round(distance)}m";
            distLabel.IsVisible = true;
        }
        else if (distLabel != null) distLabel.IsVisible = false;

        // 3. Xử lý hình ảnh
        if (image != null)
        {
            if (!string.IsNullOrEmpty(poi.Img))
            {
                image.Source = ImageSource.FromUri(new Uri($"https://gzm4vrwg-7054.asse.devtunnels.ms/uploads/{poi.Img}"));
                image.IsVisible = true;
            }
            else
            {
                image.Source = null;
                image.IsVisible = false;
            }
        }

        // 4. Đặt lại trạng thái nút Play
        if (icon != null) icon.Text = "▶";
        if (statusText != null) statusText.Text = "Phát thuyết minh";
        _isPlaying = false;

        // 5. Hiển thị Bottom Sheet
        if (sheet != null)
        {
            sheet.IsVisible = true;
            sheet.TranslationY = 400;
            await sheet.TranslateTo(0, 0, 280, Easing.CubicOut);
        }

        // 6. Gửi API tăng lượt xem
        try
        {
            using var client = new HttpClient();
            string apiUrl = $"https://gzm4vrwg-7054.asse.devtunnels.ms/api/POI/{poi.POIID}/increment-view";
            _ = client.PostAsync(apiUrl, null);
        }
        catch { /* Bỏ qua lỗi mạng */ }
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
                feature["RestaurantName"] = poi.RestaurantName;
                feature["ShortDescription"] = poi.ShortDescription;
                feature["NarrationText"] = poi.NarrationText;
                feature["Address"] = poi.Address ?? "";
                feature["Img"] = poi.Img ?? "";
                feature["Latitude"] = poi.Latitude;
                feature["Longitude"] = poi.Longitude;

                foreach (var style in CreatePinStyle(googleRed, poi.RestaurantName)) feature.Styles.Add(style);
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
            POIID = feature["POIID"]?.ToString() ?? "",
            RestaurantName = feature["RestaurantName"]?.ToString() ?? "",
            ShortDescription = feature["ShortDescription"]?.ToString() ?? "",
            NarrationText = feature["NarrationText"]?.ToString() ?? "",
            Address = feature["Address"]?.ToString(),

            // SỬA TẠI ĐÂY: Gán vào Img thay vì ImageUrl
            Img = feature["Img"]?.ToString(),

            Latitude = feature["Latitude"] is double lat ? lat : 0,
            Longitude = feature["Longitude"] is double lon ? lon : 0
        };

        await ShowPoiDetails(poi);
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
    // Đổi TappedEventArgs thành EventArgs cho chuẩn MAUI
    private async void OnPlayPauseTapped(object sender, EventArgs e)
    {
        await ToggleAudioAsync();
    }

    private async Task ToggleAudioAsync()
    {
        var icon = this.FindByName<Label>("playPauseIcon");
        var statusText = this.FindByName<Label>("playStatusText");

        if (_isPlaying)
        {
            await _ttsService.StopAsync();
            _isPlaying = false;

            if (icon != null) icon.Text = "▶";
            if (statusText != null) statusText.Text = "Phát thuyết minh";
        }
        else
        {
            // 1. Nếu nội dung rỗng, phải báo cho người dùng biết!
            if (string.IsNullOrEmpty(_currentNarration))
            {
                await Shell.Current.DisplayAlert("Thông báo", "Địa điểm này hiện chưa có nội dung thuyết minh.", "OK");
                return;
            }

            double savedSpeed = Preferences.Get("TTSSpeed", 1.0);
            _isPlaying = true;

            if (icon != null) icon.Text = "⏸";
            if (statusText != null) statusText.Text = "Đang thuyết minh...";

            if (_currentPOI != null)
            {
                _ = Task.Run(async () => {
                    try
                    {
                        using var client = new HttpClient();
                        string apiUrl = $"https://gzm4vrwg-7054.asse.devtunnels.ms/api/POI/{_currentPOI.POIID}/increment-listen";
                        await client.PostAsync(apiUrl, null);
                    }
                    catch { }
                });
            }

            try
            {
                // Thực hiện phát âm thanh
                await _ttsService.SpeakAsync(_currentNarration, (float)savedSpeed);
            }
            catch (Exception ex)
            {
                // 2. BẮT LỖI: Nếu TTS của điện thoại bị lỗi, hiện lên cho dễ sửa
                await Shell.Current.DisplayAlert("Lỗi phát giọng nói", $"Điện thoại không hỗ trợ hoặc lỗi: {ex.Message}", "OK");
            }
            finally
            {
                // Đảm bảo trạng thái quay về ban đầu khi nói xong
                _isPlaying = false;
                MainThread.BeginInvokeOnMainThread(() => {
                    if (icon != null) icon.Text = "▶";
                    if (statusText != null) statusText.Text = "Phát thuyết minh";
                });
            }
        }
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