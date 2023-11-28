/*Antonio Wiege*/
using System;
#if !Deactivate_Async
//Compute Buffers, Transforms, Lack of concurrent Hashset in .NET, etc. There are many reasons to run on the main thread, but some other things may gain performance, if offloaded to other cores.
using System.Threading.Tasks;
using System.Collections.Concurrent;
#endif
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Serialization;
using Unity.VisualScripting;

namespace ProceduralBlockWorldGeneratorToolPrototyping
{
    /* Table of contents
            variables
                Predefined by User
                Must be preassigned by user
                System & Debug Data
            methods
                Awake
                Start
                Update
                OceanAndCloudsHeight
                Mouse
                RequestPiece
                UpdateChunkSurroundings
		        TryGetPiece
                GetBlock
                SetBlock
                NeighboringBlockRefreshSetup
                SetOrAddValue
                SetMatter
                SetColor
                OnDrawGizmos
     */
    public class LandscapeTool : MonoBehaviour
    {
        //<size=138%><color=#00FFFF>[InspectorName("16 bits")] [Range(1, 6)]
        #region variables
        //Global Constants to define ahead of time
        [Tooltip("Toybrick appearance instead of cube, must be constant during play")]
        public const bool toybrickMode = true;
        [Tooltip("Scale in Meters per Dimension")]
        public const float BlockScale = 1 / 3f;
        [Tooltip("Scale in Blocks per Dimension, must be multiple of four")]
        public const int ChunkScale = 24;
        //Inspector Start
        [Header("<size=162%><color=#00AA86>Procedural Landscapes Tool Prototype")]
        [Header("<size=132%><color=#99AA00>Read the README, the rest is try and error.\n In some cases of new Unity versions\n you may not enter playmode while\n having this inspector open => crash.")]
        [Header("Hover over variables for additional information.\nConstants must be set in script.\nMultiple instances possible in parallel.")]
        [Tooltip("Toggle to skip pipeline execution")]
        public bool Pause_pipeline_loop = false;
        [Tooltip("The custom mesh can be rotated X (I,K) Y (J,L) Z (U,O) Axis(Key+,Key-)")]
        [FormerlySerializedAs("brushAndSculpt")]
        public BrushAndSculpt Sculpting_and_Brush_setup;//reorganize in main script for variables
        [Tooltip("Noise of temperature map. Used to choose from multiple biomes")]
        public NoiseInstanceSetup BiomeValues_Temperature = new NoiseInstanceSetup()
        {
            resultApplicationID = 0,
            scale = Vector4.one,
            octaveStartSize = 4,
            octaveStepPow = 2,
            octaveCount = 3,
            noiseDimensionCount = 4,
            multiplyWith = 1,
            clampEndResultMax = 1,
            seed = -2
        };
        [Tooltip("Noise of variety map. Used to choose from multiple biomes")]
        public NoiseInstanceSetup BiomeValues_Variety = new NoiseInstanceSetup()
        {
            resultApplicationID = 0,
            scale = Vector4.one,
            octaveStartSize = 4,
            octaveStepPow = 2,
            octaveCount = 3,
            noiseDimensionCount = 4,
            multiplyWith = 1,
            clampEndResultMax = 1,
            seed = -1
        };
        [Tooltip("Noise of moisture map. Used to choose from multiple biomes")]
        public NoiseInstanceSetup BiomeValues_Moisture = new NoiseInstanceSetup()
        {
            resultApplicationID = 0,
            scale = Vector4.one,
            octaveStartSize = 4,
            octaveStepPow = 2,
            octaveCount = 3,
            noiseDimensionCount = 4,
            multiplyWith = 1,
            clampEndResultMax = 1,
            seed = 0
        };
        [Tooltip("All biomes that are part of this world")]
        public List<Biome> biomes = new List<Biome>() {
        new Biome(){ point_in_Biome_Value_Space=Vector3.one*0.2f, noises = new List<NoiseInstanceSetup>(){
         new NoiseInstanceSetup(){
         seed=1,    resultApplicationID=0,weight=1,cellular = false, cellularMethodID = 2, position = Vector4.zero, interpolationMethodID = 0, noiseDimensionCount = 4, scale = Vector4.one * 1f,rotation= new Vector4(0.25f,0.1f,0.9f,0.68f), octaveStartSize = 2, octaveCount = 4, octaveStepPow=2, octaveInfluenceBias = 1.585f, planarize = true, planarizeAtY = 0, planarizeIntensity = 0.05f, choppy = true, turbulent = true, invert = true, dotsPerCell = 6, multiplyWith = 1, addPostMultiply = 0, clampEndResultMin = 0, clampEndResultMax = 1, useCustomPositioning = false, spaceDistortion = null
        }
        } },
        new Biome(){ point_in_Biome_Value_Space = Vector3.one*.5f, noises = new List<NoiseInstanceSetup>(){
        new NoiseInstanceSetup(){
      seed=2,  resultApplicationID = 0,weight=1,cellular=false,cellularMethodID=2,position=Vector3.zero,interpolationMethodID=0,noiseDimensionCount=4,scale=Vector4.one,rotation=new Vector4(1,0,0,0.25f),octaveStartSize=2,octaveCount=4,octaveInfluenceBias=1.585f,planarize=true,planarizeAtY=0,planarizeIntensity=0.05f,choppy=false,turbulent=false,invert=false,dotsPerCell=6,multiplyWith=1,addPostMultiply=0,clampEndResultMin=0,clampEndResultMax=1,useCustomPositioning=false
        },
        new NoiseInstanceSetup(){
     seed=3,       resultApplicationID=1,weight=1,cellular=false,cellularMethodID=2,position=Vector4.zero,interpolationMethodID=0,noiseDimensionCount=4,scale =Vector4.one,rotation=new Vector4(1,0,0,0.25f),octaveStartSize=2,octaveCount=4,octaveInfluenceBias=1.585f,planarize=true,planarizeAtY=0,planarizeIntensity=0.05f,choppy=false,turbulent=false,invert=false,dotsPerCell=6,multiplyWith=1,addPostMultiply=0,clampEndResultMin=0,clampEndResultMax=1,useCustomPositioning=false
        },
        new NoiseInstanceSetup(){
       seed=4,     resultApplicationID = 2, weight=2,cellular=false,cellularMethodID=2,position=Vector4.zero,interpolationMethodID=0,noiseDimensionCount=2, scale=Vector4.one, rotation=new Vector4(1,0,0,0.25f),octaveStartSize=2,octaveCount=5,octaveInfluenceBias=1.585f,planarize=true,planarizeAtY=0,planarizeIntensity=-0.01f,choppy=false,turbulent=true,invert=true,dotsPerCell=6,multiplyWith=1,addPostMultiply=0,clampEndResultMin=0,clampEndResultMax=1,useCustomPositioning=false
        },
        new NoiseInstanceSetup(){
       seed=5,     resultApplicationID=2,weight=3,cellular=true,cellularMethodID=4,position=Vector4.zero,noiseDimensionCount=4,scale=Vector4.one,rotation=new Vector4(1,0,0,0.25f),octaveStartSize=6,octaveCount=1,octaveStepPow=2,octaveInfluenceBias=1,planarize=false,invert=true,multiplyWith=1,addPostMultiply=5,clampEndResultMin=0,clampEndResultMax=1
        }
        } },
        new Biome(){ point_in_Biome_Value_Space = Vector3.one*0.8f, noises = new List<NoiseInstanceSetup>(){
         new NoiseInstanceSetup(){
    seed=6,         resultApplicationID=0,
              cellular = true,
                        cellularMethodID = 2,
                        position = Vector4.zero,
                        interpolationMethodID = 0,
                        noiseDimensionCount = 4,
                        scale = Vector4.one * 1f,
                        rotation = new Vector4(0.9f, 0.25f, 0.1f, 0.23f),
                        octaveStartSize = 2,
                        octaveCount = 4,
                        octaveStepPow = 2,
                        octaveInfluenceBias = 1.585f,
                        planarize = true,
                        planarizeAtY = 0,
                        planarizeIntensity = 0.05f,
                        choppy = true,
                        turbulent = true,
                        invert = true,
                        dotsPerCell = 6,
                        multiplyWith = 1,
                        addPostMultiply = 0,
                        clampEndResultMin = 0,
                        clampEndResultMax = 1,
                        useCustomPositioning = false,
                        spaceDistortion = null
        }
        } }
        };


