using System.Linq;
using Godot;

public partial class CharactersService : Node
{
    public static CharactersService Instance { get; private set; }

    CharactersService()
    {
        Instance = this;
    }
    
    public CharacterController FindNearestTarget(Vector3 findPosition, Vector3 findDirection)
    {
        return CharacterController.CharacterControllers
            .Where(c => (c.Target.GlobalPosition - findPosition).Normalized().Dot(findDirection) > 0.85f 
                + 0.2f * (c.Target.GlobalPosition.DistanceTo(findPosition) / 6.0f))
            .OrderBy(c => c.Target.GlobalPosition.DistanceTo(findPosition))
            .FirstOrDefault((CharacterController) null);
        
    }
}