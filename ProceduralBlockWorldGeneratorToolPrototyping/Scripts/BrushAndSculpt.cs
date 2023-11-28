/*Antonio Wiege*/
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
//the readme mentioned some scripts being horrible to read and understand. This is one of them.
namespace ProceduralBlockWorldGeneratorToolPrototyping
{
    [Serializable]//to make it show up in the inspector
    public class BrushAndSculpt
    {
        #region variables
        //User defined
        [Tooltip("Effect Type")]
        public BrushType brushType = BrushType.shape;

        [Tooltip("Effect Distribution")]
        public BrushMode brushMode;

        [Tooltip("To voxelize & use as stamp")]
        public Mesh meshToBrush;

        [Tooltip("Reference to take from Matter Library")]
        public string matterToSet = "Grass";

        [Tooltip("Reference to take from Block Library")]
        public string blockToSet = "Rock";

        [Tooltip("Overwrite block tint")]
        public Color colorToPaint = Color.cyan;

        [Tooltip("Primary & Secondary random affectors")]
        public float randomA = 0.1f, randomB = 0.1f;

        [Tooltip("Custom falloff curve for non mesh brush (std linear), needs to be set at runtime")]
        public AnimationCurve falloff = new AnimationCurve();

        [Tooltip("in meters")]
        [Range(.1f, 30)]
        public float scaleRadius = 1f;//brush has base scale this is mod later think BuffFoat&CoKG^tm

        [Tooltip("Rotation of brush is controlled via secondary axis UIOJKL -z+x+z-y-x+y")]
        public Quaternion rotation = Quaternion.identity;

        //System defined
        LandscapeTool directorInstance;

        Mesh previousMesh;

        float previousRadius = -1f;

        Quaternion previousRotation;

        bool previousConvexState;

        bool LeftClick;

        [Tooltip(" if any of the previous values != the new ones => the mesh needs to be revoxelized"), HideInInspector]
        public List<Vector3> voxelPositions = new();

        float value = 0;

        Int3 pos = Int3.zero;

        int scaleRadiusInBlocks = 0;

        #endregion

        public BrushAndSculpt(LandscapeTool inCharge)
        {
            directorInstance = inCharge;
            //create basic linear falloff in animationCurve instance
            falloff.AddKey(0, 0);
            falloff.AddKey(1, 1);
            falloff.AddKey(-1, -1);
        }


        public void Update()
        {
            //Update scale
            scaleRadius = Mathf.Clamp(scaleRadius + scaleRadius * Input.GetAxisRaw("Mouse ScrollWheel"), 0.1f, 300);
            scaleRadiusInBlocks = (int)(scaleRadius / LandscapeTool.BlockScale);
            //Update rotation (like QWEASD but UIOJKL)
            rotation *= Quaternion.Euler((Input.GetKey(KeyCode.I) ? 1 : 0) - (Input.GetKey(KeyCode.K) ? 1 : 0), (Input.GetKey(KeyCode.J) ? 1 : 0) - (Input.GetKey(KeyCode.L) ? 1 : 0), (Input.GetKey(KeyCode.U) ? 1 : 0) - (Input.GetKey(KeyCode.O) ? 1 : 0));

            //Regenerate Mesh on Change
            if (previousMesh != meshToBrush || previousConvexState != directorInstance.forceConvex || previousRadius != scaleRadius || previousRotation != rotation)
            {
                previousMesh = meshToBrush; previousConvexState = directorInstance.forceConvex; previousRadius = scaleRadius; previousRotation = rotation;
                VoxelizeBrush();//consider making this async for bigger scales
            }

            //Perform Action e.g. Build / Destroy
            if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
            {
                LeftClick = Input.GetMouseButtonDown(0);
                value = LeftClick ? 1 : -1;
                pos = LeftClick ? directorInstance.currentPosIn : directorInstance.currentPosOut;
                StrokeBrush();
            }
        }

        void VoxelizeBrush()
        {
            if (meshToBrush != null)
            {
                var meshToVoxelize = meshToBrush;
                directorInstance.helper.SetActive(true);
                directorInstance.helper.GetComponent<MeshCollider>().sharedMesh = meshToVoxelize;
                directorInstance.helper.GetComponent<MeshCollider>().convex = previousConvexState;

                directorInstance.helper.transform.localScale = Vector3.one * scaleRadius;
                directorInstance.helper.transform.localRotation = rotation;

                directorInstance.helper.transform.position = Vector3.zero;
                directorInstance.helper.GetComponent<MeshCollider>().enabled = true;
                voxelPositions = VoxelizeByCollision.Run(directorInstance.helper);
                directorInstance.helper.GetComponent<MeshCollider>().enabled = false;
                directorInstance.helper.transform.position = directorInstance.currentPosIn.AsVector3 * LandscapeTool.BlockScale;
            }
            else
            {
                voxelPositions.Clear();
            }
        }

        /// <returns>-1 to 1 inclusive</returns>
        float randomNegPosOne => (UnityEngine.Random.value - 0.5f) * 2f;


