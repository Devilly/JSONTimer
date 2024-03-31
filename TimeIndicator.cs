using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public partial class TimeIndicator : Control
{

	[Export]
	public Color executeColor;

	[Export]
	public Color restColor;

	[Export]
	public Color recuperateColor;

	private float tweenValue;
	private Color activityColor;

	private Tween currentTween;

	private Logic logic;

	private TextLabel mainText;
	private TextLabel superText;
	private TextLabel subText;

	public override void _Ready()
	{
		logic = GetParent<Logic>();
		mainText = GetNode<TextLabel>("MainText");
		superText = GetNode<TextLabel>("SuperText");
		subText = GetNode<TextLabel>("SubText");

		logic.StartActivity += OnStartActivity;
	}

	public async void OnStartActivity()
	{
		mainText.Text = "3";
		await ToSignal(GetTree().CreateTimer(1, false), SceneTreeTimer.SignalName.Timeout);
		mainText.Text = "2";
		await ToSignal(GetTree().CreateTimer(1, false), SceneTreeTimer.SignalName.Timeout);
		mainText.Text = "1";
		await ToSignal(GetTree().CreateTimer(1, false), SceneTreeTimer.SignalName.Timeout);

		ExecuteAction(logic.Actions, 0);
	}

	private void Reset()
	{
		superText.Text = "";
		mainText.Text = "";
		subText.Text = "";

		tweenValue = 0f;
		currentTween.Kill();
	}

	private async void ExecuteAction(IList<Action> actions, int executionIndex)
	{
		var action = actions[executionIndex];
		var nextAction = actions.ElementAtOrDefault(executionIndex + 1);

		float fromAngle, toAngle;

		if (action.Type == ActionType.Exercise)
		{
			superText.Text = action.Name;
			mainText.Text = action.Duration.ToString();
			subText.Text = nextAction.Name;

			fromAngle = 0f;
			toAngle = 2 * MathF.PI;

			activityColor = executeColor;

			await PlayStartSound();

			PlanUpdateSound(action, 2);
			PlanUpdateSound(action, 1);
		}
		else
		{
			superText.Text = action.Name;
			mainText.Text = action.Duration.ToString();
			subText.Text = nextAction.Name;

			fromAngle = 2 * MathF.PI;
			toAngle = 0f;

			activityColor = action.Type switch {
				ActionType.Rest => restColor,
				ActionType.Recuperate => recuperateColor
			};
		}		

		currentTween = CreateTween();
		currentTween.TweenMethod(Callable.From((float value) =>
		{
			tweenValue = value;
		}), fromAngle, toAngle, action.Duration);

		currentTween.Finished += async () =>
		{
			if (executionIndex <= actions.Count - 1)
			{
				if (action.Type == ActionType.Exercise)
				{
					await PlayEndSound();
				}

				if (executionIndex + 1 < actions.Count)
				{
					ExecuteAction(actions, executionIndex + 1);
				} else {
					Reset();
				}
			}
		};
	}

	private async Task PlayStartSound()
	{
		var audio = GetNode<AudioStreamPlayer>("%StartAudio");
		audio.Play();

		await ToSignal(audio, AudioStreamPlayer.SignalName.Finished);
	}

	private async void PlanUpdateSound(Action action, int reverseIndex)
	{
		var audio = GetNode<AudioStreamPlayer>("%UpdateAudio");
		var audioLength = audio.Stream.GetLength();

		await ToSignal(GetTree().CreateTimer(action.Duration - (reverseIndex * (audioLength + .5) * 2), false), SceneTreeTimer.SignalName.Timeout);

		audio.Play();
	}

	private async Task PlayEndSound()
	{
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
		DrawArc(Vector2.Zero, 100, 0, tweenValue, 360, activityColor, 15, true);
	}
}
