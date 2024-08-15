using Godot;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

	private Control urlConfiguration;
	private LineEdit urlInput;

	private IList<Action> actions;

    public void Start()
    {
		HttpRequest httpRequest = new HttpRequest();
		AddChild(httpRequest);

        httpRequest.RequestCompleted += (long result, long responseCode, string[] headers, byte[] body) => {
			var serializeOptions = new JsonSerializerOptions
			{
				PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
				ReadCommentHandling = JsonCommentHandling.Skip,
				Converters = {
					new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
				}
			};

			var json = Encoding.UTF8.GetString(body);
			var activity = JsonSerializer.Deserialize<Activity>(json, serializeOptions);

			urlConfiguration.QueueFree();
			ContinueStartupSequence(activity);

			httpRequest.QueueFree();
		};
		httpRequest.Request(urlInput.Text);
    }

    private void ContinueStartupSequence(Activity activity)
    {        
		actions = new List<Action>();
		foreach (Specification specification in activity.Specifications) {
			foreach (var index in Enumerable.Range(1, specification.Repetitions)) {
				actions.Add(new() {
					Name = specification.Name,
					Type = specification.Type switch
					{
						SpecificationType.Exercise => ActionType.Exercise,
						SpecificationType.Recuperate => ActionType.Recuperate
					},
					Duration = specification.Duration
				});
				
				if(index == specification.Repetitions) {
					continue;
				}

				actions.Add(new() {
					Name = specification.RestName,
					Type = ActionType.Rest,
					Duration = specification.Rest
				});
			}
		}

		EmitSignal(SignalName.StartActivity);
    }

    public IList<Action> Actions => actions;

	public void Pause() {
		GetTree().Paused = true;

		EmitSignal(SignalName.PauseActivity);
	}

	public void Resume() {
		GetTree().Paused = false;

		EmitSignal(SignalName.ResumeActivity);
	}

	public override void _Ready() {
		urlConfiguration = GetNode<Control>("UrlConfiguration");
		urlInput = urlConfiguration.GetNode<LineEdit>("UrlInput");

		var trainingUrl = OS.GetEnvironment("TRAINING_URL");
		if(trainingUrl.Length > 0) {
			urlInput.Text = trainingUrl;
		}
	}
}