        void StrokeBrush()
        {
            switch (brushMode)
            {
                case BrushMode.single:
                    value += randomA * randomNegPosOne;
                    var col = colorToPaint;
                    colorToPaint = colorToPaint * (1f - randomB) + UnityEngine.Random.ColorHSV() * randomB;
                    SubStroke();
                    colorToPaint = col;
                    break;
                case BrushMode.randomWalk:
                    for (int i = 0; i < 3 + UnityEngine.Random.value * randomA; i++)
                    {
                        var v = new Int3((int)(scaleRadiusInBlocks * randomNegPosOne * randomB), (int)(scaleRadiusInBlocks * randomNegPosOne * randomB), (int)(scaleRadiusInBlocks * randomNegPosOne * randomB));
                        pos += v;
                        SubStroke();
                    }
                    break;
                case BrushMode.sprayCan:
                    var sop = pos;
                    for (int i = 0; i < 2 + UnityEngine.Random.value * 6; i++)
                    {
                        pos = sop;
                        pos += new Int3((int)(scaleRadiusInBlocks * randomNegPosOne * 3), (int)(scaleRadiusInBlocks * randomNegPosOne * 3), (int)(scaleRadiusInBlocks * randomNegPosOne * 3));
                        SubStroke();
                    }
                    break;
                case BrushMode.randomize:
                    value += randomA * randomNegPosOne;
                    col = colorToPaint;
                    colorToPaint = colorToPaint * (1f - randomA) + UnityEngine.Random.ColorHSV() * randomA;
                    pos += new Int3((int)(scaleRadiusInBlocks * randomNegPosOne * randomB), (int)(scaleRadiusInBlocks * randomNegPosOne * randomB), (int)(scaleRadiusInBlocks * randomNegPosOne * randomB));
                    SubStroke();
                    colorToPaint = col;
                    break;
                case BrushMode.blur:
                    SubStroke(true);
                    break;
                default:
                    break;
            }
        }

        void SubStroke(bool blur = false)
        {
            switch (brushType)
            {
                case BrushType.directionalErosion:
                    Erode(true);
                    break;
                case BrushType.simpleErosion:
                    Erode(false);
                    break;
                case BrushType.color:
                    BrushStrokeColor(blur);
                    break;
                case BrushType.matter:
                    BrushStrokeMatter();
                    break;
                case BrushType.shape:
                    BrushStrokeShape(blur);
                    break;
                case BrushType.block:
                    BrushStrokeBlock();
                    break;
                case BrushType.grass:
                    BrushStrokeGrass();
                    break;
                case BrushType.tree:
                    BrushStrokeTree();
                    break;
                case BrushType.scatter:
                    BrushStrokeScatter();
                    break;
                default:
                    break;
            }
        }


