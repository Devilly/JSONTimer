using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

public partial class Logic : Control
{
	[Signal]
	public delegate void StartActivityEventHandler();

	[Signal]
	public delegate void PauseActivityEventHandler();

	[Signal]
	public delegate void ResumeActivityEventHandler();

	private Control config;
	private LineEdit configInput;

	private IList<Action> actions;

	public async Task Start()
	{
		var input = configInput.Text;
		var json = "";

		if (File.Exists(input))
		{
			using StreamReader reader = new(input);
			json = reader.ReadToEnd();
		}
		else if (Uri.TryCreate(input, UriKind.Absolute, out Uri? result)
			&& ((result.Scheme == Uri.UriSchemeHttp) || (result.Scheme == Uri.UriSchemeHttps)))
		{
			using System.Net.Http.HttpClient client = new();
            json = await client.GetStringAsync(input);
		}

		DeserializeJson(json);
	}

	private void DeserializeJson(string contents)
	{
		var serializeOptions = new JsonSerializerOptions
		{
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
			ReadCommentHandling = JsonCommentHandling.Skip,
			Converters = {
					new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
				}
		};

		var activity = JsonSerializer.Deserialize<Activity>(contents, serializeOptions);

		config.QueueFree();
		ExecuteJson(activity);
	}

	private void ExecuteJson(Activity activity)
	{
		actions = new List<Action>();
		foreach (Specification specification in activity.Specifications)
		{
			foreach (var index in Enumerable.Range(1, specification.Repetitions))
			{
				actions.Add(new()
				{
					Name = specification.Name,
					Type = specification.Type switch
					{
						SpecificationType.Exercise => ActionType.Exercise,
						SpecificationType.Recuperate => ActionType.Recuperate
					},
					Duration = specification.Duration
				});

				if (index == specification.Repetitions)
				{
					continue;
				}

				actions.Add(new()
				{
					Name = specification.RestName,
					Type = ActionType.Rest,
					Duration = specification.Rest
				});
			}
		}

		EmitSignal(SignalName.StartActivity);
	}

	public IList<Action> Actions => actions;

	public void Pause()
	{
		GetTree().Paused = true;

		EmitSignal(SignalName.PauseActivity);
	}

	public void Resume()
	{
		GetTree().Paused = false;

		EmitSignal(SignalName.ResumeActivity);
	}

	public override void _Ready()
	{
		config = GetNode<Control>("Config");
		configInput = config.GetNode<LineEdit>("ConfigInput");

		var configArgs = OS.GetCmdlineArgs();
		if (configArgs.Length > 0)
		{
			configInput.Text = configArgs[0];
		}
	}
}
