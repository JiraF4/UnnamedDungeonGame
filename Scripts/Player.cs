using Godot;
using System;

public partial class Player : CharacterBody3D
{
	[Export] private TextureRect MapRect;
	[Export] public Label DebugText;

	private Camera3D _camera;
	
	private Vector3 _moveVector;
	private Vector3 _rotationVector;
	private float _speed = 10f;

	private ulong _mousePressTime;
	private Vector2 _mouseMove;
	private Vector2 _mousePosition;
	private int _mouseWheelPosition;
	private bool _mousePressed;
	
	public override void _Input(InputEvent @event)
	{
		switch (@event)
		{
			case InputEventMouseMotion eventMouseMotion:
				_mouseMove += eventMouseMotion.Relative;
				_mousePosition = eventMouseMotion.Position;
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
		_camera = GetNode<Camera3D>("Camera3D");
		
		base._Ready();
	}
	
	public override void _Process(double delta)
	{
		_moveVector = Vector3.Zero;
		_moveVector.X += Input.IsActionPressed("Right") ? 1.0f : 0.0f;
		_moveVector.X += Input.IsActionPressed("Left") ? -1.0f : 0.0f;
		_moveVector.Z += Input.IsActionPressed("Forward") ? -1.0f : 0.0f;
		_moveVector.Z += Input.IsActionPressed("Backward") ? 1.0f : 0.0f;
		_moveVector.Y += Input.IsActionPressed("Up") ?  1.0f : 0.0f;
		_moveVector.Y += Input.IsActionPressed("Down") ? -1.0f : 0.0f;
		_moveVector = _moveVector.Normalized();
		_moveVector = _moveVector.Rotated(Vector3.Up, Rotation.Y);
		
		//Position += _moveVector * _speed * (float) delta;
		Velocity = _moveVector * _speed;
		MoveAndSlide();
		
		DebugText.Text = GetViewport().GetRenderInfo(Viewport.RenderInfoType.Visible, Viewport.RenderInfo.PrimitivesInFrame).ToString();
		
		if (Input.IsActionPressed("Look"))
		{
			RotationDegrees += new Vector3(0, -_mouseMove.X * 10.0f, 0) * (float) delta;

			var cameraRotation = _camera.RotationDegrees;
			cameraRotation += new Vector3(-_mouseMove.Y * 10.0f, 0, 0) * (float) delta;
			if (cameraRotation.X < -90.0f) cameraRotation.X = -90.0f;
			if (cameraRotation.X >  90.0f) cameraRotation.X =  90.0f;
			_camera.RotationDegrees = cameraRotation;
			
			Input.MouseMode = Input.MouseModeEnum.Captured;
		}
		else
		{
			Input.MouseMode = Input.MouseModeEnum.Visible;
		}
		
		GetViewport().DebugDraw = Input.IsActionPressed("Wireframe") ? Viewport.DebugDrawEnum.Wireframe : Viewport.DebugDrawEnum.Disabled;
		GetTree().DebugCollisionsHint = Input.IsActionPressed("Wireframe") ? true : false;
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
}
