/*Antonio Wiege*/
namespace ProceduralBlockWorldGeneratorToolPrototyping
{
    using System.Collections.Generic;
    using UnityEngine;
    /*
     A first set of predefined matters, currently no real matter system at hand, therefore tint
     */
    public static class Matter_Library
    {
        private static List<Matter> _matters = new();
        public static List<Matter> matters
        {
            get { 
                if(_matters == null)
                {
                    _matters = new();
                }
                if(_matters.Count < 1 ) {
                    _matters.AddRange(new Matter[]{ air,rock,grass,water,sand,silver,copper,dirt,wood,leaves,tallGrass,underwaterGrass,cactus});
                }
                return _matters; }
            set {  }
        }

        //*
           public static Matter air = new Matter() {name="Air",                                         color = new Color(1, 1, 1, 0), texturesAtlasOrigin = new Vector2(0, 0.875f) };
        public static Matter rock = new Matter() { name = "Rock",                                   color = new Color(.5f, .5f, .5f, 1), texturesAtlasOrigin = new Vector2(0.875f, 0.875f)  };
        public static Matter grass = new Matter() {name = "Grass",                                  color = new Color(.3f, 1, 0, 1), texturesAtlasOrigin = new Vector2(0.0f, 0.875f) };
        public static Matter water = new Matter() {name = "Water",                                  color = new Color(0, .3f, 1, .7f), texturesAtlasOrigin = new Vector2(0.25f, .5f) };
        public static Matter sand = new Matter() {name = "Sand",                                    color = new Color(.9f, .8f, 0, 1), texturesAtlasOrigin = new Vector2(0.25f, 0.875f) };
        public static Matter silver = new Matter() {name = "Silver",                                    color = new Color(1, 1, 1, 1), texturesAtlasOrigin = new Vector2(0.875f, .75f) };
        public static Matter copper = new Matter() {name = "Copper",                                color = new Color(0f, .9f, .8f, 1), texturesAtlasOrigin = new Vector2(0f, 0.5f) };
        public static Matter dirt = new Matter() {name = "Dirt",                                        color = new Color(.9f, .6f, .1f, 1), texturesAtlasOrigin = new Vector2(0.375f, 0.875f) };
        public static Matter wood = new Matter() { name = "Wood",                                   color = new Color(1,1,1, 1), texturesAtlasOrigin = new Vector2(0.25f, 0.625f) };
        public static Matter leaves = new Matter() { name = "Leaves",                               color = new Color(1, 1, 1, 1), texturesAtlasOrigin = new Vector2(0f, .75f) };
        public static Matter tallGrass = new Matter() { name = "TallGrass",                         color = new Color(.3f, .9f, 0, 1), texturesAtlasOrigin = new Vector2(0.25f, .75f) };
        public static Matter underwaterGrass = new Matter() { name = "UnderwaterGrass", color = new Color(.1f, .8f, .3f, 1), texturesAtlasOrigin = new Vector2(0.25f, .75f) };
        public static Matter cactus = new Matter() { name = "Cactus",                               color = new Color(.3f, .8f, .1f, 1), texturesAtlasOrigin = new Vector2(0.125f, .75f) };
         //*/

        public static Matter GetByName(string name)
        {
            if (matters == null)
            {
                Debug.Log("The matters libary list is null... that is not supposed to happen.");
            }
            for (int i = 0; i < matters.Count; i++)
            {
                if (matters[i] == null)
                {
                    Debug.Log("Who assigned a null value to the bloody matters library?!");
                }
                if (matters[i].name == name) return matters[i];
            }
            Debug.Log("Matter name not found, returning air.");
            return air;
        }

        public static Matter Air => air.CopyShallow();
        public static Matter Rock => rock.CopyShallow();
        public static Matter Grass => grass.CopyShallow();
        public static Matter Water => water.CopyShallow();
        public static Matter Sand => sand.CopyShallow();
        public static Matter Silver => silver.CopyShallow();
        public static Matter Copper => copper.CopyShallow();
        public static Matter Dirt => dirt.CopyShallow();
        public static Matter Wood => wood.CopyShallow();
        public static Matter Leaves => leaves.CopyShallow();
        public static Matter TallGrass => tallGrass.CopyShallow();
        public static Matter UnderwaterGrass => underwaterGrass.CopyShallow();
        public static Matter Cactus => cactus.CopyShallow();
    }
}