namespace Lab7.Core;

public sealed class Matrix4
{
    private readonly double[,] m;
    public double this[int row, int column] => m[row, column];

    public Matrix4(double[,] values)
    {
        if (values.GetLength(0) != 4 || values.GetLength(1) != 4)
            throw new ArgumentException("Нужна матрица 4 × 4.");
        m = (double[,])values.Clone();
    }

    public static Matrix4 operator *(Matrix4 a, Matrix4 b)
    {
        var result = new double[4, 4];
        for (int i = 0; i < 4; i++)
            for (int j = 0; j < 4; j++)
                for (int k = 0; k < 4; k++)
                    result[i, j] += a[i, k] * b[k, j];
        return new Matrix4(result);
    }

    public Vec3 Transform(Vec3 p)
    {
        double x = m[0, 0] * p.X + m[0, 1] * p.Y + m[0, 2] * p.Z + m[0, 3];
        double y = m[1, 0] * p.X + m[1, 1] * p.Y + m[1, 2] * p.Z + m[1, 3];
        double z = m[2, 0] * p.X + m[2, 1] * p.Y + m[2, 2] * p.Z + m[2, 3];
        double w = m[3, 0] * p.X + m[3, 1] * p.Y + m[3, 2] * p.Z + m[3, 3];
        if (Math.Abs(w) < 1e-12)
            throw new ArgumentException("Нулевая однородная координата w.");
        return new Vec3(x / w, y / w, z / w);
    }

    public double LinearDeterminant =>
        m[0, 0] * (m[1, 1] * m[2, 2] - m[1, 2] * m[2, 1]) -
        m[0, 1] * (m[1, 0] * m[2, 2] - m[1, 2] * m[2, 0]) +
        m[0, 2] * (m[1, 0] * m[2, 1] - m[1, 1] * m[2, 0]);

    public static Matrix4 Translation(double x, double y, double z) => new(new double[,]
    {
        { 1, 0, 0, x }, { 0, 1, 0, y }, { 0, 0, 1, z }, { 0, 0, 0, 1 }
    });

    public static Matrix4 Scaling(double x, double y, double z) => new(new double[,]
    {
        { x, 0, 0, 0 }, { 0, y, 0, 0 }, { 0, 0, z, 0 }, { 0, 0, 0, 1 }
    });

    public static Matrix4 Rotation(int axis, double degrees)
    {
        double a = degrees * Math.PI / 180;
        double c = Math.Cos(a), s = Math.Sin(a);
        return axis switch
        {
            0 => new(new double[,] { { 1, 0, 0, 0 }, { 0, c, -s, 0 }, { 0, s, c, 0 }, { 0, 0, 0, 1 } }),
            1 => new(new double[,] { { c, 0, s, 0 }, { 0, 1, 0, 0 }, { -s, 0, c, 0 }, { 0, 0, 0, 1 } }),
            2 => new(new double[,] { { c, -s, 0, 0 }, { s, c, 0, 0 }, { 0, 0, 1, 0 }, { 0, 0, 0, 1 } }),
            _ => throw new ArgumentException("Выберите ось X, Y или Z.")
        };
    }

    public static Matrix4 Around(Vec3 center, Matrix4 operation) =>
        Translation(center.X, center.Y, center.Z) * operation * Translation(-center.X, -center.Y, -center.Z);
}
