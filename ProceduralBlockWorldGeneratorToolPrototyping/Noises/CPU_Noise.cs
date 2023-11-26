/*Antonio Wiege*/
#if !Deactivate_Async
using System.Threading.Tasks;
#endif
using System.Collections.Generic;
using UnityEngine;

namespace ProceduralBlockWorldGeneratorToolPrototyping
{  
    /// <summary>
    /// Unoptimized, direct translation from Compute Shader to C#
    /// </summary>
    public class CPU_Noise
    {
        int resultApplicationID = 0;
        float weight=1;

        float planarize = 0;
        float choppy = 0;
        float turbulent = 0;//invert to get ridge
        float invert = 0;//together with turbulent to get ridge
        float distortSpace = 0;
        //regular values ; pre-assigning values does not work in here, the written values are merely standard suggestions
        float[] Output;
        int dispatchThreadGroupCountX;
        Vector3[] DistortionData;//overwrite local block pos

        int cellularMethodID;
        float planarizeAtY = 0;
        float planarizeIntensity = 0;

        Vector4 position = Vector4.zero;//x,y,z,w with w e.g. bias, seed, etc
        int noiseDimensionCount = 4;

        Vector4 size = Vector4.one;
        Vector4 rotation;

        float octaveStartSize = 2;
        int octaveCount = 5;
        float octaveStepPow = 2;
        float octaveInfluenceBias = 1;

        float multiplyWith = 1;
        float addPostMultiply = 0;
        float clampEndResultMin = 0;
        float clampEndResultMax = 1;

        public void PopulateNoise(Int3 key, NoiseInstanceSetup setup, float[] Output, float seed = 0.5f, int dispatchDimensions = 3, int dispatchThreadGroupCountX = LandscapeTool.ChunkScale)
        {
            this.Output = Output;
            this.dispatchThreadGroupCountX = dispatchThreadGroupCountX;
            resultApplicationID = setup.resultApplicationID;
            weight = setup.weight;
            planarize = setup.planarize ? 1f : 0f;
            choppy = setup.choppy ? 1f : 0f;
            turbulent = setup.turbulent ? 1f : 0f;
            invert = setup.invert ? 1f : 0f;
            distortSpace = setup.useCustomPositioning ? 1f : 0f;
            DistortionData = setup.spaceDistortion == null ? new Vector3[dispatchThreadGroupCountX * dispatchThreadGroupCountX * dispatchThreadGroupCountX] : setup.spaceDistortion;
            cellularMethodID = setup.cellularMethodID;
            planarizeAtY = setup.planarizeAtY;
            planarizeIntensity = setup.planarizeIntensity;
            position = new Vector4(key.x * dispatchThreadGroupCountX, key.y * dispatchThreadGroupCountX, key.z * dispatchThreadGroupCountX, seed);
            noiseDimensionCount = setup.noiseDimensionCount;
            size = setup.scale;
            rotation = setup.rotation;
            octaveStartSize = setup.octaveStartSize;
            octaveCount = setup.octaveCount;
            octaveStepPow = setup.octaveStepPow;
            octaveInfluenceBias = setup.octaveInfluenceBias;
            multiplyWith = setup.multiplyWith;
            addPostMultiply = setup.addPostMultiply;
            clampEndResultMin = setup.clampEndResultMin;
            clampEndResultMax = setup.clampEndResultMax;
            #if !Deactivate_Async
            List<Task> toAwait = new();
#endif
            if (setup.cellular)
            {
                if (dispatchDimensions == 3)
                {
                    switch (setup.dotsPerCell)
                    {
                        case 1:
#if !Deactivate_Async
                            Parallel.For(0, dispatchThreadGroupCountX, (z) =>
                            {
                                Parallel.For(0, dispatchThreadGroupCountX, (y) =>
                                {
                                    Parallel.For(0, dispatchThreadGroupCountX, (x) =>
                                    {
                                        Int3 id = new Int3(x, y, z);
                                        OneDPC3D(id, Output);
                                    });
                                });
                            });
#else
                            for (int z = 0; z < dispatchThreadGroupCountX; z++)
                            {
                                for (int y = 0; y < dispatchThreadGroupCountX; y++)
                                {
                                    for (int x = 0; x < dispatchThreadGroupCountX; x++)
                                    {
                                        Int3 id = new Int3(x, y, z);
                                        OneDPC3D(id, Output);
                                        }
                                }
                            }
#endif
                            break;
                        case 2:
#if !Deactivate_Async
                            Parallel.For(0, dispatchThreadGroupCountX, (z) =>
                            {
                                Parallel.For(0, dispatchThreadGroupCountX, (y) =>
                                {
                                    Parallel.For(0, dispatchThreadGroupCountX, (x) =>
                                    {
                                        Int3 id = new Int3(x, y, z);
                                        TwoDPC3D(id, Output);
                                    });
                                });
                            });
#else
                            for (int z = 0; z < dispatchThreadGroupCountX; z++)
                            {
                                for (int y = 0; y < dispatchThreadGroupCountX; y++)
                                {
                                    for (int x = 0; x < dispatchThreadGroupCountX; x++)
                                    {
                                        Int3 id = new Int3(x, y, z);
                                        TwoDPC3D(id, Output);
                                    }
                                }
                            }
#endif
                            break;
                        case 3:
#if !Deactivate_Async
                            Parallel.For(0, dispatchThreadGroupCountX, (z) =>
                            {
                                Parallel.For(0, dispatchThreadGroupCountX, (y) =>
                                {
                                    Parallel.For(0, dispatchThreadGroupCountX, (x) =>
                                    {
                                        Int3 id = new Int3(x, y, z);
                                        ThreeDPC3D(id, Output);
                                    });
                                });
                            });
#else
                            for (int z = 0; z < dispatchThreadGroupCountX; z++)
                            {
                                for (int y = 0; y < dispatchThreadGroupCountX; y++)
                                {
                                    for (int x = 0; x < dispatchThreadGroupCountX; x++)
                                    {
                                        Int3 id = new Int3(x, y, z);
                                        ThreeDPC3D(id, Output);
                                    }
                                }
                            }
#endif
                            break;
                        case 4:
#if !Deactivate_Async
                            Parallel.For(0, dispatchThreadGroupCountX, (z) =>
                            {
                                Parallel.For(0, dispatchThreadGroupCountX, (y) =>
                                {
                                    Parallel.For(0, dispatchThreadGroupCountX, (x) =>
                                    {
                                        Int3 id = new Int3(x, y, z);
                                        FourDPC3D(id, Output);
                                    });
                                });
                            });
#else
                            for (int z = 0; z < dispatchThreadGroupCountX; z++)
                            {
                                for (int y = 0; y < dispatchThreadGroupCountX; y++)
                                {
                                    for (int x = 0; x < dispatchThreadGroupCountX; x++)
                                    {
                                        Int3 id = new Int3(x, y, z);
                                        FourDPC3D(id, Output);
                                    }
                                }
                            }
#endif
                            break;
                        case 5:
#if !Deactivate_Async
                            Parallel.For(0, dispatchThreadGroupCountX, (z) =>
                            {
                                Parallel.For(0, dispatchThreadGroupCountX, (y) =>
                                {
                                    Parallel.For(0, dispatchThreadGroupCountX, (x) =>
                                    {
                                        Int3 id = new Int3(x, y, z);
                                        FiveDPC3D(id, Output);
                                    });
                                });
                            });
#else
                            for (int z = 0; z < dispatchThreadGroupCountX; z++)
                            {
                                for (int y = 0; y < dispatchThreadGroupCountX; y++)
                                {
                                    for (int x = 0; x < dispatchThreadGroupCountX; x++)
                                    {
                                        Int3 id = new Int3(x, y, z);
                                        FiveDPC3D(id, Output);
                                    }
                                }
                            }
#endif
                            break;
                        default:
#if !Deactivate_Async
                            Parallel.For(0, dispatchThreadGroupCountX, (z) =>
                            {
                                Parallel.For(0, dispatchThreadGroupCountX, (y) =>
                                {
                                    Parallel.For(0, dispatchThreadGroupCountX, (x) =>
                                    {
                                        Int3 id = new Int3(x, y, z);
                                        SixDPC3D(id, Output);
                                    });
                                });
                            });
#else
                            for (int z = 0; z < dispatchThreadGroupCountX; z++)
                            {
                                for (int y = 0; y < dispatchThreadGroupCountX; y++)
                                {
                                    for (int x = 0; x < dispatchThreadGroupCountX; x++)
                                    {
                                        Int3 id = new Int3(x, y, z);
                                        SixDPC3D(id, Output);
                                    }
                                }
                            }
#endif
                            break;
                    }
                }
                else
                {
                    switch (setup.dotsPerCell)
                    {
                        case 1:
#if !Deactivate_Async
                            Parallel.For(0, dispatchThreadGroupCountX, (y) =>
                                {
                                    Parallel.For(0, dispatchThreadGroupCountX, (x) =>
                                    {
                                        Int3 id = new Int3(x, y, 0);
                                        OneDPC2D(id, Output);
                                    });
                                });
#else
                            for (int y = 0; y < dispatchThreadGroupCountX; y++)
                            {
                                for (int x = 0; x < dispatchThreadGroupCountX; x++)
                                {
                                    Int3 id = new Int3(x, y, 0);
                                    OneDPC2D(id, Output);
                                }
                            }
#endif
                            break;
                        case 2:
#if !Deactivate_Async
                            Parallel.For(0, dispatchThreadGroupCountX, (y) =>
                                {
                                    Parallel.For(0, dispatchThreadGroupCountX, (x) =>
                                    {
                                        Int3 id = new Int3(x, y, 0);
                                        TwoDPC2D(id, Output);
                                    });
                                });
#else
                            for (int y = 0; y < dispatchThreadGroupCountX; y++)
                            {
                                for (int x = 0; x < dispatchThreadGroupCountX; x++)
                                {
                                    Int3 id = new Int3(x, y, 0);
                                    TwoDPC2D(id, Output);
                                }
                            }
#endif
                            break;
                        case 3:
#if !Deactivate_Async
                            Parallel.For(0, dispatchThreadGroupCountX, (y) =>
                                {
                                    Parallel.For(0, dispatchThreadGroupCountX, (x) =>
                                    {
                                        Int3 id = new Int3(x, y, 0);
                                        ThreeDPC2D(id, Output);
                                    });
                                });
#else
                            for (int y = 0; y < dispatchThreadGroupCountX; y++)
                            {
                                for (int x = 0; x < dispatchThreadGroupCountX; x++)
                                {
                                    Int3 id = new Int3(x, y, 0);
                                    ThreeDPC2D(id, Output);
                                }
                            }
#endif
                            break;
                        case 4:
#if !Deactivate_Async
                            Parallel.For(0, dispatchThreadGroupCountX, (y) =>
                                {
                                    Parallel.For(0, dispatchThreadGroupCountX, (x) =>
                                    {
                                        Int3 id = new Int3(x, y, 0);
                                        FourDPC2D(id, Output);
                                    });
                                });
#else
                            for (int y = 0; y < dispatchThreadGroupCountX; y++)
                            {
                                for (int x = 0; x < dispatchThreadGroupCountX; x++)
                                {
                                    Int3 id = new Int3(x, y, 0);
                                    FourDPC2D(id, Output);
                                }
                            }
#endif
                            break;
                        case 5:
#if !Deactivate_Async
                            Parallel.For(0, dispatchThreadGroupCountX, (y) =>
                                {
                                    Parallel.For(0, dispatchThreadGroupCountX, (x) =>
                                    {
                                        Int3 id = new Int3(x, y, 0);
                                        FiveDPC2D(id, Output);
                                    });
                                });
#else
                            for (int y = 0; y < dispatchThreadGroupCountX; y++)
                            {
                                for (int x = 0; x < dispatchThreadGroupCountX; x++)
                                {
                                    Int3 id = new Int3(x, y, 0);
                                    FiveDPC2D(id, Output);
                                }
                            }
#endif
                            break;
                        default:
#if !Deactivate_Async
                            Parallel.For(0, dispatchThreadGroupCountX, (y) =>
                                {
                                    Parallel.For(0, dispatchThreadGroupCountX, (x) =>
                                    {
                                        Int3 id = new Int3(x, y, 0);
                                        SixDPC2D(id, Output);
                                    });
                                });
#else
                            for (int y = 0; y < dispatchThreadGroupCountX; y++)
                            {
                                for (int x = 0; x < dispatchThreadGroupCountX; x++)
                                {
                                    Int3 id = new Int3(x, y, 0);
                                    SixDPC2D(id, Output);
                                }
                            }
#endif
                            break;
                    }
                }
            }
            else
            {
                if (dispatchDimensions == 3)
                {
                    switch (setup.interpolationMethodID)
                    {
                        case 1:
#if !Deactivate_Async
                            Parallel.For(0, dispatchThreadGroupCountX, (z) =>
                            {
                                Parallel.For(0, dispatchThreadGroupCountX, (y) =>
                                {
                                    Parallel.For(0, dispatchThreadGroupCountX, (x) =>
                                    {
                                        Int3 id = new Int3(x, y, z);
                                        VNnearest3D(id, Output);
                                    });
                                });
                            });
#else
                            for (int z = 0; z < dispatchThreadGroupCountX; z++)
                            {
                                for (int y = 0; y < dispatchThreadGroupCountX; y++)
                                {
                                    for (int x = 0; x < dispatchThreadGroupCountX; x++)
                                    {
                                        Int3 id = new Int3(x, y, z);
                                        VNnearest3D(id, Output);
                                    }
                                }
                            }
#endif
                            break;
                        case 2:
#if !Deactivate_Async
                            Parallel.For(0, dispatchThreadGroupCountX, (z) =>
                            {
                                Parallel.For(0, dispatchThreadGroupCountX, (y) =>
                                {
                                    Parallel.For(0, dispatchThreadGroupCountX, (x) =>
                                    {
                                        Int3 id = new Int3(x, y, z);
                                        VNlinear3D(id, Output);
                                    });
                                });
                            });
#else
                            for (int z = 0; z < dispatchThreadGroupCountX; z++)
                            {
                                for (int y = 0; y < dispatchThreadGroupCountX; y++)
                                {
                                    for (int x = 0; x < dispatchThreadGroupCountX; x++)
                                    {
                                        Int3 id = new Int3(x, y, z);
                                        VNlinear3D(id, Output);
                                    }
                                }
                            }
#endif
                            break;
                        case 3:
#if !Deactivate_Async
                            Parallel.For(0, dispatchThreadGroupCountX, (z) =>
                            {
                                Parallel.For(0, dispatchThreadGroupCountX, (y) =>
                                {
                                    Parallel.For(0, dispatchThreadGroupCountX, (x) =>
                                    {
                                        Int3 id = new Int3(x, y, z);
                                        VNcosine3D(id, Output);
                                    });
                                });
                            });
#else
                            for (int z = 0; z < dispatchThreadGroupCountX; z++)
                            {
                                for (int y = 0; y < dispatchThreadGroupCountX; y++)
                                {
                                    for (int x = 0; x < dispatchThreadGroupCountX; x++)
                                    {
                                        Int3 id = new Int3(x, y, z);
                                        VNcosine3D(id, Output);
                                    }
                                }
                            }
#endif
                            break;
                        case 4:
#if !Deactivate_Async
                            Parallel.For(0, dispatchThreadGroupCountX, (z) =>
                            {
                                Parallel.For(0, dispatchThreadGroupCountX, (y) =>
                                {
                                    Parallel.For(0, dispatchThreadGroupCountX, (x) =>
                                    {
                                        Int3 id = new Int3(x, y, z);
                                        VNcustomcosine3D(id, Output);
                                    });
                                });
                            });
#else
                            for (int z = 0; z < dispatchThreadGroupCountX; z++)
                            {
                                for (int y = 0; y < dispatchThreadGroupCountX; y++)
                                {
                                    for (int x = 0; x < dispatchThreadGroupCountX; x++)
                                    {
                                        Int3 id = new Int3(x, y, z);
                                        VNcustomcosine3D(id, Output);
                                    }
                                }
                            }
#endif
                            break;
                        case 5:
#if !Deactivate_Async
                            Parallel.For(0, dispatchThreadGroupCountX, (z) =>
                            {
                                Parallel.For(0, dispatchThreadGroupCountX, (y) =>
                                {
                                    Parallel.For(0, dispatchThreadGroupCountX, (x) =>
                                    {
                                        Int3 id = new Int3(x, y, z);
                                        VNcubic3D(id, Output);
                                    });
                                });
                            });
#else
                            for (int z = 0; z < dispatchThreadGroupCountX; z++)
                            {
                                for (int y = 0; y < dispatchThreadGroupCountX; y++)
                                {
                                    for (int x = 0; x < dispatchThreadGroupCountX; x++)
                                    {
                                        Int3 id = new Int3(x, y, z);
                                        VNcubic3D(id, Output);
                                    }
                                }
                            }
#endif
                            break;
                        default:
#if !Deactivate_Async
                            Parallel.For(0, dispatchThreadGroupCountX, (z) =>
                            {
                                Parallel.For(0, dispatchThreadGroupCountX, (y) =>
                                {
                                    Parallel.For(0, dispatchThreadGroupCountX, (x) =>
                                    {
                                        Int3 id = new Int3(x, y, z);
                                        VNcustomhermite3D(id, Output);
                                    });
                                });
                            });
#else
                            for (int z = 0; z < dispatchThreadGroupCountX; z++)
                            {
                                for (int y = 0; y < dispatchThreadGroupCountX; y++)
                                {
                                    for (int x = 0; x < dispatchThreadGroupCountX; x++)
                                    {
                                        Int3 id = new Int3(x, y, z);
                                        VNcustomhermite3D(id, Output);
                                    }
                                }
                            }
#endif
                            break;
                    }
                }
                else
                {
                    switch (setup.interpolationMethodID)
                    {
                        case 1:
#if !Deactivate_Async
                            Parallel.For(0, dispatchThreadGroupCountX, (y) =>
                                {
                                    Parallel.For(0, dispatchThreadGroupCountX, (x) =>
                                    {
                                        Int3 id = new Int3(x, y, 0);
                                        VNnearest2D(id, Output);
                                    });
                                });
#else
                            for (int y = 0; y < dispatchThreadGroupCountX; y++)
                            {
                                for (int x = 0; x < dispatchThreadGroupCountX; x++)
                                {
                                    Int3 id = new Int3(x, y, 0);
                                    VNnearest2D(id, Output);
                                }
                            }
#endif
                            break;
                        case 2:
#if !Deactivate_Async
                            Parallel.For(0, dispatchThreadGroupCountX, (y) =>
                                {
                                    Parallel.For(0, dispatchThreadGroupCountX, (x) =>
                                    {
                                        Int3 id = new Int3(x, y, 0);
                                        VNlinear2D(id, Output);
                                    });
                                });
#else
                            for (int y = 0; y < dispatchThreadGroupCountX; y++)
                            {
                                for (int x = 0; x < dispatchThreadGroupCountX; x++)
                                {
                                    Int3 id = new Int3(x, y, 0);
                                    VNlinear2D(id, Output);
                                }
                            }
#endif
                            break;
                        case 3:
#if !Deactivate_Async
                            Parallel.For(0, dispatchThreadGroupCountX, (y) =>
                                {
                                    Parallel.For(0, dispatchThreadGroupCountX, (x) =>
                                    {
                                        Int3 id = new Int3(x, y, 0);
                                        VNcosine2D(id, Output);
                                    });
                                });
#else
                            for (int y = 0; y < dispatchThreadGroupCountX; y++)
                            {
                                for (int x = 0; x < dispatchThreadGroupCountX; x++)
                                {
                                    Int3 id = new Int3(x, y, 0);
                                    VNcosine2D(id, Output);
                                }
                            }
#endif
                            break;
                        case 4:
#if !Deactivate_Async
                            Parallel.For(0, dispatchThreadGroupCountX, (y) =>
                                {
                                    Parallel.For(0, dispatchThreadGroupCountX, (x) =>
                                    {
                                        Int3 id = new Int3(x, y, 0);
                                        VNcustomcosine2D(id, Output);
                                    });
                                });
#else
                            for (int y = 0; y < dispatchThreadGroupCountX; y++)
                            {
                                for (int x = 0; x < dispatchThreadGroupCountX; x++)
                                {
                                    Int3 id = new Int3(x, y, 0);
                                    VNcustomcosine2D(id, Output);
                                }
                            }
#endif
                            break;
                        case 5:
#if !Deactivate_Async
                            Parallel.For(0, dispatchThreadGroupCountX, (y) =>
                                {
                                    Parallel.For(0, dispatchThreadGroupCountX, (x) =>
                                    {
                                        Int3 id = new Int3(x, y, 0);
                                        VNcubic2D(id, Output);
                                    });
                                });
#else
                            for (int y = 0; y < dispatchThreadGroupCountX; y++)
                            {
                                for (int x = 0; x < dispatchThreadGroupCountX; x++)
                                {
                                    Int3 id = new Int3(x, y, 0);
                                    VNcubic2D(id, Output);
                                }
                            }
#endif
                            break;
                        default:
#if !Deactivate_Async
                            Parallel.For(0, dispatchThreadGroupCountX, (y) =>
                                {
                                    Parallel.For(0, dispatchThreadGroupCountX, (x) =>
                                    {
                                        Int3 id = new Int3(x, y, 0);
                                        VNcustomhermite2D(id, Output);
                                    });
                                });
#else
                            for (int y = 0; y < dispatchThreadGroupCountX; y++)
                            {
                                for (int x = 0; x < dispatchThreadGroupCountX; x++)
                                {
                                    Int3 id = new Int3(x, y, 0);
                                    VNcustomhermite2D(id, Output);
                                }
                            }
#endif
                            break;
                    }
                }
            }
        }



