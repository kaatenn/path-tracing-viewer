using System;
using System.Windows.Media.Media3D;

namespace _3D_viewer.Tracer;

public class Bounding_Volume
{
    private Vector3D min { get; set; }
    private Vector3D max { get; set; }

    public Bounding_Volume()
    {
        this.min = new Vector3D();
        this.max = new Vector3D();
    }
    
    public Bounding_Volume(Vector3D min, Vector3D max)
    {
        this.min = min;
        this.max = max;
    }

    public void update_bounding_volume(Triangle triangle)
    {
        foreach (var vertex in triangle.vertices)
        {
            min = new Vector3D(
                Math.Min(min.X, vertex.X),
                Math.Min(min.Y, vertex.Y),
                Math.Min(min.Z, vertex.Z)
            );
            
            max = new Vector3D(
                Math.Max(max.X, vertex.X),
                Math.Max(max.Y, vertex.Y),
                Math.Max(max.Z, vertex.Z)
            );
        }
    }
}