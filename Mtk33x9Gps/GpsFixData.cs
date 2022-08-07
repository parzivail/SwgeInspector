namespace Mtk33x9Gps;

public record GpsFixData(
    int UtcHours, int UtcMin, double UtcSec,
    int LatDeg, double LatMin, Hemisphere LatHemisphere,
    int LonDeg, double LonMin, Hemisphere LonHemisphere,
    GpsQuality Quality, int NumSatellites, double Hdop,
    double AltGeoid, char AltUnit,
    double GeoidalSep, char GeoidalSepUnit
);