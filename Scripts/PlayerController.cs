using Godot;
using System;

public partial class PlayerController : Node
{
	[Export] private RigidBody3D _character;
	private CharacterControllerInputs _characterControllerInputs;
	[Export] private TextureRect MapRect;
	
	private Vector3 _moveVector;
	private Vector3 _rotationVector;

	private ulong _mousePressTime;
	private Vector2 _mouseMove;
	public Vector2 MousePosition;
	private int _mouseWheelPosition;
	private bool _mousePressed;
	
	public override void _Input(InputEvent @event)
	{
		switch (@event)
		{
			case InputEventMouseMotion eventMouseMotion:
				_mouseMove += eventMouseMotion.Relative;
				MousePosition = eventMouseMotion.Position;
				break;
			case InputEventMouseButton eventMouseButton:
				switch (eventMouseButton.ButtonIndex)
				{
					case MouseButton.None:
						break;
					case MouseButton.Left:
						_mousePressed = eventMouseButton.Pressed;
						_mousePressTime = Time.GetTicksMsec();
						break;
					case MouseButton.Right:
						break;
					case MouseButton.Middle:
						break;
					case MouseButton.WheelUp:
						if (eventMouseButton.Pressed)
							_mouseWheelPosition++;
						break;
					case MouseButton.WheelDown:
						if (eventMouseButton.Pressed)
							_mouseWheelPosition--;
						break;
					case MouseButton.WheelLeft:
						break;
					case MouseButton.WheelRight:
						break;
					case MouseButton.Xbutton1:
						break;
					case MouseButton.Xbutton2:
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
				break;
		}
	}
	public override void _Ready()
	{
		RenderingServer.SetDebugGenerateWireframes(true);
		_characterControllerInputs = (CharacterControllerInputs) _character.FindChild("CharacterControllerInputs");
		((Camera3D)_character.FindChild("Camera3D")).Current = true;
		
		_character.GetNode<Node3D>("BodyAnimation/Body/Neck/HeadAnimation/Head").Visible = false;
		_characterControllerInputs.UIControl = true;
		
		Input.MouseMode = Input.MouseModeEnum.Captured;
		
		base._Ready();
	}
	public override void _Process(double delta)
	{
		_moveVector = Vector3.Zero;
		_moveVector.X += Input.IsActionPressed("Right") ? -1.0f : 0.0f;
		_moveVector.X += Input.IsActionPressed("Left") ? 1.0f : 0.0f;
		_moveVector.Z += Input.IsActionPressed("Forward") ? 1.0f : 0.0f;
		_moveVector.Z += Input.IsActionPressed("Backward") ? -1.0f : 0.0f;
		_moveVector.Y += Input.IsActionPressed("Up") ?  1.0f : 0.0f;
		_moveVector.Y += Input.IsActionPressed("Down") ? -1.0f : 0.0f;
		_moveVector = _moveVector.Normalized();
		
		var debugText = "Render count: " + GetViewport().GetRenderInfo(Viewport.RenderInfoType.Visible, Viewport.RenderInfo.PrimitivesInFrame).ToString();
		debugText += "\nTarget: " + _characterControllerInputs.TargetNode?.Name;
		DebugInfo.AddLine(debugText);

		DebugInfo.AddLine("FPS: " + Engine.GetFramesPerSecond().ToString("0.000"));
		
		_characterControllerInputs.MoveInput = _moveVector;
		_characterControllerInputs.InteractMode = Input.IsActionPressed("InteractMode");
		_characterControllerInputs.RotateInput = new Vector3(-_mouseMove.Y * 10.0f, -_mouseMove.X * 0.3f, 0);
		_characterControllerInputs.ScreenPositionMove = _mouseMove;
		
		_characterControllerInputs.PrimaryAction = Input.IsActionPressed("PrimaryAction");
		if (Input.IsActionJustPressed("PrimaryAction")) _characterControllerInputs.PrimaryActionJustPressed = true;
		if (Input.IsActionJustReleased("PrimaryAction")) _characterControllerInputs.PrimaryActionJustReleased = true;
		
		_characterControllerInputs.SecondAction = Input.IsActionPressed("SecondAction");
		if (Input.IsActionJustPressed("SecondAction")) _characterControllerInputs.SecondActionJustPressed = true;
		if (Input.IsActionJustReleased("SecondAction")) _characterControllerInputs.SecondActionJustReleased = true;
		
		GetViewport().DebugDraw = Input.IsActionPressed("Wireframe") ? Viewport.DebugDrawEnum.Wireframe : Viewport.DebugDrawEnum.Disabled;
		GetTree().DebugCollisionsHint = Input.IsActionPressed("DebugCollisionsHint") ? true : false;
		if (!Input.IsActionPressed("MapView"))
		{
			MapRect.Size = new Vector2(256, 256);
			MapRect.Position = new Vector2(1280 - 256, 0);
		}
		else
		{
			MapRect.Size = new Vector2(720, 720);
			MapRect.Position = new Vector2(1280 - 720, 0);
		}
		_mouseMove = Vector2.Zero;
	}
	public override void _PhysicsProcess(double delta)
	{
		UpdateCursorRayCast();
		base._PhysicsProcess(delta);
	}
	private void UpdateCursorRayCast()
	{
		_characterControllerInputs.ScreenPosition = MousePosition;
		
		var inventory = (Inventory) _characterControllerInputs.TargetNode?.FindChild("Inventory");
		if (inventory != null)
		{
			if (inventory.IsScreenPositionInside(MousePosition)) return;
		}
		
		var camera = GetViewport().GetCamera3D();
		var rayOrigin = camera.ProjectRayOrigin(MousePosition);
		var rayNormal = camera.ProjectRayNormal(MousePosition);
		var maxRayDistance = 5.0f;
		var cursorRayCastParameter = new PhysicsRayQueryParameters3D()
		{
			From = rayOrigin,
			To = rayOrigin + rayNormal * maxRayDistance,
            CollisionMask = 1 << 0
		};
		var cursorRayCast = camera.GetWorld3D().DirectSpaceState.IntersectRay(cursorRayCastParameter);
		
		if (cursorRayCast.Count > 0)
		{
			_characterControllerInputs.TargetPosition = (Vector3) cursorRayCast["position"];
			_characterControllerInputs.TargetNode = (Node3D) cursorRayCast["collider"];
		}
		else
		{
			_characterControllerInputs.TargetPosition = rayOrigin + rayNormal * maxRayDistance;
			_characterControllerInputs.TargetNode = null;
		}

		//if (Input.IsActionPressed("OpenMenu")) GetTree().Quit();
		if (Input.IsActionJustPressed("OpenMenu"))
		{
			if (Input.MouseMode == Input.MouseModeEnum.Captured) Input.MouseMode = Input.MouseModeEnum.Visible;
			else if (Input.MouseMode == Input.MouseModeEnum.Visible) Input.MouseMode = Input.MouseModeEnum.Captured;
		}
	}
}
