using System.Windows.Media.Media3D;

namespace _3D_viewer.Tracer;

/// <summary>
///    Represents a planar surface in three-dimensional space.
/// </summary>
public class Surface
{
    private Vector3D position { get; set; } // A point on the surface
    private Vector3D normal { get; set; } // The normal vector of the surface
    
    /// <summary>
    /// Constructor to create a new instance of the Surface class.
    /// </summary>
    /// <param name="position">A point on the surface.</param>
    /// <param name="normal">The normal vector of the surface.</param>
    public Surface(Vector3D position, Vector3D normal)
    {
        this.position = position;
        this.normal = normal;
    }
}