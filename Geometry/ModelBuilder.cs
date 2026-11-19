using Lab7.Core;

namespace Lab7.Geometry;

public static class ModelBuilder
{
    public static Mesh Cube()
    {
        var points = new List<Vec3>();
        foreach (int x in new[] { -1, 1 })
            foreach (int y in new[] { -1, 1 })
                foreach (int z in new[] { -1, 1 })
                    points.Add(new Vec3(x, y, z));
        var mesh = new Mesh("Куб", points,
        [
            new(2, 0, 1, 3), new(1, 0, 4, 5), new(4, 0, 2, 6),
            new(3, 1, 5, 7), new(6, 2, 3, 7), new(5, 4, 6, 7)
        ]);
        mesh.OrientClosedOutward();
        return mesh;
    }

    public static Mesh Revolution(IReadOnlyList<(double Radius, double Height)> profile, int axis, int segments)
    {
        if (axis is < 0 or > 2 || segments is < 3 or > 200 || profile.Count is < 2 or > 200)
            throw new ArgumentException("Нужно 2–200 точек, 3–200 разбиений и ось X, Y или Z.");
        if (profile.Any(p => !double.IsFinite(p.Radius) || !double.IsFinite(p.Height) ||
                             p.Radius < 0 || p.Radius > 1000 || Math.Abs(p.Height) > 1000))
            throw new ArgumentException("Радиус: 0…1000; координата вдоль оси: −1000…1000.");
        if (profile.All(p => p.Radius < 1e-9))
            throw new ArgumentException("Хотя бы один радиус должен быть положительным.");
        for (int i = 1; i < profile.Count; i++)
            if (profile[i] == profile[i - 1])
                throw new ArgumentException("Соседние точки образующей не должны совпадать.");

        var vertices = new List<Vec3>();
        var faces = new List<Face>();
        var rings = new int[profile.Count][];
        for (int j = 0; j < profile.Count; j++)
        {
            var (radius, height) = profile[j];
            int count = radius < 1e-9 ? 1 : segments;
            rings[j] = new int[count];
            for (int i = 0; i < count; i++)
            {
                double angle = 2 * Math.PI * i / segments;
                double u = radius * Math.Cos(angle), v = radius * Math.Sin(angle);
                rings[j][i] = vertices.Count;
                vertices.Add(axis switch
                {
                    0 => new Vec3(height, u, v),
                    1 => new Vec3(v, height, u),
                    _ => new Vec3(u, v, height)
                });
            }
        }

        for (int j = 0; j < rings.Length - 1; j++)
        {
            int[] a = rings[j], b = rings[j + 1];
            if (a.Length == 1 && b.Length == 1)
                continue;
            for (int i = 0; i < segments; i++)
            {
                int next = (i + 1) % segments;
                if (a.Length == 1)
                    faces.Add(new Face(a[0], b[next], b[i]));
                else if (b.Length == 1)
                    faces.Add(new Face(a[i], a[next], b[0]));
                else
                    faces.Add(new Face(a[i], a[next], b[next], b[i]));
            }
        }
        double orientation = 0;
        for (int j = 0; j < profile.Count - 1; j++)
        {
            var a = profile[j];
            var b = profile[j + 1];
            orientation += (b.Height - a.Height) *
                (a.Radius * a.Radius + a.Radius * b.Radius + b.Radius * b.Radius);
        }
        if (orientation < 0)
            foreach (Face face in faces)
                Array.Reverse(face.Indices);
        var mesh = new Mesh("Фигура вращения", vertices, faces);
        return mesh;
    }

    public static double Evaluate(int function, double x, double y) => function switch
    {
        0 => Math.Sin(x) * Math.Cos(y),
        1 => (x * x + y * y) / 4,
        2 => (x * x - y * y) / 4,
        3 => Math.Cos(Math.Sqrt(x * x + y * y)),
        _ => throw new ArgumentException("Выберите функцию.")
    };

    public static Mesh Surface(int function, double x0, double x1, double y0, double y1, int nx, int ny)
    {
        if (!double.IsFinite(x0) || !double.IsFinite(x1) || !double.IsFinite(y0) || !double.IsFinite(y1) ||
            x0 >= x1 || y0 >= y1 || Math.Max(Math.Abs(x0), Math.Abs(x1)) > 1000 ||
            Math.Max(Math.Abs(y0), Math.Abs(y1)) > 1000 || nx is < 1 or > 200 || ny is < 1 or > 200)
            throw new ArgumentException("Начало диапазона должно быть меньше конца; |координата| ≤ 1000; разбиения: 1–200.");
        var vertices = new List<Vec3>();
        var faces = new List<Face>();
        for (int j = 0; j <= ny; j++)
            for (int i = 0; i <= nx; i++)
            {
                double x = x0 + (x1 - x0) * i / nx;
                double y = y0 + (y1 - y0) * j / ny;
                vertices.Add(new Vec3(x, y, Evaluate(function, x, y)));
            }
        for (int j = 0; j < ny; j++)
            for (int i = 0; i < nx; i++)
            {
                int a = j * (nx + 1) + i, b = a + 1, d = a + nx + 1, c = d + 1;
                faces.Add(new Face(a, b, c));
                faces.Add(new Face(a, c, d));
            }
        return new Mesh("График z = f(x, y)", vertices, faces);
    }
}