        [Header("Put all EffectorObjects in here (Effector is a component for external objects with colliders, like a preplay static brush/sculpt)")]
        public List<EffectorObject> effectorObjects;
        [Tooltip("How many loops to attempt to force into a second")]
        public int PipelineStepsPerSecond = 20; // in Update
        [Tooltip("How many chunks to load around " +
            "mouse hitMouse else camera")]
        public int chunksToLoadAround = 50;
        [Tooltip("no active or editable map contents beyond")]
        public int mapLimitsPlusMinusInChunks = 130;
        public int PipelineStepsPerFrameCap = 2;// in Update
        [Tooltip("Offset everything, so that the camera remains " +
            "within this many chunks distance from the origin")]
        public int offsetAfterChunkCount = 5;

        [Tooltip("Planar fully supported, all diagonals not.")]
        public DiagonalToggle diagonalMode;

        [Tooltip("Toggle trees, grass & stones")]
        public bool generateDecoratives = true;
        [Tooltip("Generate smaller stones around bigger ones"), FormerlySerializedAs("barnacling")]
        public bool barnacling = false;
        [Tooltip("Affects mesh brush and effector object behaviours. Polygon limit 256")]
        public bool forceConvex;

        [Tooltip("Change ocean plane transform")]
        public float oceanHeight = -5f;
        [Tooltip("Change cloud plane transform")]
        public float cloudHeight = 20f;

