namespace Backend.Services;

public static class BarrelService
{
    private const double FullDistanceCm = 5.0;
    private const double EmptyDistanceCm = 87.0;

    public static double CalculateFillPercentage(double distanceCm)
    {
        var percent = ((EmptyDistanceCm - distanceCm) /
                       (EmptyDistanceCm - FullDistanceCm)) * 100;

        return Math.Clamp(percent, 0, 100);
    }

    public static string GetLevelStatus(double percent)
    {
        if (percent < 15) return "danger";
        if (percent <= 75) return "warning";
        return "success";
    }
}