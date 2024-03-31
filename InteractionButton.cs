using Godot;
using System;
using System.Diagnostics;

public enum InteractionButtonState
{
	READY,
	STARTED,
	PAUSED
}

public partial class InteractionButton : TextureButton
{
	private Logic logic;

	private InteractionButtonState state;

	[Export]
	public Texture2D startImage;

	[Export]
	public Texture2D pauseImage;

	public override void _Ready()
	{
		logic = GetParent<Logic>();

		logic.StartActivity += OnStartActivity;
		logic.PauseActivity += OnPauseActivity;
		logic.ResumeActivity += OnResumeActivity;

		SetReadyState();
	}

	private void OnStartActivity()
	{
		SetStartedState();
	}

	private void OnPauseActivity()
	{
		SetPausedState();
	}

	private void OnResumeActivity()
	{
		SetStartedState();
	}

	private void SetReadyState()
	{
		state = InteractionButtonState.READY;
		TextureNormal = startImage;
	}

	private void SetStartedState()
	{
		state = InteractionButtonState.STARTED;
		TextureNormal = pauseImage;
	}

	private void SetPausedState()
	{
		state = InteractionButtonState.PAUSED;
		TextureNormal = startImage;
	}

	public void OnPressed()
	{
		if (state == InteractionButtonState.READY)
		{
			logic.Start();
		}
		else if (state == InteractionButtonState.STARTED)
		{
			logic.Pause();
		}
		else if (state == InteractionButtonState.PAUSED)
		{
			logic.Resume();
		}
	}
}