        [FormerlySerializedAs("generateImmediatelyInBounds_InsteadOfDynamicAroundCamera")]
        public bool usingPreBoundGen = false;
        [Tooltip("Chunks to prefill at the beginning")]
        public Bounds startBoundsFill;
        [FormerlySerializedAs("Auto adjust map limit to it")]
        public bool clampToStartingBounds = false;

        /// <summary>default false -> noise generation via compute on GPU </summary>
        [Tooltip("Switch from GPU to CPU noise")]
        public bool CPU_Noise = false;

        [Tooltip("Values >= this will be turned to visible blocks the rest will remain empty")]
        public float densityToRealityBorder = 0.5f;

        [Tooltip("Blend based on distance in biome value space (Temperature, Variety, Moisture")]
        public float biomeBlendDistance = 1f;

        #region User Setup - Assign Before Use
        [Header("<size=132%><color=#990086>End of user properties,\nbellow follows setup and debug")]
        [Header("Properties bellow must be assigned for this component to work. Use prefab for reference.")]
        public Material defaultMaterial;
        public Mesh Sphere;
        public Mesh Cone;
        public ComputeShader ValueNoiseNearestNeigbor;
        public ComputeShader ValueNoiseLinear;
        public ComputeShader ValueNoiseCosine;
        public ComputeShader ValueNoiseCustomCosine;
        public ComputeShader ValueNoiseCubic;
        public ComputeShader ValueNoiseCustomHermite;
        public ComputeShader CellNoiseOneDPC;
        public ComputeShader CellNoiseTwoDPC;
        public ComputeShader CellNoiseThreeDPC;
        public ComputeShader CellNoiseFourDPC;
        public ComputeShader CellNoiseFiveDPC;
        public ComputeShader CellNoiseSixDPC;
        public float gradientBottom, gradientTop;
        public Gradient mapColorGradient;

        [TextArea(1, 32)]
        //add note the UI is overloaded for some reason
        /*public//*/
        //    string developerNote = "\tThe global script define symbols for this tool are:\r\nDeactivate_Gizmos\r\nDeactivate_Debugging\r\nDeactivate_Profiling\r\nDeactivate_MultiFrameExecution\r\nDeactivate_CustomGameLoop\r\nDeactivate_ReducedMemoryUse\r\nDeactivate_Async\r\n\tall of which are undefined by default and on import.\r\n/*\r\ngo to Project Settings -> Player -> Scripting Define Symbols\r\nIf symbols do not immediately update on apply:\r\n\tadd another, leave empty and apply again.\r\n\tif still nothing restart the editor\r\nif framerate stutters or seems very low check the profiler, if the editor takes much longer than the scripts restart unity.\r\n*/\r\n\r\nPS: Editor Inspector Assigned is usually a runtime thing for play mode, to have it work in the editor too it's been copied onto this.\r\n\r\nFresh 2022.2.11 Built in Renderpipeline, add Memory Profiler 1.0.0 & Post Processing.\r\nAdd Project Settings->Input Manager  Input Axis \"Perpendicular\" in analogy to \"Horizontal\" keys in order {q, e, left shift, left ctrl}\r\nGraphics change from Forward to Deffered;\t\t\t\r\nPlayer:\tColor Space* -> Linear \tUse incremental GC -> Tick(true)";

        #endregion
        #region System & Debug Data
        public Pipeline pipeline;
        public NoiseHandler noiseHandler;
        public GameObject ocean, clouds, helper;
        [Tooltip("camera.main automatically assigned at awake" +
            "")]
        new public Camera camera;
        public RaycastHit raycastHit;
        public bool hitMouse { get; private set; }//Worldspace ignoring global offset
        [HideInInspector]
        public Int3 pointOfChunkOfRelevance;
        public Int3 currentPosIn { get; private set; }//Block point_in_Biome_Value_Space to deconstruct (Does not include offset)
        public Int3 currentPosOut { get; private set; }//Block point_in_Biome_Value_Space to construct (Does not include offset)
#if Deactivate_Async
        public Dictionary<Int3, Chunk> chunks = new();
#else
        public ConcurrentDictionary<Int3, Chunk> chunks = new();
#endif
        //public Dictionary<Int3, Block> blocks = new();//4*3int3+8reference=20B;*ChunkScale^3 worth the ease of use trade against memory, in addition to local position storage <- when ever setting a block here or in chunk need to mind the one too, therefore removed.
        [Tooltip("Offset in Chunks, that has been added to the camera, to keep it in bounds")]
        public Int3 globalOffset;
        [Tooltip("Chunks that are not in a completely finished state")]
#if Deactivate_Async
        public HashSet<Chunk> chunksUnderConstruction = new();
#else
        public ConcurrentBag<Chunk> chunksUnderConstruction = new();
#endif
        public HashSet<Chunk> currentlyVisibleChunks = new();
        // [HideInInspector]
        //public List<Chunk> inspectorDebugHashsetToList = new List<Chunk>();
        /*
#if !Deactivate_CustomGameLoop
        public HashSet<Action> UpdateActionBatch = new HashSet<Action>();
#endif
#if !Deactivate_Async
        public HashSet<Task> UpdateTaskBatch = new HashSet<Task>();
#endif//*/
        public bool withinLimit(Int3 i) => i.x > -mapLimitsPlusMinusInChunks && i.x < mapLimitsPlusMinusInChunks
                                                    && i.y > -mapLimitsPlusMinusInChunks && i.y < mapLimitsPlusMinusInChunks
                                                    && i.z > -mapLimitsPlusMinusInChunks && i.z < mapLimitsPlusMinusInChunks;
        public Int3 intoLimit(Int3 i) => new Int3((i.x > mapLimitsPlusMinusInChunks) ? mapLimitsPlusMinusInChunks : (i.x < -mapLimitsPlusMinusInChunks) ? -mapLimitsPlusMinusInChunks : i.x,
            (i.y > mapLimitsPlusMinusInChunks) ? mapLimitsPlusMinusInChunks : (i.y < -mapLimitsPlusMinusInChunks) ? -mapLimitsPlusMinusInChunks : i.y,
            (i.z > mapLimitsPlusMinusInChunks) ? mapLimitsPlusMinusInChunks : (i.z < -mapLimitsPlusMinusInChunks) ? -mapLimitsPlusMinusInChunks : i.z);
        [HideInInspector]//inspector can take a loot of editor overhead with many elements, even when collapsed
        public Int3[] sphericalOffsetLookUpTable = new Int3[0];
        [Tooltip("Seconds since game begin")]
        public float timeSincePlay;
        [Tooltip("Total generated chunk instance count")]
        public int chunksLoadedSincePlay;
        [Tooltip("Time the awake & start method needed (including pregenerating bounds)")]
        public float startUpTime;
        float timer;
        #endregion
        #endregion

