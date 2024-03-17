using Godot;
using System;

public enum StopButtonState
{
	INITIALIZED,
	DISABLED
}

public partial class StopButton : Button
{
	private Logic logic;

	public override void _Ready()
    {
		logic = GetParent<Logic>();

        logic.StartActivity += OnStartActivity;
		logic.PauseActivity += OnPauseActivity;
		logic.ResumeActivity += OnResumeActivity;
		logic.StopActivity += OnStopActivity;
    }
	
	public void OnPressed() {
		logic.Stop();
	}
	
	public void OnStartActivity() {
		Disabled = false;
	}

	public void OnPauseActivity() {
		Disabled = false;
	}

	public void OnResumeActivity() {
		Disabled = false;
	}

	public void OnStopActivity() {
		Disabled = true;
	}
}
