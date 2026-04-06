using System.Collections.ObjectModel;
using TourGuideMauiApp.Models;

namespace TourGuideMauiApp.ViewModels
{
    public class MapViewModel
    {
        public ObservableCollection<POI> POIs { get; set; } = new();

        public MapViewModel()
        {
            // sau này sẽ load từ API giống MainPage
        }
    }
}