        #region methods

        private void Awake()
        {
            startUpTime = 0;
            pipeline = new(this);
            noiseHandler = new(this);
            Sculpting_and_Brush_setup = new(this);

            CalculateSphericalOffsets();

            helper = new GameObject("Helper");
            helper.transform.parent = gameObject.transform;
            helper.AddComponent<MeshCollider>().enabled = false;
            if (forceConvex) helper.GetComponent<MeshCollider>().convex = true;

            camera = Camera.main;
            var mcc = camera.GetComponent<MainCamControlls>();
            if (mcc == null)
            {
                mcc = camera.AddComponent<MainCamControlls>();
            }

            if (!mcc.landscapeTools.Contains(this)) mcc.landscapeTools.Add(this);
            foreach (var item in effectorObjects)
            {
                item.directorInstance = this;
            }
#if !Deactivate_Async
            //initialize all rotated variants before actual need (pregenerating for thread safety)
            _ = BlockShape_Library.allGeneratedVariants;
            _ = Chunk.LocalIDtoPos;
#endif
        }

        private void Start()
        {
            foreach (var item in effectorObjects)
            {
                item.TriggerEffect();
            }
            if (usingPreBoundGen)
            {
                for (float z = -startBoundsFill.extents.z; z < startBoundsFill.extents.z; z++)
                {
                    for (float y = -startBoundsFill.extents.y; y < startBoundsFill.extents.y; y++)
                    {
                        for (float x = -startBoundsFill.extents.x; x < startBoundsFill.extents.x; x++)
                        {
                            pipeline.ForceChunkUpToState(RequestPiece(new Int3(startBoundsFill.center.x + x, startBoundsFill.center.y + y, startBoundsFill.center.z + z), true), 5);
                        }
                    }
                }
            }
            startUpTime = Time.realtimeSinceStartup;
        }


        //All correlated updates are called from here
        private void Update()
        {
            timeSincePlay = Time.realtimeSinceStartup;
            OceanAndCloudsHeight();
            camera.GetComponent<MainCamControlls>().CustomUpdate();
            Mouse();

            if (Pause_pipeline_loop) return;

            Sculpting_and_Brush_setup.Update();

            timer += Time.deltaTime;
            int safetyCapCounter = 0;
            if (PipelineStepsPerSecond > 0)
            {
                while (timer > 1f / PipelineStepsPerSecond)
                {
                    timer -= 1f / PipelineStepsPerSecond;
                    safetyCapCounter++;
                    pipeline.Update();
                    if (safetyCapCounter >= PipelineStepsPerFrameCap)
                    {
                        break;
                    }
                }
            }
            else
            {
                pipeline.Update();
            }
        }

        void OceanAndCloudsHeight()
        {
            if (ocean != null) ocean.transform.position = Vector3.up * oceanHeight + globalOffset.AsVector3 * ChunkScale * BlockScale;
            if (clouds != null) clouds.transform.position = Vector3.up * cloudHeight + globalOffset.AsVector3 * ChunkScale * BlockScale;
        }

