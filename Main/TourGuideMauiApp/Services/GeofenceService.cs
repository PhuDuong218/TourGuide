using TourGuideMauiApp.Models;

namespace TourGuideMauiApp.Services;

public class GeofenceService
{
    private readonly double _thresholdMeters = 50.0; // Khoảng cách kích hoạt (50m)
    private readonly HashSet<string> _triggeredPois = new(); // Lưu các POI đã thuyết minh để tránh lặp
    private List<POIDTO> _pois = new();

    public event Action<POIDTO>? OnGeofenceTriggered;
    public event Action<POIDTO, double>? OnNearestPoiFound;

    public void SetPois(List<POIDTO> pois)
    {
        _pois = pois;
        _triggeredPois.Clear();
    }

    public void CheckProximity(Location userLocation)
    {
        if (_pois == null || _pois.Count == 0) return;

        POIDTO? nearestPoi = null;
        double minDistance = double.MaxValue;
        bool anyInRange = false;

        foreach (var poi in _pois)
        {
            // Tính khoảng cách giữa người dùng và POI
            double distance = Location.CalculateDistance(
                userLocation.Latitude, userLocation.Longitude,
                poi.Latitude, poi.Longitude,
                DistanceUnits.Kilometers) * 1000; // Đổi sang mét

            if (distance < minDistance)
            {
                minDistance = distance;
                nearestPoi = poi;
            }

            if (distance <= _thresholdMeters)
            {
                anyInRange = true;
                // Nếu chưa thuyết minh POI này thì kích hoạt
                if (!_triggeredPois.Contains(poi.POIID))
                {
                    _triggeredPois.Add(poi.POIID);
                    OnGeofenceTriggered?.Invoke(poi);
                }
            }
        }

        // Nếu không có POI nào trong tầm (50m), gợi ý POI gần nhất
        if (!anyInRange && nearestPoi != null)
        {
            OnNearestPoiFound?.Invoke(nearestPoi, minDistance);
        }
    }

    public void Reset()
    {
        _triggeredPois.Clear();
    }
}
