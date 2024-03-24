using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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

	private async void ExecuteAction(IList<Action> actions, int executionIndex) {
		var action = actions[executionIndex];

		float fromAngle, toAngle;

		if(action.Type == ActionType.Execute) {
			fromAngle = 0f;
			toAngle = 2 * MathF.PI;

			activityColor = executeColor;

			await PlayStartSound();

			PlanUpdateSound(action, 2);
			PlanUpdateSound(action, 1);
		} else {
			fromAngle = 2 * MathF.PI;
			toAngle = 0f;

			activityColor = restColor;
		}

		currentTween = CreateTween();
		currentTween.TweenMethod(Callable.From((float value) => {
			activityArc = value;
		}), fromAngle, toAngle, action.Duration);

		currentTween.Finished += async () => {
			if(executionIndex < actions.Count - 1) {
				if(action.Type == ActionType.Execute) {
					await PlayEndSound();
				}
				
				ExecuteAction(actions, executionIndex + 1);
			}
		};
	}

	private async Task PlayStartSound() {
		var audio = GetNode<AudioStreamPlayer>("%StartAudio");
		audio.Play();

		await ToSignal(audio, AudioStreamPlayer.SignalName.Finished);
	}

	private async void PlanUpdateSound(Action action, int reverseIndex) {
		var audio = GetNode<AudioStreamPlayer>("%UpdateAudio");
		var audioLength = audio.Stream.GetLength();

		await ToSignal(GetTree().CreateTimer(action.Duration - (reverseIndex * (audioLength + .5) * 2)), SceneTreeTimer.SignalName.Timeout);
		
		audio.Play();
	}

	private async Task PlayEndSound() {
		var audio = GetNode<AudioStreamPlayer>("%EndAudio");
		audio.Play();

		await ToSignal(audio, AudioStreamPlayer.SignalName.Finished);
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