        void Mouse()
        {
            currentPosIn = Int3.error;
            currentPosOut = Int3.error;

            hitMouse = Physics.Raycast(camera.ScreenPointToRay(Input.mousePosition), out raycastHit);

            if (hitMouse)
            {
                currentPosIn = FloorToGrid.GridFloorHit(raycastHit, BlockScale).ToInt3() - globalOffset * ChunkScale;//The block pos you'd expect to deconstruct
                currentPosOut = currentPosIn + FloorToGrid.ClosestCubeSideNormal(raycastHit.normal).ToInt3();//The block pos you'd expect to build / fill
            }

            //chunks get refreshed around this point_in_Biome_Value_Space. Mouse world hit with fallback to camera pos
            pointOfChunkOfRelevance = FloorToGrid.GridFloor((hitMouse ? currentPosIn.AsVector3 * BlockScale : camera.transform.position - globalOffset.AsVector3 * ChunkScale * BlockScale), ChunkScale * BlockScale);
        }

        /// <summary>Creates the chunk if not already there to return;</summary>
        /// <param nameID="chunkPos">in chunk coordinates, raw without offset</param>
        public Chunk RequestPiece(Int3 chunkPos, bool gameActive)
        {
            if (gameActive && !withinLimit(chunkPos))
            {
                chunkPos = intoLimit(chunkPos);
            }
            else if (chunkPos.AsVector3.magnitude > 100000f)
            {
               chunkPos = intoLimit(chunkPos);
            }

            Chunk chunk;
            chunks.TryGetValue(chunkPos, out chunk);

            if (chunk == null)
            {
                chunk = pipeline.CreateAndInitializeChunk(chunkPos);
                UpdateChunkSurroundings(chunk, chunkPos);
            }

            chunk.gameActive |= gameActive;

            if (clampToStartingBounds)
            {
                if (chunk.key.x < -startBoundsFill.extents.x + startBoundsFill.center.x)
                {
                    chunk.gameActive = false;
                }
                else
                if (chunk.key.x >= startBoundsFill.extents.x + startBoundsFill.center.x)
                {
                    chunk.gameActive = false;
                }
                if (chunk.key.y < -startBoundsFill.extents.y + startBoundsFill.center.y)
                {
                    chunk.gameActive = false;
                }
                else
                if (chunk.key.y >= startBoundsFill.extents.y + startBoundsFill.center.y)
                {
                    chunk.gameActive = false;
                }
                if (chunk.key.z < -startBoundsFill.extents.z + startBoundsFill.center.z)
                {
                    chunk.gameActive = false;
                }
                else
                if (chunk.key.z >= startBoundsFill.extents.z + startBoundsFill.center.z)
                {
                    chunk.gameActive = false;
                }
            }

            return chunk;
        }


        /// <summary>check for neighbors and if available set own and their surroundings. Both to avoid time slicing conflicts.</summary>
        public void UpdateChunkSurroundings(Chunk chunk, Int3 chunkPos)
        {
            for (int z = -1; z < 2; z++)
            {
                for (int y = -1; y < 2; y++)
                {
                    for (int x = -1; x < 2; x++)
                    {
                        Int3 offset = new Int3(x, y, z);

                        var neighborChunk = TryGetPiece(chunkPos + offset);
                        if (neighborChunk != null)
                        {
                            neighborChunk.surroundChunks[1 - x, 1 - y, 1 - z] = chunk;
                            chunk.surroundChunks[1 + x, 1 + y, 1 + z] = neighborChunk;
                        }
                    }
                }
            }
        }


        /// <param nameID="chunkPos">key in directors chunk dictionairy</param>
        /// <returns>null if chunk not found</returns>
        public Chunk TryGetPiece(Int3 chunkPos)
        {
            Chunk chunk = null;
            chunks.TryGetValue(chunkPos, out chunk);
            return chunk;
        }



        public getBlockResult GetBlock(Int3 posGlobal)
        {
            Int3 chunkOrgPos = FloorToGrid.GridFloor(posGlobal, ChunkScale);
            Int3 posLocal = posGlobal - (chunkOrgPos * ChunkScale);
            Chunk chunkOrg;
            if (withinLimit(chunkOrgPos))
            {
                chunkOrg = RequestPiece(chunkOrgPos, true);
            }
            else
            {
                chunkOrg = RequestPiece(chunkOrgPos, false);
            }
            return new getBlockResult() { chunk = chunkOrg, block = chunkOrg.blocks[posLocal.ToLinearChunkScaleIndex()], localPos = posLocal };
        }

