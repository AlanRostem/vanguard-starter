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
		WalkOrSprint,
		Airborne,
		Crouch,
		Slide,
	}

	private const float VerticalLookSensitivity = .05f;
	private const float HorizontalLookSensitivity = .05f;

	private const float Gravity = 20f;

	private const float MaxWalkSpeed = 4.2f;
	private const float WalkAcceleration = 18f;
	private const float WalkDeceleration = 22f;
	
	private const float AirAcceleration = 1f;

	private const float MaxSprintSpeed = 6.1f;
	private const float SprintAcceleration = 18f;
	private const float SprintDeceleration = 22f;

	private const float JumpSpeed = 8f;

	private const float WalkFrictionGround = 2f;
	private const float SlideFrictionGround = 0.2f;

	private const float MaxSlopeAngle = 40f;

	private bool _left;
	private bool _right;
	private bool _forward;
	private bool _backward;
	private bool _jump;
	private bool _sprint;
	private bool _crouch;

	private bool _isSprinting = false;

	private Vector3 _lookingDirectionVector = Vector3.Zero;
	public Vector3 Velocity = Vector3.Zero;
	private MovementStateType _currentMovementState = MovementStateType.WalkOrSprint;

	private Camera _camera;

	public Vector2 HorizontalVelocity => new Vector2(Velocity.x, Velocity.z);

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
		_jump = Input.IsActionPressed("jump");

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

	private bool IsHoldingMovementKey()
	{
		return _forward || _backward || _right || _left;
	}
	
	private void Move(Vector3 direction, float acceleration, float deceleration, float maxSpeed, float delta, float turnPenaltyFactor = 1f)
	{
		Vector3 hVel = Velocity;
		hVel.y = 0;

		direction *= maxSpeed;

		float accel = _lookingDirectionVector.Dot(hVel) > 0 ? acceleration : deceleration;

		hVel = hVel.LinearInterpolate(direction, accel * delta * turnPenaltyFactor);
		Velocity.x = hVel.x;
		Velocity.z = hVel.z;
	}

	/// <summary>
	/// The Quake engine implementation of movement. This will only be used while mid-air to allow
	/// air-strafing and tap-strafing.
	/// </summary>
	/// <param name="intendedDirection">The input movement vector from the player</param>
	/// <param name="acceleration">Desired acceleration</param>
	/// <param name="maxSpeed">Desired maximum speed so the player does not constantly increase speed</param>
	/// <param name="delta">Delta time between each frame (in seconds)</param>
	private void QuakeMove(Vector3 intendedDirection, float acceleration, float maxSpeed, float delta)
	{
		var currentSpeed = Velocity.Dot(intendedDirection);
		var addedSpeed = Mathf.Clamp(maxSpeed - currentSpeed, 0, acceleration * delta);
		Velocity += addedSpeed * intendedDirection;
	}

	private void ApplyHorizontalFriction(float coefficient, float delta)
	{
		var speed = Velocity.Length();
		if (!(speed > 0)) return;
		var drop = speed * coefficient * delta;
		Velocity *= Mathf.Max(speed - drop, 0) / speed;
	}

	public void SetMovementState(MovementStateType movementState)
	{
		// Call the clear function for the movement state that is being changed from	
		switch (_currentMovementState)
		{
			case MovementStateType.WalkOrSprint:
				ClearWalkOrSprintMode();
				break;
			case MovementStateType.Airborne:
				ClearAirborneMode();
				break;
			case MovementStateType.Crouch:
				break;
			case MovementStateType.Slide:
				break;
		}

		_currentMovementState = movementState;

		// Call the initialize function for the movement state that is being changed to
		switch (_currentMovementState)
		{
			case MovementStateType.WalkOrSprint:
				InitializeWalkOrSprintMode();
				break;
			case MovementStateType.Airborne:
				InitializeAirborneMode();
				break;
			case MovementStateType.Crouch:
				break;
			case MovementStateType.Slide:
				break;
		}
	}

	private void InitializeWalkOrSprintMode()
	{
		
	}
	
	private void ClearWalkOrSprintMode()
	{
		
	}

	private void ProcessWalkOrSprintMode(float delta)
	{
		if (_sprint) _isSprinting = true;
		else if (!IsHoldingMovementKey()) _isSprinting = false;
		
		//if (!IsHoldingMovementKey())
		ApplyHorizontalFriction(WalkFrictionGround, delta);

		if (_isSprinting)
		{
			// QuakeMove(_lookingDirectionVector, SprintAcceleration, MaxSprintSpeed, delta);
			Move(_lookingDirectionVector, SprintAcceleration, SprintDeceleration, MaxSprintSpeed, delta);
		}
		else
		{
			// QuakeMove(_lookingDirectionVector, WalkAcceleration, MaxWalkSpeed, delta);
			Move(_lookingDirectionVector, WalkAcceleration, WalkDeceleration, MaxWalkSpeed, delta);
		}
		
		// If the player is on the floor, they can jump by pressing space
		if (IsOnFloor())
		{
			if (_jump)
				Velocity.y = JumpSpeed;
		}
	}
	
	private void InitializeAirborneMode()
	{
		
	}
	
	private void ClearAirborneMode()
	{
		
	}
	
	private void ProcessAirborneMode(float delta)
	{
		//if (IsHoldingMovementKey())
			//Move(_lookingDirectionVector, WalkAcceleration, WalkDeceleration, MaxWalkSpeed, delta, 0.2f);
		if (!_forward && !IsMovingTooFast(MaxSprintSpeed))
			QuakeMove(_lookingDirectionVector, WalkAcceleration, MaxWalkSpeed, delta);
		else QuakeMove(_lookingDirectionVector, AirAcceleration, MaxWalkSpeed, delta);
	}

	private bool IsMovingTooFast(float maxSpeed)
	{
		const float margin = 0.1f;
		return HorizontalVelocity.Length() >= maxSpeed + margin;
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

		// Simulate gravity by subtracting the y-velocity each frame
		Velocity.y += delta * -Gravity;

		if (!IsOnFloor())
		{
			SetMovementState(MovementStateType.Airborne);
		}
		else
		{
			SetMovementState(MovementStateType.WalkOrSprint);
		}

		switch (_currentMovementState)
		{
			case MovementStateType.WalkOrSprint:
				ProcessWalkOrSprintMode(delta);
				break;
			case MovementStateType.Airborne:
				ProcessAirborneMode(delta);
				break;
			case MovementStateType.Crouch:
				break;
			case MovementStateType.Slide:
				break;
		}
		
		Velocity = MoveAndSlide(Velocity, Vector3.Up, false, 4, Mathf.Deg2Rad(MaxSlopeAngle));
	}

	public override void _PhysicsProcess(float delta)
	{
		_ProcessLocalInput();
		_ProcessMovement(delta);
	}
}