        void Erode(bool directional)
        {
            var val = value;
            var org_pos = pos;
            if (meshToBrush == null) //no mesh => regular radius based brush
            {
                if (scaleRadius < LandscapeTool.BlockScale)//only 1 pos to address
                {
                    SubErode(pos, value, directional);
                }
                else //multiple within radius to address
                {
                    int r = scaleRadiusInBlocks;
                    for (int z = -r; z <= r; z++)
                    {
                        for (int y = -r; y <= r; y++)
                        {
                            for (int x = -r; x <= r; x++)
                            {
                                var m = new Vector3(x * LandscapeTool.BlockScale, y * LandscapeTool.BlockScale, z * LandscapeTool.BlockScale).magnitude;
                                if (m > scaleRadius) continue;
                                pos = org_pos + new Int3(x, y, z);
                                SubErode(pos, value * falloff.Evaluate(1f - m / scaleRadius), directional);
                            }
                        }
                    }
                }
            }
            else //mesh => use voxelized positions
            {
                for (int i = 0; i < voxelPositions.Count; i++)
                {
                    pos = org_pos + (voxelPositions[i] * 3).ToInt3();
                    SubErode(pos, value, directional);
                }
            }
            value = val;
            pos = org_pos;
        }
        public void SubErode(Int3 pos, float value, bool directional = false)
        {
            //for each block of a local snapshot store chunk, pos & value for quick reference
            Chunk[] chunks = new Chunk[3 * 3 * 3];
            Int3[] localPos = new Int3[3 * 3 * 3];
            float[] densitySnapshot = new float[3 * 3 * 3];

            //int centerID = 13;//1+3+9

            //take snapshot of last state of densities
            for (int z = -1; z < 2; z++)
            {
                for (int y = -1; y < 2; y++)
                {
                    for (int x = -1; x < 2; x++)
                    {
                        var index = (x + 1) + (y + 1) * 3 + (z + 1) * 3 * 3;
                        var subPos = pos + new Int3(x, y, z);
                        var cp = FloorToGrid.GridFloor(subPos, LandscapeTool.ChunkScale);
                        chunks[index] = directorInstance.RequestPiece(cp, true);//unchecked map bounds
                        localPos[index] = subPos - (cp * LandscapeTool.ChunkScale);//unchecked array bounds
                        directorInstance.pipeline.ForceChunkUpToState(chunks[index], 1);
                        densitySnapshot[index] = chunks[index].densityMap[localPos[index].ToLinearChunkScaleIndex()];
                    }
                }
            }

            //make edits based on the snapshot
            if (directional)
            {
                for (int z = -1; z < 2; z++)
                {
                    for (int y = -1; y < 2; y++)
                    {
                        for (int x = -1; x < 2; x++)
                        {
                            /*  only the center block gets and adds based of the snapshot
                            others set density to snapshot - value times dot erosion direction, either own or center block base value*/
                            if (x == 0 && y == 0 && z == 0) continue;
                            var index = (x + 1) + (y + 1) * 3 + (z + 1) * 3 * 3;
                            var direction = -new Vector3(x, y, z);
                            var dot = Vector3.Dot(rotation * Vector3.down, direction);
                            if (dot < 0)
                            {
                                chunks[index].densityMap[localPos[index].ToLinearChunkScaleIndex()] += densitySnapshot[index] * value * .5f * Mathf.Abs(dot);
                                chunks[13].densityMap[localPos[13].ToLinearChunkScaleIndex()] -= densitySnapshot[index] * value * .5f * Mathf.Abs(dot);
                            }
                            else
                            {
                                chunks[13].densityMap[localPos[13].ToLinearChunkScaleIndex()] += densitySnapshot[index] * value * .5f * dot;
                                chunks[index].densityMap[localPos[index].ToLinearChunkScaleIndex()] -= densitySnapshot[index] * value * .5f * dot;
                            }
                        }
                    }
                }
            }
            else
            {
                /*
                 if block density is over threshhold to exist, check neighbors beneath and spread half value
                 */
                if (chunks[1 + 3 + 9].densityMap[localPos[1 + 3 + 9].ToLinearChunkScaleIndex()] >= directorInstance.densityToRealityBorder)
                {
                    if (chunks[1 + 0 + 9].densityMap[localPos[1 + 0 + 9].ToLinearChunkScaleIndex()] < directorInstance.densityToRealityBorder)
                    {
                        chunks[1 + 3 + 9].densityMap[localPos[1 + 3 + 9].ToLinearChunkScaleIndex()] -= value * .5f;
                        chunks[1 + 0 + 9].densityMap[localPos[1 + 0 + 9].ToLinearChunkScaleIndex()] += value * .5f;
                    }
                    //if diagonal nearby has lower density and the path there is unobstructed, erode downhill
                    else
                    {
                        if (chunks[0 + 3 + 9].densityMap[localPos[0 + 3 + 9].ToLinearChunkScaleIndex()] < directorInstance.densityToRealityBorder && chunks[0 + 0 + 9].densityMap[localPos[0 + 0 + 9].ToLinearChunkScaleIndex()] < directorInstance.densityToRealityBorder)
                        {
                            chunks[1 + 3 + 9].densityMap[localPos[1 + 3 + 9].ToLinearChunkScaleIndex()] -= value * .125f;
                            chunks[0 + 0 + 9].densityMap[localPos[0 + 0 + 9].ToLinearChunkScaleIndex()] += value * .125f;
                        }
                        if (chunks[2 + 3 + 9].densityMap[localPos[2 + 3 + 9].ToLinearChunkScaleIndex()] < directorInstance.densityToRealityBorder && chunks[2 + 0 + 9].densityMap[localPos[2 + 0 + 9].ToLinearChunkScaleIndex()] < directorInstance.densityToRealityBorder)
                        {
                            chunks[1 + 3 + 9].densityMap[localPos[1 + 3 + 9].ToLinearChunkScaleIndex()] -= value * .125f;
                            chunks[2 + 0 + 9].densityMap[localPos[2 + 0 + 9].ToLinearChunkScaleIndex()] += value * .125f;
                        }
                        if (chunks[1 + 3 + 18].densityMap[localPos[1 + 3 + 18].ToLinearChunkScaleIndex()] < directorInstance.densityToRealityBorder && chunks[1 + 0 + 18].densityMap[localPos[1 + 0 + 18].ToLinearChunkScaleIndex()] < directorInstance.densityToRealityBorder)
                        {
                            chunks[1 + 3 + 9].densityMap[localPos[1 + 3 + 9].ToLinearChunkScaleIndex()] -= value * .125f;
                            chunks[1 + 0 + 18].densityMap[localPos[1 + 0 + 18].ToLinearChunkScaleIndex()] += value * .125f;
                        }
                        if (chunks[1 + 3 + 0].densityMap[localPos[1 + 3 + 0].ToLinearChunkScaleIndex()] < directorInstance.densityToRealityBorder && chunks[1 + 0 + 0].densityMap[localPos[1 + 0 + 0].ToLinearChunkScaleIndex()] < directorInstance.densityToRealityBorder)
                        {
                            chunks[1 + 3 + 9].densityMap[localPos[1 + 3 + 9].ToLinearChunkScaleIndex()] -= value * .125f;
                            chunks[1 + 0 + 0].densityMap[localPos[1 + 0 + 0].ToLinearChunkScaleIndex()] += value * .125f;
                        }

                        if (chunks[0 + 3 + 9].densityMap[localPos[0 + 3 + 9].ToLinearChunkScaleIndex()] < directorInstance.densityToRealityBorder && chunks[1 + 3 + 0].densityMap[localPos[1 + 3 + 0].ToLinearChunkScaleIndex()] < directorInstance.densityToRealityBorder && chunks[0 + 0 + 0].densityMap[localPos[0 + 0 + 0].ToLinearChunkScaleIndex()] < directorInstance.densityToRealityBorder)
                        {
                            chunks[1 + 3 + 9].densityMap[localPos[1 + 3 + 9].ToLinearChunkScaleIndex()] -= value * .125f;
                            chunks[0 + 0 + 0].densityMap[localPos[0 + 0 + 0].ToLinearChunkScaleIndex()] += value * .125f;
                        }
                        if (chunks[2 + 3 + 9].densityMap[localPos[2 + 3 + 9].ToLinearChunkScaleIndex()] < directorInstance.densityToRealityBorder && chunks[1 + 3 + 18].densityMap[localPos[1 + 3 + 18].ToLinearChunkScaleIndex()] < directorInstance.densityToRealityBorder && chunks[2 + 0 + 18].densityMap[localPos[2 + 0 + 18].ToLinearChunkScaleIndex()] < directorInstance.densityToRealityBorder)
                        {
                            chunks[1 + 3 + 9].densityMap[localPos[1 + 3 + 9].ToLinearChunkScaleIndex()] -= value * .125f;
                            chunks[2 + 0 + 18].densityMap[localPos[2 + 0 + 18].ToLinearChunkScaleIndex()] += value * .125f;
                        }
                        if (chunks[1 + 3 + 18].densityMap[localPos[1 + 3 + 18].ToLinearChunkScaleIndex()] < directorInstance.densityToRealityBorder && chunks[0 + 3 + 9].densityMap[localPos[0 + 3 + 9].ToLinearChunkScaleIndex()] < directorInstance.densityToRealityBorder && chunks[0 + 0 + 18].densityMap[localPos[0 + 0 + 18].ToLinearChunkScaleIndex()] < directorInstance.densityToRealityBorder)
                        {
                            chunks[1 + 3 + 9].densityMap[localPos[1 + 3 + 9].ToLinearChunkScaleIndex()] -= value * .125f;
                            chunks[0 + 0 + 18].densityMap[localPos[0 + 0 + 18].ToLinearChunkScaleIndex()] += value * .125f;
                        }
                        if (chunks[1 + 3 + 0].densityMap[localPos[1 + 3 + 0].ToLinearChunkScaleIndex()] < directorInstance.densityToRealityBorder && chunks[2 + 3 + 9].densityMap[localPos[2 + 3 + 9].ToLinearChunkScaleIndex()] < directorInstance.densityToRealityBorder && chunks[2 + 0 + 0].densityMap[localPos[2 + 0 + 0].ToLinearChunkScaleIndex()] < directorInstance.densityToRealityBorder)
                        {
                            chunks[1 + 3 + 9].densityMap[localPos[1 + 3 + 9].ToLinearChunkScaleIndex()] -= value * .125f;
                            chunks[2 + 0 + 0].densityMap[localPos[2 + 0 + 0].ToLinearChunkScaleIndex()] += value * .125f;
                        }
                    }
                }
            }

            for (int i = 0; i < 3 * 3 * 3; i++)
            {
                chunks[i].genState = 1;
#if !Deactivate_Async
                if (!directorInstance.chunksUnderConstruction.Contains(chunks[i]))
#endif
                    directorInstance.chunksUnderConstruction.Add(chunks[i]);
                directorInstance.NeighboringBlocksRefreshSetup(localPos[i], chunks[i], 1);
            }
        }