        /// <summary>
        /// replaces block and sets all neighbouring blocks to update
        /// <para> specifically does not set density||etc by default any edits on block are undone when you go back to before it's generation in pipeline. You are advised to use density and tint instead.</para>
        /// </summary>
        public void SetBlock(Int3 posGlobal, Block cube, bool setDensity = false)//take in cube pos without globalOffset, have it applied with.
        {
            Int3 chunkOrgPos = FloorToGrid.GridFloor(posGlobal, ChunkScale);
            Int3 posLocal = posGlobal - (chunkOrgPos * ChunkScale);
            Chunk chunkOrg;
            if (withinLimit(chunkOrgPos))
            {
                chunkOrg = RequestPiece(chunkOrgPos, true);
            }
            else
            {
                chunkOrg = RequestPiece(chunkOrgPos, false);
            }

            pipeline.ForceChunkUpToState(chunkOrg, 2);
            chunkOrg.blocks[posLocal.ToLinearChunkScaleIndex()] = cube;
            if (setDensity)
            {
                chunkOrg.densityMap[posLocal.ToLinearChunkScaleIndex()] = (cube.blockShape == BlockShape_Library.empty) ? 0 : 1;
            }
            chunkOrg.blocksToUpdate.Add(posLocal);
#if !Deactivate_Async
            if(!chunksUnderConstruction.Contains(chunkOrg))
#endif
            chunksUnderConstruction.Add(chunkOrg);
            chunkOrg.genState = 2;


            NeighboringBlocksRefreshSetup(posLocal, chunkOrg, 2);
        }

        public void NeighboringBlocksRefreshSetup(Int3 posLocal, Chunk chunkOrg, int genState = 1)//shouldn't be public
        {
            Int3 neighbourPosLocal = posLocal + Int3.top;
            if (neighbourPosLocal.y > ChunkScale - 1)
            {
                if (chunkOrg.surroundChunks[1, 2, 1] != null)
                {
                    chunkOrg.surroundChunks[1, 2, 1].blocksToUpdate.Add(new Int3(neighbourPosLocal.x, 0, neighbourPosLocal.z));
#if !Deactivate_Async
                    if (!chunksUnderConstruction.Contains(chunkOrg))
#endif
                    chunksUnderConstruction.Add(chunkOrg.surroundChunks[1, 2, 1]);
                    pipeline.ForceChunkUpToState(chunkOrg.surroundChunks[1, 2, 1], genState);
                    chunkOrg.surroundChunks[1, 2, 1].genState = genState;
                }
            }
            else
            {
                chunkOrg.blocksToUpdate.Add(neighbourPosLocal);
            }


            neighbourPosLocal = posLocal + Int3.front;
            if (neighbourPosLocal.z > ChunkScale - 1)
            {
                if (chunkOrg.surroundChunks[1, 1, 2] != null)
                {
                    chunkOrg.surroundChunks[1, 1, 2].blocksToUpdate.Add(new Int3(neighbourPosLocal.x, neighbourPosLocal.y, 0));
#if !Deactivate_Async
                    if (!chunksUnderConstruction.Contains(chunkOrg))
#endif
                    chunksUnderConstruction.Add(chunkOrg.surroundChunks[1, 1, 2]);
                    pipeline.ForceChunkUpToState(chunkOrg.surroundChunks[1, 1, 2], genState);
                    chunkOrg.surroundChunks[1, 1, 2].genState = genState;
                }
            }
            else
            {
                chunkOrg.blocksToUpdate.Add(neighbourPosLocal);
            }

            neighbourPosLocal = posLocal + Int3.right;
            if (neighbourPosLocal.x > ChunkScale - 1)
            {
                if (chunkOrg.surroundChunks[2, 1, 1] != null)
                {
                    chunkOrg.surroundChunks[2, 1, 1].blocksToUpdate.Add(new Int3(0, neighbourPosLocal.y, neighbourPosLocal.z));
#if !Deactivate_Async
                    if (!chunksUnderConstruction.Contains(chunkOrg))
#endif
                    chunksUnderConstruction.Add(chunkOrg.surroundChunks[2, 1, 1]);
                    pipeline.ForceChunkUpToState(chunkOrg.surroundChunks[2, 1, 1], genState);
                    chunkOrg.surroundChunks[2, 1, 1].genState = genState;
                }
            }
            else
            {
                chunkOrg.blocksToUpdate.Add(neighbourPosLocal);
            }

            neighbourPosLocal = posLocal + Int3.back;
            if (neighbourPosLocal.z < 0)
            {
                if (chunkOrg.surroundChunks[1, 1, 0] != null)
                {
                    chunkOrg.surroundChunks[1, 1, 0].blocksToUpdate.Add(new Int3(neighbourPosLocal.x, neighbourPosLocal.y, ChunkScale - 1));
#if !Deactivate_Async
                    if (!chunksUnderConstruction.Contains(chunkOrg))
#endif
                    chunksUnderConstruction.Add(chunkOrg.surroundChunks[1, 1, 0]);
                    pipeline.ForceChunkUpToState(chunkOrg.surroundChunks[1, 1, 0], genState);
                    chunkOrg.surroundChunks[1, 1, 0].genState = genState;
                }
            }
            else
            {
                chunkOrg.blocksToUpdate.Add(neighbourPosLocal);
            }

            neighbourPosLocal = posLocal + Int3.left;
            if (neighbourPosLocal.x < 0)
            {
                if (chunkOrg.surroundChunks[0, 1, 1] != null)
                {
                    chunkOrg.surroundChunks[0, 1, 1].blocksToUpdate.Add(new Int3(ChunkScale - 1, neighbourPosLocal.y, neighbourPosLocal.z));
#if !Deactivate_Async
                    if (!chunksUnderConstruction.Contains(chunkOrg))
#endif
                    chunksUnderConstruction.Add(chunkOrg.surroundChunks[0, 1, 1]);
                    pipeline.ForceChunkUpToState(chunkOrg.surroundChunks[0, 1, 1], genState);
                    chunkOrg.surroundChunks[0, 1, 1].genState = genState;
                }
            }
            else
            {
                chunkOrg.blocksToUpdate.Add(neighbourPosLocal);
            }

            neighbourPosLocal = posLocal + Int3.bottom;
            if (neighbourPosLocal.y < 0)
            {
                if (chunkOrg.surroundChunks[1, 0, 1] != null)
                {
                    chunkOrg.surroundChunks[1, 0, 1].blocksToUpdate.Add(new Int3(neighbourPosLocal.x, ChunkScale - 1, neighbourPosLocal.z));
                    if (!chunksUnderConstruction.Contains(chunkOrg))
                        chunksUnderConstruction.Add(chunkOrg.surroundChunks[1, 0, 1]);
                    pipeline.ForceChunkUpToState(chunkOrg.surroundChunks[1, 0, 1], genState);
                    chunkOrg.surroundChunks[1, 0, 1].genState = genState;
                }
            }
            else
            {
                chunkOrg.blocksToUpdate.Add(neighbourPosLocal);
            }
        }

