using System;
using System.Windows.Media.Media3D;

namespace _3D_viewer.Tracer;

/// <summary>
///     A static class for rendering
/// </summary>
public static class Renderer
{
    public static bool rendering(Bvh_Tree_Node root, int screen_x, int screen_y, int screen_width, int screen_height, double eye_fov)
    {
        var aspect_radio = screen_width / (double)screen_height;
        var ray_x = (2 * (screen_x + 0.5) / screen_width - 1) * aspect_radio * Math.Tan(eye_fov / 2);
        var ray_y = (1 - 2 * (screen_y + 0.5) / screen_height) * Math.Tan(eye_fov / 2);

        var ray = new Ray(new Vector3D(0, 0, 0), new Vector3D(ray_x, ray_y, -1));

        return !(Math.Abs(root.intersect_aabb(ray) - double.MaxValue) < 1e-7);
    }
}