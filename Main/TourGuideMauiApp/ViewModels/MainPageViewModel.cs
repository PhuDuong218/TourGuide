using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using TourGuideMauiApp.Models;
using TourGuideMauiApp.Services;
using Microsoft.Maui.Devices.Sensors;
using MauiLocation = Microsoft.Maui.Devices.Sensors.Location;

namespace TourGuideMauiApp.ViewModels;

public class MainPageViewModel : INotifyPropertyChanged
{
    private readonly DatabaseService _dbService;
    private bool _isLoading;
    private string _errorMessage = string.Empty;

    // Danh sách gốc chứa TẤT CẢ dữ liệu
    private List<POIDTO> _allFeatured = new();
    private List<POIDTO> _allNearby = new();

    // Trạng thái nút Xem thêm
    private bool _isFeaturedExpanded = false;
    private bool _isNearbyExpanded = false;

    // Danh sách hiển thị ra màn hình
    public ObservableCollection<POIDTO> FeaturedPOIs { get; } = new();
    public ObservableCollection<POIDTO> NearbyPOIs { get; } = new();

    // Các lệnh (Command) cho nút bấm
    public ICommand ToggleFeaturedCommand { get; }
    public ICommand ToggleNearbyCommand { get; }

    // Chữ trên nút bấm
    public string FeaturedButtonText => _isFeaturedExpanded ? "Thu gọn" : "Xem thêm";
    public string NearbyButtonText => _isNearbyExpanded ? "Thu gọn" : "Xem thêm";

    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }
    public bool IsNotLoading => !IsLoading;
    public bool IsEmpty => !IsLoading && NearbyPOIs.Count == 0;
    public bool HasData => !IsLoading && _allNearby.Count > 0;
    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);
    public int TotalCount => _allNearby.Count;

    public string ErrorMessage
    {
        get => _errorMessage;
        set { if (SetProperty(ref _errorMessage, value)) OnPropertyChanged(nameof(HasError)); }
    }

    public MainPageViewModel(DatabaseService dbService)
    {
        _dbService = dbService;

        // Xử lý sự kiện khi bấm nút Xem thêm NỔI BẬT
        ToggleFeaturedCommand = new Command(() =>
        {
            _isFeaturedExpanded = !_isFeaturedExpanded;
            UpdateFeaturedDisplay();
            OnPropertyChanged(nameof(FeaturedButtonText));
        });

        // Xử lý sự kiện khi bấm nút Xem thêm GẦN BẠN
        ToggleNearbyCommand = new Command(() =>
        {
            _isNearbyExpanded = !_isNearbyExpanded;
            UpdateNearbyDisplay();
            OnPropertyChanged(nameof(NearbyButtonText));
        });
    }

    public async Task LoadPOIsAsync()
    {
        if (IsLoading) return;
        IsLoading = true;
        ErrorMessage = string.Empty;
        NotifyStateChanged();

        try
        {
            var data = await _dbService.GetPointsOfInterestAsync();

            // 1. Lấy vị trí
            MauiLocation? userLocation = null;
            try { userLocation = await Geolocation.GetLastKnownLocationAsync() ?? await Geolocation.GetLocationAsync(new GeolocationRequest { DesiredAccuracy = GeolocationAccuracy.Medium, Timeout = TimeSpan.FromSeconds(5) }); }
            catch { /* Bỏ qua lỗi GPS */ }

            // 2. Tính khoảng cách
            var poiWithDistance = new List<(POIDTO Poi, double Distance)>();
            foreach (var poi in data)
            {
                double dist = double.MaxValue;
                if (userLocation != null)
                {
                    dist = MauiLocation.CalculateDistance(userLocation.Latitude, userLocation.Longitude, poi.Latitude, poi.Longitude, DistanceUnits.Kilometers) * 1000;
                    poi.DistanceText = $"📏 Cách bạn: {Math.Round(dist)}m";
                }
                else poi.DistanceText = "📍 Chưa rõ khoảng cách";
                poiWithDistance.Add((poi, dist));
            }

            // 3. Lưu vào danh sách gốc
            _allNearby = poiWithDistance.OrderBy(x => x.Distance).Select(x => x.Poi).ToList();
            _allFeatured = data.Take(5).ToList(); // Lấy 5 cái đầu làm nổi bật

            // 4. Reset trạng thái thu gọn
            _isFeaturedExpanded = false;
            _isNearbyExpanded = false;
            OnPropertyChanged(nameof(FeaturedButtonText));
            OnPropertyChanged(nameof(NearbyButtonText));

            // 5. Cập nhật giao diện
            UpdateFeaturedDisplay();
            UpdateNearbyDisplay();
        }
        catch (Exception ex) { ErrorMessage = $"Lỗi: {ex.Message}"; }
        finally { IsLoading = false; NotifyStateChanged(); OnPropertyChanged(nameof(TotalCount)); }
    }

    // Hàm cắt danh sách NỔI BẬT (1-2 cái)
    private void UpdateFeaturedDisplay()
    {
        FeaturedPOIs.Clear();
        var items = _isFeaturedExpanded ? _allFeatured : _allFeatured.Take(1); // Hiện 1 cái
        foreach (var p in items) FeaturedPOIs.Add(p);
    }

    // Hàm cắt danh sách GẦN BẠN (3-4 cái)
    private void UpdateNearbyDisplay()
    {
        NearbyPOIs.Clear();
        var items = _isNearbyExpanded ? _allNearby : _allNearby.Take(3); // Hiện 3 cái
        foreach (var p in items) NearbyPOIs.Add(p);
    }

    private void NotifyStateChanged()
    {
        OnPropertyChanged(nameof(IsEmpty));
        OnPropertyChanged(nameof(HasData));
        OnPropertyChanged(nameof(IsNotLoading));
    }

    #region INotifyPropertyChanged
    public event PropertyChangedEventHandler? PropertyChanged;
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string? propertyName = null)
    {
        if (Equals(storage, value)) return false;
        storage = value; OnPropertyChanged(propertyName); return true;
    }
    #endregion
}