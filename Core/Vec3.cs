namespace Lab7.Core;

public readonly record struct Vec3(double X, double Y, double Z)
{
    public static Vec3 operator +(Vec3 a, Vec3 b) => new(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
    public static Vec3 operator -(Vec3 a, Vec3 b) => new(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
    public static Vec3 operator *(Vec3 v, double k) => new(v.X * k, v.Y * k, v.Z * k);
    public double Length => Math.Sqrt(Dot(this, this));
    public bool IsFinite => double.IsFinite(X) && double.IsFinite(Y) && double.IsFinite(Z);

    public Vec3 Normalized()
    {
        double length = Length;
        if (length < 1e-12 || !double.IsFinite(length))
            throw new ArgumentException("Нельзя нормализовать нулевой или некорректный вектор.");
        return this * (1 / length);
    }

    public static double Dot(Vec3 a, Vec3 b) => a.X * b.X + a.Y * b.Y + a.Z * b.Z;
    public static Vec3 Cross(Vec3 a, Vec3 b) => new(
        a.Y * b.Z - a.Z * b.Y,
        a.Z * b.X - a.X * b.Z,
        a.X * b.Y - a.Y * b.X);
}
