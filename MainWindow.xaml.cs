using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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
    private const double TOLERANCE = 1e-7; // For float comparing.
    private readonly Scene_Object scene_object = new(); // the scenes object for shading.
    private readonly WriteableBitmap writeable_bitmap; // The bitmap will be used for shading the image.
    private const double EYE_FOV = Math.PI * 2 / 3;
    private bool can_edit = false;
    private int[] edit_vertex_id = new int[3];
    private Triangle? edit_triangle;

    public Color general_color
    {
        get => (Color)GetValue(general_color_property);
        set => SetValue(general_color_property, value);
    }

    public readonly static DependencyProperty general_color_property =
        DependencyProperty.Register(nameof(general_color), typeof(Color), typeof(MainWindow), new PropertyMetadata(Colors.Red));

    private List<string> object_add_list;
    private List<string> object_rendered_list;
    private Color detail_color;


    /// <summary>
    ///     Constructor of the main window
    /// </summary>
    public MainWindow()
    {
        InitializeComponent();
        // Initialize the bitmap
        writeable_bitmap =
            new WriteableBitmap((int)PathTracingImage.Width, (int)PathTracingImage.Height, 96, 96, PixelFormats.Bgr32,
                null);
        Loaded += (_, _) => { PathTracingImage.Source = writeable_bitmap; };
        general_color = Colors.White;
        detail_color = Colors.White;
        ColorPickerShow.Background = new SolidColorBrush(detail_color);
        DataContext = this;
        refresh_lists();
    }

    /// <summary>
    /// Refresh all list in the UI
    /// </summary>
    private void refresh_lists()
    {
        object_add_list = read_object();
        object_add_list.Add("新建物体...");

        ObjectSelectAddComboBox.ItemsSource = object_add_list;
        ObjectSelectAddComboBox.SelectedIndex = 0;
        if (object_add_list.Count == 1)
        {
            ObjectSelectAddComboBox.Text = "";
            ObjectSelectAddComboBox.IsEditable = true;
        }
        else
        {
            ObjectSelectAddComboBox.Text = (string)ObjectSelectAddComboBox.SelectedItem;
            ObjectSelectAddComboBox.IsEditable = false;
        }

        object_rendered_list = read_object();

        ObjectSelectRenderedComboBox.ItemsSource = object_rendered_list;
        if (object_rendered_list.Count == 0)
        {
            ObjectSelectRenderedComboBox.IsEnabled = false;
            ButtonRefresh.IsEnabled = false;
        }
        else
        {
            ObjectSelectRenderedComboBox.SelectedIndex = 0;
        }
    }

    /// <summary>
    /// Read all object names from the database
    /// </summary>
    /// <returns>The name list</returns>
    private static List<string> read_object()
    {
        using var db = new App_Db_Context();
        var objects = db.objects.ToList();
        var result = objects.Select(obj => obj.object_name).ToList();
        return result;
    }

    /// <summary>
    ///     create a framework for modify the pixels.
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
    ///     Managing the path tracing
    /// </summary>
    /// <param name="pixel_values">The pixel value for the image.</param>
    /// <param name="width">The width of the pixel value.</param>
    /// <param name="height">The height of the pixel value.</param>
    /// <param name="stride">The address stride between every pixel.</param>
    /// <param name="bytes_per_pixel">The memory every pixel used.</param>
    /// <returns></returns>
    private byte[] path_tracing(byte[] pixel_values, int width, int height, int stride, int bytes_per_pixel)
    {
        // 遍历所有像素并修改颜色
        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
        {
            var index = y * stride + x * bytes_per_pixel;

            var newColor = new byte[3];
            if (scene_object.bvh_tree != null)
                newColor = Renderer.render_direct(scene_object.bvh_tree, x, y, width, height, EYE_FOV);
            pixel_values[index] = newColor[2];
            pixel_values[index + 1] = newColor[1];
            pixel_values[index + 2] = newColor[0];
            pixel_values[index + 3] = 255;
        }

        return pixel_values;
    }

    /// <summary>
    ///     Check if the input number is a float and it's between 0 to 1
    /// </summary>
    /// <param name="sender">The instance of the event sender</param>
    /// <param name="e">The instance of the event</param>
    private void float_text_input_handler(object sender, TextCompositionEventArgs e)
    {
        ((TextBox)sender).BorderBrush = new SolidColorBrush(Color.FromRgb(0x40, 0x9E, 0xFF));
        // Check if the input is in float format using regular expression
        var regex = new Regex("^-?[0-1]*\\.?[0-9]*$");
        var is_format = regex.IsMatch(e.Text);

        // If the input is not in float format, disable input
        if (!is_format)
        {
            e.Handled = true;
            return;
        }

        if (e.Text is "-" or ".") return;
        // Convert the input to float
        var value = float.Parse((sender as TextBox)?.Text + e.Text);

        // If the input is not between 0 and 1, disable input
        if ((!((TextBox)sender).Name.Contains('Z') && value is >= -1 and <= 1) ||
            (((TextBox)sender).Name.Contains('Z') && value is >= -1 and <= 0)) return;

        ((TextBox)sender).BorderBrush = new SolidColorBrush(Color.FromRgb(0xF5, 0x6C, 0x6C));

        e.Handled = true;
    }

    /// <summary>
    ///     The increase button click event, it will add one triangle into the database if it is legal.
    /// </summary>
    /// <param name="sender">The instance of the event sender</param>
    /// <param name="e">The instance of the event</param>
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
            var obj_name = ObjectSelectAddComboBox.Text;

            using var db_context = new App_Db_Context();
            // Check if vertices exist in database, and get their ids
            var vertex1 = db_context.vertices.FirstOrDefault(v =>
                Math.Abs(v.x - x1) < TOLERANCE && Math.Abs(v.y - y1) < TOLERANCE && Math.Abs(v.z - z1) < TOLERANCE);
            var vertex2 = db_context.vertices.FirstOrDefault(v =>
                Math.Abs(v.x - x2) < TOLERANCE && Math.Abs(v.y - y2) < TOLERANCE && Math.Abs(v.z - z2) < TOLERANCE);
            var vertex3 = db_context.vertices.FirstOrDefault(v =>
                Math.Abs(v.x - x3) < TOLERANCE && Math.Abs(v.y - y3) < TOLERANCE && Math.Abs(v.z - z3) < TOLERANCE);
            var obj = db_context.objects.FirstOrDefault(o => obj_name == o.object_name);

            // Add new vertices to database if they don't exist and get their ids
            if (vertex1 == null)
            {
                vertex1 = new Vertices
                {
                    x = x1, y = y1, z = z1
                };
                db_context.vertices.Add(vertex1);
                db_context.SaveChanges();
            }

            if (vertex2 == null)
            {
                vertex2 = new Vertices
                {
                    x = x2, y = y2, z = z2
                };
                db_context.vertices.Add(vertex2);
                db_context.SaveChanges();
            }

            if (vertex3 == null)
            {
                vertex3 = new Vertices
                {
                    x = x3, y = y3, z = z3
                };
                db_context.vertices.Add(vertex3);
                db_context.SaveChanges();
            }

            if (obj == null)
            {
                var new_object = new Objects
                {
                    object_name = obj_name
                };
                db_context.objects.Add(new_object);
                db_context.SaveChanges();
                obj = new_object;
            }

            // Create new face with vertex ids
            var new_face = new Faces
            {
                vertex1_id = vertex1.id, vertex2_id = vertex2.id, vertex3_id = vertex3.id, object_id = obj.object_id
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
            ObjectSelectAddComboBox.Text = "";

            AddButton.Background = new SolidColorBrush(Color.FromRgb(0x40, 0x9E, 0xFF));
            refresh_lists();
        }
        catch (Exception)
        {
            AddButton.Background = new SolidColorBrush(Color.FromRgb(0xF5, 0x6C, 0x6C));
        }
    }

    /// <summary>
    ///     Read all triangles from the database, you must refresh the BVH-Tree after this
    /// </summary>
    private void read_triangles()
    {
        var triangles = new List<Triangle>();
        using var context = new App_Db_Context();
        var vertices = context.vertices.ToList();
        var obj_id = context.objects.FirstOrDefault(o => o.object_name == (string)ObjectSelectRenderedComboBox.SelectedItem)!
            .object_id;
        var faces = context.faces
            .Where(f => f.object_id == obj_id)
            .ToList();
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
            var triangle = new Triangle(triangleVertices, new[]
            {
                detail_color.R, detail_color.G, detail_color.B
            });
            triangles.Add(triangle);
        }

        foreach (var triangle in triangles) scene_object.add_triangle(triangle);
    }

    /// <summary>
    ///     The refresh button click handler, it will refresh the bitmap.
    /// </summary>
    /// <param name="sender">The instance of the event sender</param>
    /// <param name="e">The instance of the event</param>
    private void refresh_button_click(object sender, RoutedEventArgs e)
    {
        if (object_rendered_list.Count == 0) return;
        scene_object.clear_triangles();
        read_triangles();
        scene_object.refresh_bvh();
        modify_pixels();
        PathTracingImage.MouseDown += PathTracingImage_OnMouseDown;
    }

    private void ColorDetailPicker_OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        // Add mouse move event
        ColorDetailPicker.MouseMove += ColorDetailPicker_OnMouseMove;
    }

    private void ColorDetailPicker_OnMouseUp(object sender, MouseButtonEventArgs e)
    {
        // Release mouse move event
        ColorDetailPicker.MouseMove -= ColorDetailPicker_OnMouseMove;
    }

    /// <summary>
    /// Select color by mouse position and general color selector
    /// </summary>
    /// <param name="sender">The instance of the event sender</param>
    /// <param name="e">The instance of the event</param>
    private void ColorDetailPicker_OnMouseMove(object sender, MouseEventArgs e)
    {
        var mouse_position = Mouse.GetPosition(ColorDetailPicker);
        var mid_pos = ColorDetailPicker.Width / 2;
        if (mouse_position.X < mid_pos && mouse_position.X > 0)
        {
            var offset_r = general_color.R / mid_pos;
            var offset_g = general_color.G / mid_pos;
            var offset_b = general_color.B / mid_pos;
            var offset_p = mouse_position.X;
            var r = offset_r * offset_p;
            var g = offset_g * offset_p;
            var b = offset_b * offset_p;
            detail_color = Color.FromRgb((byte)r, (byte)g, (byte)b);
            ColorPickerShow.Background = new SolidColorBrush(detail_color);
        }
        else if (mouse_position.X >= mid_pos && mouse_position.X < ColorDetailPicker.Width)
        {
            var offset_r = (255 - general_color.R) / mid_pos;
            var offset_g = (255 - general_color.G) / mid_pos;
            var offset_b = (255 - general_color.B) / mid_pos;
            var offset_p = mouse_position.X - mid_pos;
            var r = offset_r * offset_p + general_color.R;
            var g = offset_g * offset_p + general_color.G;
            var b = offset_b * offset_p + general_color.B;
            detail_color = Color.FromRgb((byte)r, (byte)g, (byte)b);
            ColorPickerShow.Background = new SolidColorBrush(detail_color);
        }
        else if (mouse_position.X < 0)
        {
            detail_color = Colors.Black;
            ColorPickerShow.Background = new SolidColorBrush(detail_color);
        }
        else
        {
            detail_color = Colors.White;
            ColorPickerShow.Background = new SolidColorBrush(detail_color);
        }
    }

    private void ColorGeneralPicker_OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        // Add mouse move
        ColorGeneralPicker.MouseMove += ColorGeneralPicker_OnMouseMove;
    }

    private void ColorGeneralPicker_OnMouseUp(object sender, MouseButtonEventArgs e)
    {
        // Release mouse move
        ColorGeneralPicker.MouseMove -= ColorGeneralPicker_OnMouseMove;
    }

    /// <summary>
    /// Select general color
    /// </summary>
    /// <param name="sender">The instance of the event sender</param>
    /// <param name="e">The instance of the event</param>
    private void ColorGeneralPicker_OnMouseMove(object sender, MouseEventArgs e)
    {
        var color = general_color;

        var mouse_y = Mouse.GetPosition(ColorGeneralPicker).Y;
        var ratio_y = mouse_y / ColorGeneralPicker.Height;

        switch (ratio_y)
        {
            case < 0:
                color = Colors.Black;
                break;
            case < 0.142:
                color.R = (byte)(255 * ratio_y / 0.142);
                color.G = 0;
                color.B = 0;
                break;
            case >= 0.142 and < 0.285:
                color.R = 0xff;
                color.G = (byte)(255 * (ratio_y - 0.142) / 0.142);
                color.B = 0;
                break;
            case >= 0.285 and < 0.428:
                color.R = (byte)(255 - 255 * (ratio_y - 0.285) / 0.142);
                color.G = 0xff;
                color.B = 0;
                break;
            case >= 0.428 and < 0.571:
                color.R = 0;
                color.G = 0xff;
                color.B = (byte)(255 * (ratio_y - 0.428) / 0.142);
                break;
            case >= 0.571 and < 0.714:
                color.R = 0;
                color.G = (byte)(255 - 255 * (ratio_y - 0.571) / 0.142);
                color.B = 0xff;
                break;
            case >= 0.714 and < 0.857:
                color.R = (byte)(255 * (ratio_y - 0.714) / 0.142);
                color.G = 0;
                color.B = 0xff;
                break;
            case >= 0.857 and < 1:
                color.R = 0xff;
                color.G = (byte)(255 * (ratio_y - 0.857) / 0.142);
                color.B = 0xff;
                break;
            default:
                color = Colors.White;
                break;
        }

        general_color = color;
        detail_color = color;

        ColorPickerShow.Background = new SolidColorBrush(detail_color);
    }

    /// <summary>
    /// Set combo box status, if selected item is "new object..." than allow edit, or show the item
    /// </summary>
    /// <param name="sender">The instance of the event sender</param>
    /// <param name="e">The instance of the event</param>
    private void ObjectSelectComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if ((string)ObjectSelectAddComboBox.SelectedItem != "新建物体...")
        {
            ObjectSelectAddComboBox.Text = (string)ObjectSelectAddComboBox.SelectedItem;
            ObjectSelectAddComboBox.IsEditable = false;
        }
        else
        {
            ObjectSelectAddComboBox.Text = "";
            ObjectSelectAddComboBox.IsEditable = true;
            e.Handled = true;
        }
    }

    /// <summary>
    /// Find the first hit triangle by input coordinate
    /// </summary>
    /// <param name="x">The x of the coordinate</param>
    /// <param name="y">The y of the coordinate</param>
    private void find_triangle_on_pixel(double x, double y)
    {
        var width = writeable_bitmap.PixelWidth;
        var height = writeable_bitmap.PixelHeight;
        var aspect_radio = width / (double)height;
        var ray_x = (2 * (x + 0.5) / width - 1) * aspect_radio * Math.Tan(EYE_FOV / 2);
        var ray_y = (1 - 2 * (y + 0.5) / height) * Math.Tan(EYE_FOV / 2);
        var ray = new Ray(new Vector3D(0, 0, 0), new Vector3D(ray_x, ray_y, -1));
        var intersect = scene_object.bvh_tree?.intersect(ray);
        if (intersect is { happened: true })
        {
            edit_triangle = intersect.triangle;
            UpdateTrianglePoint1XTextBox.Text = edit_triangle?.vertices[0].X.ToString("F7");
            UpdateTrianglePoint2XTextBox.Text = edit_triangle?.vertices[1].X.ToString("F7");
            UpdateTrianglePoint3XTextBox.Text = edit_triangle?.vertices[2].X.ToString("F7");
            UpdateTrianglePoint1YTextBox.Text = edit_triangle?.vertices[0].Y.ToString("F7");
            UpdateTrianglePoint2YTextBox.Text = edit_triangle?.vertices[1].Y.ToString("F7");
            UpdateTrianglePoint3YTextBox.Text = edit_triangle?.vertices[2].Y.ToString("F7");
            UpdateTrianglePoint1ZTextBox.Text = edit_triangle?.vertices[0].Z.ToString("F7");
            UpdateTrianglePoint2ZTextBox.Text = edit_triangle?.vertices[1].Z.ToString("F7");
            UpdateTrianglePoint3ZTextBox.Text = edit_triangle?.vertices[2].Z.ToString("F7");
        }
    }
    
    /// <summary>
    /// Allow update or update the triangle
    /// </summary>
    /// <param name="sender">The instance of the event sender</param>
    /// <param name="e">The instance of the event</param>
    private void button_update_click(object sender, RoutedEventArgs e)
    {
        if (!can_edit)
        {
            set_text_box_status(true);
            UpdateButton.Background = new SolidColorBrush(Color.FromRgb(0x40, 0x9E, 0xFF));
            using var context = new App_Db_Context();
            for (int i = 0; i < 3; i++)
            {
                edit_vertex_id[i] = context.vertices.FirstOrDefault(v =>
                    Math.Abs(v.x - edit_triangle!.vertices[i].X) < TOLERANCE &&
                    Math.Abs(v.y - edit_triangle!.vertices[i].Y) < TOLERANCE &&
                    Math.Abs(v.z - edit_triangle.vertices[i].Z) < TOLERANCE)!.id;
            }

            can_edit = true;
        }
        else
        {
            using var context = new App_Db_Context();
            var vertex = context.vertices.FirstOrDefault(v => v.id == edit_vertex_id[0]);
            if (vertex != null)
            {
                vertex.x = double.Parse(UpdateTrianglePoint1XTextBox.Text);
                vertex.y = double.Parse(UpdateTrianglePoint1YTextBox.Text);
                vertex.z = double.Parse(UpdateTrianglePoint1ZTextBox.Text);
                context.vertices.Update(vertex);
                context.SaveChanges();
            }
            vertex = context.vertices.FirstOrDefault(v => v.id == edit_vertex_id[1]);
            if (vertex != null)
            {
                vertex.x = double.Parse(UpdateTrianglePoint2XTextBox.Text);
                vertex.y = double.Parse(UpdateTrianglePoint2YTextBox.Text);
                vertex.z = double.Parse(UpdateTrianglePoint2ZTextBox.Text);
                context.vertices.Update(vertex);
                context.SaveChanges();
            }
            vertex = context.vertices.FirstOrDefault(v => v.id == edit_vertex_id[2]);
            if (vertex != null)
            {
                vertex.x = double.Parse(UpdateTrianglePoint3XTextBox.Text);
                vertex.y = double.Parse(UpdateTrianglePoint3YTextBox.Text);
                vertex.z = double.Parse(UpdateTrianglePoint3ZTextBox.Text);
                context.vertices.Update(vertex);
                context.SaveChanges();
            }

            edit_vertex_id = new int[3];
            edit_triangle = null;
            set_text_box_status(false);
            scene_object.clear_triangles();
            read_triangles();
            scene_object.refresh_bvh();
            modify_pixels();
            UpdateButton.Background = new SolidColorBrush(Color.FromRgb(0xE6, 0xA2, 0x3C));
            can_edit = false;
            clear_update_text_box();
        }
    }

    /// <summary>
    /// Set text box IsEnabled by input status
    /// </summary>
    /// <param name="status">The input status</param>
    private void set_text_box_status(bool status)
    {
        UpdateTrianglePoint1XTextBox.IsEnabled = status;
        UpdateTrianglePoint2XTextBox.IsEnabled = status;
        UpdateTrianglePoint3XTextBox.IsEnabled = status;
        UpdateTrianglePoint1YTextBox.IsEnabled = status;
        UpdateTrianglePoint2YTextBox.IsEnabled = status;
        UpdateTrianglePoint3YTextBox.IsEnabled = status;
        UpdateTrianglePoint1ZTextBox.IsEnabled = status;
        UpdateTrianglePoint2ZTextBox.IsEnabled = status;
        UpdateTrianglePoint3ZTextBox.IsEnabled = status;
    }

    /// <summary>
    /// Delete the triangle and clear the database
    /// </summary>
    /// <param name="sender">The instance of the event sender</param>
    /// <param name="e">The instance of the event</param>
    private void button_delete_click(object sender, RoutedEventArgs e)
    {
        using var context = new App_Db_Context();
        for (int i = 0; i < 3; i++)
        {
            edit_vertex_id[i] = context.vertices.FirstOrDefault(v =>
                Math.Abs(v.x - edit_triangle!.vertices[i].X) < TOLERANCE &&
                Math.Abs(v.y - edit_triangle!.vertices[i].Y) < TOLERANCE &&
                Math.Abs(v.z - edit_triangle.vertices[i].Z) < TOLERANCE)!.id;
        }
        var selected_object = context.objects.FirstOrDefault(o => o.object_name == (string)ObjectSelectRenderedComboBox.SelectedItem)!;
        var selected_id = selected_object.object_id;
        var face = context.faces.FirstOrDefault(f =>
            f.vertex1_id == edit_vertex_id[0] && f.vertex2_id == edit_vertex_id[1] && f.vertex3_id == edit_vertex_id[2] &&
            f.object_id == selected_id)!;
        var object_count = context.faces.Where(f => f.object_id == selected_id).ToList().Count;
        var vertices_counts = new int[3];
        for (var i = 0; i < 3; i++)
        {
            var i1 = i;
            vertices_counts[i] = context.faces.Where(f =>
                f.vertex1_id == edit_vertex_id[i1] || f.vertex2_id == edit_vertex_id[i1] || f.vertex3_id == edit_vertex_id[i1]).ToList().Count;
        }

        context.Remove(face);
        context.SaveChanges();
        if (object_count == 1)
        {
            context.Remove(selected_object);
            context.SaveChanges();
        }

        for (var i = 0; i < 3; i++)
        {
            if (vertices_counts[i] == 1)
            {
                var remove_vertex = context.vertices.FirstOrDefault(v => v.id == edit_vertex_id[i])!;
                context.Remove(remove_vertex);
                context.SaveChanges();
            }
        }
        
        refresh_lists();
        if (object_rendered_list.Count() != 0)
        {
            scene_object.clear_triangles();
            read_triangles();
            scene_object.refresh_bvh();
            modify_pixels(); 
        }

        clear_update_text_box();
        edit_vertex_id = new int[3];
        edit_triangle = null;
    }

    /// <summary>
    /// Clear the Text in all text boxes
    /// </summary>
    private void clear_update_text_box()
    {
        UpdateTrianglePoint1XTextBox.Text = "";
        UpdateTrianglePoint2XTextBox.Text = "";
        UpdateTrianglePoint3XTextBox.Text = "";
        UpdateTrianglePoint1YTextBox.Text = "";
        UpdateTrianglePoint2YTextBox.Text = "";
        UpdateTrianglePoint3YTextBox.Text = "";
        UpdateTrianglePoint1ZTextBox.Text = "";
        UpdateTrianglePoint2ZTextBox.Text = "";
        UpdateTrianglePoint3ZTextBox.Text = "";
    }

    private void PathTracingImage_OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        var mouse_x = Mouse.GetPosition(PathTracingImage).X;
        var mouse_y = Mouse.GetPosition(PathTracingImage).Y;

        find_triangle_on_pixel(mouse_x, mouse_y);
    }
}