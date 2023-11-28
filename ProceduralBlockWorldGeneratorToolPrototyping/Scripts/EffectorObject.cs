/*Antonio Wiege*/
using System.Collections.Generic;
using UnityEngine;

namespace ProceduralBlockWorldGeneratorToolPrototyping
{
    /// <summary>Performs BrushAndSculpt operation on positions of voxel from voxelized collider. NonConvex defaults to hull only.</summary>
    public class EffectorObject : MonoBehaviour
    {
        [Header("Assign manually to the landscape tool instance")]
        public LandscapeTool directorInstance;

        [Header("This case of SetBlock will set the density too")]
        public BrushType brushMode = BrushType.shape;

        public float value;
        public string matterToSet = "Grass";
        public string blockToSet = "Rock";
        public Color colorToPaint = Color.cyan;

        List<Vector3> voxelPositions = new();

        [ContextMenu("TriggerEffect(OnlyDuringPlayMode)")]
        public void TriggerEffect()
        {
            if (directorInstance == null) return;
            if(directorInstance.pipeline == null) return;

            voxelPositions = VoxelizeByCollision.Run(gameObject);

            for (int i = 0; i < voxelPositions.Count; i++)
            {
                var pos = (voxelPositions[i] * 3).ToInt3();

                switch (brushMode)
                {
                    case BrushType.directionalErosion:
                            directorInstance.Sculpting_and_Brush_setup.
                        SubErode(pos, value, true);
                        
                        break;
                    case BrushType.simpleErosion:
                            directorInstance.Sculpting_and_Brush_setup.
                        SubErode(pos, value, false);
                        
                        break;
                    case BrushType.color:
                            directorInstance.SetColor(pos, value);
                        
                        break;
                    case BrushType.matter:
                            directorInstance.SetMatter(voxelPositions[i].ToInt3(), Matter_Library.GetByName(matterToSet));
                        
                        break;
                    case BrushType.shape:
                            directorInstance.SetOrAddValue(pos, value);
                        
                        break;
                    case BrushType.block:
                            directorInstance.SetBlock(pos, Block_Library.GetCopyByName(blockToSet), true);
                        
                        break;
                    case BrushType.grass:
                            if (Random.value < value) directorInstance.Sculpting_and_Brush_setup.SubGrass(pos);
                        
                        break;
                    case BrushType.tree:
                            if (Random.value < value) directorInstance.Sculpting_and_Brush_setup.SubTree(pos);
                        
                        break;
                    case BrushType.scatter:
                            if (Random.value < value) directorInstance.Sculpting_and_Brush_setup.SubScatter(pos);
                        break;
                    default:
                        break;
                }
            }
        }

#if !Deactivate_Gizmos
        private void OnDrawGizmos()
        {
            for (int i = 0; i < voxelPositions.Count; i++)
            {
                Gizmos.DrawWireCube(voxelPositions[i], Vector3.one * LandscapeTool.BlockScale);
            }
        }
#endif
    }
}