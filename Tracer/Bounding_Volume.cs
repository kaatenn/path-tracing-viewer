using System;
using System.Windows.Media.Media3D;

namespace _3D_viewer.Tracer;

/// <summary>
///     Represent a Bounding Volume
/// </summary>
public class Bounding_Volume
{
    private Vector3D min { get; set; } // The minimum vector of the bounding volume
    private Vector3D max { get; set; } // The maximum vector of the bounding volume

    /// <summary>
    /// Constructor to create an instance of new default bounding volume
    /// </summary>
    public Bounding_Volume()
    {
        this.min = new Vector3D();
        this.max = new Vector3D();
    }
    
    /// <summary>
    /// Constructor to create an instance of new bounding volume
    /// </summary>
    /// <param name="min">The minimum vector of the bounding volume</param>
    /// <param name="max">The maximum vector of the bounding volume</param>
    public Bounding_Volume(Vector3D min, Vector3D max)
    {
        this.min = min;
        this.max = max;
    }

    /// <summary>
    /// Update the bounding volume when a triangle is added
    /// </summary>
    /// <param name="triangle">The added triangle</param>
    public void update_bounding_volume(Triangle triangle)
    {
        foreach (var vertex in triangle.vertices)
        {
            min = new Vector3D(
                Math.Min(min.X, vertex.X),
                Math.Min(min.Y, vertex.Y),
                Math.Min(min.Z, vertex.Z)
            );
            
            max = new Vector3D(
                Math.Max(max.X, vertex.X),
                Math.Max(max.Y, vertex.Y),
                Math.Max(max.Z, vertex.Z)
            );
        }
    }
}