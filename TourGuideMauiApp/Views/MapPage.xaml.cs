using Mapsui;
using Mapsui.Layers;
using Mapsui.Projections;
using Mapsui.Styles;
using TourGuideMauiApp.Models;
using TourGuideMauiApp.Services;

// Alias tránh ambiguous
using MapsuiBrush = Mapsui.Styles.Brush;
using MapsuiColor = Mapsui.Styles.Color;
using MapsuiPen = Mapsui.Styles.Pen;
using MapsuiFont = Mapsui.Styles.Font;
using MapsuiSymbol = Mapsui.Styles.SymbolStyle;
using MapsuiLabel = Mapsui.Styles.LabelStyle;
using MapsuiOffset = Mapsui.Styles.Offset;

namespace TourGuideMauiApp.Views;

public partial class MapPage : ContentPage
{
    private readonly DatabaseService _dbService = new();
    private readonly TTSService _ttsService = new();

    private MemoryLayer _poiLayer = new() { Name = "PoiLayer" };
    private MemoryLayer _myLocationLayer = new() { Name = "MyLocationLayer" };
    private bool _mapInitialized = false;

    // Trạng thái audio
    private bool _isPlaying = false;
    private string _currentNarration = "";
    private POIDTO? _currentPOI = null;

    public MapPage()
    {
        InitializeComponent();
    }

