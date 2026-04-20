using TourGuideMauiApp.Services;
using TourGuideMauiApp.ViewModels;
using TourGuideMauiApp.Models;

namespace TourGuideMauiApp.Views;

public partial class MainPage : ContentPage
{
    private readonly MainPageViewModel _viewModel;

    public MainPage(DatabaseService dbService)
    {
        InitializeComponent();
        _viewModel = new MainPageViewModel(dbService);
        BindingContext = _viewModel;
    }

    // Tự động load data khi mở tab
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Chỉ load lần đầu, không reload mỗi lần quay lại tab
        if (!_viewModel.HasData)
        {
            await _viewModel.LoadPOIsAsync();
        }
    }

    private async void OnRefreshClicked(object sender, EventArgs e)
    {
        await _viewModel.LoadPOIsAsync();
    }
    private async void OnPoiTapped(object sender, TappedEventArgs e)
    {
        var poi = e.Parameter as POIDTO;
        if (poi == null) return;

        // Điều hướng sang tab MapPage và truyền tham số poiId
        // Sử dụng "//MapPage" để chuyển hẳn sang Tab bản đồ
        await Shell.Current.GoToAsync($"//MapPage?poiId={poi.POIID}");
    }
}
