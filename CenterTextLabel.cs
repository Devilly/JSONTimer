using Godot;

public partial class CenterTextLabel : RichTextLabel
{
	private string text;

	new public string Text {
		get => text;
		set {
			text = "[center]" + value;
			base.Text = text;
		}
	}
}