        public void OneDPC3D(Int3 id, float[] Output)
        {
            float value = 0;
            float div = 0;
            float scale = 1;
            int cid = id.x + id.y * dispatchThreadGroupCountX + id.z * dispatchThreadGroupCountX * dispatchThreadGroupCountX;
            Vector4 pos = (new Vector4(id.x, id.y, id.z, 0) + position + ((distortSpace == 0) ? Vector4.zero : new Vector4(DistortionData[cid].x, DistortionData[cid].y, DistortionData[cid].z, 0))).Mul(size);
            for (int o = (int)octaveStartSize; o < octaveStartSize + octaveCount; o++)
            {
                scale = Mathf.Pow(Mathf.Abs(octaveStepPow), o);
                value += Mathf.Abs(CellNoiseOneDPC(pos, scale) * Mathf.Pow(scale, octaveInfluenceBias) - ((choppy != 0) ? 0.5f : 0)) * ((choppy != 0) ? 2 : 1);
                div += Mathf.Pow(scale, octaveInfluenceBias);
            }
            value /= (div < 1) ? 1 : div;
            value = (turbulent != 0) ? Mathf.Abs((value - 0.5f) * 2) : value;
            value = (invert != 0) ? 1 - value : value;
            value = Mathf.Clamp(value * multiplyWith + addPostMultiply -
                ((planarize != 0) ? planarizeIntensity * (pos.y - planarizeAtY) : 0),
                clampEndResultMin, clampEndResultMax);

            switch (resultApplicationID)
            {
                case 1:
                    Output[cid] += value * weight;
                    break;
                case 2:
                    Output[cid] -= value * weight;
                    break;
                case 3:
                    Output[cid] *= value * weight;
                    break;
                case 4:
                    Output[cid] /= (value * weight == 0) ? 1 : value * weight;//do not divide by zero my friend
                    break;
                case 5:
                    Output[cid] = value * (.5f + weight * .5f) + Output[cid] * (.5f - weight * .5f);
                    break;
                case 6:
                    Output[cid] %= value;
                    break;
                case 7:
                    Output[cid] = Mathf.Abs(value - Output[cid]);
                    break;
                default:
                    Output[cid] = value;
                    break;
            }

        }

        public void TwoDPC3D(Int3 id, float[] Output)
        {
            float value = 0;
            float div = 0;
            float scale = 1;
            int cid = id.x + id.y * dispatchThreadGroupCountX + id.z * dispatchThreadGroupCountX * dispatchThreadGroupCountX;
            Vector4 pos = (new Vector4(id.x, id.y, id.z, 0) + position + ((distortSpace == 0) ? Vector4.zero : new Vector4(DistortionData[cid].x, DistortionData[cid].y, DistortionData[cid].z, 0))).Mul(size);
            for (int o = (int)octaveStartSize; o < octaveStartSize + octaveCount; o++)
            {
                scale = Mathf.Pow(Mathf.Abs(octaveStepPow), o);
                value += Mathf.Abs(CellNoiseTwoDPC(pos, scale) * Mathf.Pow(scale, octaveInfluenceBias) - ((choppy != 0) ? 0.5f : 0)) * ((choppy != 0) ? 2 : 1);
                div += Mathf.Pow(scale, octaveInfluenceBias);
            }
            value /= (div < 1) ? 1 : div;
            value = (turbulent != 0) ? Mathf.Abs((value - 0.5f) * 2) : value;
            value = (invert != 0) ? 1 - value : value;
            value = Mathf.Clamp(value * multiplyWith + addPostMultiply -
              ((planarize != 0) ? planarizeIntensity * (pos.y - planarizeAtY) : 0),
              clampEndResultMin, clampEndResultMax);

            switch (resultApplicationID)
            {
                case 1:
                    Output[cid] += value * weight;
                    break;
                case 2:
                    Output[cid] -= value * weight;
                    break;
                case 3:
                    Output[cid] *= value * weight;
                    break;
                case 4:
                    Output[cid] /= (value * weight == 0) ? 1 : value * weight;//do not divide by zero my friend
                    break;
                case 5:
                    Output[cid] = value * (.5f + weight * .5f) + Output[cid] * (.5f - weight * .5f);
                    break;
                case 6:
                    Output[cid] %= value;
                    break;
                case 7:
                    Output[cid] = Mathf.Abs(value - Output[cid]);
                    break;
                default:
                    Output[cid] = value;
                    break;
            }
        }

