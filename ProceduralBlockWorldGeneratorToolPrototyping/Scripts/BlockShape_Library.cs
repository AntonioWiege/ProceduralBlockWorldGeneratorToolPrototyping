/*Antonio Wiege*/
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace ProceduralBlockWorldGeneratorToolPrototyping
{
    public static class BlockShape_Library
    {
        #region Automated Rotation Variants
        //This library automatically adds all rotated variants, not removing doubles, for any way to address an entry.
        public static List<BlockShape> AllHardCodedEntries = new List<BlockShape>()
        {
            //   /*empty,*/block,shortDiagonal,longDiagonal,blockMissOneVert,halfTriangleBlock,odd,toybrick
        };
        public static List<BlockShape> allHardCodedEntries
        {
            get
            {
                if (AllHardCodedEntries.Count == 0)
                {
                    AllHardCodedEntries.Add(block);
                    AllHardCodedEntries.Add(shortDiagonal);
                    AllHardCodedEntries.Add(shortDiagonalOverCover);
                    AllHardCodedEntries.Add(longDiagonal);
                    allHardCodedEntries.Add(hexCorner);
                    AllHardCodedEntries.Add(blockMissOneVert);
                    AllHardCodedEntries.Add(halfTriangleBlock);
                    AllHardCodedEntries.Add(odd);
                    AllHardCodedEntries.Add(foot);
                    AllHardCodedEntries.Add(foliage);
                    AllHardCodedEntries.Add(toybrick);
                }
                return AllHardCodedEntries;
            }
        }

        public static List<Int3> rotations = new List<Int3> {
    new Int3(0,0,0),
    new Int3(1,0,0),
    new Int3(2,0,0),
    new Int3(3,0,0),
    new Int3(0,1,0),
    new Int3(1,1,0),
    new Int3(2,1,0),
    new Int3(3,1,0),
    new Int3(0,2,0),
    new Int3(1,2,0),
    new Int3(2,2,0),
    new Int3(3,2,0),
    new Int3(0,3,0),
    new Int3(1,3,0),
    new Int3(2,3,0),
    new Int3(3,3,0),
    new Int3(0,0,1),
    new Int3(1,0,1),
    new Int3(2,0,1),
    new Int3(3,0,1),
    new Int3(0,1,1),
    new Int3(1,1,1),
    new Int3(2,1,1),
    new Int3(3,1,1),
    new Int3(0,2,1),
    new Int3(1,2,1),
    new Int3(2,2,1),
    new Int3(3,2,1),
    new Int3(0,3,1),
    new Int3(1,3,1),
    new Int3(2,3,1),
    new Int3(3,3,1),
    new Int3(0,0,2),
    new Int3(1,0,2),
    new Int3(2,0,2),
    new Int3(3,0,2),
    new Int3(0,1,2),
    new Int3(1,1,2),
    new Int3(2,1,2),
    new Int3(3,1,2),
    new Int3(0,2,2),
    new Int3(1,2,2),
    new Int3(2,2,2),
    new Int3(3,2,2),
    new Int3(0,3,2),
    new Int3(1,3,2),
    new Int3(2,3,2),
    new Int3(3,3,2),
    new Int3(0,0,3),
    new Int3(1,0,3),
    new Int3(2,0,3),
    new Int3(3,0,3),
    new Int3(0,1,3),
    new Int3(1,1,3),
    new Int3(2,1,3),
    new Int3(3,1,3),
    new Int3(0,2,3),
    new Int3(1,2,3),
    new Int3(2,2,3),
    new Int3(3,2,3),
    new Int3(0,3,3),
    new Int3(1,3,3),
    new Int3(2,3,3),
    new Int3(3,3,3)
};
        public struct ShapeRotationKey
        {
            public BlockShape bs;
            public Int3 rotationsPerAxis;
        }

        private static ConcurrentDictionary<ShapeRotationKey, BlockShape> AllGeneratedVariants;
        public static ConcurrentDictionary<ShapeRotationKey, BlockShape> allGeneratedVariants
        {
            get
            {
                if (AllGeneratedVariants == null)
                {
                    AllGeneratedVariants = new ConcurrentDictionary<ShapeRotationKey, BlockShape>();
                    foreach (var item in allHardCodedEntries)
                    {
                        foreach (var mod in rotations)
                        {
                            var bs = item.ShallowCopy();
                            bs.covers = item.covers;
                            if (bs.meshSections == null)
                            {
                                bs.meshSections = new VerTriSides[item.meshSections.Length];
                                for (int i = 0; i < item.meshSections.Length; i++)
                                {
                                    bs.meshSections[i] = item.meshSections[i].DeepCopy();
                                }
                            }
                            for (int x = 0; x < mod.x; x++)
                            {
                                bs = bs.RotateX();
                            }
                            for (int y = 0; y < mod.y; y++)
                            {
                                bs = bs.RotateY();
                            }
                            for (int z = 0; z < mod.z; z++)
                            {
                                bs = bs.RotateZ();
                            }
                            AllGeneratedVariants.TryAdd(new ShapeRotationKey() { bs = item, rotationsPerAxis = mod }, bs);
                        }
                    }
                }
                return AllGeneratedVariants;
            }
        }
        #endregion

        #region Rotation
        /// <summary>ClockwiseX+, handles covers & sections </summary>
        public static BlockShape RotateX(this BlockShape old)
        {
            BlockShape shape = new BlockShape();
            shape.covers = old.covers.RotateX();
            shape.meshSections = new VerTriSides[old.meshSections.Length];
            for (int i = 0; i < old.meshSections.Length; i++)
            {
                shape.meshSections[i] = old.meshSections[i].DeepCopy();
                shape.meshSections[i].sides = shape.meshSections[i].sides.RotateX();
                for (int o = 0; o < shape.meshSections[i].verts.Length; o++)
                {
                    var vert = shape.meshSections[i].verts[o];
                    vert -= new Vector3(0.5f, 0.5f, 0.5f) * LandscapeTool.BlockScale;
                    vert = Quaternion.AngleAxis(90, Vector3.right) * vert;
                    vert += new Vector3(0.5f, 0.5f, 0.5f) * LandscapeTool.BlockScale;
                    shape.meshSections[i].verts[o] = vert;
                }
            }
            return shape;
        }
        /// <summary>ClockwiseY+, handles covers & sections </summary>
        public static BlockShape RotateY(this BlockShape old)
        {
            BlockShape shape = new BlockShape();
            shape.covers = old.covers.RotateY();
            shape.meshSections = new VerTriSides[old.meshSections.Length];
            for (int i = 0; i < old.meshSections.Length; i++)
            {
                shape.meshSections[i] = old.meshSections[i].DeepCopy();
                shape.meshSections[i].sides = shape.meshSections[i].sides.RotateY();
                for (int o = 0; o < shape.meshSections[i].verts.Length; o++)
                {
                    var vert = shape.meshSections[i].verts[o];
                    vert -= new Vector3(0.5f, 0.5f, 0.5f) * LandscapeTool.BlockScale;
                    vert = Quaternion.AngleAxis(90, Vector3.up) * vert;
                    vert += new Vector3(0.5f, 0.5f, 0.5f) * LandscapeTool.BlockScale;
                    shape.meshSections[i].verts[o] = vert;
                }
            }
            return shape;
        }
        /// <summary>ClockwiseZ-, handles covers & sections </summary>
        public static BlockShape RotateZ(this BlockShape old)
        {
            BlockShape shape = new BlockShape();
            shape.covers = old.covers.RotateZ();
            shape.meshSections = new VerTriSides[old.meshSections.Length];
            for (int i = 0; i < old.meshSections.Length; i++)
            {
                shape.meshSections[i] = old.meshSections[i].DeepCopy();
                shape.meshSections[i].sides = shape.meshSections[i].sides.RotateZ();
                for (int o = 0; o < shape.meshSections[i].verts.Length; o++)
                {
                    var vert = shape.meshSections[i].verts[o];
                    vert -= new Vector3(0.5f, 0.5f, 0.5f) * LandscapeTool.BlockScale;
                    vert = Quaternion.AngleAxis(90, Vector3.back) * vert;
                    vert += new Vector3(0.5f, 0.5f, 0.5f) * LandscapeTool.BlockScale;
                    shape.meshSections[i].verts[o] = vert;
                }
            }
            return shape;
        }
        #endregion

        #region Shapes
        public static BlockShape empty = new()
        {
            covers = BlockSides.none,//what this mesh would cover
            meshSections = new VerTriSides[] { new VerTriSides(new Vector3[0], new int[0], BlockSides.none) }//BlockSides: under what parts not there of the neighbours for the meshSection to be visible
        };
        public static BlockShape block = new()
        {
            covers = BlockSides.all,
            meshSections = new VerTriSides[] {
        new VerTriSides(new Vector3[] { new Vector3(0, 1 , 0), new Vector3(0, 1 , 1 ), new Vector3(1 , 1 , 1 ), new Vector3(1 , 1 , 0) }.Select(x=>x*LandscapeTool.BlockScale).ToArray(), new int[] { 0, 1, 2, 2, 3, 0 }, BlockSides.top)
        ,
        new VerTriSides(new Vector3[] { new Vector3(1 , 0, 1 ), new Vector3(1 , 1 , 1 ), new Vector3(0, 1 , 1 ), new Vector3(0, 0, 1 ) }.Select(x=>x*LandscapeTool.BlockScale).ToArray(), new int[] { 0, 1, 2, 2, 3, 0 }, BlockSides.front)
        ,
       new VerTriSides(new Vector3[] { new Vector3(1 , 0, 0), new Vector3(1 , 1 , 0), new Vector3(1 , 1 , 1 ), new Vector3(1 , 0, 1 ) }.Select(x=>x*LandscapeTool.BlockScale).ToArray(), new int[] { 0, 1, 2, 2, 3, 0 }, BlockSides.right)
        ,
        new VerTriSides(new Vector3[] { new Vector3(0, 0, 0), new Vector3(0, 1 , 0), new Vector3(1 , 1 , 0), new Vector3(1 , 0, 0) }.Select(x=>x * LandscapeTool.BlockScale).ToArray(), new int[] { 0, 1, 2, 2, 3, 0 }, BlockSides.back)
        ,
        new VerTriSides(new Vector3[] { new Vector3(0, 0, 1 ), new Vector3(0, 1 , 1 ), new Vector3(0, 1 , 0), new Vector3(0, 0, 0) }.Select(x=>x * LandscapeTool.BlockScale).ToArray(), new int[] { 0, 1, 2, 2, 3, 0 }, BlockSides.left)
        ,
        new VerTriSides(new Vector3[] { new Vector3(1 , 0, 0), new Vector3(1 , 0, 1 ), new Vector3(0, 0, 1 ), new Vector3(0, 0, 0) }.Select(x=>x * LandscapeTool.BlockScale).ToArray(), new int[] { 0, 1, 2, 2, 3, 0 }, BlockSides.bottom)
    }
        };
        //default orientation negatives full, positives empty, looking toward + coords
        //meaning: a blocks shape default orientation e.g. in case of a diagonal, would assume the blocks left and bottom to be full, while the blocks front and right are free, meaning the actual diagonal quad divides empty space at 1,1 and full space 0,0

        public static BlockShape shortDiagonal = new()
        {
            covers = BlockSides.backLeft,
            meshSections = new VerTriSides[] {

        new VerTriSides(new Vector3[] {new Vector3(0, 1, 0),   new Vector3(0, 1, 1),   new Vector3(1, 1, 0) }.Select(x=>x * LandscapeTool.BlockScale).ToArray(), new int[] { 0, 1, 2 }, BlockSides.top)
        ,
        new VerTriSides(new Vector3[] {   new Vector3(1, 0, 0),   new Vector3(1, 1, 0),         new Vector3(0, 1, 1),   new Vector3(0, 0, 1)}.Select(x=>x * LandscapeTool.BlockScale).ToArray(), new int[] {  0, 1, 2, 2, 3, 0 }, ~BlockSides.backLeft)
        ,
        new VerTriSides(new Vector3[] { new Vector3(0, 0, 0), new Vector3(0, 1, 0), new Vector3(1, 1, 0),new Vector3(1, 0, 0) }.Select(x=>x * LandscapeTool.BlockScale).ToArray(), new int[] { 0, 1, 2, 2, 3, 0 }, BlockSides.back)
        ,
        new VerTriSides(new Vector3[] { new Vector3(0, 0, 1),new Vector3(0, 1, 1),new Vector3(0, 1, 0),new Vector3(0, 0, 0)}.Select(x=>x * LandscapeTool.BlockScale).ToArray(), new int[] { 0, 1, 2, 2, 3, 0 }, BlockSides.left)
        ,
        new VerTriSides(new Vector3[] {  new Vector3(1, 0, 0),       new Vector3(0, 0, 1),   new Vector3(0, 0, 0) }.Select(x=>x * LandscapeTool.BlockScale).ToArray(), new int[] { 0, 1, 2 }, BlockSides.bottom)
    }
        };
        //You are advised not to use this one. It was a technical entry only and if you make use of this, it is likely that you are applying my algorithm wrong
        public static BlockShape shortDiagonalOverCover = new()
        {
            covers = BlockSides.backLeft,
            meshSections = new VerTriSides[] {

        new VerTriSides(new Vector3[] {new Vector3(0, 0, 0),   new Vector3(0, 0, 1),   new Vector3(1, 0, 1) ,   new Vector3(1, 0, 0)}.Select(x=>x * LandscapeTool.BlockScale).ToArray(), new int[] { 0, 1, 2, 2, 3, 0 }, BlockSides.top)
        ,
        new VerTriSides(new Vector3[] {new Vector3(0, 1, 0),   new Vector3(0, 1, 1),   new Vector3(1, 1, 0) }.Select(x=>x * LandscapeTool.BlockScale).ToArray(), new int[] { 0, 1, 2 }, BlockSides.top)
        ,
        new VerTriSides(new Vector3[] {   new Vector3(1, 0, 0),   new Vector3(1, 1, 0),         new Vector3(0, 1, 1),   new Vector3(0, 0, 1)}.Select(x=>x * LandscapeTool.BlockScale).ToArray(), new int[] {  0, 1, 2, 2, 3, 0 }, ~BlockSides.backLeft)
        ,
        new VerTriSides(new Vector3[] { new Vector3(0, 0, 0), new Vector3(0, 1, 0), new Vector3(1, 1, 0),new Vector3(1, 0, 0) }.Select(x=>x * LandscapeTool.BlockScale).ToArray(), new int[] { 0, 1, 2, 2, 3, 0 }, BlockSides.back)
        ,
        new VerTriSides(new Vector3[] { new Vector3(0, 0, 1),new Vector3(0, 1, 1),new Vector3(0, 1, 0),new Vector3(0, 0, 0)}.Select(x=>x * LandscapeTool.BlockScale).ToArray(), new int[] { 0, 1, 2, 2, 3, 0 }, BlockSides.left)
        ,
        new VerTriSides(new Vector3[] {  new Vector3(1, 0, 0),       new Vector3(0, 0, 1),   new Vector3(0, 0, 0) }.Select(x=>x * LandscapeTool.BlockScale).ToArray(), new int[] { 0, 1, 2 }, BlockSides.bottom)
        ,
        new VerTriSides(new Vector3[] {  new Vector3(1, 1, 0), new Vector3(1, 1, 1),       new Vector3(0, 1, 1),   new Vector3(0, 1, 0) }.Select(x=>x * LandscapeTool.BlockScale).ToArray(), new int[] { 0, 1, 2, 2, 3, 0 }, BlockSides.bottom)
    }
        };

        public static BlockShape longDiagonal = new()
        {
            covers = BlockSides.all,
            meshSections = new VerTriSides[] {

           new VerTriSides(new Vector3[] {      new Vector3(0, 1, 0),
                 new Vector3(2 / 3f, 5 / 3f, 2 / 3f),
                 new Vector3(4 / 3f, 4 / 3f, 1 / 3f),
                 new Vector3(1, 1, 0)  }.Select(x=>x * LandscapeTool.BlockScale).ToArray(), new int[] { 0, 1, 2, 2, 3, 0 }, BlockSides.lll)
        ,
           new VerTriSides(new Vector3[] {                new Vector3(1, 0, 0),
                 new Vector3(1, 1, 0),
                 new Vector3(4 / 3f, 4 / 3f, 1 / 3f),
                 new Vector3(5 / 3f, 2 / 3f, 2 / 3f)  }.Select(x=>x * LandscapeTool.BlockScale).ToArray(), new int[] { 0, 1, 2, 2, 3, 0 }, BlockSides.lll)
        ,
           new VerTriSides(new Vector3[] {    new Vector3(1, 0, 0),
                 new Vector3(5 / 3f, 2 / 3f, 2 / 3f),
                 new Vector3(4 / 3f, 1 / 3f, 4 / 3f),
                 new Vector3(1, 0, 1) }.Select(x=>x * LandscapeTool.BlockScale).ToArray(), new int[] { 0, 1, 2, 2, 3, 0 }, BlockSides.lll)
        ,
           new VerTriSides(new Vector3[] {      new Vector3(1, 0, 1),
                 new Vector3(4 / 3f, 1 / 3f, 4 / 3f),
                 new Vector3(2 / 3f, 2 / 3f, 5 / 3f),
                 new Vector3(0, 0, 1) }.Select(x=>x * LandscapeTool.BlockScale).ToArray(), new int[] { 0, 1, 2, 2, 3, 0 }, BlockSides.lll)
        ,
           new VerTriSides(new Vector3[] {     new Vector3(2 / 3f, 2 / 3f, 5 / 3f),
                 new Vector3(1 / 3f, 4 / 3f, 4 / 3f),
                 new Vector3(0, 1, 1),
                 new Vector3(0, 0, 1)  }.Select(x=>x * LandscapeTool.BlockScale).ToArray(), new int[] { 0, 1, 2, 2, 3, 0 }, BlockSides.lll)
        ,
           new VerTriSides(new Vector3[] {
                 new Vector3(0, 1, 1),
                 new Vector3(1 / 3f, 4 / 3f, 4 / 3f),
                 new Vector3(2 / 3f, 5 / 3f, 2 / 3f),
                 new Vector3(0, 1, 0) }.Select(x=>x * LandscapeTool.BlockScale).ToArray(), new int[] { 0, 1, 2, 2, 3, 0 }, BlockSides.lll)
        ,
           new VerTriSides(new Vector3[] {      new Vector3(2 / 3f, 5 / 3f, 2 / 3f),
                 new Vector3(1 / 3f, 4 / 3f, 4 / 3f),
                 new Vector3(2 / 3f, 2 / 3f, 5 / 3f),
                 new Vector3(4 / 3f, 1 / 3f, 4 / 3f)  }.Select(x=>x * LandscapeTool.BlockScale).ToArray(), new int[] { 0, 1, 2, 2, 3, 0 }, BlockSides.lll)
        ,
           new VerTriSides(new Vector3[] {             new Vector3(2 / 3f, 5 / 3f, 2 / 3f),
                 new Vector3(4 / 3f, 1 / 3f, 4 / 3f),
                 new Vector3(5 / 3f, 2 / 3f, 2 / 3f),
                 new Vector3(4 / 3f, 4 / 3f, 1 / 3f)  }.Select(x=>x * LandscapeTool.BlockScale).ToArray(), new int[] { 0, 1, 2, 2, 3, 0 }, BlockSides.lll)
        ,
        new VerTriSides(new Vector3[] { new Vector3(0, 0, 0), new Vector3(0, 1 , 0), new Vector3(1, 1, 0), new Vector3(1, 0, 0) }.Select(x=>x * LandscapeTool.BlockScale).ToArray(), new int[] { 0, 1, 2, 2, 3, 0 }, BlockSides.back)
        ,
        new VerTriSides(new Vector3[] { new Vector3(0, 0, 1 ), new Vector3(0, 1 , 1), new Vector3(0, 1, 0), new Vector3(0, 0, 0) }.Select(x=>x * LandscapeTool.BlockScale).ToArray(), new int[] { 0, 1, 2, 2, 3, 0 }, BlockSides.left)
        ,
        new VerTriSides(new Vector3[] { new Vector3(1, 0, 0), new Vector3(1, 0, 1), new Vector3(0, 0, 1), new Vector3(0, 0, 0) }.Select(x=> x*LandscapeTool.BlockScale).ToArray(), new int[] { 0, 1, 2, 2, 3, 0 }, BlockSides.bottom)
    }
        };

        public static BlockShape hexCorner = new()
        {
            covers = BlockSides.all,
            meshSections = new VerTriSides[] {

           new VerTriSides(new Vector3[] {      new Vector3(0, 1, 0),
                 new Vector3(2 / 3f, 5 / 3f, 2 / 3f)-Vector3.one/3f,
                 new Vector3(4 / 3f, 4 / 3f, 1 / 3f)-Vector3.one/3f,
                 new Vector3(1, 1, 0)  }.Select(x=>x * LandscapeTool.BlockScale).ToArray(), new int[] { 0, 1, 2, 2, 3, 0 }, BlockSides.lll)
        ,
           new VerTriSides(new Vector3[] {                new Vector3(1, 0, 0),
                 new Vector3(1, 1, 0),
                 new Vector3(4 / 3f, 4 / 3f, 1 / 3f)-Vector3.one/3f,
                 new Vector3(5 / 3f, 2 / 3f, 2 / 3f)-Vector3.one/3f  }.Select(x=>x * LandscapeTool.BlockScale).ToArray(), new int[] { 0, 1, 2, 2, 3, 0 }, BlockSides.lll)
        ,
           new VerTriSides(new Vector3[] {    new Vector3(1, 0, 0),
                 new Vector3(5 / 3f, 2 / 3f, 2 / 3f)-Vector3.one/3f,
                 new Vector3(4 / 3f, 1 / 3f, 4 / 3f)-Vector3.one/3f,
                 new Vector3(1, 0, 1) }.Select(x=>x * LandscapeTool.BlockScale).ToArray(), new int[] { 0, 1, 2, 2, 3, 0 }, BlockSides.lll)
        ,
           new VerTriSides(new Vector3[] {      new Vector3(1, 0, 1),
                 new Vector3(4 / 3f, 1 / 3f, 4 / 3f)-Vector3.one/3f,
                 new Vector3(2 / 3f, 2 / 3f, 5 / 3f)-Vector3.one/3f,
                 new Vector3(0, 0, 1) }.Select(x=>x * LandscapeTool.BlockScale).ToArray(), new int[] { 0, 1, 2, 2, 3, 0 }, BlockSides.lll)
        ,
           new VerTriSides(new Vector3[] {     new Vector3(2 / 3f, 2 / 3f, 5 / 3f)-Vector3.one/3f,
                 new Vector3(1 / 3f, 4 / 3f, 4 / 3f)-Vector3.one/3f,
                 new Vector3(0, 1, 1),
                 new Vector3(0, 0, 1)  }.Select(x=>x * LandscapeTool.BlockScale).ToArray(), new int[] { 0, 1, 2, 2, 3, 0 }, BlockSides.lll)
        ,
           new VerTriSides(new Vector3[] {
                 new Vector3(0, 1, 1),
                 new Vector3(1 / 3f, 4 / 3f, 4 / 3f)-Vector3.one/3f,
                 new Vector3(2 / 3f, 5 / 3f, 2 / 3f)-Vector3.one/3f,
                 new Vector3(0, 1, 0) }.Select(x=>x * LandscapeTool.BlockScale).ToArray(), new int[] { 0, 1, 2, 2, 3, 0 }, BlockSides.lll)
        ,
           new VerTriSides(new Vector3[] {      new Vector3(2 / 3f, 5 / 3f, 2 / 3f)-Vector3.one/3f,
                 new Vector3(1 / 3f, 4 / 3f, 4 / 3f)-Vector3.one/3f,
                 new Vector3(2 / 3f, 2 / 3f, 5 / 3f)-Vector3.one/3f,
                 new Vector3(4 / 3f, 1 / 3f, 4 / 3f)-Vector3.one/3f  }.Select(x=>x * LandscapeTool.BlockScale).ToArray(), new int[] { 0, 1, 2, 2, 3, 0 }, BlockSides.lll)
        ,
           new VerTriSides(new Vector3[] {             new Vector3(2 / 3f, 5 / 3f, 2 / 3f)-Vector3.one/3f,
                 new Vector3(4 / 3f, 1 / 3f, 4 / 3f)-Vector3.one/3f,
                 new Vector3(5 / 3f, 2 / 3f, 2 / 3f)-Vector3.one/3f,
                 new Vector3(4 / 3f, 4 / 3f, 1 / 3f)-Vector3.one/3f  }.Select(x=>x * LandscapeTool.BlockScale).ToArray(), new int[] { 0, 1, 2, 2, 3, 0 }, BlockSides.lll)
        ,
        new VerTriSides(new Vector3[] { new Vector3(0, 0, 0), new Vector3(0, 1 , 0), new Vector3(1, 1, 0), new Vector3(1, 0, 0) }.Select(x=>x * LandscapeTool.BlockScale).ToArray(), new int[] { 0, 1, 2, 2, 3, 0 }, BlockSides.back)
        ,
        new VerTriSides(new Vector3[] { new Vector3(0, 0, 1 ), new Vector3(0, 1 , 1), new Vector3(0, 1, 0), new Vector3(0, 0, 0) }.Select(x=>x * LandscapeTool.BlockScale).ToArray(), new int[] { 0, 1, 2, 2, 3, 0 }, BlockSides.left)
        ,
        new VerTriSides(new Vector3[] { new Vector3(1, 0, 0), new Vector3(1, 0, 1), new Vector3(0, 0, 1), new Vector3(0, 0, 0) }.Select(x=> x*LandscapeTool.BlockScale).ToArray(), new int[] { 0, 1, 2, 2, 3, 0 }, BlockSides.bottom)
    }
        };

        public static BlockShape blockMissOneVert = new()
        {
            covers = BlockSides.ooo,
            meshSections = new VerTriSides[] {
        new VerTriSides(new Vector3[] { new Vector3(0, 1, 0),
                    new Vector3(0, 1, 1),
                    new Vector3(1, 1, 0) }.Select(x=>x * LandscapeTool.BlockScale).ToArray(), new int[] { 0, 1, 2 }, BlockSides.top)
        ,
        new VerTriSides(new Vector3[] {           new Vector3(0, 1, 1),
                    new Vector3(0, 0, 1),
                    new Vector3(1, 0, 1) }.Select(x=>x*LandscapeTool.BlockScale).ToArray(), new int[] { 0, 1, 2 }, BlockSides.front)
        ,
       new VerTriSides(new Vector3[] {     new Vector3(1, 0, 0),
                    new Vector3(1, 1, 0),
                    new Vector3(1, 0, 1) }.Select(x=>x * LandscapeTool.BlockScale).ToArray(), new int[] { 0, 1, 2 }, BlockSides.right)
        ,
        new VerTriSides(new Vector3[] {     new Vector3(0, 0, 0),
                    new Vector3(0, 1, 0),
                    new Vector3(1, 1, 0),
                    new Vector3(1, 0, 0) }.Select(x=>x * LandscapeTool.BlockScale).ToArray(), new int[] { 0, 1, 2, 2, 3, 0 }, BlockSides.back)
        ,
        new VerTriSides(new Vector3[] {      new Vector3(0, 0, 1),
                    new Vector3(0, 1, 1),
                    new Vector3(0, 1, 0),
                    new Vector3(0, 0, 0)}.Select(x=>x * LandscapeTool.BlockScale).ToArray(), new int[] { 0, 1, 2, 2, 3, 0 }, BlockSides.left)
        ,
        new VerTriSides(new Vector3[] {   new Vector3(1, 0, 0),
                    new Vector3(1, 0, 1),
                    new Vector3(0, 0, 1),
                    new Vector3(0, 0, 0)}.Select(x=>x * LandscapeTool.BlockScale).ToArray(), new int[] { 0, 1, 2, 2, 3, 0 }, BlockSides.bottom)
        ,
        new VerTriSides(new Vector3[] {      new Vector3(1, 1, 0),
                    new Vector3(0, 1, 1),
                    new Vector3(1, 0, 1) }.Select(x=>x*LandscapeTool.BlockScale).ToArray(), new int[] { 0, 1, 2 }, BlockSides.lll)
    }
        };

        public static BlockShape halfTriangleBlock = new()//half cube
        {
            covers = BlockSides.none,
            meshSections = new VerTriSides[] {
        new VerTriSides(new Vector3[] {        new Vector3(1, 0, 0),
                    new Vector3(0, 1, 0),
                    new Vector3(0, 0, 1) }.Select(x=>x * LandscapeTool.BlockScale).ToArray(), new int[] {0, 1, 2 }, BlockSides.all)
        ,
        new VerTriSides(new Vector3[] {   new Vector3(0, 0, 0),
                    new Vector3(0, 1, 0),
                    new Vector3(1, 0, 0) }.Select(x=>x * LandscapeTool.BlockScale).ToArray(), new int[] {0, 1, 2 }, BlockSides.back)
        ,
        new VerTriSides(new Vector3[] {      new Vector3(0, 0, 1),
                    new Vector3(0, 1, 0),
                    new Vector3(0, 0, 0)
 }.Select(x=>x * LandscapeTool.BlockScale).ToArray(), new int[] {0, 1, 2 }, BlockSides.left)
        ,
        new VerTriSides(new Vector3[] {
                    new Vector3(1, 0, 0),
                    new Vector3(0, 0, 1),
                    new Vector3(0, 0, 0)}.Select(x=>x * LandscapeTool.BlockScale).ToArray(), new int[] {0, 1, 2 }, BlockSides.bottom)
    }
        };

        public static BlockShape odd = new()
        {
            covers = BlockSides.bottom,
            meshSections = new VerTriSides[] {
        new VerTriSides(new Vector3[] { new Vector3(0, 1, 0),
                    new Vector3(0, 0, 1),
                    new Vector3(1, 0, 1),
                    new Vector3(1, 0, 1),
                    new Vector3(1, 0, 0),
                    new Vector3(0, 1, 0)}.Select(x=>x*LandscapeTool.BlockScale).ToArray(), new int[] { 0, 1, 2, 3,4,5 }, ~BlockSides.bottom)
        ,
        new VerTriSides(new Vector3[] {    new Vector3(0, 0, 0),
                    new Vector3(0, 1, 0),
                    new Vector3(1, 0, 0) }.Select(x=>x * LandscapeTool.BlockScale).ToArray(), new int[] { 0, 1, 2 }, BlockSides.back)
        ,
        new VerTriSides(new Vector3[] {   new Vector3(0, 0, 0),
                    new Vector3(0, 0, 1),
                    new Vector3(0, 1, 0)
      }.Select(x=>x * LandscapeTool.BlockScale).ToArray(), new int[] { 0, 1, 2 }, BlockSides.left)
        ,
        new VerTriSides(new Vector3[] {  new Vector3(0, 0, 0),
                    new Vector3(1, 0, 0),
                    new Vector3(1, 0, 1),
                    new Vector3(0, 0, 1) }.Select(x=>x * LandscapeTool.BlockScale).ToArray(), new int[] { 0, 1, 2, 2, 3, 0 }, BlockSides.bottom)
    }
        };

        public static BlockShape foot = new()
        {
            covers = BlockSides.all,
            meshSections = new VerTriSides[] {
        new VerTriSides(new Vector3[] { new Vector3(0, 1 , 0), new Vector3(0, 1 , 1 ), new Vector3(1 , 1 , 1 ), new Vector3(1 , 1 , 0) }.Select(x=>x*LandscapeTool.BlockScale).ToArray(), new int[] { 0, 1, 2, 2, 3, 0 }, BlockSides.top)
        ,
        new VerTriSides(new Vector3[] { new Vector3(1 + 1 / 3f, 0 - 1 / 3f, 1 + 1 / 3f), new Vector3(1 , 1 , 1 ), new Vector3(0, 1 , 1 ), new Vector3(0 - 1 / 3f, 0 - 1 / 3f, 1 + 1 / 3f) }.Select(x=>x*LandscapeTool.BlockScale).ToArray(), new int[] { 0, 1, 2, 2, 3, 0 }, BlockSides.front)
        ,
       new VerTriSides(new Vector3[] { new Vector3(1 + 1 / 3f, 0 - 1 / 3f, 0 - 1 / 3f), new Vector3(1 , 1 , 0), new Vector3(1 , 1 , 1 ), new Vector3(1 + 1 / 3f, 0 - 1 / 3f, 1 + 1 / 3f) }.Select(x=>x*LandscapeTool.BlockScale).ToArray(), new int[] { 0, 1, 2, 2, 3, 0 }, BlockSides.right)
        ,
        new VerTriSides(new Vector3[] { new Vector3(0 - 1 / 3f, 0 - 1 / 3f, 0 - 1 / 3f), new Vector3(0, 1 , 0), new Vector3(1 , 1 , 0), new Vector3(1 + 1 / 3f, 0 - 1 / 3f, 0 - 1 / 3f) }.Select(x=>x * LandscapeTool.BlockScale).ToArray(), new int[] { 0, 1, 2, 2, 3, 0 }, BlockSides.back)
        ,
        new VerTriSides(new Vector3[] { new Vector3(0 - 1 / 3f, 0 - 1 / 3f, 1 + 1 / 3f), new Vector3(0, 1 , 1 ), new Vector3(0, 1 , 0), new Vector3(0 - 1 / 3f, 0 - 1 / 3f, 0 - 1 / 3f) }.Select(x=>x * LandscapeTool.BlockScale).ToArray(), new int[] { 0, 1, 2, 2, 3, 0 }, BlockSides.left)
        ,
        new VerTriSides(new Vector3[] { new Vector3(1+1/3f , 0-1/3f, 0 - 1 / 3f), new Vector3(1 + 1 / 3f, 0-1 / 3f, 1 + 1 / 3f), new Vector3(0 - 1 / 3f, 0-1 / 3f, 1 + 1 / 3f), new Vector3(0 - 1 / 3f, -1 / 3f, 0 - 1 / 3f) }.Select(x=>x * LandscapeTool.BlockScale).ToArray(), new int[] { 0, 1, 2, 2, 3, 0 }, BlockSides.bottom)
    }
        };

        public static BlockShape foliage = new()
        {
            covers = BlockSides.all,
            meshSections = new VerTriSides[] {
        new VerTriSides(new Vector3[] {  new Vector3(1 , 0, 0.51f), new Vector3(1 , 1 , 0.51f), new Vector3(0, 1 , 0.51f), new Vector3(0, 0, 0.51f) }.Select(x=>(Quaternion.Euler(0,60,0)*(x-Vector3.one*.5f)+Vector3.one*.5f)*LandscapeTool.BlockScale).ToArray(), new int[] { 0, 1, 2, 2, 3, 0 }, BlockSides.all)
        ,
        new VerTriSides(new Vector3[] { new Vector3(1 , 0, 0.51f), new Vector3(1 , 1 , 0.51f), new Vector3(0, 1 , 0.51f), new Vector3(0, 0, 0.51f) }.Select(x=>x*LandscapeTool.BlockScale).ToArray(), new int[] { 0, 1, 2, 2, 3, 0 }, BlockSides.all)
        ,
       new VerTriSides(new Vector3[] {  new Vector3(1 , 0, 0.51f), new Vector3(1 , 1 , 0.51f), new Vector3(0, 1 , 0.51f), new Vector3(0, 0, 0.51f) }.Select(x=>(Quaternion.Euler(0,-60,0)*(x-Vector3.one*.5f)+Vector3.one*.5f)*LandscapeTool.BlockScale).ToArray(), new int[] { 0, 1, 2, 2, 3, 0 }, BlockSides.all)
        ,
        new VerTriSides(new Vector3[] { new Vector3(0, 0, 0.5f), new Vector3(0, 1 , 0.5f), new Vector3(1 , 1 , 0.5f), new Vector3(1 , 0, 0.5f) }.Select(x=>x * LandscapeTool.BlockScale).ToArray(), new int[] { 0, 1, 2, 2, 3, 0 }, BlockSides.all)
        ,
        new VerTriSides(new Vector3[] {new Vector3(0, 0, 0.5f), new Vector3(0, 1 , 0.5f), new Vector3(1 , 1 , 0.5f), new Vector3(1 , 0, 0.5f) }.Select(x=>(Quaternion.Euler(0,-60,0)*(x-Vector3.one*.5f)+Vector3.one*.5f) * LandscapeTool.BlockScale).ToArray(), new int[] { 0, 1, 2, 2, 3, 0 }, BlockSides.all)
        ,
        new VerTriSides(new Vector3[] { new Vector3(0, 0, 0.5f), new Vector3(0, 1 , 0.5f), new Vector3(1 , 1 , 0.5f), new Vector3(1 , 0, 0.5f) }.Select(x=>(Quaternion.Euler(0,60,0)*(x-Vector3.one*.5f)+Vector3.one*.5f) * LandscapeTool.BlockScale).ToArray(), new int[] { 0, 1, 2, 2, 3, 0 }, BlockSides.all)
    }
        };
        //taken raw from blender and converted into this via a custom script that is no longer included in the project files. You can still copy raw mesh data from blender and format it yourself.
        public static BlockShape toybrick = new()
        {
            covers = BlockSides.all,
            meshSections = new VerTriSides[] {
            new VerTriSides(new Vector3[] {
 new Vector3(0.8383883f,1.1245f,0.6616117f),new Vector3(0.75f,1.1245f,0.6250001f),new Vector3(0.8383883f,1.1245f,0.8383884f),new Vector3(0.875f,1.1245f,0.7500001f),new Vector3(0.6616117f,1.1245f,0.6616117f),new Vector3(0.75f,1.1245f,0.8750001f),new Vector3(0.6616117f,1.1245f,0.8383884f),new Vector3(0.625f,1.1245f,0.7500001f),new Vector3(0.75f,1.1245f,0.3750001f),new Vector3(0.8383883f,1.1245f,0.3383884f),new Vector3(0.875f,1.1245f,0.2500001f),new Vector3(0.6616117f,1.1245f,0.3383884f),new Vector3(0.8383883f,1.1245f,0.1616117f),new Vector3(0.625f,1.1245f,0.2500001f),new Vector3(0.6616117f,1.1245f,0.1616117f),new Vector3(0.75f,1.1245f,0.1250001f),new Vector3(0.3383883f,1.1245f,0.1616117f),new Vector3(0.25f,1.1245f,0.1250001f),new Vector3(0.3383883f,1.1245f,0.3383884f),new Vector3(0.375f,1.1245f,0.2500001f),new Vector3(0.1616117f,1.1245f,0.1616117f),new Vector3(0.25f,1.1245f,0.3750001f),new Vector3(0.125f,1.1245f,0.2500001f),new Vector3(0.1616117f,1.1245f,0.3383884f),new Vector3(0.25f,1.1245f,0.8750001f),new Vector3(0.3383883f,1.1245f,0.8383884f),new Vector3(0.375f,1.1245f,0.7500001f),new Vector3(0.1616117f,1.1245f,0.8383884f),new Vector3(0.3383883f,1.1245f,0.6616117f),new Vector3(0.125f,1.1245f,0.7500001f),new Vector3(0.1616117f,1.1245f,0.6616117f),new Vector3(0.25f,1.1245f,0.6250001f),new Vector3(0.25f,0.9995f,0.3750001f),new Vector3(0.25f,1.1245f,0.3750001f),new Vector3(0.1616117f,1.1245f,0.3383884f),new Vector3(0.1616117f,0.9995f,0.3383884f),new Vector3(0.3383883f,0.9995f,0.3383884f),new Vector3(0.3383883f,1.1245f,0.3383884f),new Vector3(0.125f,1.1245f,0.2500001f),new Vector3(0.125f,0.9995f,0.2500001f),new Vector3(0.375f,0.9995f,0.2500001f),new Vector3(0.375f,1.1245f,0.2500001f),new Vector3(0.1616117f,1.1245f,0.1616117f),new Vector3(0.1616117f,0.9995f,0.1616117f),new Vector3(0.3383883f,0.9995f,0.1616117f),new Vector3(0.3383883f,1.1245f,0.1616117f),new Vector3(0.25f,1.1245f,0.1250001f),new Vector3(0.25f,0.9995f,0.1250001f),new Vector3(0.625f,0.9995f,0.7500001f),new Vector3(0.625f,1.1245f,0.7500001f),new Vector3(0.6616117f,1.1245f,0.6616117f),new Vector3(0.6616117f,0.9994999f,0.6616117f),new Vector3(0.6616117f,0.9994999f,0.8383884f),new Vector3(0.6616117f,1.1245f,0.8383884f),new Vector3(0.75f,1.1245f,0.6250001f),new Vector3(0.75f,0.9995f,0.6250001f),new Vector3(0.75f,0.9995f,0.8750001f),new Vector3(0.75f,1.1245f,0.8750001f),new Vector3(0.8383883f,1.1245f,0.6616117f),new Vector3(0.8383883f,0.9994999f,0.6616117f),new Vector3(0.8383883f,0.9994999f,0.8383884f),new Vector3(0.8383883f,1.1245f,0.8383884f),new Vector3(0.875f,1.1245f,0.7500001f),new Vector3(0.875f,0.9995f,0.7500001f),new Vector3(0.125f,0.9995f,0.7500001f),new Vector3(0.125f,1.1245f,0.7500001f),new Vector3(0.1616117f,1.1245f,0.6616117f),new Vector3(0.1616117f,0.9994999f,0.6616117f),new Vector3(0.1616117f,0.9994999f,0.8383884f),new Vector3(0.1616117f,1.1245f,0.8383884f),new Vector3(0.25f,1.1245f,0.6250001f),new Vector3(0.25f,0.9995f,0.6250001f),new Vector3(0.25f,0.9995f,0.8750001f),new Vector3(0.25f,1.1245f,0.8750001f),new Vector3(0.3383883f,1.1245f,0.6616117f),new Vector3(0.3383883f,0.9994999f,0.6616117f),new Vector3(0.3383883f,0.9994999f,0.8383884f),new Vector3(0.3383883f,1.1245f,0.8383884f),new Vector3(0.375f,1.1245f,0.7500001f),new Vector3(0.375f,0.9995f,0.7500001f),new Vector3(0.625f,0.9995f,0.2500001f),new Vector3(0.625f,1.1245f,0.2500001f),new Vector3(0.6616117f,1.1245f,0.1616117f),new Vector3(0.6616117f,0.9995f,0.1616117f),new Vector3(0.6616117f,0.9995f,0.3383884f),new Vector3(0.6616117f,1.1245f,0.3383884f),new Vector3(0.75f,1.1245f,0.1250001f),new Vector3(0.75f,0.9995f,0.1250001f),new Vector3(0.75f,0.9995f,0.3750001f),new Vector3(0.75f,1.1245f,0.3750001f),new Vector3(0.8383883f,1.1245f,0.1616117f),new Vector3(0.8383883f,0.9995f,0.1616117f),new Vector3(0.8383883f,0.9995f,0.3383884f),new Vector3(0.8383883f,1.1245f,0.3383884f),new Vector3(0.875f,1.1245f,0.2500001f),new Vector3(0.875f,0.9995f,0.2500001f)
            }.Select(x=>x * LandscapeTool.BlockScale).ToArray(), new int[] {
      0,1,2,0,2,3,2,1,4,2,4,5,6,5,4,6,4,7,8,9,10,8,10,11,11,10,12,11,12,13,14,13,12,14,12,15,16,17,18,16,18,19,18,17,20,18,20,21,22,23,21,22,21,20,24,25,26,24,26,27,27,26,28,27,28,29,30,29,28,30,28,31,32,33,34,32,34,35,36,37,33,36,33,32,35,34,38,35,38,39,40,41,37,40,37,36,39,38,42,39,42,43,44,45,41,44,41,40,43,42,46,43,46,47,47,46,45,47,45,44,48,49,50,48,50,51,52,53,49,52,49,48,51,50,54,51,54,55,56,57,53,56,53,52,55,54,58,55,58,59,60,61,57,60,57,56,59,58,62,59,62,63,63,62,61,63,61,60,64,65,66,64,66,67,68,69,65,68,65,64,67,66,70,67,70,71,72,73,69,72,69,68,71,70,74,71,74,75,76,77,73,76,73,72,75,74,78,75,78,79,79,78,77,79,77,76,80,81,82,80,82,83,84,85,81,84,81,80,83,82,86,83,86,87,88,89,85,88,85,84,87,86,90,87,90,91,92,93,89,92,89,88,91,90,94,91,94,95,95,94,93,95,93,92
            }, BlockSides.top),

        new VerTriSides(new Vector3[] { new Vector3(0, 1 , 0), new Vector3(0, 1 , 1 ), new Vector3(1 , 1 , 1 ), new Vector3(1 , 1 , 0) }.Select(x=>x * LandscapeTool.BlockScale).ToArray(), new int[] { 0, 1, 2, 2, 3, 0 }, BlockSides.top)
        ,
        new VerTriSides(new Vector3[] { new Vector3(1 , 0, 1 ), new Vector3(1 , 1 , 1 ), new Vector3(0, 1 , 1 ), new Vector3(0, 0, 1 ) }.Select(x=>x * LandscapeTool.BlockScale).ToArray(), new int[] { 0, 1, 2, 2, 3, 0 }, BlockSides.front)
        ,
       new VerTriSides(new Vector3[] { new Vector3(1 , 0, 0), new Vector3(1 , 1 , 0), new Vector3(1 , 1 , 1 ), new Vector3(1 , 0, 1 ) }.Select(x=>x * LandscapeTool.BlockScale).ToArray(), new int[] { 0, 1, 2, 2, 3, 0 }, BlockSides.right)
        ,
        new VerTriSides(new Vector3[] { new Vector3(0, 0, 0), new Vector3(0, 1 , 0), new Vector3(1 , 1 , 0), new Vector3(1 , 0, 0) }.Select(x=>x * LandscapeTool.BlockScale).ToArray(), new int[] { 0, 1, 2, 2, 3, 0 }, BlockSides.back)
        ,
        new VerTriSides(new Vector3[] { new Vector3(0, 0, 1 ), new Vector3(0, 1 , 1 ), new Vector3(0, 1 , 0), new Vector3(0, 0, 0) }.Select(x=>x * LandscapeTool.BlockScale).ToArray(), new int[] { 0, 1, 2, 2, 3, 0 }, BlockSides.left)
        ,
        new VerTriSides(new Vector3[] {
       new Vector3(0f,0f,0f),new Vector3(0.1250429f,0f,0.1250429f),new Vector3(0.1250429f,0f,0.8749571f),new Vector3(0f,0f,1f),new Vector3(1f,0f,0f),new Vector3(0.8749571f,0f,0.1250429f),new Vector3(0.1250429f,0f,0.1250429f),new Vector3(0f,0f,0f),new Vector3(0.1250429f,0f,0.1250429f),new Vector3(0.1250429f,0.8750824f,0.1250429f),new Vector3(0.1250429f,0.8750824f,0.8749571f),new Vector3(0.1250429f,0f,0.8749571f),new Vector3(0f,0f,1f),new Vector3(0.1250429f,0f,0.8749571f),new Vector3(0.8749571f,0f,0.8749571f),new Vector3(1f,0f,1f),new Vector3(1f,0f,1f),new Vector3(0.8749571f,0f,0.8749571f),new Vector3(0.8749571f,0f,0.1250429f),new Vector3(1f,0f,0f),new Vector3(0.1250429f,0.8750824f,0.1250429f),new Vector3(0.8749571f,0.8750824f,0.1250429f),new Vector3(0.8749571f,0.8750824f,0.8749571f),new Vector3(0.1250429f,0.8750824f,0.8749571f),new Vector3(0.8749571f,0f,0.1250429f),new Vector3(0.8749571f,0.8750824f,0.1250429f),new Vector3(0.1250429f,0.8750824f,0.1250429f),new Vector3(0.1250429f,0f,0.1250429f),new Vector3(0.8749571f,0f,0.8749571f),new Vector3(0.8749571f,0.8750824f,0.8749571f),new Vector3(0.8749571f,0.8750824f,0.1250429f),new Vector3(0.8749571f,0f,0.1250429f),new Vector3(0.1250429f,0f,0.8749571f),new Vector3(0.1250429f,0.8750824f,0.8749571f),new Vector3(0.8749571f,0.8750824f,0.8749571f),new Vector3(0.8749571f,0f,0.8749571f)
        }.Select(x=>x * LandscapeTool.BlockScale).ToArray(), new int[] {
          0,1,2,0,2,3,4,5,6,4,6,7,8,9,10,8,10,11,12,13,14,12,14,15,16,17,18,16,18,19,20,21,22,20,22,23,24,25,26,24,26,27,28,29,30,28,30,31,32,33,34,32,34,35
        }, BlockSides.bottom)
    }
        };
        #endregion
    }
}