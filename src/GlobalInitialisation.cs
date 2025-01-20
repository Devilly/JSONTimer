using Godot;
using System;

public partial class GlobalInitialisation : Node
{
	private int MagicDpiReferenceValue = 96;

	public static GlobalInitialisation Instance { get; private set; }

    public override void _Ready()
    {
        var dpi = DisplayServer.ScreenGetDpi();
		DisplayServer.WindowSetSize(DisplayServer.WindowGetSize() * (int) Math.Ceiling((float) dpi / (float) MagicDpiReferenceValue));
    }
}