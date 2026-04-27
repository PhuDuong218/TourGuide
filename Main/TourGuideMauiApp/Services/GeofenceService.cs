using TourGuideMauiApp.Models;

namespace TourGuideMauiApp.Services;

public class GeofenceService
{
    private readonly double _thresholdMeters = 50.0;
    private readonly HashSet<string> _triggeredPois = new();
    private List<POIDTO> _pois = new();

    public event Action<POIDTO>? OnGeofenceTriggered;
    public event Action<POIDTO, double>? OnNearestPoiFound;

    public void SetPois(List<POIDTO> pois)
    {
        _pois = pois;
        _triggeredPois.Clear();
    }

    private int GetPriorityScore(POIDTO poi)
    {
        // Trả về giá trị từ cột Priority trong Database
        return poi.Priority;
    }

    public void CheckProximity(Location userLocation)
    {
        if (_pois == null || _pois.Count == 0) return;

        POIDTO? nearestPoi = null;
        double minDistance = double.MaxValue;
        var poisInRange = new List<(POIDTO Poi, double Distance)>();

        foreach (var poi in _pois)
        {
            double distance = Location.CalculateDistance(
                userLocation.Latitude, userLocation.Longitude,
                poi.Latitude, poi.Longitude,
                DistanceUnits.Kilometers) * 1000;

            if (distance < minDistance)
            {
                minDistance = distance;
                nearestPoi = poi;
            }

            if (distance <= _thresholdMeters && !_triggeredPois.Contains(poi.POIID))
            {
                poisInRange.Add((poi, distance));
            }
        }

        // ========================================================
        // THUẬT TOÁN ƯU TIÊN: SỐ LỚN PHÁT TRƯỚC
        // ========================================================
        if (poisInRange.Count > 0)
        {
            var prioritizedPoi = poisInRange
                // 🔥 Tầng 1: Sắp xếp giảm dần (Descending) - Số lớn nhất đứng đầu
                .OrderByDescending(p => GetPriorityScore(p.Poi))

                // Tầng 2: Nếu cùng Priority, ưu tiên cái gần người dùng nhất (Tăng dần theo khoảng cách)
                .ThenBy(p => p.Distance)

                // Tầng 3: Nếu vẫn bằng nhau, sắp xếp theo tên
                .ThenBy(p => p.Poi.RestaurantName)

                .First().Poi;

            _triggeredPois.Add(prioritizedPoi.POIID);
            OnGeofenceTriggered?.Invoke(prioritizedPoi);
        }
        else if (nearestPoi != null)
        {
            OnNearestPoiFound?.Invoke(nearestPoi, minDistance);
        }
    }

    public void Reset()
    {
        _triggeredPois.Clear();
    }
}