        public void SetOrAddValue(Int3 posGlobal, float value, bool SetOrAdd_TrueOrFalse = false, bool localPos = false)//take in cube pos without globalOffset, have it applied with.
        {
            //if (!localPos) posGlobal -= globalOffset *ChunkScale;
            Int3 chunkOrgPos = FloorToGrid.GridFloor(posGlobal, ChunkScale);
            Int3 posLocal = posGlobal - (chunkOrgPos * ChunkScale);
            if (!posLocal.Positive() || posLocal.x > 23 || posLocal.y > 23 || posLocal.z > 23)
            {
                Debug.Log(posGlobal + ";" + chunkOrgPos + ";" + posLocal);
            }
            Chunk chunkOrg;
            if (withinLimit(chunkOrgPos))
            {
                chunkOrg = RequestPiece(chunkOrgPos, true);
            }
            else
            {
                chunkOrg = RequestPiece(chunkOrgPos, false);
            }

            pipeline.ForceChunkUpToState(chunkOrg, 1);

            if (SetOrAdd_TrueOrFalse)
            {
                chunkOrg.densityMap[posLocal.ToLinearChunkScaleIndex()] = value;
            }
            else
            {
                chunkOrg.densityMap[posLocal.ToLinearChunkScaleIndex()] += value;
            }
            chunkOrg.genState = 1;
#if !Deactivate_Async
            if (!chunksUnderConstruction.Contains(chunkOrg))
#endif
            chunksUnderConstruction.Add(chunkOrg);
            NeighboringBlocksRefreshSetup(posLocal, chunkOrg, 1);
        }

        public void SetMatter(Int3 posGlobal, Matter matter, bool localPos = false)
        {
            //if (!localPos) posGlobal -= globalOffset*ChunkScale;
            Int3 chunkOrgPos = FloorToGrid.GridFloor(posGlobal, ChunkScale);
            Int3 posLocal = posGlobal - (chunkOrgPos * ChunkScale);
            Chunk chunkOrg;
            if (withinLimit(chunkOrgPos))
            {
                chunkOrg = RequestPiece(chunkOrgPos, true);
            }
            else
            {
                chunkOrg = RequestPiece(chunkOrgPos, false);
            }

            pipeline.ForceChunkUpToState(chunkOrg, 2);
            chunkOrg.blocks[posLocal.ToLinearChunkScaleIndex()].matter = matter;//matter changes mesh uv 
            chunkOrg.blocksToUpdate.Add(posLocal);// 
#if !Deactivate_Async
            if (!chunksUnderConstruction.Contains(chunkOrg))
#endif
            chunksUnderConstruction.Add(chunkOrg);
            chunkOrg.genState = 2;

        }

        //!!! Still need to handle the bounds properly on all the cases mate
        public void SetColor(Int3 posGlobal, float value, bool localPos = false)
        {
            //if (!localPos) posGlobal -= globalOffset * ChunkScale;
            Int3 chunkOrgPos = FloorToGrid.GridFloor(posGlobal, ChunkScale);
            Int3 posLocal = posGlobal - (chunkOrgPos * ChunkScale);
            Chunk chunkOrg;
            var colorToPaint = Sculpting_and_Brush_setup.colorToPaint;
            if (withinLimit(chunkOrgPos))
            {
                chunkOrg = RequestPiece(chunkOrgPos, true);
            }
            else
            {
                chunkOrg = RequestPiece(chunkOrgPos, false);
            }

            pipeline.ForceChunkUpToState(chunkOrg, 3);

            if (value >= 0)
            {
                chunkOrg.tint[posLocal.ToLinearChunkScaleIndex()] = value * colorToPaint + (1 - value) * chunkOrg.tint[posLocal.ToLinearChunkScaleIndex()];
            }
            else
            {
                chunkOrg.tint[posLocal.ToLinearChunkScaleIndex()] = Mathf.Abs(value) * new Color(1f - colorToPaint.r, 1f - colorToPaint.g, 1f - colorToPaint.b, colorToPaint.a) + (1 - Mathf.Abs(value)) * chunkOrg.tint[posLocal.ToLinearChunkScaleIndex()];
            }
#if !Deactivate_Async
            if (!chunksUnderConstruction.Contains(chunkOrg))
#endif
            chunksUnderConstruction.Add(chunkOrg);
            chunkOrg.genState = 3;

        }