        void BrushStrokeColor(bool blur = false)
        {
            var col = colorToPaint;
            var sop = pos;
            if (meshToBrush == null)
            {
                if (scaleRadius < LandscapeTool.BlockScale)
                {
                    if (blur)
                    {
                        var origin = directorInstance.GetBlock(pos);
                        var up = directorInstance.GetBlock(pos + Int3.up);
                        var right = directorInstance.GetBlock(pos + Int3.right);
                        var down = directorInstance.GetBlock(pos + Int3.down);
                        var left = directorInstance.GetBlock(pos + Int3.left);
                        colorToPaint = (origin.chunk.tint[origin.localPos.ToLinearChunkScaleIndex()]
                            + up.chunk.tint[up.localPos.ToLinearChunkScaleIndex()]
                            + right.chunk.tint[right.localPos.ToLinearChunkScaleIndex()]
                            + down.chunk.tint[down.localPos.ToLinearChunkScaleIndex()]
                            + left.chunk.tint[left.localPos.ToLinearChunkScaleIndex()]
                            ) / 5f;
                    }
                    directorInstance.SetColor(pos, 1);
                }
                else
                {
                    int r = scaleRadiusInBlocks;
                    for (int z = -r; z <= r; z++)
                    {
                        for (int y = -r; y <= r; y++)
                        {
                            for (int x = -r; x <= r; x++)
                            {
                                var m = new Vector3(x * LandscapeTool.BlockScale, y * LandscapeTool.BlockScale, z * LandscapeTool.BlockScale).magnitude;
                                if (m > scaleRadius) continue;
                                pos = sop + new Int3(x, y, z);
                                if (blur)
                                {
                                    var origin = directorInstance.GetBlock(pos);
                                    var up = directorInstance.GetBlock(pos + Int3.up);
                                    var right = directorInstance.GetBlock(pos + Int3.right);
                                    var down = directorInstance.GetBlock(pos + Int3.down);
                                    var left = directorInstance.GetBlock(pos + Int3.left);
                                    colorToPaint = (origin.chunk.tint[origin.localPos.ToLinearChunkScaleIndex()]
                                                    + up.chunk.tint[up.localPos.ToLinearChunkScaleIndex()]
                                        + right.chunk.tint[right.localPos.ToLinearChunkScaleIndex()]
                                        + down.chunk.tint[down.localPos.ToLinearChunkScaleIndex()]
                                        + left.chunk.tint[left.localPos.ToLinearChunkScaleIndex()]
                                        ) / 5f;
                                }
                                directorInstance.SetColor(pos, value * falloff.Evaluate(1f - m / scaleRadius));
                                colorToPaint = col;
                            }
                        }
                    }
                }
            }
            else
            {
                for (int i = 0; i < voxelPositions.Count; i++)
                {
                    pos = sop + (voxelPositions[i] * 3).ToInt3();
                    if (blur)
                    {
                        var origin = directorInstance.GetBlock(pos);
                        var up = directorInstance.GetBlock(pos + Int3.up);
                        var right = directorInstance.GetBlock(pos + Int3.right);
                        var down = directorInstance.GetBlock(pos + Int3.down);
                        var left = directorInstance.GetBlock(pos + Int3.left);
                        colorToPaint = (origin.chunk.tint[origin.localPos.ToLinearChunkScaleIndex()]
                            + up.chunk.tint[up.localPos.ToLinearChunkScaleIndex()]
                            + right.chunk.tint[right.localPos.ToLinearChunkScaleIndex()]
                            + down.chunk.tint[down.localPos.ToLinearChunkScaleIndex()]
                            + left.chunk.tint[left.localPos.ToLinearChunkScaleIndex()]
                            ) / 5f;
                    }
                    directorInstance.SetColor(pos, value);
                    colorToPaint = col;
                }
            }
            colorToPaint = col;
            pos = sop;
        }


