namespace Simple3DEquipmentViewer;

public sealed class Equipment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = "Equipment";
    public string Type { get; set; } = "Tank";
    public string Status { get; set; } = "Running";
    public double X { get; set; }
    public double Y { get; set; }
    public double Z { get; set; }
    public override string ToString() => $"{Name} ({Status})";
}
