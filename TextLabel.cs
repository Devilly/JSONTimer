using Godot;

public partial class TextLabel : RichTextLabel
{
	private string text;

    public override void _Ready()
    {
        BbcodeEnabled = true;
    }

    new public string Text {
		get => text;
		set {
			text = "[center]" + value;
			base.Text = text;
		}
	}
}
