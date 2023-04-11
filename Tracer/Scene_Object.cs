using System.Collections.Generic;

namespace _3D_viewer.Tracer;

public class Scene_Object
{
    private List<Triangle> _triangles;

    public Bounding_Volume bounding_volume; 

    private double refractive_index { get; set; }
    
    public Scene_Object()
    {
        _triangles = new List<Triangle>();
        refractive_index = 1.0;
        bounding_volume = new Bounding_Volume();
    }
    
    
}