using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Lab7.Core;
using Lab7.Geometry;
using Lab7.IO;
using Lab7.Rendering;
using Microsoft.Win32;

namespace Lab7;

public partial class MainWindow : Window
{
    private readonly List<Mesh> scene = [];
    private readonly Renderer renderer = new();
    private bool ready;

    public MainWindow()
    {
        InitializeComponent();
        Loaded += (_, _) =>
        {
            ready = true;
            ReplaceScene(ModelBuilder.Cube());
        };
    }

    private static double Number(TextBox box)
    {
        if (!double.TryParse(box.Text.Trim().Replace(',', '.'), NumberStyles.Float,
                CultureInfo.InvariantCulture, out double value) || !double.IsFinite(value))
            throw new ArgumentException($"Некорректное число: «{box.Text}».");
        return value;
    }

    private static int Integer(TextBox box)
    {
        if (!int.TryParse(box.Text.Trim(), out int value))
            throw new ArgumentException($"Нужно целое число: «{box.Text}».");
        return value;
    }

    private void Run(Action action)
    {
        try
        {
            action();
        }
        catch (Exception ex) when (ex is ArgumentException or FormatException or IOException or UnauthorizedAccessException)
        {
            MessageBox.Show(this, ex.Message, "Проверьте данные", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private Mesh SelectedMesh => ObjectList.SelectedItem as Mesh
        ?? throw new ArgumentException("Выберите объект.");

    private void ReplaceScene(params Mesh[] meshes)
    {
        scene.Clear();
        scene.AddRange(meshes);
        ObjectList.ItemsSource = null;
        ObjectList.ItemsSource = scene;
        ObjectList.SelectedIndex = 0;
        renderer.SetModel(SelectedMesh);
        Render();
    }

    private void Render()
    {
        if (!ready || scene.Count == 0 || Viewport.ActualWidth < 1 || Viewport.ActualHeight < 1)
            return;
        Mesh mesh = SelectedMesh;
        SceneImage.Source = renderer.Render(mesh, Viewport.ActualWidth, Viewport.ActualHeight);
        Status.Text = $"{mesh.Name} | Вершин: {mesh.Vertices.Count} | Граней: {mesh.Faces.Count}";
    }

    private void LoadObj_Click(object sender, RoutedEventArgs e) => Run(() =>
    {
        var dialog = new OpenFileDialog { Filter = "Wavefront OBJ (*.obj)|*.obj" };
        if (dialog.ShowDialog(this) == true)
            ReplaceScene(ObjFile.Load(dialog.FileName));
    });

    private void SaveObj_Click(object sender, RoutedEventArgs e) => Run(() =>
    {
        Mesh mesh = SelectedMesh;
        var dialog = new SaveFileDialog { Filter = "Wavefront OBJ (*.obj)|*.obj", FileName = "model.obj" };
        if (dialog.ShowDialog(this) == true)
        {
            ObjFile.Save(dialog.FileName, mesh);
            Status.Text = $"Сохранено: {dialog.FileName}";
        }
    });

    private void Cube_Click(object sender, RoutedEventArgs e) => Run(() => ReplaceScene(ModelBuilder.Cube()));

    private void Revolution_Click(object sender, RoutedEventArgs e) => Run(() =>
    {
        var profile = new List<(double, double)>();
        foreach (string line in ProfileInput.Text.Split('\n', StringSplitOptions.RemoveEmptyEntries))
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;
            string[] parts = line.Trim().Replace(',', '.').Split([';', ' ', '\t'], StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 2 || !double.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out double r) ||
                !double.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out double h))
                throw new ArgumentException("Каждая строка образующей: радиус; координата вдоль оси.");
            profile.Add((r, h));
        }
        ReplaceScene(ModelBuilder.Revolution(profile, RevolutionAxis.SelectedIndex, Integer(RevolutionSegments)));
    });

    private void Surface_Click(object sender, RoutedEventArgs e) => Run(() =>
    {
        Mesh mesh = ModelBuilder.Surface(FunctionList.SelectedIndex, Number(X0), Number(X1),
            Number(Y0), Number(Y1), Integer(StepsX), Integer(StepsY));
        ReplaceScene(mesh);
    });

    private void TransformSelected(Matrix4 operation)
    {
        SelectedMesh.Transform(operation);
        Render();
    }

    private void Move_Click(object sender, RoutedEventArgs e) => Run(() =>
        TransformSelected(Matrix4.Translation(Number(MoveX), Number(MoveY), Number(MoveZ))));

    private void Scale_Click(object sender, RoutedEventArgs e) => Run(() =>
    {
        double x = Number(ScaleX), y = Number(ScaleY), z = Number(ScaleZ);
        if (new[] { x, y, z }.Any(k => Math.Abs(k) < 0.001 || Math.Abs(k) > 1000))
            throw new ArgumentException("Модуль каждого коэффициента масштаба: 0.001…1000. Нулевой масштаб вырождает грани.");
        TransformSelected(Matrix4.Around(SelectedMesh.Center, Matrix4.Scaling(x, y, z)));
    });

    private void Rotate_Click(object sender, RoutedEventArgs e) => Run(() =>
        TransformSelected(Matrix4.Around(SelectedMesh.Center, Matrix4.Rotation(RotationAxis.SelectedIndex, Number(RotationAngle)))));

    private void Reflect_Click(object sender, RoutedEventArgs e) => Run(() =>
    {
        int plane = int.Parse((string)((Button)sender).Tag, CultureInfo.InvariantCulture);
        TransformSelected(plane switch
        {
            0 => Matrix4.Scaling(1, 1, -1),
            1 => Matrix4.Scaling(1, -1, 1),
            _ => Matrix4.Scaling(-1, 1, 1)
        });
    });

    private void Viewport_SizeChanged(object sender, SizeChangedEventArgs e) => Run(Render);
}
