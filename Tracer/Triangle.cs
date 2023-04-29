using System.Windows.Media.Media3D;

namespace _3D_viewer.Tracer;

/// <summary>
///     Represents a triangle
/// </summary>
public class Triangle
{
    public Vector3D[] vertices { get; set; } // Three vertices of the triangle

    /// <summary>
    /// Constructor to create a new instance of the Triangle class.
    /// </summary>
    /// <param name="vertices">The three vertices of the triangle</param>
    public Triangle(Vector3D[] vertices, double refractive_index = 1.0)
    {
        this.vertices = vertices;
    }

    /// <summary>
    /// Get the centroid of the triangle.
    /// </summary>
    /// <returns>The Vector represent the centroid</returns>
    public Vector3D compute_triangle_centroid()
    {
        return (vertices[1] + vertices[2] + vertices[3]) / 3f;
    }
}