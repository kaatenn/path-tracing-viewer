using System.Windows.Media.Media3D;

namespace _3D_viewer.Tracer;

/// <summary>
///     Represent the path of the ray
/// </summary>
public class Path
{
    public Vector3D attenuation; // The attenuation of the path
    public int depth; // The recursion depth of the path
    public Ray ray; // The ray belongs to the path

    /// <summary>
    ///     Constructor to create an instance of the path
    /// </summary>
    /// <param name="ray">The ray belongs to the path</param>
    /// <param name="attenuation">The attenuation of the path</param>
    /// <param name="depth">The recursion depth of the path</param>
    public Path(Ray ray, Vector3D attenuation, int depth)
    {
        this.ray = ray;
        this.attenuation = attenuation;
        this.depth = depth;
    }
}