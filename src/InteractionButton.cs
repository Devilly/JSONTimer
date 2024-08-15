using Godot;

public enum InteractionButtonState
{
	READY,
	STARTED,
	PAUSED
}

public partial class InteractionButton : TextLabel
{
	private Logic logic;

	private InteractionButtonState state;

	public override void _Ready()
	{
		logic = GetParent<Button>().GetParent<Logic>();

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
		Text = "READY";
	}

	private void SetStartedState()
	{
		state = InteractionButtonState.STARTED;
		Text = "PAUSE";
	}

	private void SetPausedState()
	{
		state = InteractionButtonState.PAUSED;
		Text = "RESUME";
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
