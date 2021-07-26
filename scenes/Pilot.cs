using Godot;
using System;

public class Pilot : KinematicBody
{
	private const float VerticalLookSensitivity = .2f;
	private const float HorizontalLookSensitivity = .2f;
	
	private const float Gravity = 24.8f;
	private const float MaxWalkSpeed = 20f;
	private const float WalkAcceleration = 4.5f;
	private const float WalkDeceleration = 16f;
	private const float MaxSprintSpeed = 30f;
	private const float JumpSpeed = 18f;

	private const float WalkFrictionGround = 0.95f;
	private const float SlideFrictionGround = 0.2f;
	
	private const float MaxSlopeAngle = 40f;
	
	private bool _left;
	private bool _right;
	private bool _forward;
	private bool _backward;
	private bool _jump;
	private bool _sprint;
	private bool _crouch;

	private Vector3 _aimRotation = Vector3.Zero;
	private Vector3 _velocity = Vector3.Zero;

	private Camera _camera;
	
	public override void _Ready()
	{
		_camera = GetNode<Camera>("Camera");
		Input.SetMouseMode(Input.MouseMode.Captured);
	}

	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventMouseMotion motion)
		{
			var rotation = new Vector3(_camera.RotationDegrees);
			rotation.x -= motion.Relative.y * VerticalLookSensitivity;
			rotation.x = Mathf.Clamp(rotation.x, -90, 90);
			rotation.y -= motion.Relative.x * HorizontalLookSensitivity;
			_camera.RotationDegrees = rotation;
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

	private void Accelerate(Vector3 acceleration, float maxSpeed, float delta)
	{
		var movement = acceleration * delta;
		var length = (_velocity + movement).Length();
		if (length > maxSpeed)
		{
			movement = (length - maxSpeed) * delta * acceleration.Normalized();
			if (movement.Length() > maxSpeed)
				movement = Vector3.Zero;
		}

		_velocity += movement;
	}
	
	private void Walk(Vector3 direction, float speed, float delta)
	{
		
	}

	private void ApplyGroundFriction(float factor)
	{
		
	}
	
	private void _ProcessMovement(float delta)
	{
		_aimRotation = new Vector3();
		Transform cameraTransform = _camera.GlobalTransform;

		Vector2 inputMovementVector = new Vector2();

		if (_forward)
			inputMovementVector.y += 1;
		if (_backward)
			inputMovementVector.y -= 1;
		if (_left)
			inputMovementVector.x -= 1;
		if (_right)
			inputMovementVector.x += 1;

		inputMovementVector = inputMovementVector.Normalized();

		_aimRotation += -cameraTransform.basis.z * inputMovementVector.y;
		_aimRotation += cameraTransform.basis.x * inputMovementVector.x;

		if (IsOnFloor())
		{
			if (_jump)
				_velocity.y = JumpSpeed;
		}
		
		_aimRotation.y = 0;
		_aimRotation = _aimRotation.Normalized();

		_velocity.y += delta * -Gravity;

		Vector3 hvel = _velocity;
		hvel.y = 0;

		Vector3 target = _aimRotation;

		target *= MaxWalkSpeed;

		float accel;
		if (_aimRotation.Dot(hvel) > 0)
			accel = WalkAcceleration;
		else
			accel = WalkDeceleration;

		hvel = hvel.LinearInterpolate(target, accel * delta);
		_velocity.x = hvel.x;
		_velocity.z = hvel.z;
		_velocity = MoveAndSlide(_velocity, new Vector3(0, 1, 0), false, 4, Mathf.Deg2Rad(MaxSlopeAngle));

	}

	public override void _PhysicsProcess(float delta)
	{
		_ProcessLocalInput();
		_ProcessMovement(delta);
	}
}
