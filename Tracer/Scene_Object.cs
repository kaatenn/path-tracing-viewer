using System.Collections.Generic;

namespace _3D_viewer.Tracer;

/// <summary>
///     Represents a object in the scene.
/// </summary>
public class Scene_Object
{
    private List<Triangle> _triangles; // The list of triangles in the object.

    public Bounding_Volume bounding_volume; // The bounding volume of the object.

    private double refractive_index { get; set; } // the refractive index of the object.
    
    /// <summary>
    /// Constructor to create an instance of the default Scene_Object class.
    /// </summary>
    public Scene_Object()
    {
        _triangles = new List<Triangle>();
        refractive_index = 1.0;
        bounding_volume = new Bounding_Volume();
    }
    
    
}