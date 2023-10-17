using Godot;
using Godot.Collections;

public class SynchronizationInterpolator
{
    protected readonly Node3D Node3D;

    public SynchronizationInterpolator(Node3D node3D)
    {
        Node3D = node3D;
        _newPosition = node3D.Position;
        _oldPosition = node3D.Position;
        _newRotation = node3D.Quaternion;
        _oldRotation = node3D.Quaternion;
    }

    private Vector3 _oldPosition;
    private Vector3 _newPosition;
    private Quaternion _oldRotation;
    private Quaternion _newRotation;
    
    private float _interpolateTime = 0.02f;
    private float _currentInterpolateTime;
    private int _peerId;

    protected float InterpolatedState => _currentInterpolateTime / _interpolateTime;

    public virtual void Next(Dictionary syncData, int peerId)
    {
        Vector3? newPosition = null;
        Quaternion? newRotation = null;
        
        if (syncData.ContainsKey("Position")) newPosition = (Vector3) syncData["Position"];
        if (syncData.ContainsKey("Rotation")) newRotation = (Quaternion) syncData["Rotation"];
        
        _oldPosition = _newPosition;
        _oldRotation = _newRotation;
        _newPosition = newPosition ?? _newPosition;
        _newRotation = newRotation ?? _newRotation;
        
        _peerId = peerId;
        _interpolateTime = Synchronizator.Instance.GetDelay(_peerId);
        if (_interpolateTime <= 0.0f) _interpolateTime = 0.01f;
        _currentInterpolateTime = 0.0f;
        Interpolate(0.0f);
    }

    public virtual void Reset(Vector3 position, Quaternion quaternion)
    {
        _oldPosition = position;
        _oldRotation = quaternion;
        _newPosition = position;
        _newRotation = quaternion;
        _interpolateTime = 0.01f;
    }

    public void Interpolate(double delta)
    {
        _currentInterpolateTime += (float) delta;
        if (_currentInterpolateTime >= _interpolateTime) _currentInterpolateTime = _interpolateTime;
        InterpolateInternal(delta);
    }

    public virtual void InterpolateInternal(double delta)
    {
        Node3D.Position = _oldPosition.Lerp(_newPosition, InterpolatedState);
        Node3D.Quaternion = _oldRotation.Slerp(_newRotation, InterpolatedState);
    }
}