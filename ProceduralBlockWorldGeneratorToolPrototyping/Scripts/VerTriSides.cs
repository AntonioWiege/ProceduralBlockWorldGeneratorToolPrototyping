/*Antonio Wiege*/
using System;
using System.Collections.Generic;
using UnityEngine;
namespace ProceduralBlockWorldGeneratorToolPrototyping
{
    /// <summary>
    /// Minimal Mesh (Verts and Tris) together with what ever sides of a block they cover
    /// </summary>
    [Serializable]
    public class VerTriSides
    {
        public Vector3[] verts;
        public int[] tris;
        public BlockSides sides;

        public VerTriSides()
        {
            verts = new Vector3[0];
            tris = new int[0];
            sides = BlockSides.none;
        }

        public VerTriSides(Vector3[] verts, int[] tris, BlockSides sides)
        {
            this.verts = verts;
            this.tris = tris;
            this.sides = sides;
        }
        public VerTriSides(List<Vector3> verts, List<int> tris, BlockSides sides)
        {
            this.verts = verts.ToArray();
            this.tris = tris.ToArray();
            this.sides = sides;
        }

        public VerTriSides DeepCopy()
        {
            VerTriSides vts = new();
            vts.verts = new Vector3[verts.Length];
            for (int i = 0; i < verts.Length; i++)
            {
                vts.verts[i] = verts[i];
            }
            vts.tris = new int[tris.Length];
            for (int i = 0; i < tris.Length; i++)
            {
                vts.tris[i] = tris[i];
            }
            vts.sides = sides;
            return vts;
        }

        /// <summary>
        /// copies the original and adds offset to each copied vertice.
        /// <returns>copy of original with offset per vertice</returns>
        public static VerTriSides operator +(VerTriSides verTriSides, Vector3 offset)
        {
            var vts = new VerTriSides(new Vector3[verTriSides.verts.Length], new int[verTriSides.tris.Length], verTriSides.sides);
            verTriSides.tris.CopyTo(vts.tris, 0);
            verTriSides.verts.CopyTo(vts.verts, 0);
            for (int i = 0; i < vts.verts.Length; i++)
            {
                vts.verts[i] += offset;
            }
            return vts;
        }

        /// <summary>
        /// Combine all VerTriSides of all Blocks in a Chunk to one Mesh
        /// </summary>
        public static void ChunkToMesh(Chunk c, bool recalculateTangents = false, bool optimize = false)//IEnumerable
        {
            c.m.Clear();//This one quite matters. Not sure about the background but without this, destruction will make content arrays & lengths missmatch, causing probable triangle & other issues (not tested with&out optimization)

            int vertCount = 0, triCount = 0;

            //determine total vert & triangle count
            foreach (Block block in c.blocks)
            {
                vertCount += block.meshSection.verts.Length;
                triCount += block.meshSection.tris.Length;
            }
            foreach (Block block in c.decoBlocks)
            {
                vertCount += block.meshSection.verts.Length;
                triCount += block.meshSection.tris.Length;
            }
            foreach (var item in c.trees)
            {
                foreach (Block block in item.decoBlocks)
                {
                    vertCount += block.meshSection.verts.Length;
                    triCount += block.meshSection.tris.Length;
                }
            }

            Vector3[] vertices = new Vector3[vertCount];
            int[] tris = new int[triCount];
            Vector2[] uvs = new Vector2[vertCount];
            Color[] colors = new Color[vertCount];
            int vertIndex = 0;
            int trisIndex = 0;

            //load all mesh data, mind the offset correction for triangles
            for (int z = 0; z < LandscapeTool.ChunkScale; z++)
            {
                for (int y = 0; y < LandscapeTool.ChunkScale; y++)
                {
                    for (int x = 0; x < LandscapeTool.ChunkScale; x++)
                    {
                        int i = new Int3(x, y, z).LocalPosToLinearID();
                        foreach (var item in c.blocks[i].meshSection.tris)
                        {
                            tris[trisIndex] = item + vertIndex;
                            trisIndex++;
                        }
                        foreach (var item in c.blocks[i].meshSection.verts)
                        {
                            vertices[vertIndex] = item;
                            uvs[vertIndex] = c.blocks[i].matter.texturesAtlasOrigin;
                            colors[vertIndex] = c.blocks[i].matter.color * c.tint[i];
                            vertIndex++;
                        }
                    }
                }
            }
            for (int i = 0; i < c.decoBlocks.Count; i++)
            {
                foreach (var item in c.decoBlocks[i].meshSection.tris)
                {
                    tris[trisIndex] = item + vertIndex;
                    trisIndex++;
                }
       
                foreach (var item in c.decoBlocks[i].meshSection.verts)
                {
                    vertices[vertIndex] = item;
                    uvs[vertIndex] = c.decoBlocks[i].matter.texturesAtlasOrigin;
                    colors[vertIndex] = c.decoBlocks[i].matter.color;
                    vertIndex++;
                }
            }
            foreach (var tree in c.trees)
            {
                foreach (Block block in tree.decoBlocks)
                {
                    foreach (var item in block.meshSection.tris)
                    {
                        tris[trisIndex] = item + vertIndex;
                        trisIndex++;
                    }
             
                    foreach (var item in block.meshSection.verts)
                    {
                        vertices[vertIndex] = item;
                        uvs[vertIndex] = block.matter.texturesAtlasOrigin;
                        colors[vertIndex] = block.matter.color;
                        vertIndex++;
                    }
                }
            }
            
            //apply data to mesh instance
            c.m.SetVertices(vertices);
            c.m.SetTriangles(tris, 0);
            c.m.SetUVs(0, uvs);
            c.m.SetColors(colors);
            c.m.RecalculateBounds();
            c.m.RecalculateNormals();
            if (recalculateTangents) c.m.RecalculateTangents();
            if (optimize) c.m.Optimize();
            c.m.MarkModified();
        }

        /// <summary>
        /// Combines the separate meshData sections of a BlockShape to one whole, minding spatial offset and ignoring parts that are covered by the neighboring states
        /// </summary>
        public static VerTriSides CombineShape(BlockShape blockShape, BlockSides neighboursCovering, Vector3 offset)
        {
            //if no mesh data or completely covered return empty mesh
            if (blockShape.meshSections[0].verts.Length == 0) return BlockShape_Library.empty.meshSections[0];
            if (neighboursCovering.Count() > 5) return BlockShape_Library.empty.meshSections[0];

            int vertCount = 0, triCount = 0;
            //count total lengths
            foreach (var vts in blockShape.meshSections)
            {
                if ((vts.sides & ~neighboursCovering) == 0) continue;
                vertCount += vts.verts.Length;
                triCount += vts.tris.Length;
            }

            //combine mesh data with corrected triangle offset
            VerTriSides VTS = new() { verts = new Vector3[vertCount], tris = new int[triCount], sides = BlockSides.none };
            int vertIndex = 0;
            int trisIndex = 0;
            foreach (var vts in blockShape.meshSections)
            {
                if ((vts.sides & ~neighboursCovering) == 0) continue;
                foreach (var item in vts.tris)
                {
                    VTS.tris[trisIndex] = item + vertIndex;
                    trisIndex++;
                }
                foreach (var item in vts.verts)
                {
                    VTS.verts[vertIndex] = item + offset;
                    vertIndex++;
                }
                VTS.sides |= vts.sides;
            }
            return VTS;
        }
    }
}