/*Antonio Wiege*/
using System;
namespace ProceduralBlockWorldGeneratorToolPrototyping
{
    [Serializable]
    public class BlockShape
    {
        /// <summary>All local sides it covers</summary>
        public BlockSides covers;
        /// <summary>Mesh split into separate section. The sides here indicate what makes the mesh section visible. Every mesh bit should be unique. First Top, Last Bottom</summary>
        public VerTriSides[] meshSections;


        public BlockShape ShallowCopy()
        {
            return new BlockShape() { covers = covers, meshSections = meshSections };
        }

        //learned something about C# equality here https://grantwinney.com/how-to-compare-two-objects-testing-for-equality-in-c/

        public override bool Equals(object obj) => obj is BlockShape o && Equals(o);

        public static bool operator ==(BlockShape x, BlockShape y)
        {
            return x.Equals(y);
        }

        public static bool operator !=(BlockShape x, BlockShape y)
        {
            return !(x == y);
        }

        public bool Equals(BlockShape other)
        {
            if (this is null)
            {
                return other is null;
            }
            if (other is null)
            {
                return this is null;
            }
            if (ReferenceEquals(this, other)) return true;
            return (other.covers, other.meshSections).Equals((covers, meshSections));
        }

        //https://docs.microsoft.com/de-de/dotnet/api/system.hashcode?view=net-6.0
        public override int GetHashCode() => HashCode.Combine(covers, meshSections);

    }
}