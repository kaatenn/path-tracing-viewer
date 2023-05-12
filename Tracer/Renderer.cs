using System;
using System.Windows.Media.Media3D;

namespace _3D_viewer.Tracer;

/// <summary>
///     A static class for rendering
/// </summary>
public static class Renderer
{
    private const int SAMPLE_TIME = 4; /* 1600 */

    /// <summary>
    ///     Render the input pixel
    /// </summary>
    /// <param name="root">The root of the BVH represent the scene</param>
    /// <param name="screen_x">The pixel coordinate in axis x</param>
    /// <param name="screen_y">The pixel coordinate in axis y</param>
    /// <param name="screen_width">The width of the screen</param>
    /// <param name="screen_height">The height of the screen</param>
    /// <param name="eye_fov">The eye fov of the camera</param>
    /// <returns>return the render result of the pixel</returns>
    public static byte[] render_direct(Bvh_Tree_Node root, int screen_x, int screen_y, int screen_width,
        int screen_height, double eye_fov)
    {
        var color = new Vector3D();

        // sample the path for 1000 times
        for (var i = 0; i < SAMPLE_TIME; i++)
        {
            var ray = ray_generation(screen_x, screen_y, screen_width, screen_height, eye_fov);
            var intersect = root.intersect(ray);

            var render_result = new[]
            {
                (byte)Math.Min(intersect.color[0] * 0.08 / ((Math.Pow(intersect.coordinate.X - 0, 2) +
                                                           Math.Pow(intersect.coordinate.Y - 0, 2) +
                                                           Math.Pow(intersect.coordinate.Z - 0, 2)) * 0.95 /
                    Math.PI * intersect.cosine), 255),
                (byte)Math.Min(intersect.color[1] * 0.08 / ((Math.Pow(intersect.coordinate.X - 0, 2) +
                                                           Math.Pow(intersect.coordinate.Y - 0, 2) +
                                                           Math.Pow(intersect.coordinate.Z - 0, 2)) * 0.95 /
                    Math.PI * intersect.cosine), 255),
                (byte)Math.Min(intersect.color[2] * 0.08 / ((Math.Pow(intersect.coordinate.X - 0, 2) +
                                                           Math.Pow(intersect.coordinate.Y - 0, 2) +
                                                           Math.Pow(intersect.coordinate.Z - 0, 2)) * 0.95 /
                    Math.PI * intersect.cosine), 255)
            };
            /*render_result = shade(intersect, Bvh_Tree_Node root);*/
            color.X += render_result[0];
            color.Y += render_result[1];
            color.Z += render_result[2];
        }

        var result = new[]
            { (byte)(color.X / SAMPLE_TIME), (byte)(color.Y / SAMPLE_TIME), (byte)(color.Z / SAMPLE_TIME) };

        return result;
    }

    /*
    /// <summary>
    /// Recursively shade the pixel
    /// </summary>
    /// <param name="intersect">The former intersect</param>
    /// <returns>Rendering result color</returns>
    private static byte[] shade(Intersection intersect, Bvh_Tree_Node root)
    {
        const int P_RR = 4;
        var random = new Random();
        var rr = random.Next(0, 6);

        if (rr < P_RR)
        {
            var ray = new Ray(intersect.coordinate, new Vector3D(random.NextDouble(), random.NextDouble(), random.NextDouble()));
            if (Math.Abs(-ray.origin.X / ray.direction.X - -ray.origin.Y / ray.direction.Y) < 10e-7 && Math.Abs(-ray.origin.X / ray.direction.X - -ray.origin.Z / ray.direction.Z) < 10e-7)
            {
                var render_result = new[]
                {
                    (byte)Math.Min(intersect.color[0] * 0.08 / ((Math.Pow(intersect.coordinate.X - 0, 2) +
                                                                 Math.Pow(intersect.coordinate.Y - 0, 2) +
                                                                 Math.Pow(intersect.coordinate.Z - 0, 2)) * 0.95 /
                        Math.PI * intersect.cosine), 255),
                    (byte)Math.Min(intersect.color[1] * 0.08 / ((Math.Pow(intersect.coordinate.X - 0, 2) +
                                                                 Math.Pow(intersect.coordinate.Y - 0, 2) +
                                                                 Math.Pow(intersect.coordinate.Z - 0, 2)) * 0.95 /
                        Math.PI * intersect.cosine), 255),
                    (byte)Math.Min(intersect.color[2] * 0.08 / ((Math.Pow(intersect.coordinate.X - 0, 2) +
                                                                 Math.Pow(intersect.coordinate.Y - 0, 2) +
                                                                 Math.Pow(intersect.coordinate.Z - 0, 2)) * 0.95 /
                        Math.PI * intersect.cosine), 255)
                };
                return render_result;
            }
            else
            {
                var new_intersect = root.intersect(ray);
                var render_result = new[]
                {
                    (byte)Math.Min(intersect.color[0] * 0.08 / ((Math.Pow(shade(new_intersect, root)[0] - 0, 2) +
                                                                 Math.Pow(shade(new_intersect, root)[0] - 0, 2) +
                                                                 Math.Pow(shade(new_intersect, root)[0] - 0, 2)) * 0.95 /
                        Math.PI * intersect.cosine), 255),
                    (byte)Math.Min(intersect.color[1] * 0.08 / ((Math.Pow(shade(new_intersect, root)[1] - 0, 2) +
                                                                 Math.Pow(shade(new_intersect, root)[1] - 0, 2) +
                                                                 Math.Pow(shade(new_intersect, root)[1] - 0, 2)) * 0.95 /
                        Math.PI * intersect.cosine), 255),
                    (byte)Math.Min(intersect.color[2] * 0.08 / ((Math.Pow(shade(new_intersect, root)[2] - 0, 2) +
                                                                 Math.Pow(shade(new_intersect, root)[2] - 0, 2) +
                                                                 Math.Pow(shade(new_intersect, root)[2] - 0, 2)) * 0.95 /
                        Math.PI * intersect.cosine), 255)
                };
                return render_result;
            }
        }
        else
        {
            return new byte[] {0, 0, 0};
        }
    }*/

    /// <summary>
    ///     Randomly sample the path in one pixel
    /// </summary>
    /// <param name="screen_x">The pixel coordinate in axis x</param>
    /// <param name="screen_y">The pixel coordinate in axis y</param>
    /// <param name="screen_width">The width of the screen</param>
    /// <param name="screen_height">The height of the screen</param>
    /// <param name="eye_fov">The eye fov of the camera</param>
    /// <returns></returns>
    private static Ray ray_generation(int screen_x, int screen_y, int screen_width, int screen_height, double eye_fov)
    {
        var random = new Random();
        var x_offset = random.NextDouble();
        var y_offset = random.NextDouble();
        var aspect_radio = screen_width / (double)screen_height;
        var ray_x = (2 * (screen_x + x_offset) / screen_width - 1) * aspect_radio * Math.Tan(eye_fov / 2);
        var ray_y = (1 - 2 * (screen_y + y_offset) / screen_height) * Math.Tan(eye_fov / 2);

        var ray = new Ray(new Vector3D(0, 0, 0), new Vector3D(ray_x, ray_y, -1));
        return ray;
    }
}