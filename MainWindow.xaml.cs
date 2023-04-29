using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using _3D_viewer.models;
using _3D_viewer.Tracer;

namespace _3D_viewer;

/// <summary>
///     Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow
{
    private readonly WriteableBitmap writeable_bitmap; // The bitmap will be used for shading the image.
    private const double TOLERANCE = 1e-7; // For float comparing.
    private readonly Scene_Object scene_object = new Scene_Object(); // the scenes object for shading.

    /// <summary>
    /// create a framework for modify the pixels.
    /// </summary>
    private void modify_pixels()
    {
        var width = writeable_bitmap.PixelWidth;
        var height = writeable_bitmap.PixelHeight;
        const int bytesPerPixel = 4; // 每个像素使用 4 字节 (ARGB)
        var stride = width * bytesPerPixel;
        var pixelValues = new byte[height * stride];
        // 将 WriteableBitmap 中的像素复制到数组中
        writeable_bitmap.CopyPixels(pixelValues, stride, 0);

        // 调用 path_tracing 函数
        pixelValues = path_tracing(pixelValues, width, height, stride, bytesPerPixel);

        // 将修改后的像素数据写回 WriteableBitmap
        writeable_bitmap.WritePixels(new Int32Rect(0, 0, width, height), pixelValues, stride, 0);
    }

    /// <summary>
    /// Managing the path tracing
    /// </summary>
    /// <param name="pixelValues">The pixel value for the image.</param>
    /// <param name="width">The width of the pixel value.</param>
    /// <param name="height">The height of the pixel value.</param>
    /// <param name="stride">The address stride between every pixel.</param>
    /// <param name="bytesPerPixel">The memory every pixel used.</param>
    /// <returns></returns>
    private byte[] path_tracing(byte[] pixelValues, int width, int height, int stride, int bytesPerPixel)
    {
        read_triangles();
        scene_object.refresh_bvh();
        var bvh = scene_object.bvh_tree;
        // 遍历所有像素并修改颜色
        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var index = y * stride + x * bytesPerPixel;

                var newColor = Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF);
                var is_hit_triangle = scene_object.bvh_tree != null &&
                                      Renderer.rendering(scene_object.bvh_tree, x, y, width, height, Math.PI * 2 / 3);
                if (!is_hit_triangle) continue;
                pixelValues[index] = newColor.B;
                pixelValues[index + 1] = newColor.G;
                pixelValues[index + 2] = newColor.R;
                pixelValues[index + 3] = newColor.A;
            }
        }

        return pixelValues;
    }

    public MainWindow()
    {
        InitializeComponent();
        writeable_bitmap =
            new WriteableBitmap(720, 675, 96, 96, PixelFormats.Bgr32, null);

        Loaded += (_, _) => { PathTracingImage.Source = writeable_bitmap; };
    }

    private void float_text_input_handler(object sender, TextCompositionEventArgs e)
    {
        // Check if the input is in float format using regular expression
        var regex = new Regex("^-?[0-9]*\\.?[0-9]*$");
        var isFloat = regex.IsMatch(e.Text);

        // If the input is not in float format, disable input
        if (!isFloat)
        {
            e.Handled = true;
            return;
        }

        if (e.Text is "-" or ".") return;
        // Convert the input to float
        var value = float.Parse((sender as TextBox)?.Text + e.Text);

        // If the input is not between 0 and 1, disable input
        if (value is >= -1 and <= 1) return;

        var text_block = (TextBlock?)FindName("TrianglePointsTextBlock");
        if (text_block == null) return;
        text_block.Foreground = new SolidColorBrush(Colors.Red);
        Task.Delay(3000).ContinueWith(_ =>
            text_block.Dispatcher.Invoke(() => text_block.Foreground = new SolidColorBrush(Colors.Black)));

        e.Handled = true;
    }

    private void button_increase_click(object sender, RoutedEventArgs e)
    {
        try
        {
            var x1 = float.Parse(TrianglePoint1XTextBox.Text);
            var y1 = float.Parse(TrianglePoint1YTextBox.Text);
            var z1 = float.Parse(TrianglePoint1ZTextBox.Text);
            var x2 = float.Parse(TrianglePoint2XTextBox.Text);
            var y2 = float.Parse(TrianglePoint2YTextBox.Text);
            var z2 = float.Parse(TrianglePoint2ZTextBox.Text);
            var x3 = float.Parse(TrianglePoint3XTextBox.Text);
            var y3 = float.Parse(TrianglePoint3YTextBox.Text);
            var z3 = float.Parse(TrianglePoint3ZTextBox.Text);

            using var db_context = new App_Db_Context();
            // Check if vertices exist in database, and get their ids
            var vertex1 = db_context.vertices.FirstOrDefault(v =>
                Math.Abs(v.x - x1) < TOLERANCE && Math.Abs(v.y - y1) < TOLERANCE && Math.Abs(v.z - z1) < TOLERANCE);
            var vertex2 = db_context.vertices.FirstOrDefault(v =>
                Math.Abs(v.x - x2) < TOLERANCE && Math.Abs(v.y - y2) < TOLERANCE && Math.Abs(v.z - z2) < TOLERANCE);
            var vertex3 = db_context.vertices.FirstOrDefault(v =>
                Math.Abs(v.x - x3) < TOLERANCE && Math.Abs(v.y - y3) < TOLERANCE && Math.Abs(v.z - z3) < TOLERANCE);

            // Add new vertices to database if they don't exist and get their ids
            if (vertex1 == null)
            {
                vertex1 = new Vertices { x = x1, y = y1, z = z1 };
                db_context.vertices.Add(vertex1);
                db_context.SaveChanges();
            }

            if (vertex2 == null)
            {
                vertex2 = new Vertices { x = x2, y = y2, z = z2 };
                db_context.vertices.Add(vertex2);
                db_context.SaveChanges();
            }

            if (vertex3 == null)
            {
                vertex3 = new Vertices { x = x3, y = y3, z = z3 };
                db_context.vertices.Add(vertex3);
                db_context.SaveChanges();
            }

            // Create new face with vertex ids
            var new_face = new Faces
            {
                vertex1_id = vertex1.id, vertex2_id = vertex2.id, vertex3_id = vertex3.id,
                is_mirror = IsMirrorCheckBox.IsChecked
            };
            db_context.faces.Add(new_face);
            db_context.SaveChanges();

            TrianglePoint1XTextBox.Text = "";
            TrianglePoint2XTextBox.Text = "";
            TrianglePoint3XTextBox.Text = "";
            TrianglePoint1YTextBox.Text = "";
            TrianglePoint2YTextBox.Text = "";
            TrianglePoint3YTextBox.Text = "";
            TrianglePoint1ZTextBox.Text = "";
            TrianglePoint2ZTextBox.Text = "";
            TrianglePoint3ZTextBox.Text = "";
            IsMirrorCheckBox.IsChecked = false;
        }
        catch (Exception)
        {
            var text_block = (TextBlock?)FindName("TrianglePointsTextBlock");
            if (text_block != null)
            {
                text_block.Foreground = new SolidColorBrush(Colors.Red);
                Task.Delay(3000).ContinueWith(_ =>
                    text_block.Dispatcher.Invoke(() => text_block.Foreground = new SolidColorBrush(Colors.Black)));
                throw;
            }
        }
    }

    private void read_triangles()
    {
        var triangles = new List<Triangle>();
        using var context = new App_Db_Context();
        var vertices = context.vertices.ToList();
        var faces = context.faces.ToList();
        foreach (var face in faces)
        {
            var triangleVertices = new Vector3D[3];
            triangleVertices[0] = new Vector3D(
                vertices.FirstOrDefault(v => face.vertex1 != null && v.id == face.vertex1.id)?.x ?? 0,
                vertices.FirstOrDefault(v => face.vertex1 != null && v.id == face.vertex1.id)?.y ?? 0,
                vertices.FirstOrDefault(v => face.vertex1 != null && v.id == face.vertex1.id)?.z ?? 0);
            triangleVertices[1] = new Vector3D(
                vertices.FirstOrDefault(v => face.vertex2 != null && v.id == face.vertex2.id)?.x ?? 0,
                vertices.FirstOrDefault(v => face.vertex2 != null && v.id == face.vertex2.id)?.y ?? 0,
                vertices.FirstOrDefault(v => face.vertex2 != null && v.id == face.vertex2.id)?.z ?? 0);
            triangleVertices[2] = new Vector3D(
                vertices.FirstOrDefault(v => face.vertex3 != null && v.id == face.vertex3.id)?.x ?? 0,
                vertices.FirstOrDefault(v => face.vertex3 != null && v.id == face.vertex3.id)?.y ?? 0,
                vertices.FirstOrDefault(v => face.vertex3 != null && v.id == face.vertex3.id)?.z ?? 0);
            var triangle = new Triangle(triangleVertices, face.is_mirror != null && face.is_mirror.Value ? 0.95 : 0.05);
            triangles.Add(triangle);
        }

        foreach (var triangle in triangles)
        {
            scene_object.add_triangle(triangle);
        }

        scene_object.refresh_bvh();
    }

    private void refresh_button_click(object sender, RoutedEventArgs e)
    {
        modify_pixels();
    }
}