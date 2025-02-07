using Godot;
using System;

public partial class GlobalInitialisation : Node
{
    private int MagicDpiReferenceValue = 60;

    public static GlobalInitialisation Instance { get; private set; }

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