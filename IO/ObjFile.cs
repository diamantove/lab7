using System.Globalization;
using System.IO;
using System.Text;
using Lab7.Core;

namespace Lab7.IO;

public static class ObjFile
{
    public static Mesh Load(string path)
    {
        var vertices = new List<Vec3>();
        var faces = new List<Face>();
        int lineNumber = 0;
        foreach (string source in File.ReadLines(path))
        {
            lineNumber++;
            string[] parts = source.Split('#')[0].Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0)
                continue;
            try
            {
                if (parts[0] == "v")
                {
                    if (parts.Length < 4)
                        throw new FormatException("Для v нужны x, y, z.");
                    double Number(int i) => double.Parse(parts[i], CultureInfo.InvariantCulture);
                    double w = parts.Length == 5 ? Number(4) : 1;
                    if (!double.IsFinite(w) || Math.Abs(w) < 1e-12)
                        throw new FormatException("Некорректная координата w.");
                    var p = new Vec3(Number(1) / w, Number(2) / w, Number(3) / w);
                    if (!p.IsFinite || Math.Max(Math.Abs(p.X), Math.Max(Math.Abs(p.Y), Math.Abs(p.Z))) > 1e6)
                        throw new FormatException("Некорректные или слишком большие координаты.");
                    vertices.Add(p);
                }
                else if (parts[0] == "f")
                {
                    int Index(string token)
                    {
                        int n = int.Parse(token.Split('/')[0], CultureInfo.InvariantCulture);
                        int index = n > 0 ? n - 1 : vertices.Count + n;
                        if (n == 0 || index < 0 || index >= vertices.Count)
                            throw new FormatException("Индекс вершины вне диапазона.");
                        return index;
                    }
                    int[] indices = parts.Skip(1).Select(Index).ToArray();
                    if (indices.Length > 3 && indices[0] == indices[^1])
                        indices = indices[..^1];
                    if (indices.Distinct().Count() != indices.Length)
                        throw new FormatException("Повторяющиеся вершины грани.");
                    faces.Add(new Face(indices));
                }
                if (vertices.Count > 100000 || faces.Count > 200000)
                    throw new FormatException("Модель слишком велика: максимум 100000 вершин и 200000 граней.");
            }
            catch (Exception ex) when (ex is FormatException or OverflowException or ArgumentException)
            {
                throw new FormatException($"OBJ, строка {lineNumber}: {ex.Message}", ex);
            }
        }
        return new Mesh(Path.GetFileNameWithoutExtension(path), vertices, faces);
    }

    public static void Save(string path, Mesh mesh)
    {
        using var writer = new StreamWriter(path, false, new UTF8Encoding(false));
        writer.WriteLine("# Lab7: vertices and outward-wound faces");
        writer.WriteLine($"o {mesh.Name.Replace(' ', '_')}");
        foreach (Vec3 p in mesh.Vertices)
            writer.WriteLine(FormattableString.Invariant($"v {p.X:R} {p.Y:R} {p.Z:R}"));
        foreach (Face face in mesh.Faces)
            writer.WriteLine("f " + string.Join(" ", face.Indices.Select(i => i + 1)));
    }
}