        void BrushStrokeMatter()
        {
            if (meshToBrush == null)
            {
                if (scaleRadius < LandscapeTool.BlockScale)
                {
                    directorInstance.SetMatter(pos, Matter_Library.GetByName(matterToSet));
                }
                else
                {
                    int r = scaleRadiusInBlocks;
                    for (int z = -r; z <= r; z++)
                    {
                        for (int y = -r; y <= r; y++)
                        {
                            for (int x = -r; x <= r; x++)
                            {
                                var m = new Vector3(x * LandscapeTool.BlockScale, y * LandscapeTool.BlockScale, z * LandscapeTool.BlockScale).magnitude;
                                if (m > scaleRadius) continue;
                                directorInstance.SetMatter(pos + new Int3(x, y, z), Matter_Library.GetByName(matterToSet));
                            }
                        }
                    }
                }
            }
            else
            {
                for (int i = 0; i < voxelPositions.Count; i++)
                {
                    pos = (voxelPositions[i] * 3).ToInt3();
                    directorInstance.SetMatter(voxelPositions[i].ToInt3(), Matter_Library.GetByName(matterToSet));
                }
            }
        }


        void BrushStrokeShape(bool blur = false)
        {
            var val = value;
            var sop = pos;
            if (meshToBrush == null)
            {
                if (scaleRadius < LandscapeTool.BlockScale)
                {
                    if (blur)
                    {
                        var origin = directorInstance.GetBlock(pos);
                        var up = directorInstance.GetBlock(pos + Int3.up);
                        var right = directorInstance.GetBlock(pos + Int3.right);
                        var down = directorInstance.GetBlock(pos + Int3.down);
                        var left = directorInstance.GetBlock(pos + Int3.left);
                        value = (origin.chunk.densityMap[origin.localPos.ToLinearChunkScaleIndex()]
                            + up.chunk.densityMap[up.localPos.ToLinearChunkScaleIndex()]
                            + right.chunk.densityMap[right.localPos.ToLinearChunkScaleIndex()]
                            + down.chunk.densityMap[down.localPos.ToLinearChunkScaleIndex()]
                            + left.chunk.densityMap[left.localPos.ToLinearChunkScaleIndex()]
                            ) / 5f;
                    }
                    directorInstance.SetOrAddValue(pos, value, blur);
                    value = val;
                }
                else
                {
                    int r = scaleRadiusInBlocks;
                    for (int z = -r; z <= r; z++)
                    {
                        for (int y = -r; y <= r; y++)
                        {
                            for (int x = -r; x <= r; x++)
                            {
                                var m = new Vector3(x * LandscapeTool.BlockScale, y * LandscapeTool.BlockScale, z * LandscapeTool.BlockScale).magnitude;
                                if (m > scaleRadius) continue;
                                pos = sop + new Int3(x, y, z);
                                if (blur)
                                {
                                    var origin = directorInstance.GetBlock(pos);
                                    var up = directorInstance.GetBlock(pos + Int3.up);
                                    var right = directorInstance.GetBlock(pos + Int3.right);
                                    var down = directorInstance.GetBlock(pos + Int3.down);
                                    var left = directorInstance.GetBlock(pos + Int3.left);
                                    float orgDen = origin.chunk.densityMap[origin.localPos.ToLinearChunkScaleIndex()];
                                    value = (orgDen
                                                    + up.chunk.densityMap[up.localPos.ToLinearChunkScaleIndex()]
                                        + right.chunk.densityMap[right.localPos.ToLinearChunkScaleIndex()]
                                        + down.chunk.densityMap[down.localPos.ToLinearChunkScaleIndex()]
                                        + left.chunk.densityMap[left.localPos.ToLinearChunkScaleIndex()]
                                        ) / 5f;
                                    directorInstance.SetOrAddValue(pos, value * falloff.Evaluate(1f - m / scaleRadius) + orgDen * (1f - falloff.Evaluate(1f - m / scaleRadius)), true);
                                }
                                else
                                {
                                    directorInstance.SetOrAddValue(pos, value * falloff.Evaluate(1f - m / scaleRadius));
                                }
                                value = val;
                            }
                        }
                    }
                }
            }
            else
            {
                for (int i = 0; i < voxelPositions.Count; i++)
                {
                    pos = sop + (voxelPositions[i] * 3).ToInt3();
                    if (blur)
                    {
                        var origin = directorInstance.GetBlock(pos);
                        var up = directorInstance.GetBlock(pos + Int3.up);
                        var right = directorInstance.GetBlock(pos + Int3.right);
                        var down = directorInstance.GetBlock(pos + Int3.down);
                        var left = directorInstance.GetBlock(pos + Int3.left);
                        value = (origin.chunk.densityMap[origin.localPos.ToLinearChunkScaleIndex()]
                            + up.chunk.densityMap[up.localPos.ToLinearChunkScaleIndex()]
                            + right.chunk.densityMap[right.localPos.ToLinearChunkScaleIndex()]
                            + down.chunk.densityMap[down.localPos.ToLinearChunkScaleIndex()]
                            + left.chunk.densityMap[left.localPos.ToLinearChunkScaleIndex()]
                            ) / 5f;
                    }
                    directorInstance.SetOrAddValue(pos, value, blur);
                    value = val;
                }
            }
            value = val;
            pos = sop;
        }


