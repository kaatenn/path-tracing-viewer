﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.Media3D;

namespace _3D_viewer.Tracer;

/// <summary>
///     Represent a Bounding Volume
/// </summary>
public class Aabb
{
    /// <summary>
    ///     Constructor to create an instance of new bounding volume, could used for the leaf node of the BVH Tree
    /// </summary>
    public Aabb(IEnumerable<Triangle> triangles)
    {
        var min_point = new Vector3D(float.MaxValue, float.MaxValue, float.MaxValue);
        var max_point = new Vector3D(float.MinValue, float.MinValue, float.MinValue);
        foreach (var vertex in triangles.SelectMany(triangle => triangle.vertices))
        {
            min_point.X = Math.Min(min_point.X, vertex.X);
            min_point.Y = Math.Min(min_point.Y, vertex.Y);
            min_point.Z = Math.Min(min_point.Z, vertex.Z);

            max_point.X = Math.Max(max_point.X, vertex.X);
            max_point.Y = Math.Max(max_point.Y, vertex.Y);
            max_point.Z = Math.Max(max_point.Z, vertex.Z);
        }

        min = min_point;
        max = max_point;
    }

    /// <summary>
    ///     Constructor to create an instance of new default bounding volume
    /// </summary>
    public Aabb()
    {
        min = new Vector3D();
        max = new Vector3D();
    }

    public Vector3D min { get; set; } // The minimum vector of the bounding volume
    public Vector3D max { get; set; } // The maximum vector of the bounding volume

    /// <summary>
    ///     Get the size of the bounding box.
    /// </summary>
    /// <returns>The vector3D represent the size</returns>
    private Vector3D Size()
    {
        return max = min;
    }

    /// <summary>
    ///     Get the longest axis.
    /// </summary>
    /// <returns>The longest axis indicated by enum class Axis</returns>
    public Axis longest_axis()
    {
        var size = Size();
        if (size.X > size.Y && size.X > size.Z) return Axis.X;

        return size.Y > size.Z ? Axis.Y : Axis.Z;
    }
}