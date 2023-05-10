using System;
using System.Windows.Media.Media3D;

namespace _3D_viewer.Tracer;

/// <summary>
///     Represent the intersecting result.
/// </summary>
public class Intersection
{
    public readonly byte[] color; // represent the color should be shaded.
    public readonly double cosine; // represent the cosine between normal and ray direction
    public readonly bool happened; // represent if intersected.
    private readonly double t; // represent t of the ray
    public Vector3D coordinate; // represent the intersection coordinate.
    public Vector3D normal;


    /// <summary>
    ///     Constructor to create a default instance of intersection
    /// </summary>
    private Intersection()
    {
        happened = false;
        color = new byte[] { 0, 0, 0 };
        coordinate = new Vector3D(0, 0, 0);
        normal = new Vector3D();
        t = 0;
    }

    /// <summary>
    ///     Constructor to create an instance of intersection
    /// </summary>
    /// <param name="happened">represent if intersected.</param>
    /// <param name="coordinate">represent the intersection coordinate.</param>
    /// <param name="color">represent the color should be shaded.</param>
    /// <param name="t">represent time of the ray</param>
    /// <param name="cosine">represent the cosine between normal and ray direction</param>
    public Intersection(bool happened, Vector3D coordinate, byte[] color, double t, double cosine, Vector3D normal)
    {
        this.happened = happened;
        if (color.Length != 3) throw new FormatException("Color should be a array in length 3");
        this.color = color;
        this.coordinate = coordinate;
        this.t = t;
        this.cosine = cosine;
        this.normal = normal;
    }

    public static Intersection not_hit_intersection()
    {
        return new Intersection();
    }

    /// <summary>
    ///     Operator * for instance of intersection and a number
    /// </summary>
    /// <param name="count">The number for multiple</param>
    /// <param name="intersection">The intersection</param>
    /// <returns></returns>
    public static Intersection operator *(int count, Intersection intersection)
    {
        for (var i = 0; i < 3; i++) intersection.color[i] *= (byte)count;
        return new Intersection(intersection.happened, intersection.coordinate, intersection.color, intersection.t,
            intersection.cosine, intersection.normal);
    }

    /// <summary>
    ///     Operator less-than sign for two instance of intersection
    /// </summary>
    /// <param name="intersection1">The former param</param>
    /// <param name="intersection2">The latter param</param>
    /// <returns>Compare the t</returns>
    public static bool operator <(Intersection intersection1, Intersection intersection2)
    {
        return intersection1.t < intersection2.t;
    }

    /// <summary>
    ///     Operator greater-than sign for two instance of intersection
    /// </summary>
    /// <param name="intersection1">The former param</param>
    /// <param name="intersection2">The latter param</param>
    /// <returns>Compare the t</returns>
    public static bool operator >(Intersection intersection1, Intersection intersection2)
    {
        return intersection1.t < intersection2.t;
    }
}