    // ─── Khởi tạo bản đồ 1 lần ───────────────────────────────────────────────
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        InitMapIfNeeded();
        await LoadPoisOnMap();
        await ZoomToMyLocation();
    }

    private void InitMapIfNeeded()
    {
        if (_mapInitialized) return;

        var map = new Mapsui.Map();
        map.Layers.Add(Mapsui.Tiling.OpenStreetMap.CreateTileLayer());

        _poiLayer = new MemoryLayer { Name = "PoiLayer" };
        _myLocationLayer = new MemoryLayer { Name = "MyLocationLayer" };
        map.Layers.Add(_poiLayer);
        map.Layers.Add(_myLocationLayer);

        mapView.Map = map;
        mapView.Info += OnMapInfo;
        _mapInitialized = true;
    }

    // ─── Buttons ──────────────────────────────────────────────────────────────
    private async void OnRefreshMapClicked(object sender, EventArgs e) => await LoadPoisOnMap();
    private async void OnMyLocationClicked(object sender, EventArgs e) => await ZoomToMyLocation();

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
            foreach (var poi in pois)
            {
                var (x, y) = SphericalMercator.FromLonLat(poi.Longitude, poi.Latitude);
                var feature = new PointFeature(x, y);

                // Lưu toàn bộ thông tin vào feature
                feature["POIID"] = poi.POIID;
                feature["Name"] = poi.Name;
                feature["Description"] = poi.Description;
                feature["Narration"] = poi.Narration;
                feature["Address"] = poi.Address ?? "";
                feature["ImageUrl"] = poi.ImageUrl ?? "";

                // ── Icon ghim (SVG path giả lập bằng SymbolStyle màu đỏ) ──
                // Mapsui dùng SymbolType.Ellipse mặc định.
                // Để có hình ghim thật cần bitmap — dùng cách vẽ 2 lớp:
                // Lớp 1: hình tròn phía trên
                feature.Styles.Add(new MapsuiSymbol
                {
                    SymbolType = SymbolType.Ellipse,
                    SymbolScale = 0.45,
                    Fill = new MapsuiBrush { Color = MapsuiColor.FromArgb(255, 220, 30, 30) },
                    Outline = new MapsuiPen { Color = MapsuiColor.White, Width = 2.5f },
                    Offset = new Offset(0, 8)
                });

                // Lớp 2: hình thoi nhỏ phía dưới (đuôi ghim)
                feature.Styles.Add(new MapsuiSymbol
                {
                    SymbolType = SymbolType.Triangle,
                    SymbolScale = 0.28,
                    Fill = new MapsuiBrush { Color = MapsuiColor.FromArgb(255, 180, 20, 20) },
                    Outline = new MapsuiPen { Color = MapsuiColor.White, Width = 1 },
                    Offset = new Offset(0, -10)
                });

                // Nhãn tên
                feature.Styles.Add(new MapsuiLabel
                {
                    Text = poi.Name ?? "",
                    ForeColor = MapsuiColor.FromArgb(255, 20, 20, 20),
                    BackColor = new MapsuiBrush { Color = MapsuiColor.FromArgb(190, 255, 255, 255) },
                    Font = new MapsuiFont { FontFamily = "sans-serif", Size = 11 },
                    Offset = new MapsuiOffset(0, 26),
                    HorizontalAlignment = MapsuiLabel.HorizontalAlignmentEnum.Center
                });

                features.Add(feature);
            }

            _poiLayer.Features = features;
            mapView.RefreshGraphics();
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

            var (x, y) = SphericalMercator.FromLonLat(location.Longitude, location.Latitude);
            var myFeature = new PointFeature(x, y);

            // Chấm xanh vị trí tôi
            myFeature.Styles.Add(new MapsuiSymbol
            {
                SymbolType = SymbolType.Ellipse,
                SymbolScale = 0.45,
                Fill = new MapsuiBrush { Color = MapsuiColor.FromArgb(255, 0, 122, 255) },
                Outline = new MapsuiPen { Color = MapsuiColor.White, Width = 3 }
            });
            myFeature.Styles.Add(new MapsuiLabel
            {
                Text = "Tôi đang ở đây",
                ForeColor = MapsuiColor.FromArgb(255, 0, 80, 200),
                BackColor = new MapsuiBrush { Color = MapsuiColor.FromArgb(200, 255, 255, 255) },
                Font = new MapsuiFont { FontFamily = "sans-serif", Size = 11 },
                Offset = new MapsuiOffset(0, 20),
                HorizontalAlignment = MapsuiLabel.HorizontalAlignmentEnum.Center
            });

            _myLocationLayer.Features = new List<IFeature> { myFeature };

            mapView.Map.Navigator.CenterOnAndZoomTo(
                new MPoint(x, y),
                mapView.Map.Navigator.Resolutions[15]);
            mapView.RefreshGraphics();
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

        // Dừng audio đang phát nếu có
        await StopAudio();

        // Tìm POI đầy đủ từ feature
        _currentPOI = new POIDTO
        {
            POIID = feature["POIID"] is int id ? id : 0,
            Name = feature["Name"]?.ToString() ?? "",
            Description = feature["Description"]?.ToString() ?? "",
            Narration = feature["Narration"]?.ToString() ?? "",
            Address = feature["Address"]?.ToString(),
            ImageUrl = feature["ImageUrl"]?.ToString()
        };
        _currentNarration = string.IsNullOrEmpty(_currentPOI.Narration)
            ? _currentPOI.Description
            : _currentPOI.Narration;

        // Điền dữ liệu vào bottom sheet
        poiName.Text = _currentPOI.Name;
        poiDescription.Text = _currentPOI.Description;

        if (!string.IsNullOrEmpty(_currentPOI.Address))
        {
            poiAddress.Text = "📍 " + _currentPOI.Address;
            poiAddress.IsVisible = true;
        }
        else
        {
            poiAddress.IsVisible = false;
        }

        // Load ảnh
        if (!string.IsNullOrEmpty(_currentPOI.ImageUrl))
        {
            poiImage.Source = ImageSource.FromUri(new Uri(_currentPOI.ImageUrl));
            poiImage.IsVisible = true;
            imgPlaceholder.IsVisible = false;
        }
        else
        {
            poiImage.IsVisible = false;
            imgPlaceholder.IsVisible = true;
        }

        // Reset play icon
        playPauseIcon.Text = "▶";
        _isPlaying = false;

        // Hiện bottom sheet với animation slide up
        poiBottomSheet.IsVisible = true;
        poiBottomSheet.TranslationY = 400;
        await poiBottomSheet.TranslateTo(0, 0, 280, Easing.CubicOut);
    }

    // ─── Đóng Bottom Sheet ────────────────────────────────────────────────────
    private async void OnCloseBottomSheet(object sender, EventArgs e)
    {
        await StopAudio();
        await poiBottomSheet.TranslateTo(0, 400, 220, Easing.CubicIn);
        poiBottomSheet.IsVisible = false;
    }

    // ─── Play / Pause ─────────────────────────────────────────────────────────
    private async void OnPlayPauseTapped(object sender, TappedEventArgs e)
    {
        if (_isPlaying)
        {
            // Pause
            await _ttsService.StopAsync();
            _isPlaying = false;
            playPauseIcon.Text = "▶";
        }
        else
        {
            // Play
            _isPlaying = true;
            playPauseIcon.Text = "⏸";
            await _ttsService.SpeakAsync(_currentNarration);
            // Khi phát xong → reset về trạng thái stop
            _isPlaying = false;
            playPauseIcon.Text = "▶";
        }
    }

    // ─── Tua lùi 10s (restart với TTS native không hỗ trợ seek) ─────────────
    // MAUI TTS không có seek thật — workaround: stop rồi restart
    private async void OnSeekBackward(object sender, TappedEventArgs e)
    {
        if (!_isPlaying) return;
        await _ttsService.StopAsync();
        _isPlaying = true;
        playPauseIcon.Text = "⏸";
        await _ttsService.SpeakAsync(_currentNarration);
        _isPlaying = false;
        playPauseIcon.Text = "▶";
    }

    // ─── Tua tới 10s (tương tự — TTS native không hỗ trợ seek thật) ─────────
    private async void OnSeekForward(object sender, TappedEventArgs e)
    {
        // Nếu đang phát thì dừng (tương đương skip)
        if (_isPlaying)
        {
            await StopAudio();
        }
    }

    // ─── Stop audio helper ────────────────────────────────────────────────────
    private async Task StopAudio()
    {
        if (_isPlaying)
        {
            await _ttsService.StopAsync();
            _isPlaying = false;
            playPauseIcon.Text = "▶";
        }
    }

    // ─── Loading helpers ──────────────────────────────────────────────────────
    private void ShowLoading(string message)
    {
        loadingLabel.Text = message;
        loadingIndicator.IsRunning = true;
        loadingOverlay.IsVisible = true;
    }

    private void HideLoading()
    {
        loadingIndicator.IsRunning = false;
        loadingOverlay.IsVisible = false;
    }
}