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

    // HÀM ĐÁNH GIÁ ĐỘ ƯU TIÊN (Điểm càng nhỏ càng ưu tiên)
    private int GetPriorityScore(POIDTO poi)
    {
        // Chuyển về chữ thường để dễ so sánh
        string cat = (poi.CategoryName ?? "").ToLower();

        // Ưu tiên 1: Tham quan cốt lõi (Lịch sử, Văn hóa, Cảnh quan, Tâm linh)
        if (cat.Contains("di tích") ||
            cat.Contains("bảo tàng") ||
            cat.Contains("danh lam") ||
            cat.Contains("tâm linh"))
            return 1;

        // Ưu tiên 2: Hoạt động (Chợ, Mua sắm, Giải trí, Công viên)
        if (cat.Contains("chợ") ||
            cat.Contains("mua sắm") ||
            cat.Contains("giải trí") ||
            cat.Contains("công viên"))
            return 2;

        // Ưu tiên 3: Dịch vụ Ăn uống
        if (cat.Contains("quán ăn") ||
            cat.Contains("ẩm thực"))
            return 3;

        // Ưu tiên bét: Khác và các điểm chưa phân loại
        return 99;
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
        // ÁP DỤNG THUẬT TOÁN ƯU TIÊN ĐA TẦNG (MULTI-LEVEL SORTING)
        // ========================================================
        if (poisInRange.Count > 0)
        {
            var prioritizedPoi = poisInRange
                // Tầng 1: Ưu tiên địa điểm Quan trọng nhất trước (Điểm số nhỏ nhất)
                .OrderBy(p => GetPriorityScore(p.Poi))

                // Tầng 2: Nếu cùng độ quan trọng, thì chọn cái Gần nhất
                .ThenBy(p => p.Distance)

                // Tầng 3 (Xử lý trùng lặp hoàn toàn): 
                // Nếu khoảng cách bằng nhau y chang, ưu tiên theo Tên A-Z
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