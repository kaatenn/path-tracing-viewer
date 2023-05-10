using System.Collections.Generic;

namespace _3D_viewer.models;

public class Objects
{
    public int object_id { get; set; }
    
    public string object_name { get; set; }

    public ICollection<Faces> faces = new List<Faces>();
}