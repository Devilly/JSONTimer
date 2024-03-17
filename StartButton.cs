using Godot;
using System;

public enum StartButtonState
{
	INITIALIZED,
	STARTED,
	PAUSED,
	RESUMED
}

public partial class StartButton : Button
{
	[Export]
	public string startText;

	[Export]
	public string pauseText;

	[Export]
	public string continueText;

	private StartButtonState state;

	private Logic logic;

	public override void _Ready()
	{
		Text = startText;

		logic = GetParent<Logic>();

		logic.StartActivity += OnStartActivity;
		logic.PauseActivity += OnPauseActivity;
		logic.ResumeActivity += OnResumeActivity;
		logic.StopActivity += OnStopActivity;
	}

	public void OnPressed()
	{
		if (state == StartButtonState.INITIALIZED)
		{
			logic.Start();
		}
		else if (state == StartButtonState.STARTED || state == StartButtonState.RESUMED)
		{
			logic.Pause();
		}
		else if (state == StartButtonState.PAUSED)
		{
			logic.Resume();
		}
	}

	public void OnStartActivity()
	{
		state = StartButtonState.STARTED;
		Text = pauseText;
	}

	public void OnPauseActivity()
	{
		state = StartButtonState.PAUSED;
		Text = continueText;
	}

	public void OnResumeActivity()
	{
		state = StartButtonState.RESUMED;
		Text = pauseText;
	}

	public void OnStopActivity()
	{
		state = StartButtonState.INITIALIZED;
		Text = startText;
	}
}
