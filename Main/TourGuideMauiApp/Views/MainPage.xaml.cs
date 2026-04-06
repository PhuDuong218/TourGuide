using TourGuideMauiApp.Services;
using TourGuideMauiApp.ViewModels;

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
}
