using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

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
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
			ReadCommentHandling = JsonCommentHandling.Skip,
			Converters = {
				new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
			}
		};

		using var file = FileAccess.Open("training.json", FileAccess.ModeFlags.Read);
    	string json = file.GetAsText();

		var activity = JsonSerializer.Deserialize<Activity>(json, serializeOptions);

		actions = new List<Action>();
		foreach (Specification specification in activity.Specifications) {
			foreach (var index in Enumerable.Range(1, specification.Repetitions)) {
				actions.Add(new() {
					Name = specification.Name,
					Type = specification.Type switch
					{
						SpecificationType.Exercise => ActionType.Exercise,
						SpecificationType.Rest => ActionType.Rest
					},
					Duration = specification.Duration
				});
				
				if(index == specification.Repetitions) {
					continue;
				}

				actions.Add(new() {
					Name = specification.Name,
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
