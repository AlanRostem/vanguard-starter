using Godot;
using System;

public class Pilot : KinematicBody
{
	private const float VerticalLookSensitivity = .2f;
	private const float HorizontalLookSensitivity = .2f;
	
	private const float Gravity = .98f;
	private const float WalkSpeed = 15f;
	private const float SprintSpeed = 20f;
	
	private bool _left;
	private bool _right;
	private bool _forward;
	private bool _backward;
	private bool _jump;
	private bool _sprint;
	private bool _crouch;

	private Vector3 _aimRotation = Vector3.Zero;

	private Spatial _cameraBase;
	
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
			_aimRotation = new Vector3(
				Mathf.Deg2Rad(rotation.x),
				Mathf.Deg2Rad(rotation.y),
				Mathf.Deg2Rad(rotation.y));
		}
	}

	private void _ProcessLocalInput()
	{
		_left = Input.IsActionPressed("left");
		_right = Input.IsActionPressed("right");
		_forward = Input.IsActionPressed("forward");
		_backward = Input.IsActionPressed("backward");
		
		_crouch = Input.IsActionPressed("crouch");
		_sprint = Input.IsActionJustPressed("sprint");
		_jump = Input.IsActionJustPressed("jump");
	}

	private void _ProcessMovement(float delta)
	{
		var moveDir = new Vector3();

		if (_left) moveDir.x -= 1;
		if (_right) moveDir.x += 1;
		if (_forward) moveDir.z -= 1;
		if (_backward) moveDir.z += 1;
		
		moveDir = moveDir.Normalized();
		moveDir = moveDir.Rotated(Vector3.Up, _aimRotation.y);

		var movement= moveDir * WalkSpeed;
		movement.y = -Gravity;
		MoveAndSlide(movement, Vector3.Up);

	}

	public override void _PhysicsProcess(float delta)
	{
		_ProcessLocalInput();
		_ProcessMovement(delta);
	}
}
