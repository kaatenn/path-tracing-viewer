using System.Collections.Generic;

namespace _3D_viewer.Tracer;

/// <summary>
///     Represents a object in the scene.
/// </summary>
public class Scene_Object
{
    private readonly List<Triangle> _triangles; // The list of triangles in the object.

    /// <summary>
    ///     Constructor to create an instance of the default Scene_Object class.
    /// </summary>
    public Scene_Object()
    {
        _triangles = new List<Triangle>();
        bvh_tree = null;
    }

    public Bvh_Tree_Node? bvh_tree { get; private set; } // the BVH of the object

    /// <summary>
    ///     Refresh the BVH-Tree by triangles
    /// </summary>
    public void refresh_bvh()
    {
        bvh_tree = Bvh_Tree_Node.build_bvh(_triangles);
    }

    /// <summary>
    ///     Clear the BVH-Tree
    /// </summary>
    public void clear_triangles()
    {
        _triangles.Clear();
    }

    /// <summary>
    ///     Add triangle to this object and update the bounding volume
    /// </summary>
    /// <param name="triangle">Added triangle</param>
    public void add_triangle(Triangle triangle)
    {
        _triangles.Add(triangle);
    }
}