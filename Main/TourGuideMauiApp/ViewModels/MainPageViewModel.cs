using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TourGuideMauiApp.Models;
using TourGuideMauiApp.Services;
using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.ApplicationModel;

using MauiLocation = Microsoft.Maui.Devices.Sensors.Location;

namespace TourGuideMauiApp.ViewModels;

public class MainPageViewModel : INotifyPropertyChanged
{
    private readonly DatabaseService _dbService;
    private bool _isLoading;
    private string _errorMessage = string.Empty;

    public ObservableCollection<POIDTO> POIs { get; } = new();

    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    public bool IsNotLoading => !IsLoading;

    public bool IsEmpty => !IsLoading && POIs.Count == 0;

    public bool HasData => !IsLoading && POIs.Count > 0;

    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

    public string ErrorMessage
    {
        get => _errorMessage;
        set
        {
            if (SetProperty(ref _errorMessage, value))
            {
                OnPropertyChanged(nameof(HasError));
            }
        }
    }

    public MainPageViewModel(DatabaseService dbService)
    {
        _dbService = dbService;
    }

    public async Task LoadPOIsAsync()
    {
        if (IsLoading) return;

        IsLoading = true;
        ErrorMessage = string.Empty;
        POIs.Clear();
        OnPropertyChanged(nameof(IsEmpty));
        OnPropertyChanged(nameof(HasData));

        try
        {
            var data = await _dbService.GetPointsOfInterestAsync();

            // Lấy vị trí hiện tại của người dùng
            MauiLocation? userLocation = null;
            try
            {
                userLocation = await Geolocation.GetLastKnownLocationAsync();
                if (userLocation == null)
                {
                    userLocation = await Geolocation.GetLocationAsync(new GeolocationRequest
                    {
                        DesiredAccuracy = GeolocationAccuracy.Medium,
                        Timeout = TimeSpan.FromSeconds(5)
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[MainPage] Lỗi lấy vị trí: {ex.Message}");
            }

            foreach (var poi in data)
            {
                if (userLocation != null)
                {
                    double distance = MauiLocation.CalculateDistance(
                        userLocation.Latitude, userLocation.Longitude,
                        poi.Latitude, poi.Longitude,
                        DistanceUnits.Kilometers) * 1000; // Đổi sang mét

                    poi.DistanceText = $"📏 Cách bạn: {Math.Round(distance)}m";
                }
                else
                {
                    poi.DistanceText = "📍 Chưa rõ khoảng cách";
                }

                POIs.Add(poi);
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Lỗi tải dữ liệu: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
            OnPropertyChanged(nameof(IsEmpty));
            OnPropertyChanged(nameof(HasData));
            OnPropertyChanged(nameof(IsNotLoading));
        }
    }

    #region INotifyPropertyChanged
    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string? propertyName = null)
    {
        if (Equals(storage, value)) return false;
        storage = value;
        OnPropertyChanged(propertyName);
        return true;
    }
    #endregion
}