        public void ThreeDPC3D(Int3 id, float[] Output)
        {
            float value = 0;
            float div = 0;
            float scale = 1;
            int cid = id.x + id.y * dispatchThreadGroupCountX + id.z * dispatchThreadGroupCountX * dispatchThreadGroupCountX;
            Vector4 pos = (new Vector4(id.x, id.y, id.z, 0) + position + ((distortSpace == 0) ? Vector4.zero : new Vector4(DistortionData[cid].x, DistortionData[cid].y, DistortionData[cid].z, 0))).Mul(size);
            for (int o = (int)octaveStartSize; o < octaveStartSize + octaveCount; o++)
            {
                scale = Mathf.Pow(Mathf.Abs(octaveStepPow), o);
                value += Mathf.Abs(CellNoiseThreeDPC(pos, scale) * Mathf.Pow(scale, octaveInfluenceBias) - ((choppy != 0) ? 0.5f : 0)) * ((choppy != 0) ? 2 : 1);
                div += Mathf.Pow(scale, octaveInfluenceBias);
            }
            value /= (div < 1) ? 1 : div;
            value = (turbulent != 0) ? Mathf.Abs((value - 0.5f) * 2) : value;
            value = (invert != 0) ? 1 - value : value;
            value = Mathf.Clamp(value * multiplyWith + addPostMultiply -
              ((planarize != 0) ? planarizeIntensity * (pos.y - planarizeAtY) : 0),
              clampEndResultMin, clampEndResultMax);

            switch (resultApplicationID)
            {
                case 1:
                    Output[cid] += value * weight;
                    break;
                case 2:
                    Output[cid] -= value * weight;
                    break;
                case 3:
                    Output[cid] *= value * weight;
                    break;
                case 4:
                    Output[cid] /= (value * weight == 0) ? 1 : value * weight;//do not divide by zero my friend
                    break;
                case 5:
                    Output[cid] = value * (.5f + weight * .5f) + Output[cid] * (.5f - weight * .5f);//deprecated:weight==0?Output[cid]:
                    break;
                case 6:
                    Output[cid] %= value;
                    break;
                case 7:
                    Output[cid] = Mathf.Abs(value - Output[cid]);
                    break;
                default:
                    Output[cid] = value;
                    break;
            }
        }
        public void FourDPC3D(Int3 id, float[] Output)
        {
            float value = 0;
            float div = 0;
            float scale = 1;
            int cid = id.x + id.y * dispatchThreadGroupCountX + id.z * dispatchThreadGroupCountX * dispatchThreadGroupCountX;
            Vector4 pos = (new Vector4(id.x, id.y, id.z, 0) + position + ((distortSpace == 0) ? Vector4.zero : new Vector4(DistortionData[cid].x, DistortionData[cid].y, DistortionData[cid].z, 0))).Mul(size);
            for (int o = (int)octaveStartSize; o < octaveStartSize + octaveCount; o++)
            {
                scale = Mathf.Pow(Mathf.Abs(octaveStepPow), o);
                value += Mathf.Abs(CellNoiseFourDPC(pos, scale) * Mathf.Pow(scale, octaveInfluenceBias) - ((choppy != 0) ? 0.5f : 0)) * ((choppy != 0) ? 2 : 1);
                div += Mathf.Pow(scale, octaveInfluenceBias);
            }
            value /= (div < 1) ? 1 : div;
            value = (turbulent != 0) ? Mathf.Abs((value - 0.5f) * 2) : value;
            value = (invert != 0) ? 1 - value : value;
            value = Mathf.Clamp(value * multiplyWith + addPostMultiply -
              ((planarize != 0) ? planarizeIntensity * (pos.y - planarizeAtY) : 0),
              clampEndResultMin, clampEndResultMax);

            switch (resultApplicationID)
            {
                case 1:
                    Output[cid] += value * weight;
                    break;
                case 2:
                    Output[cid] -= value * weight;
                    break;
                case 3:
                    Output[cid] *= value * weight;
                    break;
                case 4:
                    Output[cid] /= (value * weight == 0) ? 1 : value * weight;//do not divide by zero my friend
                    break;
                case 5:
                    Output[cid] = value * (.5f + weight * .5f) + Output[cid] * (.5f - weight * .5f);
                    break;
                case 6:
                    Output[cid] %= value;
                    break;
                case 7:
                    Output[cid] = Mathf.Abs(value - Output[cid]);
                    break;
                default:
                    Output[cid] = value;
                    break;
            }
        }
        public void FiveDPC3D(Int3 id, float[] Output)
        {
            float value = 0;
            float div = 0;
            float scale = 1;
            int cid = id.x + id.y * dispatchThreadGroupCountX + id.z * dispatchThreadGroupCountX * dispatchThreadGroupCountX;
            Vector4 pos = (new Vector4(id.x, id.y, id.z, 0) + position + ((distortSpace == 0) ? Vector4.zero : new Vector4(DistortionData[cid].x, DistortionData[cid].y, DistortionData[cid].z, 0))).Mul(size);
            for (int o = (int)octaveStartSize; o < octaveStartSize + octaveCount; o++)
            {
                scale = Mathf.Pow(Mathf.Abs(octaveStepPow), o);
                value += Mathf.Abs(CellNoiseFiveDPC(pos, scale) * Mathf.Pow(scale, octaveInfluenceBias) - ((choppy != 0) ? 0.5f : 0)) * ((choppy != 0) ? 2 : 1);
                div += Mathf.Pow(scale, octaveInfluenceBias);
            }
            value /= (div < 1) ? 1 : div;
            value = (turbulent != 0) ? Mathf.Abs((value - 0.5f) * 2) : value;
            value = (invert != 0) ? 1 - value : value;
            value = Mathf.Clamp(value * multiplyWith + addPostMultiply -
              ((planarize != 0) ? planarizeIntensity * (pos.y - planarizeAtY) : 0),
              clampEndResultMin, clampEndResultMax);

            switch (resultApplicationID)
            {
                case 1:
                    Output[cid] += value * weight;
                    break;
                case 2:
                    Output[cid] -= value * weight;
                    break;
                case 3:
                    Output[cid] *= value * weight;
                    break;
                case 4:
                    Output[cid] /= (value * weight == 0) ? 1 : value * weight;//do not divide by zero my friend
                    break;
                case 5:
                    Output[cid] = value * (.5f + weight * .5f) + Output[cid] * (.5f - weight * .5f);
                    break;
                case 6:
                    Output[cid] %= value;
                    break;
                case 7:
                    Output[cid] = Mathf.Abs(value - Output[cid]);
                    break;
                default:
                    Output[cid] = value;
                    break;
            }
        }
        public void SixDPC3D(Int3 id, float[] Output)
        {
            float value = 0;
            float div = 0;
            float scale = 1;
            int cid = id.x + id.y * dispatchThreadGroupCountX + id.z * dispatchThreadGroupCountX * dispatchThreadGroupCountX;
            Vector4 pos = (new Vector4(id.x, id.y, id.z, 0) + position + ((distortSpace == 0) ? Vector4.zero : new Vector4(DistortionData[cid].x, DistortionData[cid].y, DistortionData[cid].z, 0))).Mul(size);
            for (int o = (int)octaveStartSize; o < octaveStartSize + octaveCount; o++)
            {
                scale = Mathf.Pow(Mathf.Abs(octaveStepPow), o);
                value += Mathf.Abs(CellNoiseSixDPC(pos, scale) * Mathf.Pow(scale, octaveInfluenceBias) - ((choppy != 0) ? 0.5f : 0)) * ((choppy != 0) ? 2 : 1);
                div += Mathf.Pow(scale, octaveInfluenceBias);
            }
            value /= (div < 1) ? 1 : div;
            value = (turbulent != 0) ? Mathf.Abs((value - 0.5f) * 2) : value;
            value = (invert != 0) ? 1 - value : value;
            value = Mathf.Clamp(value * multiplyWith + addPostMultiply -
              ((planarize != 0) ? planarizeIntensity * (pos.y - planarizeAtY) : 0),
              clampEndResultMin, clampEndResultMax);

            switch (resultApplicationID)
            {
                case 1:
                    Output[cid] += value * weight;
                    break;
                case 2:
                    Output[cid] -= value * weight;
                    break;
                case 3:
                    Output[cid] *= value * weight;
                    break;
                case 4:
                    Output[cid] /= (value * weight == 0) ? 1 : value * weight;//do not divide by zero my friend
                    break;
                case 5:
                    Output[cid] = value * (.5f + weight * .5f) + Output[cid] * (.5f - weight * .5f);
                    break;
                case 6:
                    Output[cid] %= value;
                    break;
                case 7:
                    Output[cid] = Mathf.Abs(value - Output[cid]);
                    break;
                default:
                    Output[cid] = value;
                    break;
            }
        }
        public void OneDPC2D(Int3 id, float[] Output)
        {
            float value = 0;
            float div = 0;
            float scale = 1;
            int cid = id.x + id.y * dispatchThreadGroupCountX;
            Vector4 pos = (new Vector4(id.x, id.y, 0, 0) + position + ((distortSpace == 0) ? Vector4.zero : new Vector4(DistortionData[cid].x, DistortionData[cid].y, DistortionData[cid].z, 0))).Mul(size);
            for (int o = (int)octaveStartSize; o < octaveStartSize + octaveCount; o++)
            {
                scale = Mathf.Pow(Mathf.Abs(octaveStepPow), o);
                value += Mathf.Abs(CellNoiseOneDPC(pos, scale) * Mathf.Pow(scale, octaveInfluenceBias) - ((choppy != 0) ? 0.5f : 0)) * ((choppy != 0) ? 2 : 1);
                div += Mathf.Pow(scale, octaveInfluenceBias);
            }
            value /= (div < 1) ? 1 : div;
            value = (turbulent != 0) ? Mathf.Abs((value - 0.5f) * 2) : value;
            value = (invert != 0) ? 1 - value : value;
            value = Mathf.Clamp(value * multiplyWith + addPostMultiply -
              ((planarize != 0) ? planarizeIntensity * (pos.y - planarizeAtY) : 0),
              clampEndResultMin, clampEndResultMax);

            switch (resultApplicationID)
            {
                case 1:
                    Output[cid] += value * weight;
                    break;
                case 2:
                    Output[cid] -= value * weight;
                    break;
                case 3:
                    Output[cid] *= value * weight;
                    break;
                case 4:
                    Output[cid] /= (value * weight == 0) ? 1 : value * weight;//do not divide by zero my friend
                    break;
                case 5:
                    Output[cid] = value * (.5f + weight * .5f) + Output[cid] * (.5f - weight * .5f);
                    break;
                case 6:
                    Output[cid] %= value;
                    break;
                case 7:
                    Output[cid] = Mathf.Abs(value - Output[cid]);
                    break;
                default:
                    Output[cid] = value;
                    break;
            }
        }
        public void TwoDPC2D(Int3 id, float[] Output)
        {
            float value = 0;
            float div = 0;
            float scale = 1;
            int cid = id.x + id.y * dispatchThreadGroupCountX;
            Vector4 pos = (new Vector4(id.x, id.y, 0, 0) + position + ((distortSpace == 0) ? Vector4.zero : new Vector4(DistortionData[cid].x, DistortionData[cid].y, DistortionData[cid].z, 0))).Mul(size);
            for (int o = (int)octaveStartSize; o < octaveStartSize + octaveCount; o++)
            {
                scale = Mathf.Pow(Mathf.Abs(octaveStepPow), o);
                value += Mathf.Abs(CellNoiseTwoDPC(pos, scale) * Mathf.Pow(scale, octaveInfluenceBias) - ((choppy != 0) ? 0.5f : 0)) * ((choppy != 0) ? 2 : 1);
                div += Mathf.Pow(scale, octaveInfluenceBias);
            }
            value /= (div < 1) ? 1 : div;
            value = (turbulent != 0) ? Mathf.Abs((value - 0.5f) * 2) : value;
            value = (invert != 0) ? 1 - value : value;
            value = Mathf.Clamp(value * multiplyWith + addPostMultiply -
              ((planarize != 0) ? planarizeIntensity * (pos.y - planarizeAtY) : 0),
              clampEndResultMin, clampEndResultMax);

            switch (resultApplicationID)
            {
                case 1:
                    Output[cid] += value * weight;
                    break;
                case 2:
                    Output[cid] -= value * weight;
                    break;
                case 3:
                    Output[cid] *= value * weight;
                    break;
                case 4:
                    Output[cid] /= (value * weight == 0) ? 1 : value * weight;//do not divide by zero my friend
                    break;
                case 5:
                    Output[cid] = value * (.5f + weight * .5f) + Output[cid] * (.5f - weight * .5f);
                    break;
                case 6:
                    Output[cid] %= value;
                    break;
                case 7:
                    Output[cid] = Mathf.Abs(value - Output[cid]);
                    break;
                default:
                    Output[cid] = value;
                    break;
            }
        }
        public void ThreeDPC2D(Int3 id, float[] Output)
        {
            float value = 0;
            float div = 0;
            float scale = 1;
            int cid = id.x + id.y * dispatchThreadGroupCountX;
            Vector4 pos = (new Vector4(id.x, id.y, 0, 0) + position + ((distortSpace == 0) ? Vector4.zero : new Vector4(DistortionData[cid].x, DistortionData[cid].y, DistortionData[cid].z, 0))).Mul(size);
            for (int o = (int)octaveStartSize; o < octaveStartSize + octaveCount; o++)
            {
                scale = Mathf.Pow(Mathf.Abs(octaveStepPow), o);
                value += Mathf.Abs(CellNoiseThreeDPC(pos, scale) * Mathf.Pow(scale, octaveInfluenceBias) - ((choppy != 0) ? 0.5f : 0)) * ((choppy != 0) ? 2 : 1);
                div += Mathf.Pow(scale, octaveInfluenceBias);
            }
            value /= (div < 1) ? 1 : div;
            value = (turbulent != 0) ? Mathf.Abs((value - 0.5f) * 2) : value;
            value = (invert != 0) ? 1 - value : value;
            value = Mathf.Clamp(value * multiplyWith + addPostMultiply -
              ((planarize != 0) ? planarizeIntensity * (pos.y - planarizeAtY) : 0),
              clampEndResultMin, clampEndResultMax);

            switch (resultApplicationID)
            {
                case 1:
                    Output[cid] += value * weight;
                    break;
                case 2:
                    Output[cid] -= value * weight;
                    break;
                case 3:
                    Output[cid] *= value * weight;
                    break;
                case 4:
                    Output[cid] /= (value * weight == 0) ? 1 : value * weight;//do not divide by zero my friend
                    break;
                case 5:
                    Output[cid] = value * (.5f + weight * .5f) + Output[cid] * (.5f - weight * .5f);
                    break;
                case 6:
                    Output[cid] %= value;
                    break;
                case 7:
                    Output[cid] = Mathf.Abs(value - Output[cid]);
                    break;
                default:
                    Output[cid] = value;
                    break;
            }
        }
        public void FourDPC2D(Int3 id, float[] Output)
        {
            float value = 0;
            float div = 0;
            float scale = 1;
            int cid = id.x + id.y * dispatchThreadGroupCountX;
            Vector4 pos = (new Vector4(id.x, id.y, 0, 0) + position + ((distortSpace == 0) ? Vector4.zero : new Vector4(DistortionData[cid].x, DistortionData[cid].y, DistortionData[cid].z, 0))).Mul(size);
            for (int o = (int)octaveStartSize; o < octaveStartSize + octaveCount; o++)
            {
                scale = Mathf.Pow(Mathf.Abs(octaveStepPow), o);
                value += Mathf.Abs(CellNoiseFourDPC(pos, scale) * Mathf.Pow(scale, octaveInfluenceBias) - ((choppy != 0) ? 0.5f : 0)) * ((choppy != 0) ? 2 : 1);
                div += Mathf.Pow(scale, octaveInfluenceBias);
            }
            value /= (div < 1) ? 1 : div;
            value = (turbulent != 0) ? Mathf.Abs((value - 0.5f) * 2) : value;
            value = (invert != 0) ? 1 - value : value;
            value = Mathf.Clamp(value * multiplyWith + addPostMultiply -
              ((planarize != 0) ? planarizeIntensity * (pos.y - planarizeAtY) : 0),
              clampEndResultMin, clampEndResultMax);

            switch (resultApplicationID)
            {
                case 1:
                    Output[cid] += value * weight;
                    break;
                case 2:
                    Output[cid] -= value * weight;
                    break;
                case 3:
                    Output[cid] *= value * weight;
                    break;
                case 4:
                    Output[cid] /= (value * weight == 0) ? 1 : value * weight;//do not divide by zero my friend
                    break;
                case 5:
                    Output[cid] = value * (.5f + weight * .5f) + Output[cid] * (.5f - weight * .5f);
                    break;
                case 6:
                    Output[cid] %= value;
                    break;
                case 7:
                    Output[cid] = Mathf.Abs(value - Output[cid]);
                    break;
                default:
                    Output[cid] = value;
                    break;
            }
        }
        public void FiveDPC2D(Int3 id, float[] Output)
        {
            float value = 0;
            float div = 0;
            float scale = 1;
            int cid = id.x + id.y * dispatchThreadGroupCountX;
            Vector4 pos = (new Vector4(id.x, id.y, 0, 0) + position + ((distortSpace == 0) ? Vector4.zero : new Vector4(DistortionData[cid].x, DistortionData[cid].y, DistortionData[cid].z, 0))).Mul(size);
            for (int o = (int)octaveStartSize; o < octaveStartSize + octaveCount; o++)
            {
                scale = Mathf.Pow(Mathf.Abs(octaveStepPow), o);
                value += Mathf.Abs(CellNoiseFiveDPC(pos, scale) * Mathf.Pow(scale, octaveInfluenceBias) - ((choppy != 0) ? 0.5f : 0)) * ((choppy != 0) ? 2 : 1);
                div += Mathf.Pow(scale, octaveInfluenceBias);
            }
            value /= (div < 1) ? 1 : div;
            value = (turbulent != 0) ? Mathf.Abs((value - 0.5f) * 2) : value;
            value = (invert != 0) ? 1 - value : value;
            value = Mathf.Clamp(value * multiplyWith + addPostMultiply -
              ((planarize != 0) ? planarizeIntensity * (pos.y - planarizeAtY) : 0),
              clampEndResultMin, clampEndResultMax);

            switch (resultApplicationID)
            {
                case 1:
                    Output[cid] += value * weight;
                    break;
                case 2:
                    Output[cid] -= value * weight;
                    break;
                case 3:
                    Output[cid] *= value * weight;
                    break;
                case 4:
                    Output[cid] /= (value * weight == 0) ? 1 : value * weight;//do not divide by zero my friend
                    break;
                case 5:
                    Output[cid] = value * (.5f + weight * .5f) + Output[cid] * (.5f - weight * .5f);
                    break;
                case 6:
                    Output[cid] %= value;
                    break;
                case 7:
                    Output[cid] = Mathf.Abs(value - Output[cid]);
                    break;
                default:
                    Output[cid] = value;
                    break;
            }
        }
        public void SixDPC2D(Int3 id, float[] Output)
        {
            float value = 0;
            float div = 0;
            float scale = 1;
            int cid = id.x + id.y * dispatchThreadGroupCountX;
            Vector4 pos = (new Vector4(id.x, id.y, 0, 0) + position + ((distortSpace == 0) ? Vector4.zero : new Vector4(DistortionData[cid].x, DistortionData[cid].y, DistortionData[cid].z, 0))).Mul(size);
            for (int o = (int)octaveStartSize; o < octaveStartSize + octaveCount; o++)
            {
                scale = Mathf.Pow(Mathf.Abs(octaveStepPow), o);
                value += Mathf.Abs(CellNoiseSixDPC(pos, scale) * Mathf.Pow(scale, octaveInfluenceBias) - ((choppy != 0) ? 0.5f : 0)) * ((choppy != 0) ? 2 : 1);
                div += Mathf.Pow(scale, octaveInfluenceBias);
            }
            value /= (div < 1) ? 1 : div;
            value = (turbulent != 0) ? Mathf.Abs((value - 0.5f) * 2) : value;
            value = (invert != 0) ? 1 - value : value;
            value = Mathf.Clamp(value * multiplyWith + addPostMultiply -
              ((planarize != 0) ? planarizeIntensity * (pos.y - planarizeAtY) : 0),
              clampEndResultMin, clampEndResultMax);

            switch (resultApplicationID)
            {
                case 1:
                    Output[cid] += value * weight;
                    break;
                case 2:
                    Output[cid] -= value * weight;
                    break;
                case 3:
                    Output[cid] *= value * weight;
                    break;
                case 4:
                    Output[cid] /= (value * weight == 0) ? 1 : value * weight;//do not divide by zero my friend
                    break;
                case 5:
                    Output[cid] = value * (.5f + weight * .5f) + Output[cid] * (.5f - weight * .5f);
                    break;
                case 6:
                    Output[cid] %= value;
                    break;
                case 7:
                    Output[cid] = Mathf.Abs(value - Output[cid]);
                    break;
                default:
                    Output[cid] = value;
                    break;
            }
        }

