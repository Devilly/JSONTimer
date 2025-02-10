using Godot;

public partial class GlobalVars : Node
{
    public static GlobalVars Instance { get; private set; }

    public static Activity Activity { get; set; }

    public override void _Ready()
    {
        Instance = this;
    }
}