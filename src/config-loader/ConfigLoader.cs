using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Godot;

public partial class ConfigLoader : Control
{
	private InputState state;

	private Control config;
	private LineEdit configInput;
	private Button button;

	public override void _Ready()
	{
		config = GetNode<Control>("Config");
		configInput = config.GetNode<LineEdit>("ConfigInput");
		button = GetNode<Button>("Button");

		var configArgs = OS.GetCmdlineArgs();
		if (configArgs.Length > 0)
		{
			configInput.Text = configArgs[0];
		}

		state = configInput.Text.Length == 0 ? new InputIncomplete(this).Enter() : new InputComplete(this).Enter();
	}

	public void HandleTextChange()
	{
		state = state.HandleTextChange().Enter();
	}

	public async void HandleButtonClick()
	{
		state = (await state.HandleButtonClick()).Enter();
	}

	public abstract class InputState
	{
		private protected ConfigLoader screen;

		public InputState(ConfigLoader screen)
		{
			this.screen = screen;
		}

		public abstract InputState Enter();
		public abstract InputState HandleTextChange();
		public abstract Task<InputState> HandleButtonClick();
	}

	public class InputIncomplete : InputState
	{
		public InputIncomplete(ConfigLoader screen) : base(screen) { }

		public override InputIncomplete Enter()
		{
			screen.button.Disabled = true;

			return new InputIncomplete(screen);
		}

		public override Task<InputState> HandleButtonClick()
		{
			throw new InvalidOperationException("The button should not be clickable in this state.");
		}

		public override InputState HandleTextChange()
		{
			return screen.configInput.Text.Length == 0 ? new InputIncomplete(screen) : new InputComplete(screen);
		}
	}

	public class InputComplete : InputState
	{
		public InputComplete(ConfigLoader screen) : base(screen) { }

		public override InputComplete Enter()
		{
			screen.button.Disabled = false;

			return new InputComplete(screen);
		}

		public async override Task<InputState> HandleButtonClick()
		{
			await RetrieveDeserializeAndPersistJson();
			screen.GetTree().ChangeSceneToFile("res://src/activity-executor/activity-executor.tscn");

			return new InputComplete(screen);
		}

		public async Task RetrieveDeserializeAndPersistJson()
		{
			var input = screen.configInput.Text;
			var json = "";

			if (File.Exists(input))
			{
				using StreamReader reader = new(input);
				json = await reader.ReadToEndAsync();
			}
			else if (Uri.TryCreate(input, UriKind.Absolute, out Uri? result)
				&& ((result.Scheme == Uri.UriSchemeHttp) || (result.Scheme == Uri.UriSchemeHttps)))
			{
				using System.Net.Http.HttpClient client = new();
				json = await client.GetStringAsync(input);
			}

			Activity activity = DeserializeJson(json);
			GlobalVars.Activity = activity;
		}

		private Activity DeserializeJson(string contents)
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
			return activity;
		}

		public override InputState HandleTextChange()
		{
			return screen.configInput.Text.Length > 0 ? new InputComplete(screen) : new InputIncomplete(screen);
		}
	}
}