        public void VNnearest3D(Int3 id, float[] Output)
        {
            float value = 0;
            float div = 0;
            float scale = 1;
            int cid = id.x + id.y * dispatchThreadGroupCountX + id.z * dispatchThreadGroupCountX * dispatchThreadGroupCountX;
            Vector4 pos = (new Vector4(id.x, id.y, id.z, 0) + position + ((distortSpace == 0) ? Vector4.zero : new Vector4(DistortionData[cid].x, DistortionData[cid].y, DistortionData[cid].z, 0))).Mul(size);
            for (int o = (int)octaveStartSize; o < octaveStartSize + octaveCount; o++)
            {
                scale = Mathf.Pow(Mathf.Abs(octaveStepPow), o);
                value += Mathf.Abs(ValueNoiseNearest(pos, scale) * Mathf.Pow(scale, octaveInfluenceBias) - ((choppy != 0) ? 0.5f : 0)) * ((choppy != 0) ? 2 : 1);
                div += Mathf.Pow(scale, octaveInfluenceBias);
            }
            value /= (div < 1) ? 1 : div;
            value = (turbulent != 0) ? Mathf.Abs((value - 0.5f) * 2) : value;
            value = (invert != 0) ? 1 - value : value;
            value = Mathf.Clamp(value * multiplyWith + addPostMultiply -
              ((planarize != 0) ? planarizeIntensity * (pos.y - planarizeAtY) : 0),
              clampEndResultMin, clampEndResultMax);

            switch (resultApplicationID)
            {
                case 1:
                    Output[cid] += value * weight;
                    break;
                case 2:
                    Output[cid] -= value * weight;
                    break;
                case 3:
                    Output[cid] *= value * weight;
                    break;
                case 4:
                    Output[cid] /= (value * weight == 0) ? 1 : value * weight;//do not divide by zero my friend
                    break;
                case 5:
                    Output[cid] = value * (.5f + weight * .5f) + Output[cid] * (.5f - weight * .5f);
                    break;
                case 6:
                    Output[cid] %= value;
                    break;
                case 7:
                    Output[cid] = Mathf.Abs(value - Output[cid]);
                    break;
                default:
                    Output[cid] = value;
                    break;
            }
        }
        public void VNlinear3D(Int3 id, float[] Output)
        {
            float value = 0;
            float div = 0;
            float scale = 1;
            int cid = id.x + id.y * dispatchThreadGroupCountX + id.z * dispatchThreadGroupCountX * dispatchThreadGroupCountX;
            Vector4 pos = (new Vector4(id.x, id.y, id.z, 0) + position + ((distortSpace == 0) ? Vector4.zero : new Vector4(DistortionData[cid].x, DistortionData[cid].y, DistortionData[cid].z, 0))).Mul(size);
            for (int o = (int)octaveStartSize; o < octaveStartSize + octaveCount; o++)
            {
                scale = Mathf.Pow(Mathf.Abs(octaveStepPow), o);
                value += Mathf.Abs(ValueNoiseLinear(pos, scale) * Mathf.Pow(scale, octaveInfluenceBias) - ((choppy != 0) ? 0.5f : 0)) * ((choppy != 0) ? 2 : 1);
                div += Mathf.Pow(scale, octaveInfluenceBias);
            }
            value /= (div < 1) ? 1 : div;
            value = (turbulent != 0) ? Mathf.Abs((value - 0.5f) * 2) : value;
            value = (invert != 0) ? 1 - value : value;
            value = Mathf.Clamp(value * multiplyWith + addPostMultiply -
              ((planarize != 0) ? planarizeIntensity * (pos.y - planarizeAtY) : 0),
              clampEndResultMin, clampEndResultMax);

            switch (resultApplicationID)
            {
                case 1:
                    Output[cid] += value * weight;
                    break;
                case 2:
                    Output[cid] -= value * weight;
                    break;
                case 3:
                    Output[cid] *= value * weight;
                    break;
                case 4:
                    Output[cid] /= (value * weight == 0) ? 1 : value * weight;//do not divide by zero my friend
                    break;
                case 5:
                    Output[cid] = value * (.5f + weight * .5f) + Output[cid] * (.5f - weight * .5f);
                    break;
                case 6:
                    Output[cid] %= value;
                    break;
                case 7:
                    Output[cid] = Mathf.Abs(value - Output[cid]);
                    break;
                default:
                    Output[cid] = value;
                    break;
            }
        }
        public void VNcosine3D(Int3 id, float[] Output)
        {
            float value = 0;
            float div = 0;
            float scale = 1;
            int cid = id.x + id.y * dispatchThreadGroupCountX + id.z * dispatchThreadGroupCountX * dispatchThreadGroupCountX;
            Vector4 pos = (new Vector4(id.x, id.y, id.z, 0) + position + ((distortSpace == 0) ? Vector4.zero : new Vector4(DistortionData[cid].x, DistortionData[cid].y, DistortionData[cid].z, 0))).Mul(size);
            for (int o = (int)octaveStartSize; o < octaveStartSize + octaveCount; o++)
            {
                scale = Mathf.Pow(Mathf.Abs(octaveStepPow), o);
                value += Mathf.Abs(ValueNoiseCosine(pos, scale) * Mathf.Pow(scale, octaveInfluenceBias) - ((choppy != 0) ? 0.5f : 0)) * ((choppy != 0) ? 2 : 1);
                div += Mathf.Pow(scale, octaveInfluenceBias);
            }
            value /= (div < 1) ? 1 : div;
            value = (turbulent != 0) ? Mathf.Abs((value - 0.5f) * 2) : value;
            value = (invert != 0) ? 1 - value : value;
            value = Mathf.Clamp(value * multiplyWith + addPostMultiply -
              ((planarize != 0) ? planarizeIntensity * (pos.y - planarizeAtY) : 0),
              clampEndResultMin, clampEndResultMax);

            switch (resultApplicationID)
            {
                case 1:
                    Output[cid] += value * weight;
                    break;
                case 2:
                    Output[cid] -= value * weight;
                    break;
                case 3:
                    Output[cid] *= value * weight;
                    break;
                case 4:
                    Output[cid] /= (value * weight == 0) ? 1 : value * weight;//do not divide by zero my friend
                    break;
                case 5:
                    Output[cid] = value * (.5f + weight * .5f) + Output[cid] * (.5f - weight * .5f);
                    break;
                case 6:
                    Output[cid] %= value;
                    break;
                case 7:
                    Output[cid] = Mathf.Abs(value - Output[cid]);
                    break;
                default:
                    Output[cid] = value;
                    break;
            }
        }
        public void VNcustomcosine3D(Int3 id, float[] Output)
        {
            float value = 0;
            float div = 0;
            float scale = 1;
            int cid = id.x + id.y * dispatchThreadGroupCountX + id.z * dispatchThreadGroupCountX * dispatchThreadGroupCountX;
            Vector4 pos = (new Vector4(id.x, id.y, id.z, 0) + position + ((distortSpace == 0) ? Vector4.zero : new Vector4(DistortionData[cid].x, DistortionData[cid].y, DistortionData[cid].z, 0))).Mul(size);
            for (int o = (int)octaveStartSize; o < octaveStartSize + octaveCount; o++)
            {
                scale = Mathf.Pow(Mathf.Abs(octaveStepPow), o);
                value += Mathf.Abs(ValueNoiseCustomCosine(pos, scale) * Mathf.Pow(scale, octaveInfluenceBias) - ((choppy != 0) ? 0.5f : 0)) * ((choppy != 0) ? 2 : 1);
                div += Mathf.Pow(scale, octaveInfluenceBias);
            }
            value /= (div < 1) ? 1 : div;
            value = (turbulent != 0) ? Mathf.Abs((value - 0.5f) * 2) : value;
            value = (invert != 0) ? 1 - value : value;
            value = Mathf.Clamp(value * multiplyWith + addPostMultiply -
              ((planarize != 0) ? planarizeIntensity * (pos.y - planarizeAtY) : 0),
              clampEndResultMin, clampEndResultMax);

            switch (resultApplicationID)
            {
                case 1:
                    Output[cid] += value * weight;
                    break;
                case 2:
                    Output[cid] -= value * weight;
                    break;
                case 3:
                    Output[cid] *= value * weight;
                    break;
                case 4:
                    Output[cid] /= (value * weight == 0) ? 1 : value * weight;//do not divide by zero my friend
                    break;
                case 5:
                    Output[cid] = value * (.5f + weight * .5f) + Output[cid] * (.5f - weight * .5f);
                    break;
                case 6:
                    Output[cid] %= value;
                    break;
                case 7:
                    Output[cid] = Mathf.Abs(value - Output[cid]);
                    break;
                default:
                    Output[cid] = value;
                    break;
            }
        }
        public void VNcubic3D(Int3 id, float[] Output)
        {
            float value = 0;
            float div = 0;
            float scale = 1;
            int cid = id.x + id.y * dispatchThreadGroupCountX + id.z * dispatchThreadGroupCountX * dispatchThreadGroupCountX;
            Vector4 pos = (new Vector4(id.x, id.y, id.z, 0) + position + ((distortSpace == 0) ? Vector4.zero : new Vector4(DistortionData[cid].x, DistortionData[cid].y, DistortionData[cid].z, 0))).Mul(size);
            for (int o = (int)octaveStartSize; o < octaveStartSize + octaveCount; o++)
            {
                scale = Mathf.Pow(Mathf.Abs(octaveStepPow), o);
                value += Mathf.Abs(ValueNoiseCubic(pos, scale) * Mathf.Pow(scale, octaveInfluenceBias) - ((choppy != 0) ? 0.5f : 0)) * ((choppy != 0) ? 2 : 1);
                div += Mathf.Pow(scale, octaveInfluenceBias);
            }
            value /= (div < 1) ? 1 : div;
            value = (turbulent != 0) ? Mathf.Abs((value - 0.5f) * 2) : value;
            value = (invert != 0) ? 1 - value : value;
            value = Mathf.Clamp(value * multiplyWith + addPostMultiply -
              ((planarize != 0) ? planarizeIntensity * (pos.y - planarizeAtY) : 0),
              clampEndResultMin, clampEndResultMax);

            switch (resultApplicationID)
            {
                case 1:
                    Output[cid] += value * weight;
                    break;
                case 2:
                    Output[cid] -= value * weight;
                    break;
                case 3:
                    Output[cid] *= value * weight;
                    break;
                case 4:
                    Output[cid] /= (value * weight == 0) ? 1 : value * weight;//do not divide by zero my friend
                    break;
                case 5:
                    Output[cid] = value * (.5f + weight * .5f) + Output[cid] * (.5f - weight * .5f);
                    break;
                case 6:
                    Output[cid] %= value;
                    break;
                case 7:
                    Output[cid] = Mathf.Abs(value - Output[cid]);
                    break;
                default:
                    Output[cid] = value;
                    break;
            }
        }
        public void VNcustomhermite3D(Int3 id, float[] Output)
        {
            float value = 0;
            float div = 0;
            float scale = 1;
            int cid = id.x + id.y * dispatchThreadGroupCountX + id.z * dispatchThreadGroupCountX * dispatchThreadGroupCountX;
            Vector4 pos = (new Vector4(id.x, id.y, id.z, 0) + position + ((distortSpace == 0) ? Vector4.zero : new Vector4(DistortionData[cid].x, DistortionData[cid].y, DistortionData[cid].z, 0))).Mul(size);
            for (int o = (int)octaveStartSize; o < octaveStartSize + octaveCount; o++)
            {
                scale = Mathf.Pow(Mathf.Abs(octaveStepPow), o);
                value += Mathf.Abs(ValueNoiseCustomHermite(pos, scale) * Mathf.Pow(scale, octaveInfluenceBias) - ((choppy != 0) ? 0.5f : 0)) * ((choppy != 0) ? 2 : 1);
                div += Mathf.Pow(scale, octaveInfluenceBias);
            }
            value /= (div < 1) ? 1 : div;
            value = (turbulent != 0) ? Mathf.Abs((value - 0.5f) * 2) : value;
            value = (invert != 0) ? 1 - value : value;
            value = Mathf.Clamp(value * multiplyWith + addPostMultiply -
              ((planarize != 0) ? planarizeIntensity * (pos.y - planarizeAtY) : 0),
              clampEndResultMin, clampEndResultMax);

            switch (resultApplicationID)
            {
                case 1:
                    Output[cid] += value * weight;
                    break;
                case 2:
                    Output[cid] -= value * weight;
                    break;
                case 3:
                    Output[cid] *= value * weight;
                    break;
                case 4:
                    Output[cid] /= (value * weight == 0) ? 1 : value * weight;//do not divide by zero my friend
                    break;
                case 5:
                    Output[cid] = value * (.5f + weight * .5f) + Output[cid] * (.5f - weight * .5f);
                    break;
                case 6:
                    Output[cid] %= value;
                    break;
                case 7:
                    Output[cid] = Mathf.Abs(value - Output[cid]);
                    break;
                default:
                    Output[cid] = value;
                    break;
            }
        }

