using Godot;
using System;
using System.Collections.Generic;

public partial class TimeIndicator : Control
{
	
	[Export]
	public Color executeColor;

	[Export]
	public Color restColor;

	private float activityArc;
	private Color activityColor;

	private Tween currentTween;

	private Logic logic;

	public override void _Ready()
	{
		logic = GetParent<Logic>();

		logic.StartActivity += OnStartActivity;
		logic.PauseActivity += OnPauseActivity;
		logic.ResumeActivity += OnResumeActivity;
		logic.StopActivity += OnStopActivity;
	}

	public void OnStartActivity()
	{
		ExecuteAction(logic.Actions, 0);
	}

	public void OnPauseActivity()
	{
		currentTween.Pause();
	}

	public void OnResumeActivity()
	{
		currentTween.Play();
	}

	public void OnStopActivity()
	{
		activityArc = 0f;
		currentTween.Kill();
	}

	private void ExecuteAction(IList<Action> actions, int executionIndex) {
		var action = actions[executionIndex];

		float fromAngle, toAngle;

		if(action.Type == ActionType.Execute) {
			fromAngle = 0f;
			toAngle = 2 * MathF.PI;

			activityColor = executeColor;
		} else {
			fromAngle = 2 * MathF.PI;
			toAngle = 0f;

			activityColor = restColor;
		}

		currentTween = CreateTween();
		currentTween.TweenMethod(Callable.From((float value) => {
			activityArc = value;
		}), fromAngle, toAngle, action.Duration);

		currentTween.Finished += () => {
			if(executionIndex < actions.Count - 1) {
				ExecuteAction(actions, executionIndex + 1);
				
				var notificationAudio = GetNode<AudioStreamPlayer>("%NotificationAudio");
				notificationAudio.Play();
			}
		};
	}
	
	public override void _Process(double delta)
	{
		QueueRedraw();
	}

	public override void _Draw()
	{
		DrawArc(Vector2.Zero, 100, 0, activityArc, 360, activityColor, 15, true);
	}
}