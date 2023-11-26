/*Antonio Wiege*/
namespace ProceduralBlockWorldGeneratorToolPrototyping
{
    /* Table of Contents
     BrushType
    BrushMode
    DiagonalToggle
    BlockSides
    BlockSurroundings
    TreeType
     */

    using System;

    [Serializable]
    public enum BrushType
    {
        directionalErosion,
        simpleErosion,
        color,
        matter,
        shape,
        block,
        grass,
        tree,
        scatter,
    }
    [Serializable]
    public enum BrushMode
    {
        single,
        randomWalk,
        sprayCan,
        randomize,
        blur
    }
    [Serializable]
    public enum DiagonalToggle
    {
        disabled,
        enabledPlanarOnly,
        enabledAllDiagonals
    }

    [Serializable, Flags]
    public enum BlockSides
    {
        //basic singulars
        none = 0,
        top = 1,
        front = 2,
        right = 4,
        back = 8,
        left = 16,
        bottom = 32,
        //axis
        all = top | front | right | back | left | bottom,
        x = right | left,
        y = top | bottom,
        z = front | back,
        //combinations dual
        topFront = top | front,
        topRight = top | right,
        topBack = top | back,
        topLeft = top | left,
        frontRight = front | right,
        rightBack = right | back,
        backLeft = back | left,
        leftFront = front | left,
        bottomFront = front | bottom,
        bottomRight = right | bottom,
        bottomBack = back | bottom,
        bottomLeft = left | bottom,
        //combinations triple
        ooo = left | bottom | back,
        loo = right | bottom | back,
        olo = left | top | back,
        llo = right | top | back,
        ool = left | bottom | front,
        lol = right | bottom | front,
        oll = left | top | front,
        lll = right | top | front
    }
    public static class BlockSidesMethods
    {
        public static int Count(this BlockSides cubeSide)
        {
            int c = 0;
            if ((cubeSide & BlockSides.top) != 0) c++;
            if ((cubeSide & BlockSides.front) != 0) c++;
            if ((cubeSide & BlockSides.right) != 0) c++;
            if ((cubeSide & BlockSides.back) != 0) c++;
            if ((cubeSide & BlockSides.left) != 0) c++;
            if ((cubeSide & BlockSides.bottom) != 0) c++;
            return c;
        }

        /// <summary>Clockwise, handles covers & sections </summary>
        public static BlockSides RotateX(this BlockSides old)
        {
            BlockSides shape = BlockSides.none;
            if ((old & BlockSides.top) != 0) shape |= BlockSides.front;
            if ((old & BlockSides.front) != 0) shape |= BlockSides.bottom;
            if ((old & BlockSides.right) != 0) shape |= BlockSides.right;
            if ((old & BlockSides.back) != 0) shape |= BlockSides.top;
            if ((old & BlockSides.left) != 0) shape |= BlockSides.left;
            if ((old & BlockSides.bottom) != 0) shape |= BlockSides.back;
            return shape;
        }
        /// <summary>Clockwise, handles covers & sections </summary>
        public static BlockSides RotateY(this BlockSides old)
        {
            BlockSides shape = BlockSides.none;
            if ((old & BlockSides.top) != 0) shape |= BlockSides.top;
            if ((old & BlockSides.front) != 0) shape |= BlockSides.right;
            if ((old & BlockSides.right) != 0) shape |= BlockSides.back;
            if ((old & BlockSides.back) != 0) shape |= BlockSides.left;
            if ((old & BlockSides.left) != 0) shape |= BlockSides.front;
            if ((old & BlockSides.bottom) != 0) shape |= BlockSides.bottom;
            return shape;
        }
        /// <summary>Clockwise, handles covers & sections </summary>
        public static BlockSides RotateZ(this BlockSides old)
        {
            BlockSides shape = BlockSides.none;
            if ((old & BlockSides.top) != 0) shape |= BlockSides.right;
            if ((old & BlockSides.front) != 0) shape |= BlockSides.front;
            if ((old & BlockSides.right) != 0) shape |= BlockSides.bottom;
            if ((old & BlockSides.back) != 0) shape |= BlockSides.back;
            if ((old & BlockSides.left) != 0) shape |= BlockSides.top;
            if ((old & BlockSides.bottom) != 0) shape |= BlockSides.right;
            return shape;
        }

