/*Antonio Wiege*/
using System;
using UnityEngine;
namespace ProceduralBlockWorldGeneratorToolPrototyping
{
    public class NoiseHandler
    {
        LandscapeTool directorInstance;
        CPU_Noise noiseCPU = new();

        public NoiseHandler(LandscapeTool inCharge)
        {
            directorInstance = inCharge;
        }
        public float[] Execute(Int3 key, NoiseInstanceSetup setup, ComputeBuffer resultBuffer, bool disposeBufferAfterward = false, bool pullDataFromGPU = false, int dispatchSize = LandscapeTool.ChunkScale, int dispatchDimensions = 3, bool useCPUinsteadOfGPU = false, float[] cpuData = null)
        {
            float[] data = null;

            if (!useCPUinsteadOfGPU)
            {

                ComputeShader cs;

                if (setup.cellular)
                {
                    switch (setup.dotsPerCell)
                    {
                        case 2:
                            cs = directorInstance.CellNoiseTwoDPC;
                            break;
                        case 3:
                            cs = directorInstance.CellNoiseThreeDPC;
                            break;
                        case 4:
                            cs = directorInstance.CellNoiseFourDPC;
                            break;
                        case 5:
                            cs = directorInstance.CellNoiseFiveDPC;
                            break;
                        case 6:
                            cs = directorInstance.CellNoiseSixDPC;
                            break;
                        default:
                            cs = directorInstance.CellNoiseOneDPC;
                            break;
                    }
                }
                else
                {
                    switch (setup.interpolationMethodID)
                    {
                        case 1:
                            cs = directorInstance.ValueNoiseNearestNeigbor;
                            break;
                        case 2:
                            cs = directorInstance.ValueNoiseLinear;
                            break;
                        case 3:
                            cs = directorInstance.ValueNoiseCosine;
                            break;
                        case 4:
                            cs = directorInstance.ValueNoiseCustomCosine;
                            break;
                        case 5:
                            cs = directorInstance.ValueNoiseCubic;
                            break;
                        default:
                            cs = directorInstance.ValueNoiseCustomHermite;
                            break;
                    }
                }

                cs.SetInt("dispatchThreadGroupCountX", dispatchSize);
                cs.SetVector("size", setup.scale);
                cs.SetVector("rotation", setup.rotation);
                cs.SetInt("resultApplicationID", setup.resultApplicationID);
                cs.SetFloat("weight", setup.weight);
                cs.SetInt("noiseDimensionCount", setup.noiseDimensionCount);
                cs.SetFloat("octaveStartSize", setup.octaveStartSize);
                cs.SetInt("octaveCount", setup.octaveCount);
                cs.SetFloat("octaveStepPow", setup.octaveStepPow);
                cs.SetFloat("octaveInfluenceBias", setup.octaveInfluenceBias);
                cs.SetFloat("planarizeAtY", setup.planarizeAtY);
                cs.SetFloat("planarizeIntensity", setup.planarizeIntensity);
                cs.SetFloat("choppy", setup.choppy ? 1 : 0);
                cs.SetFloat("turbulent", setup.turbulent ? 1 : 0);
                cs.SetFloat("invert", setup.invert ? 1 : 0);
                cs.SetFloat("planarize", setup.planarize ? 1 : 0);
                cs.SetFloat("distortSpace", setup.useCustomPositioning ? 1 : 0);
                cs.SetFloat("multiplyWith", setup.multiplyWith);
                cs.SetFloat("addPostMultiply", setup.addPostMultiply);
                cs.SetFloat("clampEndResultMin", setup.clampEndResultMin);
                cs.SetFloat("clampEndResultMax", setup.clampEndResultMax);
                cs.SetInt("cellularMethodID", setup.cellularMethodID);                   
                ComputeBuffer redefinedSpace;
                switch (dispatchDimensions)
                {
                    case 2:
                        cs.SetVector("position", new Vector4(key.x * dispatchSize, key.y * dispatchSize, key.z * dispatchSize, setup.seed));
                        if (resultBuffer == null)
                        {
                            resultBuffer = new ComputeBuffer(dispatchSize * dispatchSize, sizeof(float));//Cant use bools due to minimal stride length of 4
                            data = new float[dispatchSize * dispatchSize];
                            resultBuffer.SetData(data);
                        }
                        cs.SetBuffer(1, "Output", resultBuffer);
                        redefinedSpace = new ComputeBuffer(dispatchSize * dispatchSize, sizeof(float) * 3);
                        redefinedSpace.SetData(setup.spaceDistortion == null ? new Vector3[dispatchDimensions * dispatchDimensions] : setup.spaceDistortion);
                        cs.SetBuffer(1, "DistortionData", redefinedSpace);
                        cs.Dispatch(1, dispatchSize / 8, dispatchSize / 8, 1);
                        break;
                    default:
                        cs.SetVector("position", new Vector4(key.x * dispatchSize, key.y * dispatchSize, key.z * dispatchSize, setup.seed));
                        if (resultBuffer == null)
                        {
                            resultBuffer = new ComputeBuffer(dispatchSize * dispatchSize * dispatchSize, sizeof(float));//Cant use bools due to minimal stride length of 4
                            data = new float[dispatchSize * dispatchSize * dispatchSize];
                            resultBuffer.SetData(data);
                        }
                        cs.SetBuffer(0, "Output", resultBuffer);
                        redefinedSpace = new ComputeBuffer(dispatchSize * dispatchSize * dispatchSize, sizeof(float) * 3);
                        redefinedSpace.SetData(setup.spaceDistortion == null ? new Vector3[dispatchDimensions * dispatchDimensions * dispatchDimensions] : setup.spaceDistortion);
                        cs.SetBuffer(0, "DistortionData", redefinedSpace);
                        cs.Dispatch(0, dispatchSize / 4, dispatchSize / 4, dispatchSize / 4);
                        break;
                }
                redefinedSpace.Dispose();
                if (pullDataFromGPU) { data = dispatchDimensions == 2 ? new float[dispatchSize * dispatchSize] : new float[dispatchSize * dispatchSize * dispatchSize]; resultBuffer.GetData(data); }
                if (disposeBufferAfterward) { resultBuffer.Dispose(); resultBuffer = null; }


            }
            else
            {
                //in case of CPU noise computation one may not use GPU buffers and will instead pass the float[] to work upon
                //this will not grab contents from the result buffer itself
                noiseCPU.PopulateNoise(key, setup, cpuData, setup.seed, dispatchDimensions, dispatchSize);
                data = cpuData;
            }

            return data;
        }
    }
    /// <summary>
    /// Contains all user values to turn into valid noise
    /// </summary>
    [Serializable]
    public struct NoiseInstanceSetup
    {
        /*
         Resort Variables:

         */
        [Tooltip("ID of method how the resulting data will be applied onto the input buffer.\r\n      0  overwrite,\r\n      1  add,\r\n      2  subtract,\r\n      3  multiply,\r\n      4  divide,\r\n      5  average,\r\n     6   modulo,\r\n      7  difference")]
        public int resultApplicationID;
        [Tooltip("Noise redefining value")]
        public float seed;
        [Tooltip("Influence")]
        public float weight;
        [Tooltip("ValueNoise if false")]
        public bool cellular;
        [Tooltip("0 Shards, 1 Distance from point, 2 inverse distance from point, 3 two point equidistant, 4 three point equidistant, 5 four point equidistant")]
        public int cellularMethodID;// = 0;

