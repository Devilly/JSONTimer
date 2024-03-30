using Godot;
using System;

public partial class InteractionButton : TextureButton
{
	private Logic logic;

	public override void _Ready()
	{
		logic = GetParent<Logic>();
	}
	public void OnPressed()
	{
		logic.Start();

		Disabled = true;
	}
}
