using System;
using System.Windows.Media.Media3D;

namespace _3D_viewer.Tracer;

/// <summary>
///     Represents a triangle
/// </summary>
public class Triangle
{
    // The Material is always diffuse.

    /// <summary>
    ///     Constructor to create a new instance of the Triangle class.
    /// </summary>
    /// <param name="vertices">The three vertices of the triangle</param>
    /// <param name="color">The color of the triangle</param>
    public Triangle(Vector3D[] vertices, byte[] color)
    {
        this.vertices = vertices;
        if (color.Length != 3) throw new FormatException("Color should be a array in length 3");
        this.color = color;
    }

    public Vector3D[] vertices { get; } // Three vertices of the triangle

    public byte[] color { get; } // The color of the triangle

    /// <summary>
    ///     Get the centroid of the triangle.
    /// </summary>
    /// <returns>The Vector represent the centroid</returns>
    public Vector3D compute_triangle_centroid()
    {
        return (vertices[1] + vertices[2] + vertices[3]) / 3f;
    }

    public Vector3D get_normal()
    {
        return Vector3D.CrossProduct(vertices[1] - vertices[0], vertices[2] - vertices[0]);
    }
}