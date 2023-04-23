using System;
using System.Collections.Generic;
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

    /// <summary>
    /// Constructor to create an instance of the default BVH_Tree_Node class.
    /// </summary>
    /// <param name="aabb">The aabb of the node</param>
    /// <param name="left_child">The left child of the BVH-Tree node</param>
    /// <param name="right_child">The right child of the BVH-Tree node</param>
    private Bvh_Tree_Node(Aabb aabb, Bvh_Tree_Node? left_child = null, Bvh_Tree_Node? right_child = null)
    {
        this.aabb = aabb;
        this.left_child = left_child;
        this.right_child = right_child;
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
            return new Bvh_Tree_Node(new Aabb(triangles));
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
        var left_node = Bvh_Tree_Node.build_bvh(left_triangles);
        var right_node = Bvh_Tree_Node.build_bvh(right_triangles);

        return new Bvh_Tree_Node(aabb, left_node, right_node);
    }
}