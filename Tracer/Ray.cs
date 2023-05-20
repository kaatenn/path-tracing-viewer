using System.Windows.Media.Media3D;

namespace _3D_viewer.Tracer;

/// <summary>
///     Represent the ray
/// </summary>
public class Ray
{
    public Vector3D direction; // The direction of ray
    public Vector3D origin; // The origin point of ray

    /// <summary>
    ///     Constructor to create an instance of the ray
    /// </summary>
    /// <param name="origin">The origin point of the ray</param>
    /// <param name="direction">The direction of the ray</param>
    public Ray(Vector3D origin, Vector3D direction)
    {
        this.origin = origin;
        this.direction = direction;
        if (this.direction != new Vector3D(0, 0, 0)) this.direction.Normalize();
    }
}