        public static BlockSides Opposite(this BlockSides cubeSide) => opposite(cubeSide);
        /// <summary> </summary>
        /// <param name="cubeSide"></param>
        /// <returns>Known: center mirror; Unknown: inverted bit mask (~)</returns>
        public static BlockSides opposite(this BlockSides cubeSide)
        {
            switch (cubeSide)
            {
                case BlockSides.none:
                    return BlockSides.all;
                case BlockSides.top:
                    return BlockSides.bottom;
                case BlockSides.front:
                    return BlockSides.back;
                case BlockSides.right:
                    return BlockSides.left;
                case BlockSides.back:
                    return BlockSides.front;
                case BlockSides.left:
                    return BlockSides.right;
                case BlockSides.bottom:
                    return BlockSides.top;
                case BlockSides.all:
                    return BlockSides.none;
                case BlockSides.x:
                    return ~BlockSides.x;
                case BlockSides.y:
                    return ~BlockSides.y;
                case BlockSides.z:
                    return ~BlockSides.z;
                case BlockSides.topFront:
                    return BlockSides.bottomBack;
                case BlockSides.topRight:
                    return BlockSides.bottomLeft;
                case BlockSides.topBack:
                    return BlockSides.bottomFront;
                case BlockSides.topLeft:
                    return BlockSides.bottomRight;
                case BlockSides.frontRight:
                    return BlockSides.backLeft;
                case BlockSides.rightBack:
                    return BlockSides.leftFront;
                case BlockSides.backLeft:
                    return BlockSides.frontRight;
                case BlockSides.leftFront:
                    return BlockSides.rightBack;
                case BlockSides.bottomFront:
                    return BlockSides.topBack;
                case BlockSides.bottomRight:
                    return BlockSides.topLeft;
                case BlockSides.bottomBack:
                    return BlockSides.topFront;
                case BlockSides.bottomLeft:
                    return BlockSides.topRight;
                case BlockSides.ooo:
                    return BlockSides.lll;
                case BlockSides.loo:
                    return BlockSides.oll;
                case BlockSides.olo:
                    return BlockSides.lol;
                case BlockSides.llo:
                    return BlockSides.ool;
                case BlockSides.ool:
                    return BlockSides.llo;
                case BlockSides.lol:
                    return BlockSides.olo;
                case BlockSides.oll:
                    return BlockSides.loo;
                case BlockSides.lll:
                    return BlockSides.ooo;
                default:
                    return ~cubeSide;
            }
        }


    }

    /// <summary>
    /// 3x3 each being 1, 0 or -1 (p ositive, o, n egative)
    /// </summary>
    [Serializable, Flags]
    public enum BlockSurroundings
    {
        none = 0,
        xp_yp_zp = 1,
        xo_yp_zp = 2,
        xn_yp_zp = 4,
        xp_yo_zp = 8,
        xo_yo_zp = 16,
        xn_yo_zp = 32,
        xp_yn_zp = 64,
        xo_yn_zp = 128,
        xn_yn_zp = 256,

        xp_yp_zo = 512,
        xo_yp_zo = 1024,
        xn_yp_zo = 2048,
        xp_yo_zo = 4096,
        xo_yo_zo = 8192,
        xn_yo_zo = 16384,
        xp_yn_zo = 32768,
        xo_yn_zo = 65536,
        xn_yn_zo = 131072,

        xp_yp_zn = 262144,
        xo_yp_zn = 524288,
        xn_yp_zn = 1048576,
        xp_yo_zn = 2097152,
        xo_yo_zn = 4194304,
        xn_yo_zn = 8388608,
        xp_yn_zn = 16777216,
        xo_yn_zn = 33554432,
        xn_yn_zn = 67108864
    }
    [Serializable]
    public enum TreeType
    {
        oak,
        pine,
        cactus,
        none
    }

}