namespace Lab7.Core;

public sealed class Face
{
    public int[] Indices { get; }

    public Face(params int[] indices)
    {
        if (indices.Length < 3)
            throw new ArgumentException("Грань должна содержать хотя бы три вершины.");
        Indices = (int[])indices.Clone();
    }

}

public sealed class Mesh
{
    public string Name { get; }
    public List<Vec3> Vertices { get; }
    public List<Face> Faces { get; }
    public Vec3 Center => Vertices.Aggregate(new Vec3(), (sum, p) => sum + p) * (1.0 / Vertices.Count);

    public Mesh(string name, IEnumerable<Vec3> vertices, IEnumerable<Face> faces)
    {
        Name = name;
        Vertices = vertices.ToList();
        Faces = faces.ToList();
        if (Vertices.Count == 0 || Faces.Count == 0 || Vertices.Any(p => !p.IsFinite))
            throw new ArgumentException("Модель должна содержать конечные вершины и грани.");
        if (Faces.Any(f => f.Indices.Any(i => i < 0 || i >= Vertices.Count)))
            throw new ArgumentException("Индекс грани выходит за список вершин.");
    }

    public void Transform(Matrix4 matrix)
    {
        Vec3[] points = Vertices.Select(matrix.Transform).ToArray();
        if (points.Any(p => !p.IsFinite || Math.Max(Math.Abs(p.X), Math.Max(Math.Abs(p.Y), Math.Abs(p.Z))) > 1e6))
            throw new ArgumentException("Слишком большие координаты модели (предел 10⁶).");
        for (int i = 0; i < points.Length; i++)
            Vertices[i] = points[i];
        if (matrix.LinearDeterminant < 0)
            foreach (Face face in Faces)
                Array.Reverse(face.Indices);
    }

    public void OrientClosedOutward()
    {
        double volume = 0;
        foreach (Face face in Faces)
            for (int i = 1; i < face.Indices.Length - 1; i++)
                volume += Vec3.Dot(Vertices[face.Indices[0]],
                    Vec3.Cross(Vertices[face.Indices[i]], Vertices[face.Indices[i + 1]]));
        if (volume < 0)
            foreach (Face face in Faces)
                Array.Reverse(face.Indices);
    }
}
