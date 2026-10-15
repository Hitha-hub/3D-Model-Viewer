using Microsoft.Win32;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Simple3DEquipmentViewer;

public partial class MainWindow : Window
{
    private readonly List<Equipment> _equipment = new();
    private readonly Dictionary<GeometryModel3D, Equipment> _modelToEquipment = new();
    private bool _updatingControls;
    private Equipment? SelectedEquipment => EquipmentList.SelectedItem as Equipment;

    public MainWindow()
    {
        InitializeComponent();
        _equipment.Add(new Equipment { Name = "Tank-101", Type = "Tank", X = -3, Status = "Running" });
        _equipment.Add(new Equipment { Name = "Pump-201", Type = "Box", X = 0, Status = "Warning" });
        _equipment.Add(new Equipment { Name = "Tank-102", Type = "Tank", X = 3, Status = "Stopped" });
        RefreshScene();
        EquipmentList.SelectedIndex = 0;
        FooterText.Text = "Loaded sample equipment.";
    }

    private void AddTank_Click(object sender, RoutedEventArgs e) => AddEquipment("Tank");
    private void AddBox_Click(object sender, RoutedEventArgs e) => AddEquipment("Box");

    private void AddEquipment(string type)
    {
        var item = new Equipment {
            Name = $"{type}-{_equipment.Count + 1:000}", Type = type,
            X = _equipment.Count * 1.5 - 2, Status = "Running"
        };
        _equipment.Add(item);
        RefreshScene();
        EquipmentList.SelectedItem = item;
        FooterText.Text = $"Added {item.Name}.";
    }

    private void Remove_Click(object sender, RoutedEventArgs e)
    {
        var selected = SelectedEquipment;
        if (selected is null) { MessageBox.Show("Select equipment to remove."); return; }
        _equipment.Remove(selected);
        RefreshScene();
        FooterText.Text = $"Removed {selected.Name}.";
    }

