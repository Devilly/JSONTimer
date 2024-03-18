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

	private IList<Action> actions;

    public void Start()
    {
		var serializeOptions = new JsonSerializerOptions
		{
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase
		};

		var jsonField = GetNode<TextEdit>("%JSONField");	
		var activity = JsonSerializer.Deserialize<Activity>(jsonField.Text, serializeOptions);

		actions = new List<Action>();
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

		EmitSignal(SignalName.StartActivity);
    }

	public IList<Action> Actions => actions;

	public void Pause() {
		EmitSignal(SignalName.PauseActivity);
	}

	public void Resume() {
		EmitSignal(SignalName.ResumeActivity);
	}

	public void Stop() {
		EmitSignal(SignalName.StopActivity);
	}
}
