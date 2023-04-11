using System.Windows.Media.Media3D;

namespace _3D_viewer.Tracer;

public class Triangle
{
    public Point3D[] vertices { get; set; }
    private readonly Vector3D[] edge = new Vector3D[3];
    private Surface surface;

    public Triangle(Point3D[] vertices)
    {
        this.vertices = vertices;
        for (var i = 0; i < 3; i++)
        {
            edge[i] = new Vector3D(vertices[(i + 1) % 3].X - vertices[i].X, vertices[(i + 1) % 3].Y - vertices[i].Y,
                vertices[(i + 1) % 3].Z - vertices[i].Z);
        }

        surface = new Surface(vertices[0], Vector3D.CrossProduct(edge[1], edge[2]));
    }
}