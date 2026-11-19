using System.Windows;
using System.Windows.Media;
using Lab7.Core;

namespace Lab7.Rendering;

public sealed class Renderer
{
    private readonly Matrix4 view = Matrix4.Rotation(0, 25) * Matrix4.Rotation(1, -35);
    private double centerX;
    private double centerY;
    private double spanX;
    private double spanY;

    public void SetModel(Mesh mesh)
    {
        Vec3[] points = mesh.Vertices.Select(view.Transform).ToArray();
        double minX = points.Min(p => p.X), maxX = points.Max(p => p.X);
        double minY = points.Min(p => p.Y), maxY = points.Max(p => p.Y);
        centerX = (minX + maxX) / 2;
        centerY = (minY + maxY) / 2;
        spanX = Math.Max(0.1, maxX - minX);
        spanY = Math.Max(0.1, maxY - minY);
    }

    public DrawingImage Render(Mesh mesh, double width, double height)
    {
        double scale = 0.85 * Math.Min(width / spanX, height / spanY);
        Point[] points = mesh.Vertices.Select(p =>
        {
            Vec3 v = view.Transform(p);
            return new Point(width / 2 + (v.X - centerX) * scale,
                height / 2 - (v.Y - centerY) * scale);
        }).ToArray();

        var drawing = new DrawingGroup();
        using (DrawingContext context = drawing.Open())
        {
            context.DrawRectangle(new SolidColorBrush(Color.FromRgb(32, 38, 48)), null,
                new Rect(0, 0, width, height));
            context.PushClip(new RectangleGeometry(new Rect(0, 0, width, height)));
            var pen = new Pen(new SolidColorBrush(Color.FromRgb(81, 169, 219)), 1);
            var edges = new HashSet<(int, int)>();
            foreach (Face face in mesh.Faces)
            {
                for (int i = 0; i < face.Indices.Length; i++)
                {
                    int a = face.Indices[i], b = face.Indices[(i + 1) % face.Indices.Length];
                    if (edges.Add((Math.Min(a, b), Math.Max(a, b))))
                        context.DrawLine(pen, points[a], points[b]);
                }
            }
            context.Pop();
        }
        drawing.Freeze();
        return new DrawingImage(drawing);
    }
}