        public void VNnearest2D(Int3 id, float[] Output)
        {
            float value = 0;
            float div = 0;
            float scale = 1;
            int cid = id.x + id.y * dispatchThreadGroupCountX;
            Vector4 pos = (new Vector4(id.x, id.y, 0, 0) + position + ((distortSpace == 0) ? Vector4.zero : new Vector4(DistortionData[cid].x, DistortionData[cid].y, DistortionData[cid].z, 0))).Mul(size);
            for (int o = (int)octaveStartSize; o < octaveStartSize + octaveCount; o++)
            {
                scale = Mathf.Pow(Mathf.Abs(octaveStepPow), o);
                value += Mathf.Abs(ValueNoiseNearest(pos, scale) * Mathf.Pow(scale, octaveInfluenceBias) - ((choppy != 0) ? 0.5f : 0)) * ((choppy != 0) ? 2 : 1);
                div += Mathf.Pow(scale, octaveInfluenceBias);
            }
            value /= (div < 1) ? 1 : div;
            value = (turbulent != 0) ? Mathf.Abs((value - 0.5f) * 2) : value;
            value = (invert != 0) ? 1 - value : value;
            value = Mathf.Clamp(value * multiplyWith + addPostMultiply -
              ((planarize != 0) ? planarizeIntensity * (pos.y - planarizeAtY) : 0),
              clampEndResultMin, clampEndResultMax);

            switch (resultApplicationID)
            {
                case 1:
                    Output[cid] += value * weight;
                    break;
                case 2:
                    Output[cid] -= value * weight;
                    break;
                case 3:
                    Output[cid] *= value * weight;
                    break;
                case 4:
                    Output[cid] /= (value * weight == 0) ? 1 : value * weight;//do not divide by zero my friend
                    break;
                case 5:
                    Output[cid] = value * (.5f + weight * .5f) + Output[cid] * (.5f - weight * .5f);
                    break;
                case 6:
                    Output[cid] %= value;
                    break;
                case 7:
                    Output[cid] = Mathf.Abs(value - Output[cid]);
                    break;
                default:
                    Output[cid] = value;
                    break;
            }
        }
        public void VNlinear2D(Int3 id, float[] Output)
        {
            float value = 0;
            float div = 0;
            float scale = 1;
            int cid = id.x + id.y * dispatchThreadGroupCountX;
            Vector4 pos = (new Vector4(id.x, id.y, 0, 0) + position + ((distortSpace == 0) ? Vector4.zero : new Vector4(DistortionData[cid].x, DistortionData[cid].y, DistortionData[cid].z, 0))).Mul(size);
            for (int o = (int)octaveStartSize; o < octaveStartSize + octaveCount; o++)
            {
                scale = Mathf.Pow(Mathf.Abs(octaveStepPow), o);
                value += Mathf.Abs(ValueNoiseLinear(pos, scale) * Mathf.Pow(scale, octaveInfluenceBias) - ((choppy != 0) ? 0.5f : 0)) * ((choppy != 0) ? 2 : 1);
                div += Mathf.Pow(scale, octaveInfluenceBias);
            }
            value /= (div < 1) ? 1 : div;
            value = (turbulent != 0) ? Mathf.Abs((value - 0.5f) * 2) : value;
            value = (invert != 0) ? 1 - value : value;
            value = Mathf.Clamp(value * multiplyWith + addPostMultiply -
              ((planarize != 0) ? planarizeIntensity * (pos.y - planarizeAtY) : 0),
              clampEndResultMin, clampEndResultMax);

            switch (resultApplicationID)
            {
                case 1:
                    Output[cid] += value * weight;
                    break;
                case 2:
                    Output[cid] -= value * weight;
                    break;
                case 3:
                    Output[cid] *= value * weight;
                    break;
                case 4:
                    Output[cid] /= (value * weight == 0) ? 1 : value * weight;//do not divide by zero my friend
                    break;
                case 5:
                    Output[cid] = value * (.5f + weight * .5f) + Output[cid] * (.5f - weight * .5f);
                    break;
                case 6:
                    Output[cid] %= value;
                    break;
                case 7:
                    Output[cid] = Mathf.Abs(value - Output[cid]);
                    break;
                default:
                    Output[cid] = value;
                    break;
            }
        }
        public void VNcosine2D(Int3 id, float[] Output)
        {
            float value = 0;
            float div = 0;
            float scale = 1;
            int cid = id.x + id.y * dispatchThreadGroupCountX;
            Vector4 pos = (new Vector4(id.x, id.y, 0, 0) + position + ((distortSpace == 0) ? Vector4.zero : new Vector4(DistortionData[cid].x, DistortionData[cid].y, DistortionData[cid].z, 0))).Mul(size);
            for (int o = (int)octaveStartSize; o < octaveStartSize + octaveCount; o++)
            {
                scale = Mathf.Pow(Mathf.Abs(octaveStepPow), o);
                value += Mathf.Abs(ValueNoiseCosine(pos, scale) * Mathf.Pow(scale, octaveInfluenceBias) - ((choppy != 0) ? 0.5f : 0)) * ((choppy != 0) ? 2 : 1);
                div += Mathf.Pow(scale, octaveInfluenceBias);
            }
            value /= (div < 1) ? 1 : div;
            value = (turbulent != 0) ? Mathf.Abs((value - 0.5f) * 2) : value;
            value = (invert != 0) ? 1 - value : value;
            value = Mathf.Clamp(value * multiplyWith + addPostMultiply -
              ((planarize != 0) ? planarizeIntensity * (pos.y - planarizeAtY) : 0),
              clampEndResultMin, clampEndResultMax);

            switch (resultApplicationID)
            {
                case 1:
                    Output[cid] += value * weight;
                    break;
                case 2:
                    Output[cid] -= value * weight;
                    break;
                case 3:
                    Output[cid] *= value * weight;
                    break;
                case 4:
                    Output[cid] /= (value * weight == 0) ? 1 : value * weight;//do not divide by zero my friend
                    break;
                case 5:
                    Output[cid] = value * (.5f + weight * .5f) + Output[cid] * (.5f - weight * .5f);
                    break;
                case 6:
                    Output[cid] %= value;
                    break;
                case 7:
                    Output[cid] = Mathf.Abs(value - Output[cid]);
                    break;
                default:
                    Output[cid] = value;
                    break;
            }
        }
        public void VNcustomcosine2D(Int3 id, float[] Output)
        {
            float value = 0;
            float div = 0;
            float scale = 1;
            int cid = id.x + id.y * dispatchThreadGroupCountX;
            Vector4 pos = (new Vector4(id.x, id.y, 0, 0) + position + ((distortSpace == 0) ? Vector4.zero : new Vector4(DistortionData[cid].x, DistortionData[cid].y, DistortionData[cid].z, 0))).Mul(size);
            for (int o = (int)octaveStartSize; o < octaveStartSize + octaveCount; o++)
            {
                scale = Mathf.Pow(Mathf.Abs(octaveStepPow), o);
                value += Mathf.Abs(ValueNoiseCustomCosine(pos, scale) * Mathf.Pow(scale, octaveInfluenceBias) - ((choppy != 0) ? 0.5f : 0)) * ((choppy != 0) ? 2 : 1);
                div += Mathf.Pow(scale, octaveInfluenceBias);
            }
            value /= (div < 1) ? 1 : div;
            value = (turbulent != 0) ? Mathf.Abs((value - 0.5f) * 2) : value;
            value = (invert != 0) ? 1 - value : value;
            value = Mathf.Clamp(value * multiplyWith + addPostMultiply -
              ((planarize != 0) ? planarizeIntensity * (pos.y - planarizeAtY) : 0),
              clampEndResultMin, clampEndResultMax);

            switch (resultApplicationID)
            {
                case 1:
                    Output[cid] += value * weight;
                    break;
                case 2:
                    Output[cid] -= value * weight;
                    break;
                case 3:
                    Output[cid] *= value * weight;
                    break;
                case 4:
                    Output[cid] /= (value * weight == 0) ? 1 : value * weight;//do not divide by zero my friend
                    break;
                case 5:
                    Output[cid] = value * (.5f + weight * .5f) + Output[cid] * (.5f - weight * .5f);
                    break;
                case 6:
                    Output[cid] %= value;
                    break;
                case 7:
                    Output[cid] = Mathf.Abs(value - Output[cid]);
                    break;
                default:
                    Output[cid] = value;
                    break;
            }
        }
        public void VNcubic2D(Int3 id, float[] Output)
        {
            float value = 0;
            float div = 0;
            float scale = 1;
            int cid = id.x + id.y * dispatchThreadGroupCountX;
            Vector4 pos = (new Vector4(id.x, id.y, 0, 0) + position + ((distortSpace == 0) ? Vector4.zero : new Vector4(DistortionData[cid].x, DistortionData[cid].y, DistortionData[cid].z, 0))).Mul(size);
            for (int o = (int)octaveStartSize; o < octaveStartSize + octaveCount; o++)
            {
                scale = Mathf.Pow(Mathf.Abs(octaveStepPow), o);
                value += Mathf.Abs(ValueNoiseCubic(pos, scale) * Mathf.Pow(scale, octaveInfluenceBias) - ((choppy != 0) ? 0.5f : 0)) * ((choppy != 0) ? 2 : 1);
                div += Mathf.Pow(scale, octaveInfluenceBias);
            }
            value /= (div < 1) ? 1 : div;
            value = (turbulent != 0) ? Mathf.Abs((value - 0.5f) * 2) : value;
            value = (invert != 0) ? 1 - value : value;
            value = Mathf.Clamp(value * multiplyWith + addPostMultiply -
              ((planarize != 0) ? planarizeIntensity * (pos.y - planarizeAtY) : 0),
              clampEndResultMin, clampEndResultMax);

            switch (resultApplicationID)
            {
                case 1:
                    Output[cid] += value * weight;
                    break;
                case 2:
                    Output[cid] -= value * weight;
                    break;
                case 3:
                    Output[cid] *= value * weight;
                    break;
                case 4:
                    Output[cid] /= (value * weight == 0) ? 1 : value * weight;//do not divide by zero my friend
                    break;
                case 5:
                    Output[cid] = value * (.5f + weight * .5f) + Output[cid] * (.5f - weight * .5f);
                    break;
                case 6:
                    Output[cid] %= value;
                    break;
                case 7:
                    Output[cid] = Mathf.Abs(value - Output[cid]);
                    break;
                default:
                    Output[cid] = value;
                    break;
            }
        }
        public void VNcustomhermite2D(Int3 id, float[] Output)
        {
            float value = 0;
            float div = 0;
            float scale = 1;
            int cid = id.x + id.y * dispatchThreadGroupCountX;
            Vector4 pos = (new Vector4(id.x, id.y, 0, 0) + position + ((distortSpace == 0) ? Vector4.zero : new Vector4(DistortionData[cid].x, DistortionData[cid].y, DistortionData[cid].z, 0))).Mul(size);
            for (int o = (int)octaveStartSize; o < octaveStartSize + octaveCount; o++)
            {
                scale = Mathf.Pow(Mathf.Abs(octaveStepPow), o);
                value += Mathf.Abs(ValueNoiseCustomHermite(pos, scale) * Mathf.Pow(scale, octaveInfluenceBias) - ((choppy != 0) ? 0.5f : 0)) * ((choppy != 0) ? 2 : 1);
                div += Mathf.Pow(scale, octaveInfluenceBias);
            }
            value /= (div < 1) ? 1 : div;
            value = (turbulent != 0) ? Mathf.Abs((value - 0.5f) * 2) : value;
            value = (invert != 0) ? 1 - value : value;
            value = Mathf.Clamp(value * multiplyWith + addPostMultiply -
              ((planarize != 0) ? planarizeIntensity * (pos.y - planarizeAtY) : 0),
              clampEndResultMin, clampEndResultMax);

            switch (resultApplicationID)
            {
                case 1:
                    Output[cid] += value * weight;
                    break;
                case 2:
                    Output[cid] -= value * weight;
                    break;
                case 3:
                    Output[cid] *= value * weight;
                    break;
                case 4:
                    Output[cid] /= (value * weight == 0) ? 1 : value * weight;//do not divide by zero my friend
                    break;
                case 5:
                    Output[cid] = value * (.5f + weight * .5f) + Output[cid] * (.5f - weight * .5f);
                    break;
                case 6:
                    Output[cid] %= value;
                    break;
                case 7:
                    Output[cid] = Mathf.Abs(value - Output[cid]);
                    break;
                default:
                    Output[cid] = value;
                    break;
            }
        }





        public float ValueNoiseNearest(Vector4 pos, float size)
        {

            pos = Rotate(pos);

            Vector4 o = GridFloor(pos, size); //original in size space
            Vector4 p = o * size; //stepped in real space
            Vector4 r = (pos - p) / size; //0-1 pos within step

            float[] points = new float[16];
            for (int w = 0; w <= 1; w++)
            {
                for (int z = 0; z <= 1; z++)
                {
                    for (int y = 0; y <= 1; y++)
                    {
                        for (int x = 0; x <= 1; x++)
                        {
                            int id = x + y * 2 + z * 4 + w * 8;
                            points[id] = Hash2Float(
                         ((noiseDimensionCount > 0) ? Hash1(p.x + x * size) : 1) *
                              ((noiseDimensionCount > 1) ? Hash2(p.y + y * size) : 1) *
                              ((noiseDimensionCount > 2) ? Hash3(p.z + z * size) : 1) *
                              ((noiseDimensionCount > 3) ? Hash4(p.w + w * size) : 1));
                        }
                    }
                }
            }
            int i = 0;
            for (i = 0; i < 8; i++)
            {
                points[i * 2] = InterpolateNearestNeighbor(points[i * 2], points[i * 2 + 1], r.x);
            }
            for (i = 0; i < 4; i++)
            {
                points[i * 4] = InterpolateNearestNeighbor(points[i * 4], points[i * 4 + 2], r.y);
            }
            for (i = 0; i < 2; i++)
            {
                points[i * 8] = InterpolateNearestNeighbor(points[i * 8], points[i * 8 + 4], r.z);
            }
            return InterpolateNearestNeighbor(points[0], points[8], r.w);
        }


        public float ValueNoiseLinear(Vector4 pos, float size)
        {

            pos = Rotate(pos);

            Vector4 o = GridFloor(pos, size); //original in size space
            Vector4 p = o * size; //stepped in real space
            Vector4 r = (pos - p) / size; //0-1 pos within step

            float[] points = new float[16];
            for (int w = 0; w <= 1; w++)
            {
                for (int z = 0; z <= 1; z++)
                {
                    for (int y = 0; y <= 1; y++)
                    {
                        for (int x = 0; x <= 1; x++)
                        {
                            int id = x + y * 2 + z * 4 + w * 8;
                            points[id] = Hash2Float(
                         ((noiseDimensionCount > 0) ? Hash1(p.x + x * size) : 1) *
                              ((noiseDimensionCount > 1) ? Hash2(p.y + y * size) : 1) *
                              ((noiseDimensionCount > 2) ? Hash3(p.z + z * size) : 1) *
                              ((noiseDimensionCount > 3) ? Hash4(p.w + w * size) : 1));
                        }
                    }
                }
            }
            int i = 0;

            for (i = 0; i < 8; i++)
            {
                points[i * 2] = InterpolateLinear(points[i * 2], points[i * 2 + 1], r.x);
            }
            for (i = 0; i < 4; i++)
            {
                points[i * 4] = InterpolateLinear(points[i * 4], points[i * 4 + 2], r.y);
            }
            for (i = 0; i < 2; i++)
            {
                points[i * 8] = InterpolateLinear(points[i * 8], points[i * 8 + 4], r.z);
            }
            return InterpolateLinear(points[0], points[8], r.w);
        }


        public float ValueNoiseCosine(Vector4 pos, float size)
        {

            pos = Rotate(pos);

            Vector4 o = GridFloor(pos, size); //original in size space
            Vector4 p = o * size; //stepped in world space
            Vector4 r = (pos - p) / size; //0-1 pos within step

            float[] points = new float[16];
            for (int w = 0; w <= 1; w++)
            {
                for (int z = 0; z <= 1; z++)
                {
                    for (int y = 0; y <= 1; y++)
                    {
                        for (int x = 0; x <= 1; x++)
                        {
                            int id = x + y * 2 + z * 4 + w * 8;
                            points[id] = Hash2Float(
                         ((noiseDimensionCount > 0) ? Hash1(p.x + x * size) : 1) *
                              ((noiseDimensionCount > 1) ? Hash2(p.y + y * size) : 1) *
                              ((noiseDimensionCount > 2) ? Hash3(p.z + z * size) : 1) *
                              ((noiseDimensionCount > 3) ? Hash4(p.w + w * size) : 1));
                        }
                    }
                }
            }
            int i = 0;

            for (i = 0; i < 8; i++)
            {
                points[i * 2] = InterpolateCosine(points[i * 2], points[i * 2 + 1], r.x);
            }
            for (i = 0; i < 4; i++)
            {
                points[i * 4] = InterpolateCosine(points[i * 4], points[i * 4 + 2], r.y);
            }
            for (i = 0; i < 2; i++)
            {
                points[i * 8] = InterpolateCosine(points[i * 8], points[i * 8 + 4], r.z);
            }
            return InterpolateCosine(points[0], points[8], r.w);
        }


        public float ValueNoiseCustomCosine(Vector4 pos, float size)
        {

            pos = Rotate(pos);

            Vector4 o = GridFloor(pos, size); //original in size space
            Vector4 p = o * size; //stepped in world space
            Vector4 r = (pos - p) / size; //0-1 pos within step

            float[] points = new float[16];
            for (int w = 0; w <= 1; w++)
            {
                for (int z = 0; z <= 1; z++)
                {
                    for (int y = 0; y <= 1; y++)
                    {
                        for (int x = 0; x <= 1; x++)
                        {
                            int id = x + y * 2 + z * 4 + w * 8;
                            points[id] = Hash2Float(
                         ((noiseDimensionCount > 0) ? Hash1(p.x + x * size) : 1) *
                              ((noiseDimensionCount > 1) ? Hash2(p.y + y * size) : 1) *
                              ((noiseDimensionCount > 2) ? Hash3(p.z + z * size) : 1) *
                              ((noiseDimensionCount > 3) ? Hash4(p.w + w * size) : 1));
                        }
                    }
                }
            }
            int i = 0;


            for (i = 0; i < 8; i++)
            {
                points[i * 2] = InterpolateCosinish(points[i * 2], points[i * 2 + 1], r.x);
            }
            for (i = 0; i < 4; i++)
            {
                points[i * 4] = InterpolateCosinish(points[i * 4], points[i * 4 + 2], r.y);
            }
            for (i = 0; i < 2; i++)
            {
                points[i * 8] = InterpolateCosinish(points[i * 8], points[i * 8 + 4], r.z);
            }
            return InterpolateCosinish(points[0], points[8], r.w);
        }


