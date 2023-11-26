/*Antonio Wiege*/
using System;
namespace ProceduralBlockWorldGeneratorToolPrototyping
{
    [Serializable]
    public class Block
    {
        //changes rarely, minded on compariosn
        /// <summary>The complete shape of this cube</summary>
        public BlockShape blockShape = BlockShape_Library.empty;
        public Matter matter = Matter_Library.air;

        //changes likely, ignored on comparison
        /// <summary>The current part of the blocks total shape, that should be rendered</summary>
        public VerTriSides meshSection = BlockShape_Library.empty.meshSections[0];
        BlockSides lastNeighborState;


        /// <summary>
        /// Update MeshSection by checking whether there is any potential mesh to show, then assemble based on neighbours occupiance
        /// </summary>
        public void UpdateMeshSection(Int3 posLocal, Chunk piece)
        {
            var newNeighborState = CalcNeighbourCovers(posLocal, piece);
            if (lastNeighborState != newNeighborState)
            {
                lastNeighborState = newNeighborState;
                meshSection = VerTriSides.CombineShape(blockShape, newNeighborState, posLocal.ToVector3() * LandscapeTool.BlockScale);
            }
        }

        /// <summary>
        /// check what neighbours occlude; Needs neighbors to be sufficiently generated to address correct data states
        /// </summary>
        public BlockSides CalcNeighbourCovers(Int3 posLocal, Chunk piece, bool existanceOnly=false)
        {
            BlockSides neighboursCovering = BlockSides.none;
            Int3 neighbourPos;

                neighbourPos = posLocal + Int3.top;
                if (neighbourPos.y > LandscapeTool.ChunkScale - 1)
                {
                    if ((piece.surroundChunks[1, 2, 1]?.blocks[posLocal.x + posLocal.z * LandscapeTool.ChunkScale * LandscapeTool.ChunkScale].blockShape.covers & (existanceOnly?BlockSides.all:BlockSides.bottom)) != 0)
                    {
                        neighboursCovering |= BlockSides.top;
                    }
                }
                else if ((piece.blocks[neighbourPos.ToLinearChunkScaleIndex()].blockShape.covers & (existanceOnly ? BlockSides.all : BlockSides.bottom)) != 0)
                {
                    neighboursCovering |= BlockSides.top;
                }

                neighbourPos = posLocal + Int3.front;
                if (neighbourPos.z > LandscapeTool.ChunkScale - 1)
                {
                    if ((piece.surroundChunks[1, 1, 2]?.blocks[posLocal.x + posLocal.y * LandscapeTool.ChunkScale].blockShape.covers & (existanceOnly ? BlockSides.all : BlockSides.back)) != 0)
                    {
                        neighboursCovering |= BlockSides.front;
                    }
                }
                else if ((piece.blocks[neighbourPos.ToLinearChunkScaleIndex()].blockShape.covers & (existanceOnly ? BlockSides.all : BlockSides.back)) != 0)
                {
                    neighboursCovering |= BlockSides.front;
                }

                neighbourPos = posLocal + Int3.right;
                if (neighbourPos.x > LandscapeTool.ChunkScale - 1)
                {
                    if ((piece.surroundChunks[2, 1, 1]?.blocks[posLocal.y * LandscapeTool.ChunkScale + posLocal.z + LandscapeTool.ChunkScale * LandscapeTool.ChunkScale].blockShape.covers & (existanceOnly ? BlockSides.all : BlockSides.left)) != 0)
                    {
                        neighboursCovering |= BlockSides.right;
                    }
                }
                else if ((piece.blocks[neighbourPos.ToLinearChunkScaleIndex()].blockShape.covers & (existanceOnly ? BlockSides.all : BlockSides.left)   ) != 0)
                {
                    neighboursCovering |= BlockSides.right;
                }

                neighbourPos = posLocal + Int3.back;
                if (neighbourPos.z < 0)
                {
                    if ((piece.surroundChunks[1, 1, 0]?.blocks[posLocal.x + posLocal.y * LandscapeTool.ChunkScale + (LandscapeTool.ChunkScale - 1) * LandscapeTool.ChunkScale * LandscapeTool.ChunkScale].blockShape.covers & (existanceOnly ? BlockSides.all : BlockSides.front)) != 0)
                    {
                        neighboursCovering |= BlockSides.back;
                    }
                }
                else if ((piece.blocks[neighbourPos.ToLinearChunkScaleIndex()].blockShape.covers & (existanceOnly ? BlockSides.all : BlockSides.front)) != 0)
                {
                    neighboursCovering |= BlockSides.back;
                }

                neighbourPos = posLocal + Int3.left;
                if (neighbourPos.x < 0)
                {
                    if ((piece.surroundChunks[0, 1, 1]?.blocks[LandscapeTool.ChunkScale - 1 + posLocal.y * LandscapeTool.ChunkScale + posLocal.z * LandscapeTool.ChunkScale * LandscapeTool.ChunkScale].blockShape.covers & (existanceOnly ? BlockSides.all : BlockSides.right)) != 0)
                    {
                        neighboursCovering |= BlockSides.left;
                    }
                }
                else if ((piece.blocks[neighbourPos.ToLinearChunkScaleIndex()].blockShape.covers & (existanceOnly ? BlockSides.all : BlockSides.right)) != 0)
                {
                    neighboursCovering |= BlockSides.left;
                }

                neighbourPos = posLocal + Int3.bottom;
                if (neighbourPos.y < 0)
                {
                    if ((piece.surroundChunks[1, 0, 1]?.blocks[posLocal.x + (LandscapeTool.ChunkScale - 1) * LandscapeTool.ChunkScale + posLocal.z * LandscapeTool.ChunkScale * LandscapeTool.ChunkScale].blockShape.covers & (existanceOnly ? BlockSides.all : BlockSides.top)) != 0)
                    {
                        neighboursCovering |= BlockSides.bottom;
                    }
                }
                else if ((piece.blocks[neighbourPos.ToLinearChunkScaleIndex()].blockShape.covers & (existanceOnly ? BlockSides.all : BlockSides.top)) != 0)
                {
                    neighboursCovering |= BlockSides.bottom;
                }

            return neighboursCovering;
        }



        public Block CopyShallow()//to side step object boxing and new assignement
        {
            return new Block() { blockShape = blockShape, meshSection = meshSection, matter = matter };
        }

        //
        //  Equals & Hashcode ignore the likely to change data and only evaluate the rarely changing data in this works context.
        //

        public override bool Equals(object obj) => obj is Block o && Equals(o);

        public static bool operator ==(Block x, Block y)
        {
            return Equals(x, y);
        }

        public static bool operator !=(Block x, Block y)
        {
            return !(x == y);
        }

        public bool Equals(Block other)
        {
            if (this is null) return other is null;
            
            if (other is null)return this is null;
            
            if (ReferenceEquals(this, other)) return true;
            return (other.blockShape, other.matter).Equals((blockShape, matter));
        }
        public static bool Equals(Block one, Block other)
        {
            if (one is null)  return other is null;
            if (other is null) return one is null;
            if (ReferenceEquals(one, other)) return true;
            return (other.blockShape, other.matter).Equals((one.blockShape, one.matter));
        }

        public override int GetHashCode() => HashCode.Combine(blockShape, matter);
    }
}