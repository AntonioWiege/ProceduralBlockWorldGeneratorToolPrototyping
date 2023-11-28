/*Antonio Wiege*/
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProceduralBlockWorldGeneratorToolPrototyping
{
    public class Chunk : MonoBehaviour
    {
        #region variables
        /// <summary> translate linear index to 3DlocalPos </summary>
        private static Int3[] _localIDtoPos;
        public static Int3[] LocalIDtoPos
        {
            get
            {
                if (_localIDtoPos == null)
                {
                    _localIDtoPos = new Int3[LandscapeTool.ChunkScale * LandscapeTool.ChunkScale * LandscapeTool.ChunkScale];
                    for (int z = 0; z < LandscapeTool.ChunkScale; z++)
                    {
                        for (int y = 0; y < LandscapeTool.ChunkScale; y++)
                        {
                            for (int x = 0; x < LandscapeTool.ChunkScale; x++)
                            {
                                Int3 pos = new Int3(x, y, z);
                                _localIDtoPos[pos.ToLinearChunkScaleIndex()] = pos;
                            }
                        }
                    }
                }
                return _localIDtoPos;
            }
        }

        public LandscapeTool directorInstance;

        public bool gameActive;

        public Chunk[,,] surroundChunks = new Chunk[3, 3, 3];
        /// <summary>position in chunk space and unique identifier in assigned directorInstance</summary>
        public Int3 key;

        /// <summary> step in pipeline (any one step), 
        /// may only be set without prior ForceChunkUpToState by 
        /// the step defining functions in the pipeline themselfes </summary>
        public int genState;

        public bool updateAllBlocks;

        public HashSet<Int3> blocksToUpdate = new();

        public Mesh m;
        public MeshFilter mf;
        public MeshRenderer mr;
        public MeshCollider mc;

        [HideInInspector]
        public float[] densityMap = new float[LandscapeTool.ChunkScale * LandscapeTool.ChunkScale * LandscapeTool.ChunkScale];

        [HideInInspector]
        public Block[] blocks = new Block[LandscapeTool.ChunkScale * LandscapeTool.ChunkScale * LandscapeTool.ChunkScale];

        [HideInInspector]
        public int[] primaryBiomeID = new int[LandscapeTool.ChunkScale * LandscapeTool.ChunkScale * LandscapeTool.ChunkScale];

        [HideInInspector]
        public Color[] tint = new Color[LandscapeTool.ChunkScale * LandscapeTool.ChunkScale * LandscapeTool.ChunkScale];

        public bool updateDecorations = true;

        //could put beneath into a new class for safety, but as long as minded perfectly in parallel remain separate for the benefits of original type.
        [Tooltip("In Local Chunk Space"), HideInInspector]
        public List<Int3> decoPositions = new();

        [HideInInspector]
        public List<Block> decoBlocks = new();

        [Tooltip("In Local Chunk Space"), HideInInspector]
        public List<Int3> scatteredPositions = new();

        [HideInInspector]
        public List<GameObject> scatteredObjects = new();

        public List<TreeInstance> trees = new();//just like grass becomes part of the main mesh

        // need new entries for TreeInstance regeneration
        public bool alreadyDecorated = false;
        #endregion

        public override string ToString()
        {
            return "Chunk:" + key.ValueSetOnly_ToString();
        }

        public List<Chunk> SurroundRealToList
        {
            get
            {
                var s = new List<Chunk>();
                foreach (var item in surroundChunks)
                {
                    if (item == null) continue;
                    s.Add(item);
                }
                return s;
            }
            set { }
        }

        //By all means there should never be the same chunk with the same key and directorInstance twice, so this will suffice for quicker comparison
        public override int GetHashCode()
        {
            return HashCode.Combine(directorInstance, key);
        }

        //reaffirmed equlas operations and hashcode by checking the bellow
        //https://grantwinney.com/how-to-compare-two-objects-testing-for-equality-in-c/
        //https://stackoverflow.com/questions/25461585/operator-overloading-equals
        //https://docs.microsoft.com/de-de/dotnet/api/system.hashcode?view=net-6.0
        public override bool Equals(object obj) => obj is Chunk o && Equals(o);

        public static bool operator ==(Chunk x, Chunk y)
        {
            return Equals(x, y);
        }

        public static bool operator !=(Chunk x, Chunk y)
        {
            return !(x == y);
        }

        public bool Equals(Chunk other)
        {
            if (this is null) return other is null;

            if (other is null) return this is null;

            if (ReferenceEquals(this, other)) return true;
            return (other.directorInstance, other.key).Equals((directorInstance, key));
        }

        public static bool Equals(Chunk one, Chunk other)
        {
            if (one is null) return other is null;
            if (other is null) return one is null;
            if (ReferenceEquals(one, other)) return true;
            return (other.directorInstance, other.key).Equals((one.directorInstance, one.key));
        }


#if !Deactivate_Gizmos
        private void OnDrawGizmos()
        {
            float m = (directorInstance.sphericalOffsetLookUpTable[Mathf.Clamp(directorInstance.chunksToLoadAround, 0, directorInstance.sphericalOffsetLookUpTable.Length - 1)].AsVector3 * LandscapeTool.ChunkScale * LandscapeTool.BlockScale).magnitude;
            var t = Mathf.Clamp01(
                (m - Vector3.Distance(transform.position, directorInstance.pointOfChunkOfRelevance.AsVector3 * LandscapeTool.ChunkScale * LandscapeTool.BlockScale)) / m
                );

            Gizmos.color = Color.white * t + Color.black * (1f - t);

            Gizmos.DrawWireCube(transform.position + Vector3.one * LandscapeTool.ChunkScale * LandscapeTool.BlockScale * 0.5f, Vector3.one * LandscapeTool.ChunkScale * LandscapeTool.BlockScale);
        }
#endif
    }
}
