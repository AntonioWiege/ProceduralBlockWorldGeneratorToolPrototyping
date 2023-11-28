/*Antonio Wiege*/
using ProceduralBlockWorldGeneratorToolPrototyping;
using System;
using System.Collections.Generic;
using UnityEngine;
namespace ProceduralBlockWorldGeneratorToolPrototyping
{
    [Serializable]
    public class Biome
    {
        [Tooltip("x,y,z:red,green,blue:temperature,variant,moisture")]
        [InspectorName("Temp, Var, Moist")]
        public Vector3 point_in_Biome_Value_Space;

        [Tooltip("List of all noises that shape this biomes terrain")]
        public List<NoiseInstanceSetup> noises;

        public Matter SurfaceLand = Matter_Library.Grass, SurfaceUnderwater = Matter_Library.Sand, Underground=Matter_Library.Rock;

        [Tooltip("What ever gameObjects are assigned here will be scattered on the ground of the biome, for demonstration without chance for starters.")]
        public List<GameObject> scatterDecoratives = new();

        [Tooltip("Select type of tree")]
        public TreeType treeType=TreeType.oak;

        [Tooltip("Amount of trees generated")]
        public float treeDensity=0.01f;

        [Tooltip("Amount of grass generated")]
        public float grassDensity=0.1f;

        [Tooltip("Amount of scatterings")]
        public float scatterDensity = 0.05f;

        [Tooltip("Toggle grass generation")]
        public bool generateGrass=true;

        [Tooltip("How deep trees may reach into water")]
        public float treeWaterTollerance = 0f;
    }
}

[Serializable]
public class TreeInstance
{
    //separate mesh objects, as to not be part of being painted & other terrain brushes. Checks real neighbors, but renders separately like grass.
    [Tooltip("Local Tree Space, trunk begins at origin bottom")]
    public List<Int3> decoPositions = new();

    public Int3 chunkSpaceOrigin;

    public List<Block> decoBlocks = new();
}