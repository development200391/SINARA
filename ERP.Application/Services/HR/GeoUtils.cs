namespace ERP.Application.Services.HR;

internal static class GeoUtils
{
    private const double EarthRadiusMeters = 6371000;

    public static double DistanceMeters(decimal lat1, decimal lng1, decimal lat2, decimal lng2)
    {
        var lat1Rad = DegreesToRadians((double)lat1);
        var lat2Rad = DegreesToRadians((double)lat2);
        var deltaLat = DegreesToRadians((double)(lat2 - lat1));
        var deltaLng = DegreesToRadians((double)(lng2 - lng1));

        var a = Math.Sin(deltaLat / 2) * Math.Sin(deltaLat / 2) +
                Math.Cos(lat1Rad) * Math.Cos(lat2Rad) *
                Math.Sin(deltaLng / 2) * Math.Sin(deltaLng / 2);
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return EarthRadiusMeters * c;
    }

    private static double DegreesToRadians(double degrees) => degrees * Math.PI / 180;
}