        /// <summary>
        /// GIZMOS WITHOUT GLOBAL OFFSET
        /// </summary>
#if !Deactivate_Gizmos
        private void OnDrawGizmos()
        {
            if (hitMouse)
            {
                Gizmos.DrawWireCube(globalOffset.AsVector3*ChunkScale*BlockScale+ currentPosIn.ToVector3() * BlockScale + Vector3.one * BlockScale * .5f , Vector3.one * BlockScale);
                Gizmos.DrawWireCube(globalOffset.AsVector3 * ChunkScale * BlockScale + currentPosOut.ToVector3() * BlockScale + Vector3.one * BlockScale * .5f , Vector3.one * BlockScale);

                Gizmos.DrawLine(globalOffset.AsVector3 * ChunkScale * BlockScale + currentPosOut.ToVector3() * BlockScale + Sculpting_and_Brush_setup.rotation * Vector3.up , globalOffset.AsVector3 * ChunkScale * BlockScale + currentPosOut.ToVector3() * BlockScale + Sculpting_and_Brush_setup.rotation * Vector3.down );
                Gizmos.DrawLine(globalOffset.AsVector3 * ChunkScale * BlockScale + currentPosOut.ToVector3() * BlockScale + Sculpting_and_Brush_setup.rotation * new Vector3(-1,0,0) , globalOffset.AsVector3 * ChunkScale * BlockScale + currentPosOut.ToVector3() * BlockScale + Sculpting_and_Brush_setup.rotation * Vector3.down );
                Gizmos.DrawLine(globalOffset.AsVector3 * ChunkScale * BlockScale + currentPosOut.ToVector3() * BlockScale + Sculpting_and_Brush_setup.rotation * new Vector3(1,0,0) , globalOffset.AsVector3 * ChunkScale * BlockScale + currentPosOut.ToVector3() * BlockScale + Sculpting_and_Brush_setup.rotation * Vector3.down );
            }
            if (hitMouse) Gizmos.DrawWireSphere(globalOffset.AsVector3 * ChunkScale * BlockScale + currentPosIn.ToVector3() * BlockScale + Vector3.one * BlockScale * .5f , Sculpting_and_Brush_setup.scaleRadius);
            for (int i = 0; i < Sculpting_and_Brush_setup.voxelPositions.Count; i++)
            {
                Gizmos.DrawWireCube(globalOffset.AsVector3 * ChunkScale * BlockScale + currentPosIn.AsVector3 * BlockScale + Sculpting_and_Brush_setup.voxelPositions[i] , Vector3.one * BlockScale);
            }
        }
#endif
        /// <summary>
        /// Creates a distance sorted list of points in a volumetric grid around center origin.
        /// </summary>
        public void CalculateSphericalOffsets()
        {
            List<Int3> positions = new();
#if Deactivate_ReducedMemoryUse
       for (int z = -32; z < 32; z++)
            {
                for (int y = -32; y < 32; y++)
                {
                    for (int x = -32; x < 32; x++)
                    {
                        positions.Add(new Int3(x, y, z));
                    }
                }
            }
#else
            for (int z = -8; z < 8; z++)
            {
                for (int y = -8; y < 8; y++)
                {
                    for (int x = -8; x < 8; x++)
                    {
                        positions.Add(new Int3(x, y, z));
                    }
                }
            }
#endif
            var list = positions.OrderBy(x => x.AsVector3.magnitude).ToList();
            //minor adjustments as seemed most quality improving during edits
            list.Remove(Int3.zero);
            list.Remove(Int3.bottom);
            list.Insert(0, Int3.bottom);
            list.Insert(0, Int3.zero);
            sphericalOffsetLookUpTable = list.ToArray();
        }
    }
    #endregion


    public struct getBlockResult
    {
        public Chunk chunk;
        public Block block;
        public Int3 localPos;
    }
}
#if !Deactivate_Async
public static class AsyncError
{
    /// <summary>
    /// ContinueWith((t)=>{
    /// throw t.Exception;
    /// },TaskContinuationOptions.OnlyOnFaulted);
    /// </summary>
    /// <param name="e"></param>
    /// <returns></returns>
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
    public static async Task Throw(Exception e)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
    {
        throw e;
    }
}
#endif