        public float ValueNoiseCubic(Vector4 pos, float size)
        {

            pos = Rotate(pos);

            Vector4 o = GridFloor(pos, size); //original in size space
            Vector4 p = o * size; //stepped in world space
            Vector4 r = (pos - p) / size; //0-1 pos within step

            float[] points = new float[256];
            for (int w = -1; w <= 2; w++)
            {
                for (int z = -1; z <= 2; z++)
                {
                    for (int y = -1; y <= 2; y++)
                    {
                        for (int x = -1; x <= 2; x++)
                        {
                            int id = (x + 1) + (y + 1) * 4 + (z + 1) * 16 + (w + 1) * 64;
                            points[id] = Hash2Float(
                         ((noiseDimensionCount > 0) ? Hash1(p.x + x * size) : 1) *
                              ((noiseDimensionCount > 1) ? Hash2(p.y + y * size) : 1) *
                              ((noiseDimensionCount > 2) ? Hash3(p.z + z * size) : 1) *
                              ((noiseDimensionCount > 3) ? Hash4(p.w + w * size) : 1));
                        }
                    }
                }
            }
            int i = 0;

            for (i = 0; i < 64; i++)
            {
                points[i * 4] = InterpolateCubic(points[i * 4], points[i * 4 + 1], points[i * 4 + 2], points[i * 4 + 3], r.x);
            }
            for (i = 0; i < 16; i++)
            {
                points[i * 16] = InterpolateCubic(points[i * 16], points[i * 16 + 4], points[i * 16 + 8], points[i * 16 + 12], r.y);
            }
            for (i = 0; i < 4; i++)
            {
                points[i * 64] = InterpolateCubic(points[i * 64], points[i * 64 + 16], points[i * 64 + 32], points[i * 64 + 48], r.z);
            }
            return InterpolateCubic(points[0], points[64], points[128], points[192], r.w);
        }



        public float ValueNoiseCustomHermite(Vector4 pos, float size)
        {

            pos = Rotate(pos);

            Vector4 o = GridFloor(pos, size); //original in size space
            Vector4 p = o * size; //stepped in world space
            Vector4 r = (pos - p) / size; //0-1 pos within step

            float[] points = new float[256];
            for (int w = -1; w <= 2; w++)
            {
                for (int z = -1; z <= 2; z++)
                {
                    for (int y = -1; y <= 2; y++)
                    {
                        for (int x = -1; x <= 2; x++)
                        {
                            int id = (x + 1) + (y + 1) * 4 + (z + 1) * 16 + (w + 1) * 64;

                            Vector4 pp = new Vector4(p.x + x * size, p.y + y * size, p.z + z * size, p.w + w * size);

                            //  pp= Rotate(pp);

                            points[id] = Hash2Float(
                         ((noiseDimensionCount > 0) ? Hash1(pp.x) : 1) *
                              ((noiseDimensionCount > 1) ? Hash2(pp.y) : 1) *
                              ((noiseDimensionCount > 2) ? Hash3(pp.z) : 1) *
                              ((noiseDimensionCount > 3) ? Hash4(pp.w) : 1));
                        }
                    }
                }
            }

            int i = 0;
            for (i = 0; i < 64; i++)
            {
                points[i * 4] = InterpolateTensionedHermite(points[i * 4], points[i * 4 + 1], points[i * 4 + 2], points[i * 4 + 3], r.x);
            }
            for (i = 0; i < 16; i++)
            {
                points[i * 16] = InterpolateTensionedHermite(points[i * 16], points[i * 16 + 4], points[i * 16 + 8], points[i * 16 + 12], r.y);
            }
            for (i = 0; i < 4; i++)
            {
                points[i * 64] = InterpolateTensionedHermite(points[i * 64], points[i * 64 + 16], points[i * 64 + 32], points[i * 64 + 48], r.z);
            }
            return InterpolateTensionedHermite(points[0], points[64], points[128], points[192], r.w);
        }




        public float CellNoiseOneDPC(Vector4 pos, float size)
        {

            pos = Rotate(pos);

            Vector4 o = GridFloor(pos, size); //original in size space
            Vector4 p = o * size; //stepped in real space
            Vector4 r = (pos - p) / size; //0-1 pos within step
                                          //posXYZ&Value now using 4 points per cell
            Vector4[] points = new Vector4[3 * 3 * 3 * 3];
            //*
            for (int w = -1; w < 2; w++)
            {
                for (int z = -1; z < 2; z++)
                {
                    for (int y = -1; y < 2; y++)
                    {
                        for (int x = -1; x < 2; x++)
                        {
                            int id = (x + 1) + (y + 1) * 3 + (z + 1) * 9 + (w + 1) * 27;

                            Vector4 offset = p + new Vector4(x, y, z, w) * size;

                            points[id] = new Vector4(
               ((noiseDimensionCount > 0) ? offset.x + Hash2Float(Hash0(offset.x) * Hash1(-offset.y) + Hash2(offset.z) + Hash3(offset.w)) * size : 0),
                 ((noiseDimensionCount > 1) ? offset.y + Hash2Float(Hash3(offset.y) * Hash4(-offset.z) + Hash5(offset.x) + Hash6(offset.w)) * size : 0),
                 ((noiseDimensionCount > 2) ? offset.z + Hash2Float(Hash6(offset.z) * Hash7(-offset.x) + Hash0(offset.y) + Hash1(offset.w)) * size : 0),
                 ((noiseDimensionCount > 3) ? offset.w + Hash2Float(Hash1(offset.z) * Hash2(-offset.x) + Hash3(offset.y) + Hash4(offset.w)) * size : 0))
                                ;

                        }
                    }
                }
            }

            float min0 = 10000;
            float min1 = 10000;
            float min2 = 10000;
            float min3 = 10000;
            float current = 0;
            Vector4 minP = new Vector4(-100000, -100000, -100000, -100000);

            Vector4 comparerPos = new Vector4(
    ((noiseDimensionCount > 0) ? pos.x : 0),
    ((noiseDimensionCount > 1) ? pos.y : 0),
    ((noiseDimensionCount > 2) ? pos.z : 0),
    ((noiseDimensionCount > 3) ? pos.w : 0));

            for (int i = 0; i < 3 * 3 * 3 * 3; i++)
            {
                current = Vector4.Distance(points[i], comparerPos);
                if (current < min0)
                {
                    min3 = min2;
                    min2 = min1;
                    min1 = min0;
                    min0 = current;
                    minP = points[i];
                }
            }

            //better return all the mins in vector form plus the final point_in_Biome_Value_Space and then switch on demand last.
            switch (cellularMethodID)
            {
                case 0:
                    return Hash2Float(Hash0(minP.x) * Hash1(-minP.y) + Hash2(minP.z) + Hash3(minP.w));
                case 2:
                    return 1 - Vector4.Distance(pos, minP) / size;
                case 3:
                    return Mathf.Abs(min1 - min0);
                case 4:
                    return Mathf.Abs(min2 - min1) + Mathf.Abs(min1 - min0) + Mathf.Abs(min0 - min2);
                case 5:
                    return Mathf.Abs(min2 - min1) + Mathf.Abs(min1 - min0) + Mathf.Abs(min0 - min2) + Mathf.Abs(min3 - min0) + Mathf.Abs(min3 - min1) + Mathf.Abs(min3 - min2);
                default:
                    return Vector4.Distance(pos, minP) / size;
            }
        }


        public float CellNoiseTwoDPC(Vector4 pos, float size)
        {

            pos = Rotate(pos);

            Vector4 o = GridFloor(pos, size); //original in size space
            Vector4 p = o * size; //stepped in real space
            Vector4 r = (pos - p) / size; //0-1 pos within step
                                          //posXYZ&Value now using 4 points per cell
            Vector4[] points = new Vector4[3 * 3 * 3 * 3 * 2];
            //*
            for (int w = -1; w < 2; w++)
            {
                for (int z = -1; z < 2; z++)
                {
                    for (int y = -1; y < 2; y++)
                    {
                        for (int x = -1; x < 2; x++)
                        {
                            int id = (x + 1) + (y + 1) * 3 + (z + 1) * 9 + (w + 1) * 27;

                            Vector4 offset = p + new Vector4(x, y, z, w) * size;

                            points[id * 2] = new Vector4(
               ((noiseDimensionCount > 0) ? offset.x + Hash2Float(Hash0(offset.x) * Hash1(-offset.y) + Hash2(offset.z) + Hash3(offset.w)) * size : 0),
                 ((noiseDimensionCount > 1) ? offset.y + Hash2Float(Hash3(offset.y) * Hash4(-offset.z) + Hash5(offset.x) + Hash6(offset.w)) * size : 0),
                 ((noiseDimensionCount > 2) ? offset.z + Hash2Float(Hash6(offset.z) * Hash7(-offset.x) + Hash0(offset.y) + Hash1(offset.w)) * size : 0),
                 ((noiseDimensionCount > 3) ? offset.w + Hash2Float(Hash1(offset.z) * Hash2(-offset.x) + Hash3(offset.y) + Hash4(offset.w)) * size : 0))
                                ;

                            points[id * 2 + 1] = new Vector4(
        ((noiseDimensionCount > 0) ? offset.x + Hash2Float(Hash1(offset.x) * Hash2(-offset.y) + Hash3(offset.z) + Hash4(offset.w)) * size : 0),
        ((noiseDimensionCount > 1) ? offset.y + Hash2Float(Hash0(offset.y) * Hash1(-offset.z) + Hash2(offset.x) + Hash3(offset.w)) * size : 0),
         ((noiseDimensionCount > 2) ? offset.z + Hash2Float(Hash3(offset.z) * Hash4(-offset.x) + Hash5(offset.y) + Hash6(offset.w)) * size : 0),
          ((noiseDimensionCount > 3) ? offset.w + Hash2Float(Hash6(offset.z) * Hash7(-offset.x) + Hash0(offset.y) + Hash1(offset.w)) * size : 0))
                        ;

                        }
                    }
                }
            }

            float min0 = 10000;
            float min1 = 10000;
            float min2 = 10000;
            float min3 = 10000;
            float current = 0;
            Vector4 minP = new Vector4(-100000, -100000, -100000, -100000);

            Vector4 comparerPos = new Vector4(
    ((noiseDimensionCount > 0) ? pos.x : 0),
    ((noiseDimensionCount > 1) ? pos.y : 0),
    ((noiseDimensionCount > 2) ? pos.z : 0),
    ((noiseDimensionCount > 3) ? pos.w : 0));

            for (int i = 0; i < 3 * 3 * 3 * 3 * 2; i++)
            {
                current = Vector4.Distance(points[i], comparerPos);
                if (current < min0)
                {
                    min3 = min2;
                    min2 = min1;
                    min1 = min0;
                    min0 = current;
                    minP = points[i];
                }
            }

            //better return all the mins in vector form plus the final point_in_Biome_Value_Space and then switch on demand last.

            switch (cellularMethodID)
            {
                case 0:
                    return Hash2Float(Hash0(minP.x) * Hash1(-minP.y) + Hash2(minP.z) + Hash3(minP.w));
                case 2:
                    return 1 - Vector4.Distance(pos, minP) / size;
                case 3:
                    return Mathf.Abs(min1 - min0);
                case 4:
                    return Mathf.Abs(min2 - min1) + Mathf.Abs(min1 - min0) + Mathf.Abs(min0 - min2);
                case 5:
                    return Mathf.Abs(min2 - min1) + Mathf.Abs(min1 - min0) + Mathf.Abs(min0 - min2) + Mathf.Abs(min3 - min0) + Mathf.Abs(min3 - min1) + Mathf.Abs(min3 - min2);
                default:
                    return Vector4.Distance(pos, minP) / size;
            }
        }


        public float CellNoiseThreeDPC(Vector4 pos, float size)
        {

            pos = Rotate(pos);

            Vector4 o = GridFloor(pos, size); //original in size space
            Vector4 p = o * size; //stepped in real space
            Vector4 r = (pos - p) / size; //0-1 pos within step
                                          //posXYZ&Value now using 4 points per cell
            Vector4[] points = new Vector4[3 * 3 * 3 * 3 * 3];
            //*
            for (int w = -1; w < 2; w++)
            {
                for (int z = -1; z < 2; z++)
                {
                    for (int y = -1; y < 2; y++)
                    {
                        for (int x = -1; x < 2; x++)
                        {
                            int id = (x + 1) + (y + 1) * 3 + (z + 1) * 9 + (w + 1) * 27;

                            Vector4 offset = p + new Vector4(x, y, z, w) * size;

                            points[id * 3] = new Vector4(
               ((noiseDimensionCount > 0) ? offset.x + Hash2Float(Hash0(offset.x) * Hash1(-offset.y) + Hash2(offset.z) + Hash3(offset.w)) * size : 0),
                 ((noiseDimensionCount > 1) ? offset.y + Hash2Float(Hash3(offset.y) * Hash4(-offset.z) + Hash5(offset.x) + Hash6(offset.w)) * size : 0),
                 ((noiseDimensionCount > 2) ? offset.z + Hash2Float(Hash6(offset.z) * Hash7(-offset.x) + Hash0(offset.y) + Hash1(offset.w)) * size : 0),
                 ((noiseDimensionCount > 3) ? offset.w + Hash2Float(Hash1(offset.z) * Hash2(-offset.x) + Hash3(offset.y) + Hash4(offset.w)) * size : 0))
                                ;

                            points[id * 3 + 1] = new Vector4(
        ((noiseDimensionCount > 0) ? offset.x + Hash2Float(Hash1(offset.x) * Hash2(-offset.y) + Hash3(offset.z) + Hash4(offset.w)) * size : 0),
        ((noiseDimensionCount > 1) ? offset.y + Hash2Float(Hash0(offset.y) * Hash1(-offset.z) + Hash2(offset.x) + Hash3(offset.w)) * size : 0),
         ((noiseDimensionCount > 2) ? offset.z + Hash2Float(Hash3(offset.z) * Hash4(-offset.x) + Hash5(offset.y) + Hash6(offset.w)) * size : 0),
          ((noiseDimensionCount > 3) ? offset.w + Hash2Float(Hash6(offset.z) * Hash7(-offset.x) + Hash0(offset.y) + Hash1(offset.w)) * size : 0))
                        ;

                            points[id * 3 + 2] = new Vector4(
           ((noiseDimensionCount > 0) ? offset.x + Hash2Float(Hash6(offset.x) * Hash7(-offset.y) + Hash0(offset.z) + Hash1(offset.w)) * size : 0),
            ((noiseDimensionCount > 1) ? offset.y + Hash2Float(Hash1(offset.y) * Hash2(-offset.z) + Hash3(offset.x) + Hash4(offset.w)) * size : 0),
            ((noiseDimensionCount > 2) ? offset.z + Hash2Float(Hash0(offset.z) * Hash1(-offset.x) + Hash2(offset.y) + Hash3(offset.w)) * size : 0),
             ((noiseDimensionCount > 3) ? offset.w + Hash2Float(Hash3(offset.z) * Hash4(-offset.x) + Hash5(offset.y) + Hash6(offset.w)) * size : 0))
                        ;

                        }
                    }
                }
            }

            float min0 = 10000;
            float min1 = 10000;
            float min2 = 10000;
            float min3 = 10000;
            float current = 0;
            Vector4 minP = new Vector4(-100000, -100000, -100000, -100000);

            Vector4 comparerPos = new Vector4(
    ((noiseDimensionCount > 0) ? pos.x : 0),
    ((noiseDimensionCount > 1) ? pos.y : 0),
    ((noiseDimensionCount > 2) ? pos.z : 0),
    ((noiseDimensionCount > 3) ? pos.w : 0));

            for (int i = 0; i < 3 * 3 * 3 * 3 * 3; i++)
            {
                current = Vector4.Distance(points[i], comparerPos);
                if (current < min0)
                {
                    min3 = min2;
                    min2 = min1;
                    min1 = min0;
                    min0 = current;
                    minP = points[i];
                }
            }

            //better return all the mins in vector form plus the final point_in_Biome_Value_Space and then switch on demand last.
            switch (cellularMethodID)
            {
                case 0:
                    return Hash2Float(Hash0(minP.x) * Hash1(-minP.y) + Hash2(minP.z) + Hash3(minP.w));
                case 2:
                    return 1 - Vector4.Distance(pos, minP) / size;
                case 3:
                    return Mathf.Abs(min1 - min0);
                case 4:
                    return Mathf.Abs(min2 - min1) + Mathf.Abs(min1 - min0) + Mathf.Abs(min0 - min2);
                case 5:
                    return Mathf.Abs(min2 - min1) + Mathf.Abs(min1 - min0) + Mathf.Abs(min0 - min2) + Mathf.Abs(min3 - min0) + Mathf.Abs(min3 - min1) + Mathf.Abs(min3 - min2);
                default:
                    return Vector4.Distance(pos, minP) / size;
            }
        }



