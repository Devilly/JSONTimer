using Godot;

public partial class GlobalInitialisation : Node
{
    private int MagicDpiReferenceValue = 96;

    public override void _Ready()
    {
        var dpi = DisplayServer.ScreenGetDpi();
        var desiredScaling = (float)dpi / (float)MagicDpiReferenceValue;
        var currentWindowSize = DisplayServer.WindowGetSize();

        DisplayServer.WindowSetSize(new() {
            X = (int)(currentWindowSize.X * desiredScaling),
            Y =  (int)(currentWindowSize.Y * desiredScaling)
        });
    }
}