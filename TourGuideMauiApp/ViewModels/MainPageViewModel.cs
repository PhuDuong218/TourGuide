using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TourGuideMauiApp.Models;
using TourGuideMauiApp.Services;

namespace TourGuideMauiApp.ViewModels;

public class MainPageViewModel : INotifyPropertyChanged
{
    private readonly DatabaseService _dbService;

    public ObservableCollection<POIDTO> POIs { get; } = new();

    // ─── Loading state ────────────────────────────────────────────────────────
    private bool _isLoading;
    public bool IsLoading
    {
        get => _isLoading;
        set { _isLoading = value; OnPropertyChanged(); OnPropertyChanged(nameof(IsNotLoading)); }
    }
    public bool IsNotLoading => !_isLoading;

    // ─── Empty state ──────────────────────────────────────────────────────────
    public bool IsEmpty => !IsLoading && POIs.Count == 0 && !HasError;
    public bool HasData => POIs.Count > 0;

    // ─── Error state ──────────────────────────────────────────────────────────
    private string _errorMessage = "";
    public string ErrorMessage
    {
        get => _errorMessage;
        set { _errorMessage = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasError)); }
    }
    public bool HasError => !string.IsNullOrEmpty(_errorMessage);

    public MainPageViewModel(DatabaseService dbService)
    {
        _dbService = dbService;
    }

    public async Task LoadPOIsAsync()
    {
        IsLoading = true;
        ErrorMessage = "";
        POIs.Clear();
        OnPropertyChanged(nameof(IsEmpty));
        OnPropertyChanged(nameof(HasData));

        try
        {
            var result = await _dbService.GetPointsOfInterestAsync();

            if (result == null || result.Count == 0)
            {
                ErrorMessage = "Không có dữ liệu. Kiểm tra lại kết nối server.";
            }
            else
            {
                foreach (var poi in result)
                    POIs.Add(poi);
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Lỗi kết nối: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
            OnPropertyChanged(nameof(IsEmpty));
            OnPropertyChanged(nameof(HasData));
        }
    }

    // ─── INotifyPropertyChanged ───────────────────────────────────────────────
    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}