        public float CellNoiseFourDPC(Vector4 pos, float size)
        {

            pos = Rotate(pos);

            Vector4 o = GridFloor(pos, size); //original in size space
            Vector4 p = o * size; //stepped in real space
            Vector4 r = (pos - p) / size; //0-1 pos within step
                                          //posXYZ&Value now using 4 points per cell
            Vector4[] points = new Vector4[3 * 3 * 3 * 3 * 4];
            //*
            for (int w = -1; w < 2; w++)
            {
                for (int z = -1; z < 2; z++)
                {
                    for (int y = -1; y < 2; y++)
                    {
                        for (int x = -1; x < 2; x++)
                        {
                            int id = (x + 1) + (y + 1) * 3 + (z + 1) * 9 + (w + 1) * 27;

                            Vector4 offset = p + new Vector4(x, y, z, w) * size;

                            points[id * 4] = new Vector4(
               ((noiseDimensionCount > 0) ? offset.x + Hash2Float(Hash0(offset.x) * Hash1(-offset.y) + Hash2(offset.z) + Hash3(offset.w)) * size : 0),
                 ((noiseDimensionCount > 1) ? offset.y + Hash2Float(Hash3(offset.y) * Hash4(-offset.z) + Hash5(offset.x) + Hash6(offset.w)) * size : 0),
                 ((noiseDimensionCount > 2) ? offset.z + Hash2Float(Hash6(offset.z) * Hash7(-offset.x) + Hash0(offset.y) + Hash1(offset.w)) * size : 0),
                 ((noiseDimensionCount > 3) ? offset.w + Hash2Float(Hash1(offset.z) * Hash2(-offset.x) + Hash3(offset.y) + Hash4(offset.w)) * size : 0))
                                ;

                            points[id * 4 + 1] = new Vector4(
        ((noiseDimensionCount > 0) ? offset.x + Hash2Float(Hash1(offset.x) * Hash2(-offset.y) + Hash3(offset.z) + Hash4(offset.w)) * size : 0),
        ((noiseDimensionCount > 1) ? offset.y + Hash2Float(Hash0(offset.y) * Hash1(-offset.z) + Hash2(offset.x) + Hash3(offset.w)) * size : 0),
         ((noiseDimensionCount > 2) ? offset.z + Hash2Float(Hash3(offset.z) * Hash4(-offset.x) + Hash5(offset.y) + Hash6(offset.w)) * size : 0),
          ((noiseDimensionCount > 3) ? offset.w + Hash2Float(Hash6(offset.z) * Hash7(-offset.x) + Hash0(offset.y) + Hash1(offset.w)) * size : 0))
                        ;

                            points[id * 4 + 2] = new Vector4(
           ((noiseDimensionCount > 0) ? offset.x + Hash2Float(Hash6(offset.x) * Hash7(-offset.y) + Hash0(offset.z) + Hash1(offset.w)) * size : 0),
            ((noiseDimensionCount > 1) ? offset.y + Hash2Float(Hash1(offset.y) * Hash2(-offset.z) + Hash3(offset.x) + Hash4(offset.w)) * size : 0),
            ((noiseDimensionCount > 2) ? offset.z + Hash2Float(Hash0(offset.z) * Hash1(-offset.x) + Hash2(offset.y) + Hash3(offset.w)) * size : 0),
             ((noiseDimensionCount > 3) ? offset.w + Hash2Float(Hash3(offset.z) * Hash4(-offset.x) + Hash5(offset.y) + Hash6(offset.w)) * size : 0))
                        ;
                            points[id * 4 + 3] = new Vector4(
                ((noiseDimensionCount > 0) ? offset.x + Hash2Float(Hash3(offset.x) * Hash4(-offset.y) + Hash5(offset.z) + Hash6(offset.w)) * size : 0),
                ((noiseDimensionCount > 1) ? offset.y + Hash2Float(Hash6(offset.y) * Hash7(-offset.z) + Hash0(offset.x) + Hash1(offset.w)) * size : 0),
                 ((noiseDimensionCount > 2) ? offset.z + Hash2Float(Hash1(offset.z) * Hash2(-offset.x) + Hash3(offset.y) + Hash4(offset.w)) * size : 0),
                 ((noiseDimensionCount > 3) ? offset.w + Hash2Float(Hash0(offset.z) * Hash1(-offset.x) + Hash2(offset.y) + Hash3(offset.w)) * size : 0))
                            ;

                            points[id * 4 + 3] = new Vector4(
        ((noiseDimensionCount > 0) ? offset.x + Hash2Float(Hash7(offset.x) * Hash6(-offset.y) + Hash5(offset.z) + Hash4(offset.w)) * size : 0),
        ((noiseDimensionCount > 1) ? offset.y + Hash2Float(Hash4(offset.y) * Hash3(-offset.z) + Hash2(offset.x) + Hash1(offset.w)) * size : 0),
         ((noiseDimensionCount > 2) ? offset.z + Hash2Float(Hash1(offset.z) * Hash0(-offset.x) + Hash7(offset.y) + Hash6(offset.w)) * size : 0),
          ((noiseDimensionCount > 3) ? offset.w + Hash2Float(Hash6(offset.z) * Hash5(-offset.x) + Hash4(offset.y) + Hash3(offset.w)) * size : 0))
                        ;

                        }
                    }
                }
            }

            float min0 = 10000;
            float min1 = 10000;
            float min2 = 10000;
            float min3 = 10000;
            float current = 0;
            Vector4 minP = new Vector4(-100000, -100000, -100000, -100000);

            Vector4 comparerPos = new Vector4(
    ((noiseDimensionCount > 0) ? pos.x : 0),
    ((noiseDimensionCount > 1) ? pos.y : 0),
    ((noiseDimensionCount > 2) ? pos.z : 0),
    ((noiseDimensionCount > 3) ? pos.w : 0));

            for (int i = 0; i < 3 * 3 * 3 * 3 * 4; i++)
            {
                current = Vector4.Distance(points[i], comparerPos);
                if (current < min0)
                {
                    min3 = min2;
                    min2 = min1;
                    min1 = min0;
                    min0 = current;
                    minP = points[i];
                }
            }

            //better return all the mins in vector form plus the final point_in_Biome_Value_Space and then switch on demand last.
            switch (cellularMethodID)
            {
                case 0:
                    return Hash2Float(Hash0(minP.x) * Hash1(-minP.y) + Hash2(minP.z) + Hash3(minP.w));
                case 2:
                    return 1 - Vector4.Distance(pos, minP) / size;
                case 3:
                    return Mathf.Abs(min1 - min0);
                case 4:
                    return Mathf.Abs(min2 - min1) + Mathf.Abs(min1 - min0) + Mathf.Abs(min0 - min2);
                case 5:
                    return Mathf.Abs(min2 - min1) + Mathf.Abs(min1 - min0) + Mathf.Abs(min0 - min2) + Mathf.Abs(min3 - min0) + Mathf.Abs(min3 - min1) + Mathf.Abs(min3 - min2);
                default:
                    return Vector4.Distance(pos, minP) / size;
            }
        }



        public float CellNoiseFiveDPC(Vector4 pos, float size)
        {

            pos = Rotate(pos);

            Vector4 o = GridFloor(pos, size); //original in size space
            Vector4 p = o * size; //stepped in real space
            Vector4 r = (pos - p) / size; //0-1 pos within step
                                          //posXYZ&Value now using 4 points per cell
            Vector4[] points = new Vector4[3 * 3 * 3 * 3 * 5];
            //*
            for (int w = -1; w < 2; w++)
            {
                for (int z = -1; z < 2; z++)
                {
                    for (int y = -1; y < 2; y++)
                    {
                        for (int x = -1; x < 2; x++)
                        {
                            int id = (x + 1) + (y + 1) * 3 + (z + 1) * 9 + (w + 1) * 27;

                            Vector4 offset = p + new Vector4(x, y, z, w) * size;

                            points[id * 5] = new Vector4(
               ((noiseDimensionCount > 0) ? offset.x + Hash2Float(Hash0(offset.x) * Hash1(-offset.y) + Hash2(offset.z) + Hash3(offset.w)) * size : 0),
                 ((noiseDimensionCount > 1) ? offset.y + Hash2Float(Hash3(offset.y) * Hash4(-offset.z) + Hash5(offset.x) + Hash6(offset.w)) * size : 0),
                 ((noiseDimensionCount > 2) ? offset.z + Hash2Float(Hash6(offset.z) * Hash7(-offset.x) + Hash0(offset.y) + Hash1(offset.w)) * size : 0),
                 ((noiseDimensionCount > 3) ? offset.w + Hash2Float(Hash1(offset.z) * Hash2(-offset.x) + Hash3(offset.y) + Hash4(offset.w)) * size : 0))
                                ;

                            points[id * 5 + 1] = new Vector4(
        ((noiseDimensionCount > 0) ? offset.x + Hash2Float(Hash1(offset.x) * Hash2(-offset.y) + Hash3(offset.z) + Hash4(offset.w)) * size : 0),
        ((noiseDimensionCount > 1) ? offset.y + Hash2Float(Hash0(offset.y) * Hash1(-offset.z) + Hash2(offset.x) + Hash3(offset.w)) * size : 0),
         ((noiseDimensionCount > 2) ? offset.z + Hash2Float(Hash3(offset.z) * Hash4(-offset.x) + Hash5(offset.y) + Hash6(offset.w)) * size : 0),
          ((noiseDimensionCount > 3) ? offset.w + Hash2Float(Hash6(offset.z) * Hash7(-offset.x) + Hash0(offset.y) + Hash1(offset.w)) * size : 0))
                        ;

                            points[id * 5 + 2] = new Vector4(
           ((noiseDimensionCount > 0) ? offset.x + Hash2Float(Hash6(offset.x) * Hash7(-offset.y) + Hash0(offset.z) + Hash1(offset.w)) * size : 0),
            ((noiseDimensionCount > 1) ? offset.y + Hash2Float(Hash1(offset.y) * Hash2(-offset.z) + Hash3(offset.x) + Hash4(offset.w)) * size : 0),
            ((noiseDimensionCount > 2) ? offset.z + Hash2Float(Hash0(offset.z) * Hash1(-offset.x) + Hash2(offset.y) + Hash3(offset.w)) * size : 0),
             ((noiseDimensionCount > 3) ? offset.w + Hash2Float(Hash3(offset.z) * Hash4(-offset.x) + Hash5(offset.y) + Hash6(offset.w)) * size : 0))
                        ;
                            points[id * 5 + 3] = new Vector4(
                ((noiseDimensionCount > 0) ? offset.x + Hash2Float(Hash3(offset.x) * Hash4(-offset.y) + Hash5(offset.z) + Hash6(offset.w)) * size : 0),
                ((noiseDimensionCount > 1) ? offset.y + Hash2Float(Hash6(offset.y) * Hash7(-offset.z) + Hash0(offset.x) + Hash1(offset.w)) * size : 0),
                 ((noiseDimensionCount > 2) ? offset.z + Hash2Float(Hash1(offset.z) * Hash2(-offset.x) + Hash3(offset.y) + Hash4(offset.w)) * size : 0),
                 ((noiseDimensionCount > 3) ? offset.w + Hash2Float(Hash0(offset.z) * Hash1(-offset.x) + Hash2(offset.y) + Hash3(offset.w)) * size : 0))
                            ;

                            points[id * 5 + 3] = new Vector4(
        ((noiseDimensionCount > 0) ? offset.x + Hash2Float(Hash7(offset.x) * Hash6(-offset.y) + Hash5(offset.z) + Hash4(offset.w)) * size : 0),
        ((noiseDimensionCount > 1) ? offset.y + Hash2Float(Hash4(offset.y) * Hash3(-offset.z) + Hash2(offset.x) + Hash1(offset.w)) * size : 0),
         ((noiseDimensionCount > 2) ? offset.z + Hash2Float(Hash1(offset.z) * Hash0(-offset.x) + Hash7(offset.y) + Hash6(offset.w)) * size : 0),
          ((noiseDimensionCount > 3) ? offset.w + Hash2Float(Hash6(offset.z) * Hash5(-offset.x) + Hash4(offset.y) + Hash3(offset.w)) * size : 0))
                        ;

                            points[id * 5 + 4] = new Vector4(
           ((noiseDimensionCount > 0) ? offset.x + Hash2Float(Hash3(offset.x) * Hash2(-offset.y) + Hash1(offset.z) + Hash0(offset.w)) * size : 0),
            ((noiseDimensionCount > 1) ? offset.y + Hash2Float(Hash0(offset.y) * Hash7(-offset.z) + Hash6(offset.x) + Hash5(offset.w)) * size : 0),
            ((noiseDimensionCount > 2) ? offset.z + Hash2Float(Hash5(offset.z) * Hash4(-offset.x) + Hash3(offset.y) + Hash2(offset.w)) * size : 0),
             ((noiseDimensionCount > 3) ? offset.w + Hash2Float(Hash2(offset.z) * Hash1(-offset.x) + Hash0(offset.y) + Hash7(offset.w)) * size : 0))
                        ;

                        }
                    }
                }
            }

            float min0 = 10000;
            float min1 = 10000;
            float min2 = 10000;
            float min3 = 10000;
            float current = 0;
            Vector4 minP = new Vector4(-100000, -100000, -100000, -100000);

            Vector4 comparerPos = new Vector4(
    ((noiseDimensionCount > 0) ? pos.x : 0),
    ((noiseDimensionCount > 1) ? pos.y : 0),
    ((noiseDimensionCount > 2) ? pos.z : 0),
    ((noiseDimensionCount > 3) ? pos.w : 0));

            for (int i = 0; i < 3 * 3 * 3 * 3 * 5; i++)
            {
                current = Vector4.Distance(points[i], comparerPos);
                if (current < min0)
                {
                    min3 = min2;
                    min2 = min1;
                    min1 = min0;
                    min0 = current;
                    minP = points[i];
                }
            }

            //better return all the mins in vector form plus the final point_in_Biome_Value_Space and then switch on demand last.
            switch (cellularMethodID)
            {
                case 0:
                    return Hash2Float(Hash0(minP.x) * Hash1(-minP.y) + Hash2(minP.z) + Hash3(minP.w));
                case 2:
                    return 1 - Vector4.Distance(pos, minP) / size;
                case 3:
                    return Mathf.Abs(min1 - min0);
                case 4:
                    return Mathf.Abs(min2 - min1) + Mathf.Abs(min1 - min0) + Mathf.Abs(min0 - min2);
                case 5:
                    return Mathf.Abs(min2 - min1) + Mathf.Abs(min1 - min0) + Mathf.Abs(min0 - min2) + Mathf.Abs(min3 - min0) + Mathf.Abs(min3 - min1) + Mathf.Abs(min3 - min2);
                default:
                    return Vector4.Distance(pos, minP) / size;
            }
        }


