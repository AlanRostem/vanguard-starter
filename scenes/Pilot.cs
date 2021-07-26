using Godot;
using System;

public class Pilot : KinematicBody
{
	private enum CollisionMode
	{
		Slide,
		Snap,
		Collide
	}
	
	public enum MovementStateType
	{
		Walk,
		Sprint,
		Airborne,
		Crouch,
		Slide,
	}
	
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

	private Vector3 _lookingDirectionVector = Vector3.Zero;
	private Vector3 _velocity = Vector3.Zero;
	private MovementStateType _currentMovementState = MovementStateType.Walk;

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
		
		// This code is not needed for player movement. It makes it easier for us exit the game since
		// this code allows us to press Esc and release the mouse from the window. 
		if (Input.IsActionJustPressed("ui_cancel"))
		{
			if (Input.GetMouseMode() == Input.MouseMode.Visible)
				Input.SetMouseMode(Input.MouseMode.Captured);
			else
				Input.SetMouseMode(Input.MouseMode.Visible);
		}
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

	public void SetMovementState(MovementStateType movementState)
	{
		// Call the clear function for the movement state that is being changed from	
		switch (_currentMovementState)
		{
			
		}

		_currentMovementState = movementState;

		// Call the initialize function for the movement state that is being changed to
		switch (_currentMovementState)
		{
			
		}
	}
	
	private void _ProcessMovement(float delta)
	{
		// Clear the looking direction vector for each frame
		_lookingDirectionVector = new Vector3();

		Vector2 inputMovementVector = new Vector2();

		// Based on the input booleans, we determine the walking direction using a unit vector
		if (_forward)
			inputMovementVector.y += 1;
		if (_backward)
			inputMovementVector.y -= 1;
		if (_left)
			inputMovementVector.x -= 1;
		if (_right)
			inputMovementVector.x += 1;

		inputMovementVector = inputMovementVector.Normalized();

		// Using the camera transform that we change based on mouse movement, we combine that with
		// the player's keyboard input to create an appropriate movement vector. 
		
		Transform cameraTransform = _camera.GlobalTransform;
		
		_lookingDirectionVector += -cameraTransform.basis.z * inputMovementVector.y;
		_lookingDirectionVector += cameraTransform.basis.x * inputMovementVector.x;
		_lookingDirectionVector.y = 0;
		_lookingDirectionVector = _lookingDirectionVector.Normalized();
		
		// If the player is on the floor, they can jump by pressing space
		if (IsOnFloor())
		{
			if (_jump)
				_velocity.y = JumpSpeed;
		}

		// Simulate gravity by subtracting the velocity each frame
		_velocity.y += delta * -Gravity;

		Vector3 hVel = _velocity;
		hVel.y = 0;

		Vector3 target = _lookingDirectionVector;

		target *= MaxWalkSpeed;

		float accel = _lookingDirectionVector.Dot(hVel) > 0 ? WalkAcceleration : WalkDeceleration;

		hVel = hVel.LinearInterpolate(target, accel * delta);
		_velocity.x = hVel.x;
		_velocity.z = hVel.z;
		_velocity = MoveAndSlide(_velocity, new Vector3(0, 1, 0), false, 4, Mathf.Deg2Rad(MaxSlopeAngle));

	}

	public override void _PhysicsProcess(float delta)
	{
		_ProcessLocalInput();
		_ProcessMovement(delta);
	}
}