        void BrushStrokeBlock()
        {
            var sop = pos;
            var tmp = blockToSet;
            if (value == -1) blockToSet = "Air";
            if (meshToBrush == null)
            {
                if (scaleRadius < LandscapeTool.BlockScale)
                {
                    directorInstance.SetBlock(pos, Block_Library.GetCopyByName(blockToSet));
                }
                else
                {
                    int r = (int)(scaleRadius / LandscapeTool.BlockScale);
                    for (int z = -r; z <= r; z++)
                    {
                        for (int y = -r; y <= r; y++)
                        {
                            for (int x = -r; x <= r; x++)
                            {
                                var m = new Vector3(x * LandscapeTool.BlockScale, y * LandscapeTool.BlockScale, z * LandscapeTool.BlockScale).magnitude;
                                if (m > scaleRadius) continue;
                                directorInstance.SetBlock(pos + new Int3(x, y, z), Block_Library.GetCopyByName(blockToSet));
                            }
                        }
                    }
                }
            }
            else
            {
                for (int i = 0; i < voxelPositions.Count; i++)
                {
                    pos = sop + (voxelPositions[i] * 3).ToInt3();
                    directorInstance.SetBlock(voxelPositions[i].ToInt3(), Block_Library.GetCopyByName(blockToSet));
                }
            }
            blockToSet = tmp;
        }


        void BrushStrokeGrass()
        {
            var sop = pos;
            if (meshToBrush == null)
            {
                if (scaleRadius < LandscapeTool.BlockScale)
                {
                    SubGrass(pos);
                }
                else
                {
                    int r = (int)(scaleRadius / LandscapeTool.BlockScale);
                    for (int z = -r; z <= r; z++)
                    {
                        for (int y = -r; y <= r; y++)
                        {
                            for (int x = -r; x <= r; x++)
                            {
                                var m = new Vector3(x * LandscapeTool.BlockScale, y * LandscapeTool.BlockScale, z * LandscapeTool.BlockScale).magnitude;
                                if (m > scaleRadius) continue;
                                SubGrass(sop + new Int3(x, y, z));
                            }
                        }
                    }
                }
            }
            else
            {
                for (int i = 0; i < voxelPositions.Count; i++)
                {
                    pos = sop + (voxelPositions[i] * 3).ToInt3();
                    SubGrass(sop + voxelPositions[i].ToInt3());
                }
            }
        }