        [Tooltip("Any unused dimension can be used as seed")]
        public Vector4 position;
        [Tooltip("0 Custom Hermite, 1 Nearest Neighbor, 2 Linear, 3 Cosine, 4 Custom Cosine, 5 Cubic")]
        public int interpolationMethodID;// = 0;
        [Tooltip("Dimensions of noise, not input nor output but during calculus")]
        public int noiseDimensionCount;// = 4;

        [Tooltip("Transform scale")]
        public Vector4 scale;// = 1;
        [Header("When using <3D on >2D output set rotation to X=1 and W=.25\n  to convert XY noise to XZ map orientation")]
        [Tooltip("Rotate around XYZ axis amount W in turns")]
        public Vector4 rotation;// = 1;

        public float octaveStartSize;// = 2;
        public int octaveCount;// = 5;
        public float octaveStepPow;// = 2;
        public float octaveInfluenceBias;// = 1;

        [Tooltip("Force noise closer to second dimension along XZ")]
        public bool planarize;// = false;
        public float planarizeAtY;// = 0;
        public float planarizeIntensity;// = 0.01f;

        [Tooltip("Choppy is like turbulent but at every step, while turbulent only happens at the end. Can both be used together.")]
        public bool choppy;// =false;
        [Tooltip("Turbulent is like choppy, but only happens at the end. Can both be used together.")]
        public bool turbulent;// = false;
        [Tooltip("1-X")]
        public bool invert;// = false;

        [Tooltip("In case of cellular noise only, multiple random dots per quadrant")]
        public int dotsPerCell;

        public float multiplyWith;//=1;
        public float addPostMultiply;// = 0;
        public float clampEndResultMin;// = 0;
        public float clampEndResultMax;// = 1;

        [Tooltip("Space Distortion (needs others result as input), not supported for every system"), Header("Space Distortion (needs others result as input)")]
        public bool useCustomPositioning;
        public Vector3[] spaceDistortion;

        public NoiseInstanceSetup(bool b = false)
        {
            resultApplicationID = 0;
            seed = 1;
            weight = 1;
            cellular = false;
            cellularMethodID = 1;
            position = Vector4.zero;
            interpolationMethodID = 0;
            noiseDimensionCount = 4;
            scale = Vector4.one;
            rotation = Vector4.zero;
            octaveStartSize = 3;
            octaveCount = 3;
            octaveStepPow = 2;
            octaveInfluenceBias = 1;
            planarize = false;
            planarizeAtY = 0;
            planarizeIntensity = 0.01f;
            choppy=false;
            turbulent=false;
            invert=false;
            dotsPerCell = 6;
            multiplyWith = 1;
            addPostMultiply = 0;
            clampEndResultMin = 0;
            clampEndResultMax = 1;
            useCustomPositioning = false;
            spaceDistortion=new Vector3[] { };
        }
    }
}