using Godot;

public class SynchronizationInterpolator
{
    private readonly Node3D _node3D;

    public SynchronizationInterpolator(Node3D node3D)
    {
        _node3D = node3D;
        _newPosition = node3D.Position;
        _oldPosition = node3D.Position;
        _newRotation = node3D.Quaternion;
        _oldRotation = node3D.Quaternion;
    }

    private Vector3 _newPosition;
    private Quaternion _newRotation;
    private Vector3 _oldPosition;
    private Quaternion _oldRotation;
    private float _interpolateTime = 0.02f;
    private float _currentInterpolateTime;

    public void Next(Vector3 newPosition, Quaternion newRotation, float interpolateTime)
    {
        _oldPosition = _newPosition;
        _oldRotation = _newRotation;
        _newPosition = newPosition;
        _newRotation = newRotation;
        _interpolateTime = interpolateTime == 0.0f ? 0.02f : interpolateTime;
        _currentInterpolateTime = 0.0f;
        Interpolate(0.0f);
    }

    public void Interpolate(double delta)
    {
        _currentInterpolateTime += (float) delta;
        if (_currentInterpolateTime >= _interpolateTime) _currentInterpolateTime = _interpolateTime;
        _node3D.Position = _oldPosition.Lerp(_newPosition, _currentInterpolateTime / _interpolateTime);
        _node3D.Quaternion = _oldRotation.Slerp(_newRotation, _currentInterpolateTime / _interpolateTime);
    }
}