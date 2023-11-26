/*Antonio Wiege*/
using System.Collections.Generic;
using UnityEngine;

namespace ProceduralBlockWorldGeneratorToolPrototyping
{
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

        public void TriggerEffect()
        {
            if (directorInstance == null) return;
            if(directorInstance.pipeline == null) return;
            voxelPositions = VoxelizeByCollision.Run(gameObject);//GetComponent<Collider>().bounds,true
            switch (brushMode)
            {
                case BrushType.directionalErosion:
                    for (int i = 0; i < voxelPositions.Count; i++)
                    {
                        var pos = (voxelPositions[i] * 3).ToInt3();
                        directorInstance.Sculpting_and_Brush_setup.
                    SubErode(pos, value, true);
                    }
                    break;
                case BrushType.simpleErosion:
                    for (int i = 0; i < voxelPositions.Count; i++)
                    {
                        var pos = (voxelPositions[i] * 3).ToInt3();
                        directorInstance.Sculpting_and_Brush_setup.
                    SubErode(pos, value, false);
                    }
                    break;
                case BrushType.color:
                    for (int i = 0; i < voxelPositions.Count; i++)
                    {
                        var pos = (voxelPositions[i] * 3).ToInt3();
                        directorInstance.SetColor(pos, value);
                    }
                    break;
                case BrushType.matter:
                    for (int i = 0; i < voxelPositions.Count; i++)
                    {
                        var pos = (voxelPositions[i] * 3).ToInt3();
                        directorInstance.SetMatter(voxelPositions[i].ToInt3(), Matter_Library.GetByName(matterToSet));
                    }
                    break;
                case BrushType.shape:
                    for (int i = 0; i < voxelPositions.Count; i++)
                    {
                        var pos = (voxelPositions[i] * 3).ToInt3();
                        directorInstance.SetOrAddValue(pos, value);
                    }
                    break;
                case BrushType.block:
                    for (int i = 0; i < voxelPositions.Count; i++)
                    {
                        var pos = (voxelPositions[i] * 3).ToInt3();
                        directorInstance.SetBlock(pos, Block_Library.GetCopyByName(blockToSet),true);
                    }
                    break;
                case BrushType.grass:
                    for (int i = 0; i < voxelPositions.Count; i++)
                    {
                        var pos = (voxelPositions[i] * 3).ToInt3();
                        if (Random.value < value) directorInstance.Sculpting_and_Brush_setup.SubGrass(pos);
                    }
                    break;
                case BrushType.tree:
                    for (int i = 0; i < voxelPositions.Count; i++)
                    {
                        var pos = (voxelPositions[i] * 3).ToInt3();
                       if(Random.value<value) directorInstance.Sculpting_and_Brush_setup.SubTree(pos);
                    }
                    break;
                case BrushType.scatter:
                    for (int i = 0; i < voxelPositions.Count; i++)
                    {
                        var pos = (voxelPositions[i] * 3).ToInt3();
                        if (Random.value < value) directorInstance.Sculpting_and_Brush_setup.SubScatter(pos);
                    }
                    break;
                default:
                    break;
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