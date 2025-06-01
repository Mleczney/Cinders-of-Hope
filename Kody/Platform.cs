using Godot;
using System;

public partial class Platform : Node3D
{
	Button btn;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		btn = GetNode<Button>("Exit");
		btn.Visible = false;
	}
	
	private void OnButtonDown()
	{
		GetTree().Quit();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (Input.MouseMode == Input.MouseModeEnum.Confined)
		{
			btn.Visible = true;
		}
	}
}
