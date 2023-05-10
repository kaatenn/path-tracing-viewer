namespace _3D_viewer.models;

public class Faces
{
    public int vertex1_id { get; set; }
    public int vertex2_id { get; set; }
    public int vertex3_id { get; set; }

    public bool? is_mirror { get; set; }
    public int object_id { get; set; }

    public Vertices? vertex1 { get; set; }
    public Vertices? vertex2 { get; set; }
    public Vertices? vertex3 { get; set; }
    
    public Objects? objects { get; set; }

    public override bool Equals(object? obj)
    {
        if (!(obj is Faces)) return false;
        var other = (Faces)obj;
        return other.vertex1_id == vertex1_id && other.vertex2_id == vertex2_id && other.vertex3_id == vertex3_id;
    }

    public override int GetHashCode()
    {
        unchecked //无需检查溢出
        {
            // 设置初始哈希码为第一个字段的值
            var hash_code = vertex1_id;

            // 使用 FNV-1a 哈希算法计算哈希码，乘以常数 397 是为了增加随机性
            hash_code = (hash_code * 397) ^ vertex2_id;
            hash_code ^= (hash_code * 397) ^ vertex3_id;

            // 返回最终哈希码
            return hash_code;
        }
    }
}