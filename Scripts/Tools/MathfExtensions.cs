using Godot;

namespace Dungeon.Tools
{
    public class MathfExtensions
    {
        public static float DeltaAngleDeg(float firstAngle, float secondAngle)
        {
            var difference = secondAngle - firstAngle;
            while (difference < -180) difference += 360;
            while (difference > 180) difference -= 360;
            return difference;
        }
        
        public static float DeltaAngleRad(float firstAngle, float secondAngle)
        {
            var difference = secondAngle - firstAngle;
            while (difference < Mathf.DegToRad(-180)) difference += Mathf.DegToRad(360);
            while (difference > Mathf.DegToRad( 180)) difference -= Mathf.DegToRad(360);
            return difference;
        }
    }
}