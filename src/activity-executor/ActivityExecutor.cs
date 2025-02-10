using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public partial class ActivityExecutor : Control
{
	[Export]
	public Color actColor;

	[Export]
	public Color restColor;

	[Export]
	public Color recuperateColor;

	private TextLabel mainText;
	private TextLabel superText;
	private TextLabel subText;

	private float tweenedValue;
	private Color activityColor;

	public override async void _Ready()
	{
		mainText = GetNode<TextLabel>("MainText");
		superText = GetNode<TextLabel>("SuperText");
		subText = GetNode<TextLabel>("SubText");

		mainText.Text = "3";
		await ToSignal(GetTree().CreateTimer(1, false), SceneTreeTimer.SignalName.Timeout);
		mainText.Text = "2";
		await ToSignal(GetTree().CreateTimer(1, false), SceneTreeTimer.SignalName.Timeout);
		mainText.Text = "1";
		await ToSignal(GetTree().CreateTimer(1, false), SceneTreeTimer.SignalName.Timeout);


		var specifications = UnpackSpecifications(GlobalVars.Activity);
		ExecuteSpecification(specifications, 0);
	}

	private static IList<Specification> UnpackSpecifications(Activity activity)
	{
		var specifications = new List<Specification>();
		foreach (Specification specification in activity.Specifications)
		{
			foreach (var index in Enumerable.Range(1, specification.Repetitions))
			{
				specifications.Add(new()
				{
					Name = specification.Name,
					Type = specification.Type switch
					{
						SpecificationType.Exercise => SpecificationType.Exercise,
						SpecificationType.Recuperate => SpecificationType.Recuperate
					},
					Duration = specification.Duration
				});

				if (index == specification.Repetitions)
				{
					continue;
				}

				specifications.Add(new()
				{
					Name = specification.RestName,
					Type = SpecificationType.Rest,
					Duration = specification.Rest
				});
			}
		}

		return specifications;
	}

	private async void ExecuteSpecification(IList<Specification> specifications, int executionIndex)
	{
		var specification = specifications[executionIndex];
		var nextSpecification = specifications.ElementAtOrDefault(executionIndex + 1);

		float fromAngle, toAngle;

		if (specification.Type == SpecificationType.Exercise)
		{
			superText.Text = specification.Name;
			mainText.Text = specification.Duration.ToString();
			subText.Text = nextSpecification.Name;

			fromAngle = 0f;
			toAngle = 2 * MathF.PI;

			activityColor = actColor;

			await PlayStartSound();

			PlanUpdateSound(specification, 2);
			PlanUpdateSound(specification, 1);
		}
		else
		{
			superText.Text = specification.Name;
			mainText.Text = specification.Duration.ToString();
			subText.Text = nextSpecification.Name;

			fromAngle = 2 * MathF.PI;
			toAngle = 0f;

			activityColor = specification.Type switch {
				SpecificationType.Rest => restColor,
				SpecificationType.Recuperate => recuperateColor
			};
		}		

		var currentTween = CreateTween();
		currentTween.TweenMethod(Callable.From((float value) =>
		{
			tweenedValue = value;
		}), fromAngle, toAngle, specification.Duration);

		currentTween.Finished += async () =>
		{
			if (executionIndex <= specifications.Count - 1)
			{
				if (specification.Type == SpecificationType.Exercise)
				{
					await PlayEndSound();
				}

				if (executionIndex + 1 < specifications.Count)
				{
					ExecuteSpecification(specifications, executionIndex + 1);
				} else {
					GetTree().Quit();
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

	private async void PlanUpdateSound(Specification specification, int reverseIndex)
	{
		var audio = GetNode<AudioStreamPlayer>("%UpdateAudio");
		var audioLength = audio.Stream.GetLength();

		await ToSignal(GetTree().CreateTimer(specification.Duration - (reverseIndex * (audioLength + .5) * 2), false), SceneTreeTimer.SignalName.Timeout);

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
		DrawArc(Vector2.Zero, 100, 0, tweenedValue, 360, activityColor, 15, true);
	}
}
