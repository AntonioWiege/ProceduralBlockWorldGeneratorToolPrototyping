/*Antonio Wiege*/
using System.Collections.Generic;
using UnityEngine;
namespace ProceduralBlockWorldGeneratorToolPrototyping
{
    public class VoxTest : MonoBehaviour
    {
        public float scale = 1;
        public List<Vector3> voxelPositions = new();
        [ContextMenu("Vox")]
        void Start()
        {
            voxelPositions = VoxelizeByCollision.Run(gameObject);
        }
#if !Deactivate_Gizmos
        private void OnDrawGizmos()
        {
            for (int i = 0; i < voxelPositions.Count; i++)
            {
                Gizmos.DrawWireCube((voxelPositions[i] * 3).ToInt3().AsVector3 / 3f, Vector3.one * LandscapeTool.BlockScale * scale);
            }
        }
#endif
    }
}