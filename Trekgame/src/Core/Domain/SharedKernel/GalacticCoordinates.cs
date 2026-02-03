namespace StarTrekGame.Domain.SharedKernel;

/// <summary>
/// Represents a position in 3D galactic space.
/// </summary>
public sealed class GalacticCoordinates : ValueObject
{
    public double X { get; }
    public double Y { get; }
    public double Z { get; }

    public GalacticCoordinates(double x, double y, double z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    /// <summary>
    /// Calculate distance to another point in light years.
    /// </summary>
    public double DistanceTo(GalacticCoordinates other)
    {
        var dx = X - other.X;
        var dy = Y - other.Y;
        var dz = Z - other.Z;
        return Math.Sqrt(dx * dx + dy * dy + dz * dz);
    }

    /// <summary>
    /// Get a point along the path to another coordinate.
    /// </summary>
    public GalacticCoordinates PointTowards(GalacticCoordinates target, double fraction)
    {
        fraction = Math.Clamp(fraction, 0, 1);
        return new GalacticCoordinates(
            X + (target.X - X) * fraction,
            Y + (target.Y - Y) * fraction,
            Z + (target.Z - Z) * fraction
        );
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Math.Round(X, 6);
        yield return Math.Round(Y, 6);
        yield return Math.Round(Z, 6);
    }

    public override string ToString() => $"({X:F2}, {Y:F2}, {Z:F2})";

    public static GalacticCoordinates Origin => new(0, 0, 0);
    public static GalacticCoordinates Zero => Origin;
}
