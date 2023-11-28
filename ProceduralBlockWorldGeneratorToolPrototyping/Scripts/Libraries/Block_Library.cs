/*Antonio Wiege*/

namespace ProceduralBlockWorldGeneratorToolPrototyping
{
    public static class Block_Library
    {
        //Collection of predefined block setups to use.

        private static Block air                    = new Block() { blockShape = BlockShape_Library.empty, matter = Matter_Library.air, meshSection = BlockShape_Library.empty.meshSections[0] };
        private static Block rock                   = new Block() { blockShape = LandscapeTool.toybrickMode ? BlockShape_Library.toybrick : BlockShape_Library.block, matter = Matter_Library.rock, meshSection = VerTriSides.CombineShape(LandscapeTool.toybrickMode ? BlockShape_Library.toybrick : BlockShape_Library.block, BlockSides.none, UnityEngine.Vector3.zero) };
        private static Block grass                  = new Block() { blockShape = LandscapeTool.toybrickMode ? BlockShape_Library.toybrick : BlockShape_Library.block, matter = Matter_Library.grass, meshSection = VerTriSides.CombineShape(LandscapeTool.toybrickMode ? BlockShape_Library.toybrick : BlockShape_Library.block, BlockSides.none, UnityEngine.Vector3.zero) };
        private static Block water                  = new Block() { blockShape = LandscapeTool.toybrickMode ? BlockShape_Library.toybrick : BlockShape_Library.block, matter = Matter_Library.water, meshSection = VerTriSides.CombineShape(LandscapeTool.toybrickMode ? BlockShape_Library.toybrick : BlockShape_Library.block, BlockSides.none, UnityEngine.Vector3.zero) };
        private static Block wood                   = new Block() { blockShape = LandscapeTool.toybrickMode ? BlockShape_Library.toybrick : BlockShape_Library.block, matter = Matter_Library.wood, meshSection = VerTriSides.CombineShape(LandscapeTool.toybrickMode ? BlockShape_Library.toybrick : BlockShape_Library.block, BlockSides.none, UnityEngine.Vector3.zero) };
        private static Block leaves                 = new Block() { blockShape = LandscapeTool.toybrickMode ? BlockShape_Library.toybrick : BlockShape_Library.block, matter = Matter_Library.leaves, meshSection = VerTriSides.CombineShape(LandscapeTool.toybrickMode ? BlockShape_Library.toybrick : BlockShape_Library.block, BlockSides.none, UnityEngine.Vector3.zero) };
        private static Block tallGrass              = new Block() { blockShape = BlockShape_Library.foliage, matter = Matter_Library.tallGrass, meshSection = VerTriSides.CombineShape(BlockShape_Library.foliage, BlockSides.none, UnityEngine.Vector3.zero) };
        private static Block underwaterGrass  = new Block() { blockShape = BlockShape_Library.foliage, matter = Matter_Library.underwaterGrass, meshSection = VerTriSides.CombineShape(BlockShape_Library.foliage, BlockSides.none, UnityEngine.Vector3.zero) };
        private static Block cactus                 = new Block() { blockShape = LandscapeTool.toybrickMode ? BlockShape_Library.toybrick : BlockShape_Library.block, matter = Matter_Library.cactus, meshSection = VerTriSides.CombineShape(LandscapeTool.toybrickMode ? BlockShape_Library.toybrick : BlockShape_Library.block, BlockSides.none, UnityEngine.Vector3.zero) };

        /// <returns>copy of the original behind it; Defaults to air / Returns air if no match found</returns>
        public static Block GetCopyByName(string name)
        {
            switch (name)
            {
                case "Air":
                    return Air;
                case "Rock":
                    return Rock;
                case "Grass":
                    return Grass;
                case "Water":
                    return Water;
                case "Wood":
                    return Wood;
                case "Leaves":
                    return Leaves;
                case "TallGrass":
                    return TallGrass;
                case "UnderwaterGrass":
                    return UnderwaterGrass;
                case "Cactus":
                    return Cactus;
                default:
                    //Debug.Log("Block type not found, returning air");
                    return Air;
            }
        }

        /// <returns>Shallow copy of original block type</returns>
        public static Block Air => air.CopyShallow();
        public static Block Rock => rock.CopyShallow();
        public static Block Grass => grass.CopyShallow();
        public static Block Water => water.CopyShallow();
        public static Block Wood => wood.CopyShallow();
        public static Block Leaves => leaves.CopyShallow();
        public static Block TallGrass => tallGrass.CopyShallow();
        public static Block UnderwaterGrass => underwaterGrass.CopyShallow();
        public static Block Cactus => cactus.CopyShallow();

    }
}