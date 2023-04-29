using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Media.Media3D;

namespace _3D_viewer.Tracer;

/// <summary>
///     Represent the BVH Tree Node of the scene
/// </summary>
public class Bvh_Tree_Node
{
    private Aabb aabb { get; set; } // the info(bounding box) of the node
    private Bvh_Tree_Node? left_child { get; set; } // the left child
    private Bvh_Tree_Node? right_child { get; set; } // the right child
    private List<Triangle>? triangles { get; set; } // the triangle list of the node

    /// <summary>
    /// Constructor to create an instance of the default BVH_Tree_Node class.
    /// </summary>
    /// <param name="aabb">The aabb of the node</param>
    /// <param name="left_child">The left child of the BVH-Tree node</param>
    /// <param name="right_child">The right child of the BVH-Tree node</param>
    /// <param name="triangles">The triangle list of the BVH-Tree node(it should be null when the node is not a leaf node)</param>
    private Bvh_Tree_Node(Aabb aabb, Bvh_Tree_Node? left_child = null, Bvh_Tree_Node? right_child = null,
        List<Triangle>? triangles = null)
    {
        this.aabb = aabb;
        this.left_child = left_child;
        this.right_child = right_child;
        this.triangles = triangles;
    }


    /// <summary>
    ///     Create BVH Tree by static factory
    /// </summary>
    /// <param name="triangles"></param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    /// <returns></returns>
    public static Bvh_Tree_Node build_bvh(List<Triangle> triangles)
    {
        if (triangles.Count <= 5)
        {
            return new Bvh_Tree_Node(new Aabb(triangles), null, null, triangles);
        }

        var aabb = new Aabb(triangles);
        var split_axis = aabb.longest_axis();
        var split_index = triangles.Count / 2;

        triangles.Sort((a, b) =>
        {
            var a_centroid = a.compute_triangle_centroid();
            var b_centroid = b.compute_triangle_centroid();

            return split_axis switch
            {
                Axis.X => a_centroid.X < b_centroid.X ? -1 : 1,
                Axis.Y => a_centroid.Y < b_centroid.Y ? -1 : 1,
                Axis.Z => a_centroid.Z < b_centroid.Z ? -1 : 1,
                _ => throw new ArgumentOutOfRangeException()
            };
        });

        var left_triangles = triangles.GetRange(0, split_index);
        var right_triangles = triangles.GetRange(split_index, triangles.Count);
        var left_node = build_bvh(left_triangles);
        var right_node = build_bvh(right_triangles);

        return new Bvh_Tree_Node(aabb, left_node, right_node);
    }

    /// <summary>
    /// Assert the ray is intersect with any triangle in the object.
    /// </summary>
    /// <param name="ray">the waiting ray</param>
    /// <returns>the time of the ray</returns>
    private double intersect_triangle(Ray ray)
    {
        var t_min = double.MaxValue;
        Debug.Assert(triangles != null, nameof(triangles) + " != null");
        foreach (var triangle in triangles)
        {
            var vertices = triangle.vertices;
            var e1 = vertices[1] - vertices[0];
            var e2 = vertices[2] - vertices[0];
            var s = ray.origin - vertices[0];
            var s1 = Vector3D.CrossProduct(ray.direction, e2);
            var s2 = Vector3D.CrossProduct(s, e1);
            var t_near = Vector3D.DotProduct(s2, e2) / Vector3D.DotProduct(s1, e1);
            var u = Vector3D.DotProduct(s1, s) / Vector3D.DotProduct(s1, e1);
            var v = Vector3D.DotProduct(s2, ray.direction) / Vector3D.DotProduct(s1, e1);
            if (t_min > t_near && t_near > 0 && u >= 0 && v >= 0 && (1 - u - v) >= 0)
            {
                t_min = t_near;
            }
        }

        return t_min;
    }

    private double intersect_axis_align_face(Ray ray, int location, Vector3D coordinate)
    {
        var t = location switch
        {
            0 => (coordinate.X - ray.origin.X) / ray.direction.X,
            1 => (coordinate.Y - ray.origin.Y) / ray.direction.Y,
            2 => (coordinate.Z - ray.origin.Z) / ray.direction.Z,
            _ => double.MaxValue
        };
        return t;
    }

    /// <summary>
    /// check if the ray intersect with the AABB
    /// </summary>
    /// <param name="ray">the checked ray</param>
    /// <returns>the time of the ray, if the ray misses the AABB, then return double.MaxValue</returns>
    public double intersect_aabb(Ray ray)
    {
        // Check if the ray is intersecting with the node's bounding box

        var t_min = new double[3];
        var t_max = new double[3];


        for (var i = 0; i < 3; i++)
        {
            var t1 = intersect_axis_align_face(ray, i, aabb.min);
            var t2 = intersect_axis_align_face(ray, i, aabb.max);
            t_min[i] = Math.Min(t1, t2);
            t_max[i] = Math.Max(t1, t2);
        }

        var t_enter = t_min.Max();
        var t_exit = t_max.Min();

        if (t_enter > t_exit || t_exit < 0)
        {
            //This condition means the ray is missing the bounding box.
            return double.MaxValue;
        }

        if (left_child == null && right_child == null)
        {
            // This condition means the node is the leaf node.
            return intersect_triangle(ray);
        }

        var hit1 = left_child!.intersect_aabb(ray);
        var hit2 = right_child!.intersect_aabb(ray);

        return hit1 < hit2 ? hit1 : hit2;
    }
}