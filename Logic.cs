using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

public readonly struct Activity {
	public string Name { get; init; }
	public IList<Specification> Specifications { get; init; }
}

public readonly struct Specification {
	public int Repetitions { get; init; }
	public int Duration { get; init; }
	public int Rest { get; init; }
}

public enum ActionType {
	Execute,
	Rest
}

public struct Action {
	public ActionType Type;
	public int Duration;
}

public partial class Logic : Control
{
	[Signal]
	public delegate void StartActivityEventHandler();

	[Signal]
	public delegate void PauseActivityEventHandler();

	[Signal]
	public delegate void ResumeActivityEventHandler();

	[Signal]
	public delegate void StopActivityEventHandler();

	[Export]
	public Color executeColor;

	[Export]
	public Color restColor;

	private float activityArc = 0;
	private Color activityColor;

	private Tween currentTween;

    public void Start()
    {
		EmitSignal(SignalName.StartActivity);

		var serializeOptions = new JsonSerializerOptions
		{
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase
		};

		var jsonField = GetNode<TextEdit>("%JSONField");	
		var activity = JsonSerializer.Deserialize<Activity>(jsonField.Text, serializeOptions);

		var actions = new List<Action>();
		foreach (Specification specification in activity.Specifications) {
			foreach (var index in Enumerable.Range(1, specification.Repetitions)) {
				actions.Add(new() {
					Type = ActionType.Execute,
					Duration = specification.Duration
				});
				
				if(index == specification.Repetitions) {
					continue;
				}

				actions.Add(new() {
					Type = ActionType.Rest,
					Duration = specification.Rest
				});
			}
		}

		ExecuteAction(actions, 0);
    }

	public void Pause() {
		EmitSignal(SignalName.PauseActivity);

		currentTween.Pause();
	}

	public void Resume() {
		EmitSignal(SignalName.ResumeActivity);

		currentTween.Play();
	}

	public void Stop() {
		EmitSignal(SignalName.StopActivity);

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
			}
		};
	}

    public override void _Process(double delta)
	{
		QueueRedraw();
	}

	public override void _Draw()
    {
		DrawArc(new(800, 300), 100, 0, activityArc, 360, activityColor, 10, true);
    }
}
