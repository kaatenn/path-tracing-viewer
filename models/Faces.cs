namespace _3D_viewer.models
{
    public class Faces
    {
        public int triangle1_id { get; set; }
        public int triangle2_id { get; set; }
        public int triangle3_id { get; set; }

        public Triangles? triangle1 { get; set; }
        public Triangles? triangle2 { get; set; }
        public Triangles? triangle3 { get; set; }

        public override bool Equals(object? obj)
        {
            if (!(obj is Faces)) return false;
            var other = (Faces)obj;
            return other.triangle1_id == triangle1_id && other.triangle2_id == triangle2_id && other.triangle3_id == triangle3_id;
        }

        public override int GetHashCode()
        {
            unchecked //无需检查溢出
            {
                // 设置初始哈希码为第一个字段的值
                var hash_code = triangle1_id;

                // 使用 FNV-1a 哈希算法计算哈希码，乘以常数 397 是为了增加随机性
                hash_code = (hash_code * 397) ^ triangle2_id;
                hash_code ^= (hash_code * 397) ^ triangle3_id;

                // 返回最终哈希码
                return hash_code;
            }
        }
    }
}
