using System.Windows.Media.Media3D;

namespace _3D_viewer.Tracer;

/// <summary>
///     Represent the ray
/// </summary>
public class Ray
{
    public Point3D origin; // The origin point of ray
    public Vector3D direction; // The direction of ray

    /// <summary>
    /// Constructor to create an instance of the ray
    /// </summary>
    /// <param name="origin">The origin point of the ray</param>
    /// <param name="direction">The direction of the ray</param>
    public Ray(Point3D origin, Vector3D direction)
    {
        this.origin = origin;
        this.direction = direction;
    }
}