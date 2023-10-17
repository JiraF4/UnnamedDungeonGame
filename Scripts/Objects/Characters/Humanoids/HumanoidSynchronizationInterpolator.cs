using Godot;
using Godot.Collections;

public class HumanoidSynchronizationInterpolator : SynchronizationInterpolator
{
    private readonly HumanoidDoll _doll;
    
    
    public HumanoidSynchronizationInterpolator(HumanoidDoll doll) : base(doll)
    {
        _newLinearVelocity = doll.LinearVelocity;
        _oldLinearVelocity = doll.LinearVelocity;
        _newBodyRotation = doll.BodyRotation;
        _oldBodyRotation = doll.BodyRotation;
        _doll = doll; 
    }
    
    private Vector3 _oldLinearVelocity;
    private Vector3 _newLinearVelocity;
    private Vector3 _oldBodyRotation;
    private Vector3 _newBodyRotation;
    
    
    public override void Next(Dictionary syncData, int peerId)
    {
        Vector3? newLinearVelocity = null;
        Vector3? newBodyRotation = null;
        
        if (syncData.ContainsKey("LinearVelocity")) newLinearVelocity = (Vector3) syncData["LinearVelocity"];
        if (syncData.ContainsKey("BodyRotation")) newBodyRotation = (Vector3) syncData["BodyRotation"];
        
        _oldLinearVelocity = _newLinearVelocity;
        _oldBodyRotation = _newBodyRotation;
        _newLinearVelocity = newLinearVelocity ?? _newLinearVelocity;
        _newBodyRotation = newBodyRotation ?? _newBodyRotation;
        
        base.Next(syncData, peerId);
    }

    public override void InterpolateInternal(double delta)
    {
        _doll.LinearVelocity = _oldLinearVelocity.Lerp(_newLinearVelocity, InterpolatedState);
        _doll.BodyRotation = _oldBodyRotation.Lerp(_newBodyRotation, InterpolatedState);
        base.InterpolateInternal(delta);
    }
}