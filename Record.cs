using System.Collections.Generic;
using _3D_viewer.models;

namespace _3D_viewer;

public class Record
{
    public string object_id;
    public List<List<Record_Vertex>> record_vertices;

    public Record(string object_id, List<List<Record_Vertex>> record_vertices)
    {
        this.object_id = object_id;
        this.record_vertices = record_vertices;
    }
}

public class Record_Vertex
{
    public double x, y, z;

    public Record_Vertex(double x, double y, double z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public static Record_Vertex create_from_model(Vertices? vertices)
    {
        return new Record_Vertex(vertices!.x, vertices.y, vertices.z);
    }
}