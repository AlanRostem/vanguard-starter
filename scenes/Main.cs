using Godot;
using System;

public class Main : Spatial
{
	private Label _speedLabel;
	private Pilot _pilot;
	public override void _Ready()
	{
		_speedLabel = GetNode<Label>("CanvasLayer/SpeedLabel");
		_pilot = GetNode<Pilot>("Pilot");
	}

	public override void _Process(float delta)
	{
		var hVel = new Vector2(_pilot.Velocity.x, _pilot.Velocity.z);
		_speedLabel.Text = hVel.Length() * 3.6f + " km/h";
	}
}