        public void SubGrass(Int3 pos)
        {
            var posGlobal = pos;// - directorInstance.globalOffset*LandscapeTool.ChunkScale;
            Int3 chunkOrgPos = FloorToGrid.GridFloor(posGlobal, LandscapeTool.ChunkScale);
            Int3 posLocal = posGlobal - (chunkOrgPos * LandscapeTool.ChunkScale);
            Chunk chunkOrg;
            if (directorInstance.withinLimit(chunkOrgPos))
            {
                chunkOrg = directorInstance.RequestPiece(chunkOrgPos, true);
            }
            else
            {
                chunkOrg = directorInstance.RequestPiece(chunkOrgPos, false);
            }

            directorInstance.pipeline.ForceChunkUpToState(chunkOrg, 1);

            //chunkOrg.blocks[posLocal.ToLinearChunkScaleIndex()] = cube;

            var c = chunkOrg;
            pos = posLocal;
            var x = pos.x;
            var y = pos.y;
            var z = pos.z;
            if (c.decoPositions.Contains(pos)) return;

            var id = pos.ToLinearChunkScaleIndex();
            if (c.densityMap[id] < directorInstance.densityToRealityBorder) return;
            var bio = directorInstance.biomes[c.primaryBiomeID[id]];
            var posAbove = new Int3(x, y + 1, z);
            var idAbove = posAbove.ToLinearChunkScaleIndex();
            Chunk chunkAbove = directorInstance.RequestPiece(c.key + Int3.up, false);
            if (y < LandscapeTool.ChunkScale - 1)
            {
                if (c.densityMap[idAbove] >= directorInstance.densityToRealityBorder) return;
                if (c.blocks[idAbove].meshSection.verts.Length > 1) return;
            }
            else
            {
                if (chunkAbove.densityMap[new Int3(x, 0, z).ToLinearChunkScaleIndex()] >= directorInstance.densityToRealityBorder) return;
            }

            if (UnityEngine.Random.value <= randomA)
            {
                bool underwater = c.key.y * LandscapeTool.ChunkScale * LandscapeTool.BlockScale + (y + 1) * LandscapeTool.BlockScale < directorInstance.oceanHeight;
                if (y < LandscapeTool.ChunkScale - 1)
                {
                    c.decoPositions.Add(posAbove);
                    c.decoBlocks.Add(underwater ? Block_Library.UnderwaterGrass : Block_Library.TallGrass);
                }
                else
                {
                    chunkAbove.decoPositions.Add(new Int3(x, 0, z));
                    chunkAbove.decoBlocks.Add(underwater ? Block_Library.UnderwaterGrass : Block_Library.TallGrass);
                }
            }

#if !Deactivate_Async
            if (!directorInstance.chunksUnderConstruction.Contains(chunkOrg))
#endif
                directorInstance.chunksUnderConstruction.Add(chunkOrg);
            chunkOrg.genState = 1;

            directorInstance.NeighboringBlocksRefreshSetup(posLocal, chunkOrg, 1);
        }


        void BrushStrokeTree()
        {
            var sop = pos;
            var tmp = blockToSet;
            if (value == -1) blockToSet = "Air";
            if (meshToBrush == null)
            {
                if (scaleRadius < LandscapeTool.BlockScale)
                {
                    SubTree(pos);
                }
                else
                {
                    int r = (int)(scaleRadius / LandscapeTool.BlockScale);
                    for (int z = -r; z <= r; z++)
                    {
                        for (int y = -r; y <= r; y++)
                        {
                            for (int x = -r; x <= r; x++)
                            {
                                var m = new Vector3(x * LandscapeTool.BlockScale, y * LandscapeTool.BlockScale, z * LandscapeTool.BlockScale).magnitude;
                                if (m > scaleRadius) continue;
                                SubTree(sop + new Int3(x, y, z));
                            }
                        }
                    }
                }
            }
            else
            {
                for (int i = 0; i < voxelPositions.Count; i++)
                {
                    pos = sop + (voxelPositions[i] * 3).ToInt3();
                    SubTree(sop + voxelPositions[i].ToInt3());
                }
            }
            blockToSet = tmp;
        }


        public void SubTree(Int3 pos)
        {
            var posGlobal = pos;
            Int3 chunkOrgPos = FloorToGrid.GridFloor(posGlobal, LandscapeTool.ChunkScale);
            Int3 posLocal = posGlobal - (chunkOrgPos * LandscapeTool.ChunkScale);
            Chunk chunkOrg;
            if (directorInstance.withinLimit(chunkOrgPos))
            {
                chunkOrg = directorInstance.RequestPiece(chunkOrgPos, true);
            }
            else
            {
                chunkOrg = directorInstance.RequestPiece(chunkOrgPos, false);
            }

            directorInstance.pipeline.ForceChunkUpToState(chunkOrg, 1);


            var c = chunkOrg;
            pos = posLocal;
            var x = pos.x;
            var y = pos.y;
            var z = pos.z;

            foreach (var item in c.trees)
            {
                if (item.chunkSpaceOrigin == pos) return;
            }

            var id = pos.ToLinearChunkScaleIndex();
            if (c.densityMap[id] < directorInstance.densityToRealityBorder) return;
            var bio = directorInstance.biomes[c.primaryBiomeID[id]];
            var posAbove = new Int3(x, y + 1, z);
            var idAbove = posAbove.ToLinearChunkScaleIndex();
            Chunk chunkAbove = directorInstance.RequestPiece(c.key + Int3.up, false);
            if (y < LandscapeTool.ChunkScale - 1)
            {
                if (c.densityMap[idAbove] >= directorInstance.densityToRealityBorder) return;
                if (c.blocks[idAbove].meshSection.verts.Length > 1) return;
            }
            else
            {
                if (chunkAbove.densityMap[new Int3(x, 0, z).ToLinearChunkScaleIndex()] >= directorInstance.densityToRealityBorder) return;
            }
            if (UnityEngine.Random.value <= randomA)
            {
                directorInstance.pipeline.InsertTree(bio, c, chunkAbove, x, y, z);
            }

#if !Deactivate_Async
            if (!directorInstance.chunksUnderConstruction.Contains(chunkOrg))
#endif
                directorInstance.chunksUnderConstruction.Add(chunkOrg);
            chunkOrg.genState = 1;

            directorInstance.NeighboringBlocksRefreshSetup(posLocal, chunkOrg, 1);
        }


