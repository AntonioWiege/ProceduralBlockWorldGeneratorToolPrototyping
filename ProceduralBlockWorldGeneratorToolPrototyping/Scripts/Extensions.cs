/*Antonio Wiege*/
using UnityEngine;
namespace ProceduralBlockWorldGeneratorToolPrototyping
{
    /* Table of contents
    Int3
    VectorN
    Array
     */
    public static class Extensions
    {

        //
        //      Int3
        //

        public static Int3 ToInt3(this Vector3 v)
        {
            return Int3.ToInt3(v);
        }

        public static Int3 AsInt3(this Vector3 v) => Int3.ToInt3(v);

        public static int LocalPosToLinearID(this Int3 pos) => pos.x + pos.y * LandscapeTool.ChunkScale + pos.z * LandscapeTool.ChunkScale * LandscapeTool.ChunkScale;
        public static int ToLinearChunkScaleIndex(this Int3 pos) => pos.x + pos.y * LandscapeTool.ChunkScale + pos.z * LandscapeTool.ChunkScale * LandscapeTool.ChunkScale;

        //
        //      VectorN
        //

        public static Vector3 Divide(this Vector3 a, Vector3 b) => new Vector3(a.x / b.x, a.y / b.y, a.z / b.z);

        public static float Volume(this Vector3 v) => v.x * v.y * v.z;

        public static Vector3[] OuterPoints(this Bounds b)
        {
            return new Vector3[] {b.min,
            new Vector3(b.max.x,b.min.y,b.min.z),
        new Vector3(b.min.x,b.max.y,b.min.z),
        new Vector3(b.max.x,b.max.y,b.min.z),
        new Vector3(b.min.x,b.min.y,b.max.z),
        new Vector3(b.max.x,b.min.y,b.max.z),
        new Vector3(b.min.x,b.max.y,b.max.z),
        new Vector3(b.max.x,b.max.y,b.max.z)};
        }

        public static Vector4 Mul(this Vector4 a, Vector4 b) => new Vector4(a.x * b.x, a.y * b.y, a.z * b.z, a.w * b.w);

        //
        //      Array
        //

        public static bool ArrayEqual<T>(this T[] first, T[] second)
        {
            if (ReferenceEquals(first, second))
            {
                return true;
            }
            if (second == null)
            {
                return false;
            }
            if (first.Length != second.Length)
            {
                return false;
            }
            if (first.GetHashCode() != second.GetHashCode())
            {
                return false;
            }
            for (int i = 0; i < first.Length; i++)
            {
                if (!Equals(first[i], second[i]))
                {
                    return false;
                }
            }
            return true;
        }
        public static bool ArrayEqualContentTest<T>(this T[] first, T[] second)
        {
            for (int i = 0; i < first.Length; i++)
            {
                if (!Equals(first[i], second[i]))
                {
                    return false;
                }
            }
            return true;
        }

        public static int ArrayHashCode1723<T>(this T[] array)
        {
            if (array == null)
            {
                return 0;
            }
            unchecked
            {
                int hash = 17;
                foreach (T item in array)
                {
                    hash = hash * 23 + item.GetHashCode();
                }
                return hash;
            }
        }

    }
}