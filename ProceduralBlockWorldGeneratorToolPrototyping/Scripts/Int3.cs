/*Antonio Wiege*/
using System;
using UnityEngine;
namespace ProceduralBlockWorldGeneratorToolPrototyping
{
    /*
    Includes: 
    Conversion
    Static word variants (top, down, etc)
    Positivity Check & Clamping
    Operators
    ToString
    Equals
    Hashcode
     */
    /// <summary>
    /// Integer based three-dimensional vector
    /// </summary>
    [Serializable]
    public struct Int3 : IEquatable<Int3>
    {
        public int x, y, z;

        public Int3(int x, int y, int z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
        public Int3(float x, float y, float z)
        {
            this.x = (int)x;
            this.y = (int)y;
            this.z = (int)z;
        }
        public Int3(Vector3 v)
        {
            this.x = (int)v.x;
            this.y = (int)v.y;
            this.z = (int)v.z;
        }
        public Int3(object x, object y, object z)
        {
            this.x = (int)x;
            this.y = (int)y;
            this.z = (int)z;
        }

        public static readonly Int3 error = new Int3(int.MaxValue, int.MinValue, int.MinValue);//workaround to use as null state; can't be initialized in top values yet as it is a C#10 feature, working with C#9.0 due to unity.

        public static readonly Int3 top = new Int3(0, 1, 0);
        public static readonly Int3 front = new Int3(0, 0, 1);
        public static readonly Int3 right = new Int3(1, 0, 0);
        public static readonly Int3 back = new Int3(0, 0, -1);
        public static readonly Int3 left = new Int3(-1, 0, 0);
        public static readonly Int3 bottom = new Int3(0, -1, 0);
        public static readonly Int3 up = new Int3(0, 1, 0);
        public static readonly Int3 down = new Int3(0, -1, 0);
        public static readonly Int3 forward = new Int3(0, 0, 1);
        public static readonly Int3 fwd = new Int3(0, 0, 1);
        public static readonly Int3 zero = new Int3(0, 0, 0);
        public static readonly Int3 one = new Int3(1, 1, 1);

        public static readonly Int3 xp_yp_zp = new Int3(1, 1, 1);
        public static readonly Int3 xo_yp_zp = new Int3(0, 1, 1);
        public static readonly Int3 xn_yp_zp = new Int3(-1, 1, 1);
        public static readonly Int3 xp_yo_zp = new Int3(1, 0, 1);
        public static readonly Int3 xo_yo_zp = new Int3(0, 0, 1);
        public static readonly Int3 xn_yo_zp = new Int3(-1, 0, 1);
        public static readonly Int3 xp_yn_zp = new Int3(1, -1, 1);
        public static readonly Int3 xo_yn_zp = new Int3(0, -1, 1);
        public static readonly Int3 xn_yn_zp = new Int3(-1, -1, 1);

        public static readonly Int3 xp_yp_zo = new Int3(1, 1, 0);
        public static readonly Int3 xo_yp_zo = new Int3(0, 1, 0);
        public static readonly Int3 xn_yp_zo = new Int3(-1, 1, 0);
        public static readonly Int3 xp_yo_zo = new Int3(1, 0, 0);
        public static readonly Int3 xo_yo_zo = new Int3(0, 0, 0);
        public static readonly Int3 xn_yo_zo = new Int3(-1, 0, 0);
        public static readonly Int3 xp_yn_zo = new Int3(1, -1, 0);
        public static readonly Int3 xo_yn_zo = new Int3(0, -1, 0);
        public static readonly Int3 xn_yn_zo = new Int3(-1, -1, 0);

        public static readonly Int3 xp_yp_zn = new Int3(1, 1, -1);
        public static readonly Int3 xo_yp_zn = new Int3(0, 1, -1);
        public static readonly Int3 xn_yp_zn = new Int3(-1, 1, -1);
        public static readonly Int3 xp_yo_zn = new Int3(1, 0, -1);
        public static readonly Int3 xo_yo_zn = new Int3(0, 0, -1);
        public static readonly Int3 xn_yo_zn = new Int3(-1, 0, -1);
        public static readonly Int3 xp_yn_zn = new Int3(1, -1, -1);
        public static readonly Int3 xo_yn_zn = new Int3(0, -1, -1);
        public static readonly Int3 xn_yn_zn = new Int3(-1, -1, -1);

        public static Int3 ToInt3(Vector3 v)
        {
            return new Int3((int)v.x, (int)v.y, (int)v.z);
        }

        public Vector3 ToVector3()
        {
            return new Vector3(x, y, z);
        }
        public Vector3 AsVector3 => new Vector3(x, y, z);


        public bool Positive()
        {
            return !(x < 0 || y < 0 || z < 0);
        }

        public Int3 clampPositive()
        {
            if (x < 0) x = 0;
            if (y < 0) y = 0;
            if (z < 0) z = 0;
            return this;
        }

        public static Int3 operator +(Int3 a, Int3 b)
        => new Int3(a.x + b.x, a.y + b.y, a.z + b.z);

        public static Int3 operator -(Int3 a, Int3 b)
        => new Int3(a.x - b.x, a.y - b.y, a.z - b.z);

        public static Int3 operator -(Int3 a)
        => new Int3(-a.x, -a.y, -a.z);

        public static Int3 operator *(Int3 a, int d)
        => new Int3(a.x * d, a.y * d, a.z * d);

        public static Int3 operator *(int d, Int3 a)
        => new Int3(a.x * d, a.y * d, a.z * d);

        public static Int3 operator *(Int3 a, Int3 b)
        => new Int3(a.x * b.x, a.y * b.y, a.z * b.z);

        public static Int3 operator /(Int3 a, int d)
        => new Int3(a.x / d, a.y / d, a.z / d);

        public static Int3 operator %(Int3 a, int m)
        => new Int3(a.x % m, a.y % m, a.z % m);


        public static bool operator ==(Int3 left, Int3 right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Int3 left, Int3 right)
        {
            return !(left == right);
        }

        public override string ToString()
        {
            return "Int3(" + x + "," + y + "," + z + ")";
        }
        public string ValueSetOnly_ToString()
        {
            return "[" + x + "," + y + "," + z + "]";
        }

        public bool Equals(Int3 other)
        {
            return (other.x, other.y, other.z).Equals((x, y, z));
        }

        public override bool Equals(object obj)
        {
            return obj is Int3 int3 &&
                   x == int3.x &&
                   y == int3.y &&
                   z == int3.z;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(x, y, z);
        }
    }
}