        void BrushStrokeScatter()
        {
            var sop = pos;
            var tmp = blockToSet;
            if (value == -1) blockToSet = "Air";
            if (meshToBrush == null)
            {
                if (scaleRadius < LandscapeTool.BlockScale)
                {
                    SubScatter(pos);
                }
                else
                {
                    int r = (int)(scaleRadius / LandscapeTool.BlockScale);
                    for (int z = -r; z <= r; z++)
                    {
                        for (int y = -r; y <= r; y++)
                        {
                            for (int x = -r; x <= r; x++)
                            {
                                var m = new Vector3(x * LandscapeTool.BlockScale, y * LandscapeTool.BlockScale, z * LandscapeTool.BlockScale).magnitude;
                                if (m > scaleRadius) continue;
                                SubScatter(sop + new Int3(x, y, z));
                            }
                        }
                    }
                }
            }
            else
            {
                for (int i = 0; i < voxelPositions.Count; i++)
                {
                    pos = sop + (voxelPositions[i] * 3).ToInt3();
                    directorInstance.SetBlock(voxelPositions[i].ToInt3(), Block_Library.GetCopyByName(blockToSet));
                    SubScatter(sop + voxelPositions[i].ToInt3());
                }
            }
            blockToSet = tmp;
        }


        public void SubScatter(Int3 pos)
        {
            var posGlobal = pos;
            Int3 chunkOrgPos = FloorToGrid.GridFloor(posGlobal, LandscapeTool.ChunkScale);
            Int3 posLocal = posGlobal - (chunkOrgPos * LandscapeTool.ChunkScale);
            Chunk chunkOrg;
            if (directorInstance.withinLimit(chunkOrgPos))
            {
                chunkOrg = directorInstance.RequestPiece(chunkOrgPos, true);
            }
            else
            {
                chunkOrg = directorInstance.RequestPiece(chunkOrgPos, false);
            }

            directorInstance.pipeline.ForceChunkUpToState(chunkOrg, 1);

            var c = chunkOrg;
            pos = posLocal;
            var x = pos.x;
            var y = pos.y;
            var z = pos.z;

            foreach (var item in c.scatteredPositions)
            {
                if (item == pos) return;
            }
            var id = pos.ToLinearChunkScaleIndex();
            if (c.densityMap[id] < directorInstance.densityToRealityBorder) return;
            var bio = directorInstance.biomes[c.primaryBiomeID[id]];
            var posAbove = new Int3(x, y + 1, z);
            var idAbove = posAbove.ToLinearChunkScaleIndex();
            Chunk chunkAbove = directorInstance.RequestPiece(c.key + Int3.up, false);
            if (y < LandscapeTool.ChunkScale - 1)
            {
                if (c.densityMap[idAbove] >= directorInstance.densityToRealityBorder) return;
                if (c.blocks[idAbove].meshSection.verts.Length > 1) return;
            }
            else
            {
                if (chunkAbove.densityMap[new Int3(x, 0, z).ToLinearChunkScaleIndex()] >= directorInstance.densityToRealityBorder) return;
            }

            if (UnityEngine.Random.value <= randomA && bio.scatterDecoratives.Count > 0)
            {
                var toScatter = bio.scatterDecoratives[UnityEngine.Random.Range(0, bio.scatterDecoratives.Count)];
                var gm = MonoBehaviour.Instantiate(toScatter, c.transform);
                gm.SetActive(false);
                pos += Int3.up;
                var worldPos = (c.key + directorInstance.globalOffset).AsVector3 * LandscapeTool.ChunkScale * LandscapeTool.BlockScale + pos.AsVector3 * LandscapeTool.BlockScale + new Vector3(0.5f, 0, 0.5f) * LandscapeTool.BlockScale;
                gm.transform.position = worldPos;
                gm.transform.Rotate(Vector3.up, 360f * UnityEngine.Random.value);
                c.scatteredObjects.Add(gm);
                c.scatteredPositions.Add(pos);
                if (directorInstance.barnacling)
                {
                    Bounds bounds = toScatter.GetComponent<MeshFilter>().sharedMesh.bounds;
                    for (int i = 2; i < 2 + UnityEngine.Random.value * 4; i++)
                    {
                        var offset = new Vector3(UnityEngine.Random.value - 0.5f, 0, UnityEngine.Random.value - 0.5f).normalized;
                        var subGm = MonoBehaviour.Instantiate(toScatter, gm.transform);
                        subGm.SetActive(false);
                        subGm.transform.position = gm.transform.position + offset * Mathf.Abs(bounds.max.z - bounds.min.z) * gm.transform.lossyScale.z * .5f;//+Vector3.down *Mathf.Abs(bounds.max.y-bounds.min.y)*gm.transform.lossyScale.z * 1/3;
                        subGm.transform.Rotate(Vector3.up, 360f * UnityEngine.Random.value);
                        subGm.transform.localScale = Vector3.one * .5f;
                    }
                }
            }


#if !Deactivate_Async
            if (!directorInstance.chunksUnderConstruction.Contains(chunkOrg))
#endif
                directorInstance.chunksUnderConstruction.Add(chunkOrg);
            chunkOrg.genState = 1;

            directorInstance.NeighboringBlocksRefreshSetup(posLocal, chunkOrg, 1);
        }


    }
}