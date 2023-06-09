﻿using System.Collections.Generic;

namespace _3D_viewer.models;

public class Vertices
{
    public int id { get; set; }
    public double x { get; set; }
    public double y { get; set; }
    public double z { get; set; }

    public ICollection<Faces> faces1 { get; set; } = new List<Faces>();
    public ICollection<Faces> faces2 { get; set; } = new List<Faces>();
    public ICollection<Faces> faces3 { get; set; } = new List<Faces>();
}