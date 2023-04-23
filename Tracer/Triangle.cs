using System.Diagnostics.CodeAnalysis;
using System.Windows.Media.Media3D;

namespace _3D_viewer.Tracer;

/// <summary>
///     Represents a triangle
/// </summary>

public class Triangle
{
    public Vector3D[] vertices { get; set; } // Three vertices of the triangle
    private readonly Vector3D[] edges = new Vector3D[3]; // Three edges of the triangle
    private Surface surface; // The surface which the triangle belongs to
    private double refractive_index { get; set; } // the refractive index of the triangle.

    /// <summary>
    /// Constructor to create a new instance of the Triangle class.
    /// </summary>
    /// <param name="vertices">The three vertices of the triangle</param>
    /// <param name="refractive_index">The refractive index of the triangle</param>
    public Triangle(Vector3D[] vertices, double refractive_index = 1.0)
    {
        this.vertices = vertices;
        for (var i = 0; i < 3; i++)
        {
            edges[i] = new Vector3D(vertices[(i + 1) % 3].X - vertices[i].X, vertices[(i + 1) % 3].Y - vertices[i].Y,
                vertices[(i + 1) % 3].Z - vertices[i].Z);
        }

        surface = new Surface(vertices[0], Vector3D.CrossProduct(edges[1], edges[2]));
        this.refractive_index = refractive_index;
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