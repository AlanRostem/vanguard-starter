using Godot;
using System;

public class Pilot : KinematicBody
{
	private class InputStates
	{
		public bool Left;
		public bool Right;
		public bool Forward;
		public bool Backward;
		public bool Jump;
		public bool Crouch;
	}
	
	private const float VerticalLookSensitivity = .2f;
	private const float HorizontalLookSensitivity = .2f;

	private Spatial _cameraBase;

	private InputStates _inputStates = new InputStates();

	public override void _Ready()
	{
		_cameraBase = GetNode<Spatial>("CameraBase");
		Input.SetMouseMode(Input.MouseMode.Captured);
	}

	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventMouseMotion motion)
		{
			var rotation = new Vector3(_cameraBase.RotationDegrees);
			rotation.x -= motion.Relative.y * VerticalLookSensitivity;
			rotation.x = Mathf.Clamp(rotation.x, -90, 90);
			rotation.y -= motion.Relative.x * HorizontalLookSensitivity;
			_cameraBase.RotationDegrees = rotation;
		}
	}

	private void _ProcessLocalInput()
	{
		_inputStates.Left = Input.IsActionPressed("left");
		_inputStates.Right = Input.IsActionPressed("right");
		_inputStates.Forward = Input.IsActionPressed("forward");
		_inputStates.Backward = Input.IsActionPressed("backward");
		
		_inputStates.Crouch = Input.IsActionPressed("crouch");
		
		_inputStates.Jump = Input.IsActionJustPressed("jump");
	}

	public override void _PhysicsProcess(float delta)
	{
		_ProcessLocalInput();
		
	}
}