        public float CellNoiseSixDPC(Vector4 pos, float size)
        {

            pos = Rotate(pos);

            Vector4 o = GridFloor(pos, size); //original in size space
            Vector4 p = o * size; //stepped in real space
            Vector4 r = (pos - p) / size; //0-1 pos within step
                                          //posXYZ&Value now using 4 points per cell
            Vector4[] points = new Vector4[3 * 3 * 3 * 3 * 6];
            //*
            for (int w = -1; w < 2; w++)
            {
                for (int z = -1; z < 2; z++)
                {
                    for (int y = -1; y < 2; y++)
                    {
                        for (int x = -1; x < 2; x++)
                        {
                            int id = (x + 1) + (y + 1) * 3 + (z + 1) * 9 + (w + 1) * 27;

                            Vector4 offset = p + new Vector4(x, y, z, w) * size;

                            points[id * 6] = new Vector4(
               ((noiseDimensionCount > 0) ? offset.x + Hash2Float(Hash0(offset.x) * Hash1(-offset.y) + Hash2(offset.z) + Hash3(offset.w)) * size : 0),
                 ((noiseDimensionCount > 1) ? offset.y + Hash2Float(Hash3(offset.y) * Hash4(-offset.z) + Hash5(offset.x) + Hash6(offset.w)) * size : 0),
                 ((noiseDimensionCount > 2) ? offset.z + Hash2Float(Hash6(offset.z) * Hash7(-offset.x) + Hash0(offset.y) + Hash1(offset.w)) * size : 0),
                 ((noiseDimensionCount > 3) ? offset.w + Hash2Float(Hash1(offset.z) * Hash2(-offset.x) + Hash3(offset.y) + Hash4(offset.w)) * size : 0))
                                ;

                            points[id * 6 + 1] = new Vector4(
        ((noiseDimensionCount > 0) ? offset.x + Hash2Float(Hash1(offset.x) * Hash2(-offset.y) + Hash3(offset.z) + Hash4(offset.w)) * size : 0),
        ((noiseDimensionCount > 1) ? offset.y + Hash2Float(Hash0(offset.y) * Hash1(-offset.z) + Hash2(offset.x) + Hash3(offset.w)) * size : 0),
         ((noiseDimensionCount > 2) ? offset.z + Hash2Float(Hash3(offset.z) * Hash4(-offset.x) + Hash5(offset.y) + Hash6(offset.w)) * size : 0),
          ((noiseDimensionCount > 3) ? offset.w + Hash2Float(Hash6(offset.z) * Hash7(-offset.x) + Hash0(offset.y) + Hash1(offset.w)) * size : 0))
                        ;

                            points[id * 6 + 2] = new Vector4(
           ((noiseDimensionCount > 0) ? offset.x + Hash2Float(Hash6(offset.x) * Hash7(-offset.y) + Hash0(offset.z) + Hash1(offset.w)) * size : 0),
            ((noiseDimensionCount > 1) ? offset.y + Hash2Float(Hash1(offset.y) * Hash2(-offset.z) + Hash3(offset.x) + Hash4(offset.w)) * size : 0),
            ((noiseDimensionCount > 2) ? offset.z + Hash2Float(Hash0(offset.z) * Hash1(-offset.x) + Hash2(offset.y) + Hash3(offset.w)) * size : 0),
             ((noiseDimensionCount > 3) ? offset.w + Hash2Float(Hash3(offset.z) * Hash4(-offset.x) + Hash5(offset.y) + Hash6(offset.w)) * size : 0))
                        ;
                            points[id * 6 + 3] = new Vector4(
                ((noiseDimensionCount > 0) ? offset.x + Hash2Float(Hash3(offset.x) * Hash4(-offset.y) + Hash5(offset.z) + Hash6(offset.w)) * size : 0),
                ((noiseDimensionCount > 1) ? offset.y + Hash2Float(Hash6(offset.y) * Hash7(-offset.z) + Hash0(offset.x) + Hash1(offset.w)) * size : 0),
                 ((noiseDimensionCount > 2) ? offset.z + Hash2Float(Hash1(offset.z) * Hash2(-offset.x) + Hash3(offset.y) + Hash4(offset.w)) * size : 0),
                 ((noiseDimensionCount > 3) ? offset.w + Hash2Float(Hash0(offset.z) * Hash1(-offset.x) + Hash2(offset.y) + Hash3(offset.w)) * size : 0))
                            ;

                            points[id * 6 + 3] = new Vector4(
        ((noiseDimensionCount > 0) ? offset.x + Hash2Float(Hash7(offset.x) * Hash6(-offset.y) + Hash5(offset.z) + Hash4(offset.w)) * size : 0),
        ((noiseDimensionCount > 1) ? offset.y + Hash2Float(Hash4(offset.y) * Hash3(-offset.z) + Hash2(offset.x) + Hash1(offset.w)) * size : 0),
         ((noiseDimensionCount > 2) ? offset.z + Hash2Float(Hash1(offset.z) * Hash0(-offset.x) + Hash7(offset.y) + Hash6(offset.w)) * size : 0),
          ((noiseDimensionCount > 3) ? offset.w + Hash2Float(Hash6(offset.z) * Hash5(-offset.x) + Hash4(offset.y) + Hash3(offset.w)) * size : 0))
                        ;

                            points[id * 6 + 4] = new Vector4(
           ((noiseDimensionCount > 0) ? offset.x + Hash2Float(Hash3(offset.x) * Hash2(-offset.y) + Hash1(offset.z) + Hash0(offset.w)) * size : 0),
            ((noiseDimensionCount > 1) ? offset.y + Hash2Float(Hash0(offset.y) * Hash7(-offset.z) + Hash6(offset.x) + Hash5(offset.w)) * size : 0),
            ((noiseDimensionCount > 2) ? offset.z + Hash2Float(Hash5(offset.z) * Hash4(-offset.x) + Hash3(offset.y) + Hash2(offset.w)) * size : 0),
             ((noiseDimensionCount > 3) ? offset.w + Hash2Float(Hash2(offset.z) * Hash1(-offset.x) + Hash0(offset.y) + Hash7(offset.w)) * size : 0))
                        ;
                            points[id * 6 + 5] = new Vector4(
                ((noiseDimensionCount > 0) ? offset.x + Hash2Float(Hash2(offset.x) * Hash0(-offset.y) + Hash7(offset.z) + Hash2(offset.w)) * size : 0),
                ((noiseDimensionCount > 1) ? offset.y + Hash2Float(Hash2(offset.y) * Hash6(-offset.z) + Hash4(offset.x) + Hash7(offset.w)) * size : 0),
                 ((noiseDimensionCount > 2) ? offset.z + Hash2Float(Hash4(offset.z) * Hash3(-offset.x) + Hash1(offset.y) + Hash5(offset.w)) * size : 0),
                 ((noiseDimensionCount > 3) ? offset.w + Hash2Float(Hash2(offset.z) * Hash5(-offset.x) + Hash7(offset.y) + Hash0(offset.w)) * size : 0))
                            ;

                        }
                    }
                }
            }

            float min0 = 10000;
            float min1 = 10000;
            float min2 = 10000;
            float min3 = 10000;
            float current = 0;
            Vector4 minP = new Vector4(-100000, -100000, -100000, -100000);

            Vector4 comparerPos = new Vector4(
    ((noiseDimensionCount > 0) ? pos.x : 0),
    ((noiseDimensionCount > 1) ? pos.y : 0),
    ((noiseDimensionCount > 2) ? pos.z : 0),
    ((noiseDimensionCount > 3) ? pos.w : 0));

            for (int i = 0; i < 3 * 3 * 3 * 3 * 6; i++)
            {
                current = Vector4.Distance(points[i], comparerPos);
                if (current < min0)
                {
                    min3 = min2;
                    min2 = min1;
                    min1 = min0;
                    min0 = current;
                    minP = points[i];
                }
            }

            //better return all the mins in vector form plus the final point_in_Biome_Value_Space and then switch on demand last.
            switch (cellularMethodID)
            {
                case 0:
                    return Hash2Float(Hash0(minP.x) * Hash1(-minP.y) + Hash2(minP.z) + Hash3(minP.w));
                case 2:
                    return 1 - Vector4.Distance(pos, minP) / size;
                case 3:
                    return Mathf.Abs(min1 - min0);
                case 4:
                    return Mathf.Abs(min2 - min1) + Mathf.Abs(min1 - min0) + Mathf.Abs(min0 - min2);
                case 5:
                    return Mathf.Abs(min2 - min1) + Mathf.Abs(min1 - min0) + Mathf.Abs(min0 - min2) + Mathf.Abs(min3 - min0) + Mathf.Abs(min3 - min1) + Mathf.Abs(min3 - min2);
                default:
                    return Vector4.Distance(pos, minP) / size;
            }
        }






        public float InterpolateNearestNeighbor(float P1, float P2, float t)
        {
            return (t > .5) ? P2 : P1;
        }


        public float InterpolateLinear(float P1, float P2, float t)//Linear Interpolation, http://paulbourke.net/miscellaneous/interpolation/
        {
            return P1 * (1 - t) + P2 * t;
        }



        public float InterpolateCosine(float P1, float P2, float t)//Cosine Interpolation, http://paulbourke.net/miscellaneous/interpolation/
        {
            float m = (1 - Mathf.Cos(t * 3.14159265358979323846264338f)) * 0.5f; //custom cos & lin blend
            return P1 * (1 - m) + P2 * m;
        }


        public float InterpolateCosinish(float P1, float P2, float t)//modified: //Cosine Interpolation, http://paulbourke.net/miscellaneous/interpolation/
        {
            float m = (1 - Mathf.Cos(t * 3.14159265358979323846264338f)) * 0.1875f + t * 0.625f; //custom cos & lin blend
            return P1 * (1 - m) + P2 * m;
        }


        public float InterpolateCubic(float P0, float P1, float P2, float P3, float t)//Cubic Interpolation, http://paulbourke.net/miscellaneous/interpolation/
        {   //tension 1 ~ cosine , tension -1 overflow. 
            float w0, w1, w2, w3, t_sqrt;
            t_sqrt = t * t;
            w0 = P3 - P2 - P0 + P1;
            w1 = P0 - P1 - w0;
            w2 = P2 - P0;
            w3 = P1;
            return (w0 * t * t_sqrt + w1 * t_sqrt + w2 * t + w3);
        }

        public float InterpolateTensionedHermite(float P0, float P1, float P2, float P3, float t)//modified: based off of Hermite Interpolation, http://paulbourke.net/miscellaneous/interpolation/, thanks pal.
        {   //tension 1 ~ cosine , tension -1 overflow. 
            float a0, a1, t_sqrt, t_cubic;
            float w0, w1, w2, w3;
            t_sqrt = t * t;
            t_cubic = t_sqrt * t;
            float tensionA = Mathf.Abs(P1 - P0) * 2 - 1;
            float tensionB = Mathf.Abs(P2 - P1) * 2 - 1;
            float tensionC = Mathf.Abs(P3 - P2) * 2 - 1;
            tensionA += tensionB; tensionA *= .5f;
            tensionC += tensionB; tensionC *= .5f;
            float tension = tensionA * Mathf.Clamp(0.5f - t, 0, 1) + tensionB * (0.5f - Mathf.Abs(t - 0.5f)) + tensionC * Mathf.Clamp(t - 0.5f, 0, 1);
            a0 = (P1 - P0) * (1 - tension) / 2;
            a0 += (P2 - P1) * (1 - tension) / 2;
            a1 = (P2 - P1) * (1 - tension) / 2;
            a1 += (P3 - P2) * (1 - tension) / 2;
            w0 = 2 * t_cubic - 3 * t_sqrt + 1;
            w1 = t_cubic - 2 * t_sqrt + t;
            w2 = t_cubic - t_sqrt;
            w3 = -2 * t_cubic + 3 * t_sqrt;
            return (w0 * P1 + w1 * a0 + w2 * a1 + w3 * P2);
        }





        //Rotation happens in 3D space. The rotation defines a vector in xyz plus the angle w to rotate around it (w 1 = 360 | 2PI)
        public Vector4 Rotate(Vector4 p)
        {
            Vector3 rxyz = new Vector3(rotation.x, rotation.y, rotation.z);
            //from here https://www.geeks3d.com/20141201/how-to-rotate-a-vertex-by-a-quaternion-in-glsl/
            if (rxyz.sqrMagnitude == 0) rotation.y = 1;
            float ha = rotation.w * 3.14159265358979323846264338f;
            Vector3 n = rotation.normalized;
            Vector3 qxyz = new Vector3(
                n.x * Mathf.Sin(ha),
            n.y * Mathf.Sin(ha),
            n.z * Mathf.Sin(ha));
            Vector4 q = new Vector4(
            qxyz.x, qxyz.y, qxyz.z,
            Mathf.Cos(ha));
            //from here https://code.google.com/archive/p/kri/wikis/Quaternions.wiki
            Vector3 v = new Vector3(p.x, p.y, p.z);
            Vector3 o = v + 2.0f * Vector3.Cross(qxyz, Vector3.Cross(qxyz, v) + q.w * v);
            return new Vector4(o.x, o.y, o.z, p.w);
        }


        public float GridFloor(float value, float gridSize)
        {
            float r = (value - value % gridSize) / gridSize;
            if (value < 0 && gridSize > value) r -= 1;
            return r;
        }

        public Vector2 GridFloor(Vector2 value, float gridSize)
        {
            return new Vector2(GridFloor(value.x, gridSize), GridFloor(value.y, gridSize));
        }

        public Vector3 GridFloor(Vector3 value, float gridSize)
        {
            return new Vector3(GridFloor(value.x, gridSize), GridFloor(value.y, gridSize), GridFloor(value.z, gridSize));
        }

        public Vector4 GridFloor(Vector4 value, float gridSize)
        {
            return new Vector4(GridFloor(value.x, gridSize), GridFloor(value.y, gridSize), GridFloor(value.z, gridSize), GridFloor(value.w, gridSize));
        }



        /*
    Note on RNG:

    Tried using the usual shader way with dot & frac -> smaller coordinate coverage and more questionable quality.
    Tried combining with passed presets (range of numbers to pick from) -> more rng controll & overhead not worth over post control.
     Other custom things so far.. just no.
     Current Hash implementation based on https://burtleburtle.net/bob/hash/integer.html, https://burtleburtle.net/bob/hash/integer.html & based off of from H. Schechter & R. Bridson, goo.gl/RXiKaH
    */

        public float Hash2Float(uint hash)
        {
            return Mathf.Clamp((float)(hash / 4294967295.0), -1, 1); // 2^32-1
        }

        public uint Hash0(float seed)
        {
            uint s = (uint)(seed - 1367.97546) - 2437962429u;
            s ^= 1331311319u;
            s *= 1954568973u;
            s ^= s >> 17;
            s *= 2987348578u;
            s ^= s >> 13;
            s *= 1862398427u;
            return s;
        }

        public uint Hash1(float seed)
        {
            uint s = (uint)(seed + 3573.217801) + 3456735731u;
            s ^= s << 13;
            s *= 3698471546u;
            s ^= s >> 17;
            s *= 1876428345u;
            s ^= s << 5;
            return s;
        }

        public uint Hash2(float seed)
        {
            uint s = (uint)(seed + 4823.753849) + 1863897315u;
            s = (s ^ 61) ^ (s >> 16);
            s *= 9;
            s = s ^ (s >> 4);
            s *= 668265261u;
            s = s ^ (s >> 15);
            return s;
        }

        public uint Hash3(float seed)
        {
            uint s = (uint)(seed - 5886.34896) - 2887962599u;
            s ^= 2747636419u;
            s *= 2654435769u;
            s ^= s >> 16;
            s *= 2654435769u;
            s ^= s >> 16;
            s *= 2654435769u;
            return s;
        }

        public uint Hash4(float seed)
        {
            uint s = (uint)(seed - 5846.34896) - 1887962593u;
            s ^= 1747636419u;
            s *= 3654435769u;
            s ^= s >> 11;
            s *= 1654435769u;
            s ^= s >> 14;
            s *= 3654435769u;
            return s;
        }

        public uint Hash5(float seed)
        {
            uint s = (uint)(seed + 7645.35896) + 2938475611u;
            s ^= s << 18;
            s *= 3698471546u;
            s ^= s >> 14;
            s *= 1876428345u;
            s ^= s << 7;
            return s;
        }

        public uint Hash6(float seed)
        {
            uint s = (uint)(seed + 2387.357890) + 3847562931u;
            s = (s ^ 13) ^ (s >> 31);
            s *= 6;
            s = s ^ (s >> 3);
            s *= 886265269u;
            s = s ^ (s >> 17);
            return s;
        }

        public uint Hash7(float seed)
        {
            uint s = (uint)(seed - 1999.86401) - 3928374651u;
            s ^= 1918275634u;
            s *= 3029385716u;
            s ^= s >> 17;
            s *= 1295847452u;
            s ^= s >> 17;
            s *= 1928636475u;
            return s;
        }


    }
}