    private void ApplyChanges_Click(object sender, RoutedEventArgs e)
    {
        var selected = SelectedEquipment;
        if (selected is null) { MessageBox.Show("Select equipment first."); return; }

        string newName = NameBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(newName)) {
            MessageBox.Show("Equipment name cannot be empty.", "Validation",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        if (_equipment.Any(x => x.Id != selected.Id &&
            string.Equals(x.Name, newName, StringComparison.OrdinalIgnoreCase))) {
            MessageBox.Show("An equipment item with this name already exists.", "Validation",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        selected.Name = newName;
        if (StatusCombo.SelectedItem is ComboBoxItem item && item.Content is string status)
            selected.Status = status;

        RefreshScene();
        EquipmentList.SelectedItem = selected;
        FooterText.Text = $"Updated {selected.Name}.";
    }

    private void EquipmentList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var selected = SelectedEquipment;
        _updatingControls = true;
        NameBox.Text = selected?.Name ?? "";
        TypeBox.Text = selected?.Type ?? "";
        StatusCombo.SelectedIndex = selected?.Status switch {
            "Running" => 0, "Warning" => 1, "Stopped" => 2, _ => -1
        };
        _updatingControls = false;
    }

    private void StatusCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        // Changes are applied when the user clicks Apply Changes.
        if (_updatingControls) return;
    }

    private void RefreshScene()
    {
        EquipmentList.ItemsSource = null;
        EquipmentList.ItemsSource = _equipment;

        for (int i = SceneViewport.Children.Count - 1; i >= 0; i--)
        {
            if (SceneViewport.Children[i] is ModelVisual3D visual &&
                visual.Content is GeometryModel3D model && _modelToEquipment.ContainsKey(model))
                SceneViewport.Children.RemoveAt(i);
        }

        _modelToEquipment.Clear();
        foreach (var item in _equipment)
        {
            GeometryModel3D model = item.Type.Equals("Tank", StringComparison.OrdinalIgnoreCase)
                ? CreateCylinder(item) : CreateBox(item);
            SceneViewport.Children.Add(new ModelVisual3D { Content = model });
            _modelToEquipment[model] = item;
        }
    }

    private static GeometryModel3D CreateBox(Equipment item)
    {
        const double half = 0.65, height = 1.3;
        var mesh = new MeshGeometry3D {
            Positions = new Point3DCollection {
                new(item.X-half,item.Y,item.Z-half), new(item.X+half,item.Y,item.Z-half),
                new(item.X+half,item.Y+height,item.Z-half), new(item.X-half,item.Y+height,item.Z-half),
                new(item.X-half,item.Y,item.Z+half), new(item.X+half,item.Y,item.Z+half),
                new(item.X+half,item.Y+height,item.Z+half), new(item.X-half,item.Y+height,item.Z+half)
            },
            TriangleIndices = new Int32Collection {
                0,2,1, 0,3,2, 4,5,6, 4,6,7, 0,1,5, 0,5,4,
                3,7,6, 3,6,2, 1,2,6, 1,6,5, 0,4,7, 0,7,3
            }
        };
        return MakeModel(item, mesh);
    }

    private static GeometryModel3D CreateCylinder(Equipment item)
    {
        const int segments = 32;
        const double radius = 0.55, height = 1.5;
        var positions = new Point3DCollection();
        var triangles = new Int32Collection();
        for (int i = 0; i < segments; i++) {
            double angle = 2 * Math.PI * i / segments;
            double x = item.X + radius * Math.Cos(angle);
            double z = item.Z + radius * Math.Sin(angle);
            positions.Add(new Point3D(x, item.Y, z));
            positions.Add(new Point3D(x, item.Y + height, z));
        }
        int bottomCenter = positions.Count;
        positions.Add(new Point3D(item.X, item.Y, item.Z));
        int topCenter = positions.Count;
        positions.Add(new Point3D(item.X, item.Y + height, item.Z));

        for (int i = 0; i < segments; i++) {
            int next = (i + 1) % segments, b0 = i * 2, t0 = b0 + 1, b1 = next * 2, t1 = b1 + 1;
            triangles.Add(b0); triangles.Add(t0); triangles.Add(t1);
            triangles.Add(b0); triangles.Add(t1); triangles.Add(b1);
            triangles.Add(bottomCenter); triangles.Add(b1); triangles.Add(b0);
            triangles.Add(topCenter); triangles.Add(t0); triangles.Add(t1);
        }
        return MakeModel(item, new MeshGeometry3D { Positions = positions, TriangleIndices = triangles });
    }

    private static GeometryModel3D MakeModel(Equipment item, MeshGeometry3D mesh)
    {
        Color color = item.Status switch {
            "Warning" => Colors.Orange, "Stopped" => Colors.IndianRed, _ => Colors.SteelBlue
        };
        var material = new DiffuseMaterial(new SolidColorBrush(color));
        return new GeometryModel3D(mesh, material) { BackMaterial = material };
    }

    private void SceneViewport_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton != MouseButton.Left) return;
        var point = e.GetPosition(SceneViewport.Viewport);
        var hit = VisualTreeHelper.HitTest(SceneViewport.Viewport, point);
        if (hit is RayMeshGeometry3DHitTestResult rayHit &&
            rayHit.ModelHit is GeometryModel3D model &&
            _modelToEquipment.TryGetValue(model, out var item)) {
            EquipmentList.SelectedItem = item;
            FooterText.Text = $"Selected {item.Name}.";
        }
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new SaveFileDialog { Filter = "JSON files (*.json)|*.json", FileName = "equipment-scene.json" };
        if (dialog.ShowDialog() != true) return;
        try {
            File.WriteAllText(dialog.FileName,
                JsonSerializer.Serialize(_equipment, new JsonSerializerOptions { WriteIndented = true }));
            FooterText.Text = $"Saved {_equipment.Count} equipment item(s).";
        } catch (Exception ex) {
            MessageBox.Show($"Could not save the scene.\n{ex.Message}", "Save error",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void Load_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog { Filter = "JSON files (*.json)|*.json" };
        if (dialog.ShowDialog() != true) return;
        try {
            var loaded = JsonSerializer.Deserialize<List<Equipment>>(File.ReadAllText(dialog.FileName));
            if (loaded is null) throw new InvalidDataException("The file does not contain an equipment list.");
            string[] validTypes = { "Tank", "Box" };
            string[] validStatuses = { "Running", "Warning", "Stopped" };
            if (loaded.Any(x => string.IsNullOrWhiteSpace(x.Name) ||
                !validTypes.Contains(x.Type, StringComparer.OrdinalIgnoreCase) ||
                !validStatuses.Contains(x.Status))) {
                throw new InvalidDataException("The file contains invalid equipment data.");
            }
            _equipment.Clear();
            _equipment.AddRange(loaded);
            RefreshScene();
            EquipmentList.SelectedIndex = _equipment.Count > 0 ? 0 : -1;
            FooterText.Text = $"Loaded {_equipment.Count} equipment item(s).";
        } catch (Exception ex) {
            MessageBox.Show($"Could not load the scene.\n{ex.Message}", "Load error",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
