/*Antonio Wiege*/
namespace ProceduralBlockWorldGeneratorToolPrototyping
{
    using System;
    using UnityEngine;
    [Serializable]
    public class Matter
    {
        [Tooltip("Material Name")]
        public string name;//used to find & select material
        [Tooltip("Texture Atlas Local Pos")]
        public Vector2 texturesAtlasOrigin;
        //public Vector2 texturesAtlasOrigin_Top, texturesAtlasOrigin_Side, texturesAtlasOrigin_Bottom;//may add different textures within block
        [Tooltip("Tint")]
        public Color color;//to multiply texture with (shader tint)

        public Matter CopyShallow()
        {
            return new Matter() {name=name,texturesAtlasOrigin=texturesAtlasOrigin,color=color};
        }
    }
}