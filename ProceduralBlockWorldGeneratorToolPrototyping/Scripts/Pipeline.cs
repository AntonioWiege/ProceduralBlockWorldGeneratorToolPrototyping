/*Antonio Wiege*/
#if !Deactivate_Async
using System.Threading.Tasks;
using System.Collections.Concurrent;
#endif
using UnityEngine;
using UnityEngine.Profiling;
using System.Linq;
using System.Collections.Generic;
using Unity.VisualScripting;

//a word on threading in the main script

namespace ProceduralBlockWorldGeneratorToolPrototyping
{
    public class Pipeline
    {
        LandscapeTool directorInstance;
        public bool hadToWork;

        public Pipeline(LandscapeTool inCharge)
        {
            directorInstance = inCharge;
        }

        public void Update()
        {
            ProcessMostRelevantChunkInNeed();
        }


        public void ProcessMostRelevantChunkInNeed()
        {
            if (directorInstance.usingPreBoundGen) return;
#if !Deactivate_Profiling
            Profiler.BeginSample("ProcessMostRelevantChunkInNeed");
#endif
            hadToWork = false;
            //if (directorInstance.chunksUnderConstruction.Count > 0)
            //{
                for (int i = 0; i < directorInstance.chunksToLoadAround; i++)
                {
                    Int3 pos = directorInstance.pointOfChunkOfRelevance + directorInstance.sphericalOffsetLookUpTable[i];
                    if (!directorInstance.withinLimit(pos)) continue;
                    if (!directorInstance.chunks.ContainsKey(pos))
                    {
                    directorInstance.RequestPiece(pos,true);
                    }
                    else
                    {
                        var c = directorInstance.chunks[pos];
                        if (!directorInstance.chunksUnderConstruction.Contains(c)) continue;//if chunk not marked as underconstruction skip
                        ForceChunkUpToState(c, c.genState + 1);
                    }
                    break;
                }
            //}
#if !Deactivate_Profiling
            Profiler.EndSample();
#endif
        }



        public Chunk ForceChunkUpToState(Chunk c, int state)
        {//Depending on the chunks genState proceed accordingly
#if !Deactivate_Profiling
            Profiler.BeginSample("ForceChunkUpToState on " + c.ToString());
#endif
            bool outOfBounds = false;
            while (c.genState < state)
            {
                hadToWork = true;
                switch (c.genState)
                {
                    case -1:
                        CreateAndInitializeChunk(c.key);
                        break;
                    case 0:
                        DensityNoiseGen(c);
                        break;
                    case 1:
                        DensityToBlocks(c);
                        break;
                    case 2:
                        DecorativesAndDiagonalsPass(c);
                        break;
                    case 3:
                        BlocksToMesh(c);
                        break;
                    case 4:
                        MeshCombinationAndColoring(c);
                        break;
                    default:
                        Debug.Log("GenState out of range");
                        outOfBounds = true;
                        break;
                }
                if (outOfBounds) break;
            }
#if !Deactivate_Profiling
            Profiler.EndSample();
#endif
            return c;
        }



        public Chunk CreateAndInitializeChunk(Int3 chunkPos)
        {
#if !Deactivate_Profiling
            Profiler.BeginSample("Creating Chunk with key " + chunkPos.ValueSetOnly_ToString());
#endif
            directorInstance.chunksLoadedSincePlay++;
            //create & init chunk
            var c = new GameObject("Chunk " + chunkPos.ValueSetOnly_ToString()).AddComponent<Chunk>();
            c.directorInstance = directorInstance;

            c.genState = -1;
#if !Deactivate_Async
            if (!directorInstance.chunksUnderConstruction.Contains(c))
#endif
                directorInstance.chunksUnderConstruction.Add(c);
            c.transform.parent = directorInstance.transform;
            //c.transform.position = (chunkPos- directorInstance.globalOffset).AsVector3 * LandscapeTool.ChunkScale * LandscapeTool.BlockScale;
            c.transform.position = (chunkPos + directorInstance.globalOffset).AsVector3 * LandscapeTool.ChunkScale * LandscapeTool.BlockScale;
#if Deactivate_Async
            directorInstance.chunks.Add(chunkPos, c);
#else
            directorInstance.chunks.TryAdd(chunkPos, c);
#endif
            directorInstance.currentlyVisibleChunks.Add(c);

            c.updateAllBlocks = true;
            c.key = chunkPos;
            c.mf = c.gameObject.AddComponent<MeshFilter>();
            c.mc = c.gameObject.AddComponent<MeshCollider>();
            c.mr = c.gameObject.AddComponent<MeshRenderer>();
            c.mr.sharedMaterial = directorInstance.defaultMaterial;
            c.m = new();
            c.m.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            c.m.MarkDynamic();

            var child = new GameObject("Decoratives");
            child.transform.parent = c.transform;
            child.AddComponent<MeshFilter>();
            child.AddComponent<MeshCollider>();
            child.AddComponent<MeshRenderer>();
            c.updateDecorations = true;

            c.blocks = new Block[LandscapeTool.ChunkScale * LandscapeTool.ChunkScale * LandscapeTool.ChunkScale];
            c.tint = new Color[LandscapeTool.ChunkScale * LandscapeTool.ChunkScale * LandscapeTool.ChunkScale];
            for (int i = 0; i < c.blocks.Length; i++)
            {
                c.blocks[i] = new Block() { blockShape = Block_Library.Air.blockShape, matter = Block_Library.Air.matter, meshSection = Block_Library.Air.meshSection };
            }

            //tint by gradient from inspector
#if Deactivate_Async
  for (int z = 0; z < LandscapeTool.ChunkScale; z++)
            {
                for (int y = 0; y < LandscapeTool.ChunkScale; y++)
                {
                    for (int x = 0; x < LandscapeTool.ChunkScale; x++)
                    {
                        var offset = new Int3(x, y, z);
                        var i = offset.LocalPosToLinearID();
                        var worldSpaceHeight = c.key.y * LandscapeTool.ChunkScale * LandscapeTool.BlockScale+y*LandscapeTool.BlockScale;
                        var value = Mathf.Clamp01(Mathf.Abs(worldSpaceHeight - directorInstance.gradientBottom) /Mathf.Abs(directorInstance.gradientTop- directorInstance.gradientBottom+0.001f));
                        c.tint[i] = directorInstance.mapColorGradient.Evaluate(value);
                    }
                }
            }
#else
            Parallel.For(0, LandscapeTool.ChunkScale,
                z =>
                {
                    Parallel.For(0, LandscapeTool.ChunkScale,
                    y =>
                    {
                        for (int x = 0; x < LandscapeTool.ChunkScale; x++)
                        {
                            var offset = new Int3(x, y, z);
                            var i = offset.LocalPosToLinearID();
                            var worldSpaceHeight = c.key.y * LandscapeTool.ChunkScale * LandscapeTool.BlockScale + y * LandscapeTool.BlockScale;
                            var value = Mathf.Clamp01(Mathf.Abs(worldSpaceHeight - directorInstance.gradientBottom) / Mathf.Abs(directorInstance.gradientTop - directorInstance.gradientBottom + 0.001f));
                            c.tint[i] = directorInstance.mapColorGradient.Evaluate(value);
                        }
                    });
                });
#endif

            c.genState = 0;

#if !Deactivate_Profiling
            Profiler.EndSample();
#endif

            return c;
        }



        public Chunk DensityNoiseGen(Chunk c)
        {
#if !Deactivate_Profiling
            Profiler.BeginSample("DensityNoiseGen on " + c.ToString());
#endif

            ForceChunkUpToState(c, 0);

            ComputeBuffer resultBuffer = new ComputeBuffer(LandscapeTool.ChunkScale * LandscapeTool.ChunkScale * LandscapeTool.ChunkScale, sizeof(float));
            resultBuffer.SetData(new float[LandscapeTool.ChunkScale * LandscapeTool.ChunkScale * LandscapeTool.ChunkScale]);
            float[] cpuDat = new float[LandscapeTool.ChunkScale * LandscapeTool.ChunkScale * LandscapeTool.ChunkScale];

            //global biome determining values
            float[] cube_temperature = directorInstance.noiseHandler.Execute(c.key, directorInstance.BiomeValues_Temperature, resultBuffer, false, true, LandscapeTool.ChunkScale, 3,directorInstance.CPU_Noise,cpuDat);
            float[] cube_variety = directorInstance.noiseHandler.Execute(c.key, directorInstance.BiomeValues_Variety, resultBuffer, false, true, LandscapeTool.ChunkScale, 3, directorInstance.CPU_Noise, cpuDat);
            float[] cube_moisture = directorInstance.noiseHandler.Execute(c.key, directorInstance.BiomeValues_Moisture, resultBuffer, false, true, LandscapeTool.ChunkScale, 3, directorInstance.CPU_Noise, cpuDat);
            float blendDistance = directorInstance.biomeBlendDistance* Mathf.Pow(3, 1 / 3f);// var cubicDiagonal = Mathf.Pow(3,1/3f);

            //get nearby biomes in inspetor defined range
            HashSet<Biome> nearbyBiomesSet = new();
            int idOfClosest=0;
            float distanceOfClosest=99999;
            for (int i = 0; i < cube_temperature.Length; i++)
            {
                var v = new Vector3(cube_temperature[i], cube_variety[i], cube_moisture[i]);
                float delta = Vector3.Distance(v, directorInstance.biomes[0].point_in_Biome_Value_Space);

                for (int o = 0; o < directorInstance.biomes.Count; o++)
                {
                    var d = Vector3.Distance(v, directorInstance.biomes[o].point_in_Biome_Value_Space);
                    if (d < blendDistance)
                    {
                        nearbyBiomesSet.Add(directorInstance.biomes[o]);
                    }
                    if (d < distanceOfClosest)
                    {
                        idOfClosest = o;
                        distanceOfClosest = d;
                    }
                }
            }
            if (nearbyBiomesSet.Count < 1)
            {//make sure at least one biome is there even if outside of selected distance
                nearbyBiomesSet.Add(directorInstance.biomes[idOfClosest]);
            }

            float[][] block_densities = new float[nearbyBiomesSet.Count][];
            var nearbyBiomes = nearbyBiomesSet.ToArray();


            for (int i = 0; i < nearbyBiomes.Length; i++)
            {
                for (int o = 0; o < nearbyBiomes[i].noises.Count - 1; o++)
                {
                                       _ = directorInstance.noiseHandler.Execute(c.key, nearbyBiomes[i].noises[o], resultBuffer, false, false, LandscapeTool.ChunkScale, 3, directorInstance.CPU_Noise,cpuDat);
                }
                block_densities[i] = directorInstance.noiseHandler.Execute(c.key, nearbyBiomes[i].noises.Last(), resultBuffer, false, true, LandscapeTool.ChunkScale, 3, directorInstance.CPU_Noise, cpuDat);
            }
            resultBuffer.Dispose();


            float[] density = new float[LandscapeTool.ChunkScale * LandscapeTool.ChunkScale * LandscapeTool.ChunkScale];

#if Deactivate_Async
            float[] distances = new float[a.Length];
            float[] influences = new float[a.Length];
            for (int i = 0; i < density.Length; i++)
            {
                Biome closest = a[0];
                var v = new Vector3(cube_temperature[i], cube_variety[i], cube_moisture[i]);
                float delta = Vector3.Distance(v, closest.point_in_Biome_Value_Space);
                for (int o = 1; o < a.Length; o++)
                {
                    var d = Vector3.Distance(v, a[o].point_in_Biome_Value_Space);
                    distances[o] = d;
                    if (d < delta)
                    {
                        delta = d;
                        closest = a[o];
                    }
                }

                c.primaryBiomeID[i] = directorInstance.biomes.IndexOf(closest);

                float total = 0f;
                for (int d = 0; d < a.Length; d++)
                {
                    influences[d] = Mathf.Clamp01((blendDistance - (distances[d] - delta)) / blendDistance);
                    total += influences[d];
                }
                density[i] = 0;
                for (int d = 0; d < a.Length; d++)
                {
                    density[i] += block_densities[d][i] * influences[d] / total;
                }
            }
#else
            Parallel.For(0, density.Length, i =>
            {
                float[] distances = new float[nearbyBiomes.Length];
                float[] influences = new float[nearbyBiomes.Length];
                Biome closest = nearbyBiomes[0];
                var v = new Vector3(cube_temperature[i], cube_variety[i], cube_moisture[i]);
                float delta = Vector3.Distance(v, closest.point_in_Biome_Value_Space);
                //buffer biome space distances per block
                for (int o = 1; o < nearbyBiomes.Length; o++)
                {
                    var d = Vector3.Distance(v, nearbyBiomes[o].point_in_Biome_Value_Space);
                    distances[o] = d;
                    if (d < delta)
                    {
                        delta = d;
                        closest = nearbyBiomes[o];
                    }
                }
                //store closest biome reference
                c.primaryBiomeID[i] = directorInstance.biomes.IndexOf(closest);

                float total = 0f;
                for (int d = 0; d < nearbyBiomes.Length; d++)
                {
                    influences[d] = Mathf.Clamp01(1 / (1+distances[d] - delta));//prev.:(blendDistance - (distances[d] - delta)) / blendDistance);
                    total += influences[d];
                }
                density[i] = 0;
                for (int d = 0; d < nearbyBiomes.Length; d++)
                {
                    density[i] += block_densities[d][i] * influences[d] / total;
                }
            });
#endif


            /*//originalGPU
            float[] density = null;
            for (int i = 0; i < directorInstance.biomeOne.noises.Count-1; i++)
            {
                density = directorInstance.noiseHandler.Execute(c.key, directorInstance.biomeOne.noises[i], resultBuffer, false, false, LandscapeTool.ChunkScale, 3, 0.5f);
            }
            density = directorInstance.noiseHandler.Execute(c.key, directorInstance.biomeOne.noises.Last(), resultBuffer, true, true, LandscapeTool.ChunkScale, 3, 0.5f);//*/
            /*//testingCPU
            var density = new float[LandscapeTool.ChunkScale * LandscapeTool.ChunkScale * LandscapeTool.ChunkScale];
            for (int i = 0; i < directorInstance.biomeOne.noises.Count - 1; i++)
            {
                density = directorInstance.noiseHandler.Execute(c.key, directorInstance.biomeOne.noises[i], null, false, false, LandscapeTool.ChunkScale, 3, 0.5f,true,density);
            }
            density = directorInstance.noiseHandler.Execute(c.key, directorInstance.biomeOne.noises.Last(), null, true, true, LandscapeTool.ChunkScale, 3, 0.5f,true,density);//*/

            c.densityMap = density;
            c.updateAllBlocks = true;
            c.genState = 1;

#if !Deactivate_Profiling
            Profiler.EndSample();
#endif

            return c;
        }



        /*
         Force chunk neighbors up to state & perform opotional blur & erosion passes
         */


        public Chunk DensityToBlocks(Chunk c)
        {
#if !Deactivate_Profiling
            Profiler.BeginSample("DensityToBlocks on " + c.ToString());
#endif

            ForceChunkUpToState(c, 1);

#if Deactivate_Async
            for (int i = 0; i < c.densityMap.Length; i++)
            {
                //initial block set
                if (c.densityMap[i] >= directorInstance.densityToRealityBorder)
                {
                    c.blocks[i] = Block_Library.Rock;
                    if(Chunk.LocalIDtoPos[i].y< LandscapeTool.ChunkScale - 1)
                    {
                        if (c.blocks[i+LandscapeTool.ChunkScale].blockShape.meshSections.Length>1)
                        {
                            c.blocks[i].matter = directorInstance.biomes[c.primaryBiomeID[i]].Underground;
                            continue;
                        }
                    }
                        var height = Chunk.LocalIDtoPos[i].y * LandscapeTool.BlockScale + c.key.y * LandscapeTool.ChunkScale * LandscapeTool.BlockScale;
                        c.blocks[i].matter = height < directorInstance.oceanHeight ? directorInstance.biomes[c.primaryBiomeID[i]].SurfaceUnderwater : directorInstance.biomes[c.primaryBiomeID[i]].SurfaceLand;
                    
                }
                else
                {
                    c.blocks[i] = Block_Library.Air;
                }
            }
#else
            Parallel.For(0, c.densityMap.Length, i =>
            {
                //initial block set
                if (c.densityMap[i] >= directorInstance.densityToRealityBorder)
                {
                    c.blocks[i] = Block_Library.Rock;
                    if (Chunk.LocalIDtoPos[i].y < LandscapeTool.ChunkScale - 1)//make sure there is enough space to check local neighbor //skipping reading next chunk
                    {
                        if (c.blocks[i + LandscapeTool.ChunkScale].blockShape.meshSections.Length > 1)//block above exists?
                        {
                            c.blocks[i].matter = directorInstance.biomes[c.primaryBiomeID[i]].Underground;
                            return;
                        }
                    }
                    var height = Chunk.LocalIDtoPos[i].y * LandscapeTool.BlockScale + c.key.y * LandscapeTool.ChunkScale * LandscapeTool.BlockScale;
                    c.blocks[i].matter = height < directorInstance.oceanHeight ? directorInstance.biomes[c.primaryBiomeID[i]].SurfaceUnderwater : directorInstance.biomes[c.primaryBiomeID[i]].SurfaceLand;
                }
                else
                {
                    c.blocks[i] = Block_Library.Air;
                }
            });
#endif


#if Deactivate_Async
            foreach (var item in c.surroundChunks)//once this chunk finishes it's contents the neighbors have to update; PUSH post decor instance later
            {
                if (item == null) continue;
                item.updateAllBlocks = true;
                if (item.genState > 2)
                {
                    item.genState = 2;
                }
            }
#else
            Parallel.ForEach(c.SurroundRealToList, item =>
            {
                item.updateAllBlocks = true;
                item.updateDecorations = true;
                if (item.genState > 2)
                {
                    if (!directorInstance.chunksUnderConstruction.Contains(item))
                        directorInstance.chunksUnderConstruction.Add(item);
                    item.genState = 2;
                }
            });
#endif
            c.updateAllBlocks = true;

            if (c.alreadyDecorated)
            {
                for (int i = 0; i < c.decoPositions.Count; i++)
                {
                    var item = c.decoPositions[i];
                    if (item.ToLinearChunkScaleIndex() >= c.densityMap.Length)
                    {
                        c.trees.RemoveAt(i); continue;
                    }
                    if (c.densityMap[item.ToLinearChunkScaleIndex()] >= directorInstance.densityToRealityBorder || c.blocks[item.ToLinearChunkScaleIndex()].meshSection.verts.Length > 1)
                    {
                        int o = 0;
                        bool success = false;
                        while (item.y + o < LandscapeTool.ChunkScale - 1)
                        {
                            o++;
                            if (c.densityMap[(item + Int3.up * o).ToLinearChunkScaleIndex()] < directorInstance.densityToRealityBorder || c.blocks[(item + Int3.up * o).ToLinearChunkScaleIndex()].meshSection.verts.Length <= 1)
                            {
                                success = true;
                                c.decoPositions[i] = item + Int3.up * o;
                                break;
                            }
                        }
                        if (success) continue;
                        o = 0;
                        while (item.y + o > 0)
                        {
                            o--;
                            if (c.densityMap[(item + Int3.up * o).ToLinearChunkScaleIndex()] < directorInstance.densityToRealityBorder || c.blocks[(item + Int3.up * o).ToLinearChunkScaleIndex()].meshSection.verts.Length <= 1)
                            {
                                success = true;
                                c.decoPositions[i] = item + Int3.up * o;
                                break;
                            }
                        }
                        if (!success)
                        {
                            c.decoPositions.RemoveAt(i);
                            c.decoBlocks.RemoveAt(i);
                        }
                    }

                    if (item.y > 0)
                    {
                        if (c.densityMap[(item + Int3.down).ToLinearChunkScaleIndex()] < directorInstance.densityToRealityBorder || c.blocks[(item + Int3.down).ToLinearChunkScaleIndex()].meshSection.verts.Length <= 1)
                        {
                            int o = 0;
                            bool success = false;
                            o = 0;
                            while (item.y + o > 1)
                            {
                                o--;
                                if (c.densityMap[(item + Int3.down + Int3.up * o).ToLinearChunkScaleIndex()] >= directorInstance.densityToRealityBorder || c.blocks[(item + Int3.down + Int3.up * o).ToLinearChunkScaleIndex()].meshSection.verts.Length > 1)
                                {
                                    success = true;
                                    c.decoPositions[i] = item + Int3.up * o;
                                    break;
                                }
                            }
                            if (!success)
                            {
                                c.decoPositions.RemoveAt(i);
                                c.decoBlocks.RemoveAt(i);
                            }
                        }
                    }
                    else
                    {
                        c.decoPositions.RemoveAt(i);
                        c.decoBlocks.RemoveAt(i);
                    }
                }
                for (int i = 0; i < c.trees.Count; i++)
                {
                    var item = c.trees[i].chunkSpaceOrigin;
                    if (item.ToLinearChunkScaleIndex() >= c.densityMap.Length)
                    {
                        c.trees.RemoveAt(i); continue;
                    }
                    if (c.densityMap[item.ToLinearChunkScaleIndex()] >= directorInstance.densityToRealityBorder || c.blocks[item.ToLinearChunkScaleIndex()].meshSection.verts.Length > 1)
                    {
                        int o = 0;
                        bool success = false;
                        while (item.y + o < LandscapeTool.ChunkScale - 1)
                        {
                            o++;
                            if (c.densityMap[(item + Int3.up * o).ToLinearChunkScaleIndex()] < directorInstance.densityToRealityBorder || c.blocks[(item + Int3.up * o).ToLinearChunkScaleIndex()].meshSection.verts.Length <= 1)
                            {
                                success = true;
                                c.trees[i].chunkSpaceOrigin = item + Int3.up * o;
                                break;
                            }
                        }
                        if (success) continue;
                        o = 0;
                        while (item.y + o > 0)
                        {
                            o--;
                            if (c.densityMap[(item + Int3.up * o).ToLinearChunkScaleIndex()] < directorInstance.densityToRealityBorder || c.blocks[(item + Int3.up * o).ToLinearChunkScaleIndex()].meshSection.verts.Length <= 1)
                            {
                                success = true;
                                c.trees[i].chunkSpaceOrigin = item + Int3.up * o;
                                break;
                            }
                        }
                        if (!success)
                        {
                            c.trees.RemoveAt(i);
                        }
                    }

                    if (item.y > 0)
                    {
                        if (c.densityMap[(item + Int3.down).ToLinearChunkScaleIndex()] < directorInstance.densityToRealityBorder || c.blocks[(item + Int3.down).ToLinearChunkScaleIndex()].meshSection.verts.Length <= 1)
                        {
                            int o = 0;
                            bool success = false;
                            o = 0;
                            while (item.y + o > 1)
                            {
                                o--;
                                if (c.densityMap[(item + Int3.down + Int3.up * o).ToLinearChunkScaleIndex()] >= directorInstance.densityToRealityBorder || c.blocks[(item + Int3.down + Int3.up * o).ToLinearChunkScaleIndex()].meshSection.verts.Length > 1)
                                {
                                    success = true;
                                    c.trees[i].chunkSpaceOrigin = item + Int3.up * o;
                                    break;
                                }
                            }
                            if (!success)
                            {
                                c.trees.RemoveAt(i);
                            }
                        }
                    }
                    else
                    {
                        c.trees.RemoveAt(i);
                    }
                }
                for (int i = 0; i < c.scatteredObjects.Count; i++)
                {
                    var item = c.scatteredPositions[i];
                    if (item.ToLinearChunkScaleIndex() >= c.densityMap.Length)
                    {
                        MonoBehaviour.Destroy(c.scatteredObjects[i]);continue;
                    }
                    if (c.densityMap[item.ToLinearChunkScaleIndex()] >= directorInstance.densityToRealityBorder || c.blocks[item.ToLinearChunkScaleIndex()].meshSection.verts.Length > 1)
                        {
                            int o = 0;
                            bool success = false;
                            while (item.y + o < LandscapeTool.ChunkScale - 1)
                            {
                                o++;
                                if (c.densityMap[(item + Int3.up * o).ToLinearChunkScaleIndex()] < directorInstance.densityToRealityBorder || c.blocks[(item + Int3.up * o).ToLinearChunkScaleIndex()].meshSection.verts.Length <= 1)
                                {
                                    success = true;
                                    c.scatteredPositions[i] = item + Int3.up * o;
                                    c.scatteredObjects[i].transform.position = (c.key + directorInstance.globalOffset).AsVector3 * LandscapeTool.ChunkScale * LandscapeTool.BlockScale + c.scatteredPositions[i].AsVector3 * LandscapeTool.BlockScale + new Vector3(0.5f, 0, 0.5f) * LandscapeTool.BlockScale;
                                    break;
                                }
                            }
                            if (success) continue;
                            o = 0;
                            while (item.y + o > 0)
                            {
                                o--;
                                if (c.densityMap[(item + Int3.up * o).ToLinearChunkScaleIndex()] < directorInstance.densityToRealityBorder || c.blocks[(item + Int3.up * o).ToLinearChunkScaleIndex()].meshSection.verts.Length <= 1)
                                {
                                    success = true;
                                    c.scatteredPositions[i] = item + Int3.up * o;
                                    c.scatteredObjects[i].transform.position = (c.key + directorInstance.globalOffset).AsVector3 * LandscapeTool.ChunkScale * LandscapeTool.BlockScale + c.scatteredPositions[i].AsVector3 * LandscapeTool.BlockScale + new Vector3(0.5f, 0, 0.5f) * LandscapeTool.BlockScale;
                                    break;
                                }
                            }
                            if (!success)
                            {
                                c.scatteredPositions.RemoveAt(i);
                                MonoBehaviour.Destroy(c.scatteredObjects[i]);
                                c.scatteredObjects.RemoveAt(i);
                            }
                        }
                    if (item.y > 0)
                    {
                        if (c.densityMap[(item + Int3.down).ToLinearChunkScaleIndex()] < directorInstance.densityToRealityBorder || c.blocks[(item + Int3.down).ToLinearChunkScaleIndex()].meshSection.verts.Length <= 1)
                        {
                            int o = 0;
                            bool success = false;
                            o = 0;
                            while (item.y + o > 1)
                            {
                                o--;
                                if (c.densityMap[(item + Int3.down + Int3.up * o).ToLinearChunkScaleIndex()] >= directorInstance.densityToRealityBorder || c.blocks[(item + Int3.down + Int3.up * o).ToLinearChunkScaleIndex()].meshSection.verts.Length > 1)
                                {
                                    success = true;
                                    c.scatteredPositions[i] = item + Int3.up * o;
                                    c.scatteredObjects[i].transform.position = (c.key + directorInstance.globalOffset).AsVector3 * LandscapeTool.ChunkScale * LandscapeTool.BlockScale + c.scatteredPositions[i].AsVector3 * LandscapeTool.BlockScale + new Vector3(0.5f, 0, 0.5f) * LandscapeTool.BlockScale;
                                    break;
                                }
                            }
                            if (!success)
                            {
                                c.scatteredPositions.RemoveAt(i);
                                MonoBehaviour.Destroy(c.scatteredObjects[i]);
                                c.scatteredObjects.RemoveAt(i);
                            }
                        }
                    }
                    else
                    {
                        c.scatteredPositions.RemoveAt(i);
                        MonoBehaviour.Destroy(c.scatteredObjects[i]);
                        c.scatteredObjects.RemoveAt(i);
                    }
                }
                c.updateDecorations = true;
            }

            c.genState = 2;

            return c;
        }



        /*
            Foliage & Details Pass.
        Force chunk above to be there.
        Check all surface bottom full top free blocks and perform height above check.
        Set blocks & spawn details as necessary.
        => need new block types for simple grass (above & underwater) optionally more.
        The grass & other new mesh variants should follow the vertice count of the block and the shader block pass should contain a variable to distinguish the types to construct.
        + feature to randomly throw in gameObjects that are put into the biome collection if space checks out
         */

        /*
         !!! this needs to move to a separate logic layer. So that the decoration generation doesnt depend on itself on regeneration
         */


        public Chunk DecorativesAndDiagonalsPass(Chunk c)
        {
#if !Deactivate_Profiling
            Profiler.BeginSample("Decorate on " + c.ToString());
#endif
            if (directorInstance.generateDecoratives && !c.alreadyDecorated && c.updateDecorations)
            {
                ForceChunkUpToState(c, 2);
                Chunk chunkAbove = directorInstance.RequestPiece(c.key + Int3.up, false);
                ForceChunkUpToState(chunkAbove, 2);

                /*
                 Get solid with free above.
                Chance TreeInstance (types oak, pine, cactus) * chance
                Grass above or beneath water + chance
                Scatter objects instanciation (need a script on them, if chunk updates automatically raycast to reallign vertically otherwise delete, also brush to delete)
                 */

                for (int z = 0; z < LandscapeTool.ChunkScale; z++)
                {
                    for (int y = 0; y < LandscapeTool.ChunkScale; y++)
                    {
                        for (int x = 0; x < LandscapeTool.ChunkScale; x++)
                        {
                            var pos = new Int3(x, y, z);
                            var id = pos.ToLinearChunkScaleIndex();
                            if (c.densityMap[id] < directorInstance.densityToRealityBorder) continue;
                            var bio = directorInstance.biomes[c.primaryBiomeID[id]];
                            var posAbove = new Int3(x, y + 1, z);
                            var idAbove = posAbove.ToLinearChunkScaleIndex();
                            if (y < LandscapeTool.ChunkScale - 1)
                            {
                                if (c.densityMap[idAbove] >= directorInstance.densityToRealityBorder) continue;
                                if (c.blocks[idAbove].meshSection.verts.Length > 1) continue;
                            }
                            else
                            {
                                if (chunkAbove.densityMap[new Int3(x, 0, z).ToLinearChunkScaleIndex()] >= directorInstance.densityToRealityBorder) continue;
                            }

                            //until here we established: The current block exist in terms of density and the block above does not, nor in mesh of custom injection form.

                            if (Random.value <= bio.treeDensity && c.key.y * LandscapeTool.ChunkScale * LandscapeTool.BlockScale + y * LandscapeTool.BlockScale > directorInstance.oceanHeight - bio.treeWaterTollerance)//allow trees one meter beneath water                // Add a round stump block?
                            {
                                InsertTree(bio, c, chunkAbove, x, y, z);
                            }
                            else if (Random.value <= bio.grassDensity && bio.generateGrass)
                            {
                                var h = c.key.y * LandscapeTool.ChunkScale * LandscapeTool.BlockScale + (y + 1) * LandscapeTool.BlockScale;
                                bool underwater = h < directorInstance.oceanHeight;
                                if (underwater)
                                {
                                    int distanceToSurface = (int)(directorInstance.oceanHeight - h) * 3;
                                    //create tall grass "tree"
                                    if (distanceToSurface > 0) InsertUnderwaterGrass(bio, c, chunkAbove, x, y, z, distanceToSurface);
                                }
                                else
                                {
                                    if (y < LandscapeTool.ChunkScale - 1)
                                    {
                                        c.decoPositions.Add(posAbove);
                                        //c.decoBlocks.Add(underwater ? Block_Library.UnderwaterGrass : Block_Library.TallGrass);
                                        c.decoBlocks.Add(Block_Library.TallGrass);/*
                                        switch (directorInstance.diagonalMode)
                                    {
                                        case DiagonalToggle.disabled:
                                            c.decoBlocks.Last().blockShape = LandscapeTool.toybrickMode ? BlockShape_Library.toybrick : BlockShape_Library.block;
                                            break;
                                        case DiagonalToggle.enabledPlanarOnly:
                                            c.decoBlocks.Last().blockShape = BlockShape_Library.shortDiagonal;
                                            break;
                                        case DiagonalToggle.enabledAllDiagonals:
                                            c.decoBlocks.Last().blockShape = BlockShape_Library.allGeneratedVariants[new BlockShape_Library.ShapeRotationKey() { bs = BlockShape_Library.shortDiagonal, rotationsPerAxis = Int3.forward * 3 }];
                                            break;
                                        default:
                                            break;
                                    }*/
                                    }
                                    else 
                                    {
                                        chunkAbove.decoPositions.Add(new Int3(x, 0, z));
                                        //chunkAbove.decoBlocks.Add(underwater ? Block_Library.UnderwaterGrass : Block_Library.TallGrass);
                                        chunkAbove.decoBlocks.Add(Block_Library.TallGrass);/*
                                        switch (directorInstance.diagonalMode)
                                    {
                                        case DiagonalToggle.disabled:
                                            chunkAbove.decoBlocks.Last().blockShape = LandscapeTool.toybrickMode ? BlockShape_Library.toybrick : BlockShape_Library.block;
                                            break;
                                        case DiagonalToggle.enabledPlanarOnly:
                                            chunkAbove.decoBlocks.Last().blockShape = BlockShape_Library.shortDiagonal;
                                            break;
                                        case DiagonalToggle.enabledAllDiagonals:
                                            chunkAbove.decoBlocks.Last().blockShape = BlockShape_Library.allGeneratedVariants[new BlockShape_Library.ShapeRotationKey() { bs = BlockShape_Library.shortDiagonal, rotationsPerAxis = Int3.forward * 3 }];
                                            break;
                                        default:
                                            break;
                                    }*/
                                    }
                                }
                            }
                            else if (Random.value <= bio.scatterDensity && bio.scatterDecoratives.Count > 0)
                            {
                                var toScatter = bio.scatterDecoratives[Random.Range(0, bio.scatterDecoratives.Count)];
                                var gm = MonoBehaviour.Instantiate(toScatter, c.transform);
                                gm.SetActive(false);
                                pos += Int3.up;
                                var worldPos = (c.key + directorInstance.globalOffset).AsVector3 * LandscapeTool.ChunkScale * LandscapeTool.BlockScale + pos.AsVector3 * LandscapeTool.BlockScale + new Vector3(0.5f, 0, 0.5f) * LandscapeTool.BlockScale;
                                //gm.transform.position = worldPos-directorInstance.globalOffset.AsVector3 * LandscapeTool.ChunkScale * LandscapeTool.BlockScale;
                                gm.transform.position = worldPos;
                                gm.transform.Rotate(Vector3.up, 360f * Random.value);
                                c.scatteredObjects.Add(gm);
                                c.scatteredPositions.Add(pos);
                                if (directorInstance.barnacling)
                                {
                                    Bounds bounds = toScatter.GetComponent<MeshFilter>().sharedMesh.bounds;
                                    for (int i = 2; i < 2 + Random.value * 4; i++)
                                    {
                                        var offset = new Vector3(Random.value - 0.5f, 0, Random.value - 0.5f).normalized;
                                        var subGm = MonoBehaviour.Instantiate(toScatter, gm.transform);
                                        subGm.SetActive(false);
                                        subGm.transform.position = gm.transform.position + offset * Mathf.Abs(bounds.max.z - bounds.min.z) * gm.transform.lossyScale.z * .5f;//+Vector3.down *Mathf.Abs(bounds.max.y-bounds.min.y)*gm.transform.lossyScale.z * 1/3;
                                        subGm.transform.Rotate(Vector3.up, 360f * Random.value);
                                        subGm.transform.localScale = Vector3.one * .5f;
                                    }
                                }
                            }
                        }
                    }
                }
                c.alreadyDecorated = true;
            }

            /* Optional Diagonal Pass*/
            DealWithTheDiagonals(c);

            c.genState = 3;
#if !Deactivate_Profiling
            Profiler.EndSample();
#endif
            return c;
        }

        /*
         Now works with a parallel system, which measn the voxelized trees will need a custom implementation of brush add, aswell as brush later too.  Which also means a custom diagonals pass unless make it perfeclty replacable. The later should be the goal
         */
        public void InsertTree(Biome bio, Chunk c, Chunk chunkAbove, int x, int y, int z)
        {
            if (bio.treeType == TreeType.none) return;
            int height = 1;
            var pos = new Int3(x, y, z);
            TreeInstance tree = new TreeInstance();
            c.trees.Add(tree);
            tree.chunkSpaceOrigin = pos;
            switch (bio.treeType)
            {
                case TreeType.oak://random height from five to fifteen blocks with a stretched ellipsoid scaled to half height set to peak with top.  optional branching logic with diagonal later.
                    height = Random.Range(5, 16);
                    for (int i = 0; i < height; i++)
                    {
                       // if (CheckUp(c, pos, i)) { height = i; break; }
                   if(pos.y + 1 < LandscapeTool.ChunkScale)     if (c.blocks[(pos + Int3.up).ToLinearChunkScaleIndex()].blockShape.covers != BlockSides.none) { height = i; break; }
                        tree.decoPositions.Add(new Int3(0, i, 0));
                        tree.decoBlocks.Add(Block_Library.Wood);
                    }
                    if (height == 0) return;
                    directorInstance.helper.SetActive(true);
                    directorInstance.helper.GetComponent<MeshCollider>().sharedMesh = directorInstance.Sphere;
                    directorInstance.helper.transform.position = Vector3.up * LandscapeTool.BlockScale * height * .6f;
                    directorInstance.helper.transform.localScale = Vector3.one * .5f * height;

                    directorInstance.helper.GetComponent<MeshCollider>().enabled = true;
                    var vox = VoxelizeByCollision.Run(directorInstance.helper);
                    directorInstance.helper.GetComponent<MeshCollider>().enabled = false;
                    for (int i = 0; i < vox.Count; i++)
                    {
                        if (tree.decoPositions.Contains(vox[i].ToInt3() + Int3.up * height)) continue;
                        tree.decoPositions.Add(vox[i].ToInt3() + Int3.up * height);
                        tree.decoBlocks.Add(Block_Library.Leaves);
                    }

                    directorInstance.helper.transform.localScale = Vector3.one;
                    //directorInstance.helper.SetActive(false);
                    break;
                case TreeType.pine://random height from five to fifteen blocks with cone starting at block three and scaling up to tip
                    height = Random.Range(5, 16);
                    for (int i = 0; i < height; i++)
                    {
                        // if (CheckUp(c, pos, i)) { height = i; break; }
                        if (pos.y + 1 < LandscapeTool.ChunkScale) if (c.blocks[(pos + Int3.up).ToLinearChunkScaleIndex()].blockShape.covers != BlockSides.none) { height = i; break; }
                        tree.decoPositions.Add(new Int3(0, i, 0));
                        tree.decoBlocks.Add(Block_Library.Wood);
                    }
                    if (height == 0) return;
                    directorInstance.helper.SetActive(true);
                    directorInstance.helper.GetComponent<MeshCollider>().sharedMesh = directorInstance.Cone;
                    directorInstance.helper.transform.position = Vector3.up * LandscapeTool.BlockScale;
                    directorInstance.helper.transform.localScale = new Vector3(height * .5f, height, height * .5f);

                    directorInstance.helper.GetComponent<MeshCollider>().enabled = true;
                    var xov = VoxelizeByCollision.Run(directorInstance.helper);
                    directorInstance.helper.GetComponent<MeshCollider>().enabled = false;
                    for (int i = 0; i < xov.Count; i++)
                    {
                        if (tree.decoPositions.Contains(xov[i].ToInt3() + Int3.up * 3)) continue;
                        tree.decoPositions.Add(xov[i].ToInt3() + Int3.up * 3);
                        tree.decoBlocks.Add(Block_Library.Leaves);
                    }

                    directorInstance.helper.transform.localScale = Vector3.one;
                    //directorInstance.helper.SetActive(false);
                    break;
                case TreeType.cactus://one to seven blocks high with an optional arm any four directions at step height three, which forces the height to at least five
                    bool arm = Random.value > 0.9f;
                    if (x < 3 || y < 1 || z < 3 || x > LandscapeTool.ChunkScale - 3 || y > LandscapeTool.ChunkScale - 6 || z > LandscapeTool.ChunkScale - 3) arm = false;
                    height = (int)Mathf.Clamp(Random.value * 7.1f, arm ? 5 : 1, 7);

                    for (int i = 0; i < height; i++)
                    {
                        // if (CheckUp(c, pos, i)) { height = i; break; }
                        if (pos.y + 1 < LandscapeTool.ChunkScale) if (c.blocks[(pos+Int3.up).ToLinearChunkScaleIndex()].blockShape.covers != BlockSides.none) { height = i; break; }
                        tree.decoPositions.Add(new Int3(0, i, 0));
                        tree.decoBlocks.Add(Block_Library.Cactus);
                    }
                    if (height == 0) return;
                    if (arm)
                    {
                        switch (Random.Range(0, 4))//already checked bounds, there is enough space for the arm
                        {
                            case 1:
                                tree.decoPositions.Add(new Int3(1, 2, 0));
                                tree.decoPositions.Add(new Int3(2, 2, 0));
                                tree.decoPositions.Add(new Int3(2, 3, 0));
                                tree.decoPositions.Add(new Int3(2, 4, 0));

                                tree.decoBlocks.Add(Block_Library.Cactus);
                                tree.decoBlocks.Add(Block_Library.Cactus);
                                tree.decoBlocks.Add(Block_Library.Cactus);
                                tree.decoBlocks.Add(Block_Library.Cactus);

                                break;
                            case 2:
                                tree.decoPositions.Add(new Int3(1, 2, 0));
                                tree.decoPositions.Add(new Int3(2, 2, 0));
                                tree.decoPositions.Add(new Int3(2, 3, 0));
                                tree.decoPositions.Add(new Int3(2, 4, 0));

                                tree.decoBlocks.Add(Block_Library.Cactus);
                                tree.decoBlocks.Add(Block_Library.Cactus);
                                tree.decoBlocks.Add(Block_Library.Cactus);
                                tree.decoBlocks.Add(Block_Library.Cactus);
                                break;
                            case 3:

                                tree.decoPositions.Add(new Int3(0, 2, 1));
                                tree.decoPositions.Add(new Int3(0, 2, 2));
                                tree.decoPositions.Add(new Int3(0, 3, 2));
                                tree.decoPositions.Add(new Int3(0, 4, 2));

                                tree.decoBlocks.Add(Block_Library.Cactus);
                                tree.decoBlocks.Add(Block_Library.Cactus);
                                tree.decoBlocks.Add(Block_Library.Cactus);
                                tree.decoBlocks.Add(Block_Library.Cactus);
                                break;
                            default:

                                tree.decoPositions.Add(new Int3(0, 2, 1));
                                tree.decoPositions.Add(new Int3(0, 2, 2));
                                tree.decoPositions.Add(new Int3(0, 3, 2));
                                tree.decoPositions.Add(new Int3(0, 4, 2));

                                tree.decoBlocks.Add(Block_Library.Cactus);
                                tree.decoBlocks.Add(Block_Library.Cactus);
                                tree.decoBlocks.Add(Block_Library.Cactus);
                                tree.decoBlocks.Add(Block_Library.Cactus);
                                break;
                        }
                    }
                    break;
            }
            tree.decoBlocks[0].blockShape = BlockShape_Library.foot;
        }

        public void InsertUnderwaterGrass(Biome bio, Chunk c, Chunk chunkAbove, int x, int y, int z, int distanceToSurface)
        {
            int height = distanceToSurface;
            var pos = new Int3(x, y+1, z);//plus one as we start above the actual cube
            TreeInstance tree = new TreeInstance();
            c.trees.Add(tree);
            tree.chunkSpaceOrigin = pos;
            for (int i = 0; i < height; i++)
            {
                if (CheckUp(c,pos,i)) { height = i; break; }
                tree.decoPositions.Add(new Int3(0, i, 0));
                tree.decoBlocks.Add(Block_Library.UnderwaterGrass);
            }
            if (height == 0) return;
            //tree.decoBlocks[0].blockShape = BlockShape_Library.foot;
        }

        public bool CheckUp(Chunk c, Int3 pos, int i)
        {
            //if(i==0)Debug.Log(directorInstance.GetBlock(c.key * LandscapeTool.ChunkScale + (pos + Int3.up * i)).block.blockShape.covers);
            //return directorInstance.GetBlock(c.key * LandscapeTool.ChunkScale + (pos + Int3.up * i)).block.blockShape.meshSections[0].verts.Length==0;
            //return directorInstance.GetBlock(c.key * LandscapeTool.ChunkScale + (pos + Int3.up * (i+1))).block.matter != Matter_Library.Air;
            return directorInstance.GetBlock(c.key * LandscapeTool.ChunkScale + (pos + Int3.up * (i + 1))).block.blockShape.covers != BlockSides.none;
            //return false;
        }


        public Chunk BlocksToMesh(Chunk c)
        {
#if !Deactivate_Profiling
            Profiler.BeginSample("BlocksToMesh on " + c.ToString());
#endif
            ForceChunkUpToState(c, 3);
            if (c.updateAllBlocks)
            {
#if Deactivate_Async
                for (int z = 0; z < LandscapeTool.ChunkScale; z++)
                {
                    for (int y = 0; y < LandscapeTool.ChunkScale; y++)
                    {
                        for (int x = 0; x < LandscapeTool.ChunkScale; x++)
                        {
                            Int3 index = new Int3(x, y, z);
                            c.blocks[index.LocalPosToLinearID()].UpdateMeshSection(index, c);
                        }
                    }
                }
#else
                Parallel.For(0, LandscapeTool.ChunkScale, z =>
                {
                    for (int y = 0; y < LandscapeTool.ChunkScale; y++)
                    {
                        for (int x = 0; x < LandscapeTool.ChunkScale; x++)
                        {
                            Int3 index = new Int3(x, y, z);
                            c.blocks[index.LocalPosToLinearID()].UpdateMeshSection(index, c);
                        }
                    }
                });
#endif
                c.updateAllBlocks = false;
            }
            else
            {
                foreach (var item in c.blocksToUpdate)
                {
                    c.blocks[item.ToLinearChunkScaleIndex()].UpdateMeshSection(item, c);
                }
                c.blocksToUpdate.Clear();
            }
            if (c.updateDecorations)
            {
                for (int i = 0; i < c.decoBlocks.Count; i++)
                {
                    c.decoBlocks[i].UpdateMeshSection(c.decoPositions[i], c);
                }
                foreach (var item in c.trees)
                {
                    for (int i = 0; i < item.decoPositions.Count; i++)
                    {
                        //*//goes out of bounds beyond one chunk, skipping reduction in this instance
                        //item.decoBlocks[i].UpdateMeshSection(item.chunkSpaceOrigin + item.decoPositions[i],c);
                        item.decoBlocks[i].meshSection = VerTriSides.CombineShape(item.decoBlocks[i].blockShape, BlockSides.none, (item.chunkSpaceOrigin + item.decoPositions[i]).ToVector3() * LandscapeTool.BlockScale);
                    }
                }
                c.updateDecorations = false;
            }

            c.genState = 4;
#if !Deactivate_Profiling
            Profiler.EndSample();
#endif
            return c;
        }



        public Chunk MeshCombinationAndColoring(Chunk c)
        {
#if !Deactivate_Profiling
            Profiler.BeginSample("MeshCombinationAndColoring on " + c.ToString());
#endif
            ForceChunkUpToState(c, 4);

            VerTriSides.ChunkToMesh(c, true);
            c.mf.sharedMesh = c.m;
            if (c.m.vertexCount > 0)
            {
                c.mc.enabled = true;
                c.mc.sharedMesh = c.m;
            }
            else
            {
                c.mc.enabled = false;
            }

            foreach (var item in c.scatteredObjects)
            {
                if (item == null) continue;
                item.SetActive(true);
                foreach (Transform child in item.transform)
                {
                    if (child == null) continue;
                    child.gameObject.SetActive(true);
                }
            }

            c.genState = 5;
            //keep the bellow on the last method
#if Deactivate_Async
            directorInstance.chunksUnderConstruction.Remove(c);
#else
            var chunks = directorInstance.chunksUnderConstruction.ToList();
            chunks.Remove(c);
            directorInstance.chunksUnderConstruction = new ConcurrentBag<Chunk>();
            foreach (var item in chunks)
            {
                directorInstance.chunksUnderConstruction.Add(item);
            }
#endif
#if !Deactivate_Profiling
            Profiler.EndSample();
#endif

            return c;
        }



#if Deactivate_Async
   void DealWithTheDiagonals(Chunk c)
        {
            if (directorInstance.diagonalMode == DiagonalToggle.enabledPlanarOnly)
            {
                for (int z = 0; z < LandscapeTool.ChunkScale; z++)
                {
                    for (int y = 0; y < LandscapeTool.ChunkScale; y++)
                    {
                        for (int x = 0; x < LandscapeTool.ChunkScale; x++)
                        {
                            var pos = new Int3(x, y, z);
                            var block = c.blocks[pos.LocalPosToLinearID()];
                            if (block.blockShape == null) continue;
                            if (block.blockShape == BlockShape_Library.empty) continue;
                            if (block.blockShape.covers == BlockSides.none) continue;
                            var neighboursCovering = block.CalcNeighbourCovers(pos, c);

                            if ((neighboursCovering & BlockSides.x) == BlockSides.x || (neighboursCovering & BlockSides.z) == BlockSides.z)
                            {
                                continue;
                            }

                            if ((neighboursCovering & BlockSides.front) != 0)
                            {
                                if ((neighboursCovering & BlockSides.right) != 0)
                                {
                                    block.blockShape = BlockShape_Library.allGeneratedVariants[new BlockShape_Library.ShapeRotationKey() { bs = BlockShape_Library.shortDiagonal, rotationsPerAxis = new Int3(0, 2, 0) }];
                                }
                                else if ((neighboursCovering & BlockSides.left) != 0)
                                {
                                    block.blockShape = BlockShape_Library.allGeneratedVariants[new BlockShape_Library.ShapeRotationKey() { bs = BlockShape_Library.shortDiagonal, rotationsPerAxis = new Int3(0, 1, 0) }];
                                }
                            }
                            else if ((neighboursCovering & BlockSides.right) != 0)
                            {
                                if ((neighboursCovering & BlockSides.back) != 0)
                                {
                                    block.blockShape = BlockShape_Library.allGeneratedVariants[new BlockShape_Library.ShapeRotationKey() { bs = BlockShape_Library.shortDiagonal, rotationsPerAxis = new Int3(0, 3, 0) }];
                                }
                                else if ((neighboursCovering & BlockSides.front) != 0)
                                {
                                    block.blockShape = BlockShape_Library.allGeneratedVariants[new BlockShape_Library.ShapeRotationKey() { bs = BlockShape_Library.shortDiagonal, rotationsPerAxis = new Int3(0, 2, 0) }];
                                }
                            }
                            else if ((neighboursCovering & BlockSides.back) != 0)
                            {
                                if ((neighboursCovering & BlockSides.left) != 0)
                                {
                                    block.blockShape = BlockShape_Library.allGeneratedVariants[new BlockShape_Library.ShapeRotationKey() { bs = BlockShape_Library.shortDiagonal, rotationsPerAxis = new Int3(0, 0, 0) }];
                                }
                                else if ((neighboursCovering & BlockSides.right) != 0)
                                {
                                    block.blockShape = BlockShape_Library.allGeneratedVariants[new BlockShape_Library.ShapeRotationKey() { bs = BlockShape_Library.shortDiagonal, rotationsPerAxis = new Int3(0, 3, 0) }];
                                }
                            }
                            else
                            {
                                if ((neighboursCovering & BlockSides.front) != 0)
                                {
                                    block.blockShape = BlockShape_Library.allGeneratedVariants[new BlockShape_Library.ShapeRotationKey() { bs = BlockShape_Library.shortDiagonal, rotationsPerAxis = new Int3(0, 1, 0) }];
                                }
                                else if ((neighboursCovering & BlockSides.back) != 0)
                                {
                                    block.blockShape = BlockShape_Library.allGeneratedVariants[new BlockShape_Library.ShapeRotationKey() { bs = BlockShape_Library.shortDiagonal, rotationsPerAxis = new Int3(0, 0, 0) }];
                                }
                            }
                        }
                    }
                }
            }
            else if (directorInstance.diagonalMode == DiagonalToggle.enabledAllDiagonals)
            {
                for (int z = 0; z < LandscapeTool.ChunkScale; z++)
                {
                    for (int y = 0; y < LandscapeTool.ChunkScale; y++)
                    {
                        for (int x = 0; x < LandscapeTool.ChunkScale; x++)
                        {
                            var pos = new Int3(x, y, z);
                            var block = c.blocks[pos.LocalPosToLinearID()];
                            if (block.blockShape == null) continue;
                            if (block.blockShape == BlockShape_Library.empty) continue;
                            if (block.blockShape.covers == BlockSides.none) continue;
                            var neighboursCovering = block.CalcNeighbourCovers(pos, c, true);
                            //if ((neighboursCovering & BlockSides.x) == BlockSides.x || (neighboursCovering & BlockSides.y) == BlockSides.y || (neighboursCovering & BlockSides.z) == BlockSides.z) return;//no possible diagonal case if opposing sites connect
                            var neighborCoverCount = neighboursCovering.Count();
                            if (neighborCoverCount > 5) continue;

                            //hexagonal corners
                            if (neighboursCovering == BlockSides.lll && (neighboursCovering & BlockSides.ooo) == 0)
                            {
                                block.blockShape = BlockShape_Library.allGeneratedVariants[new BlockShape_Library.ShapeRotationKey() { bs = BlockShape_Library.hexCorner, rotationsPerAxis = new Int3(1, 2, 0) }]; continue;
                            }
                            if (neighboursCovering == BlockSides.llo && (neighboursCovering & BlockSides.ool) == 0)
                            {
                                block.blockShape = BlockShape_Library.allGeneratedVariants[new BlockShape_Library.ShapeRotationKey() { bs = BlockShape_Library.hexCorner, rotationsPerAxis = new Int3(1, 3, 0) }]; continue;
                            }
                            if (neighboursCovering == BlockSides.oll && (neighboursCovering & BlockSides.loo) == 0)
                            {
                                block.blockShape = BlockShape_Library.allGeneratedVariants[new BlockShape_Library.ShapeRotationKey() { bs = BlockShape_Library.hexCorner, rotationsPerAxis = new Int3(2, 0, 0) }]; continue;
                            }
                            if (neighboursCovering == BlockSides.olo && (neighboursCovering & BlockSides.lol) == 0)
                            {
                                block.blockShape = BlockShape_Library.allGeneratedVariants[new BlockShape_Library.ShapeRotationKey() { bs = BlockShape_Library.hexCorner, rotationsPerAxis = new Int3(1, 0, 0) }]; continue;
                            }
                            if (neighboursCovering == BlockSides.lol && (neighboursCovering & BlockSides.olo) == 0)
                            {
                                block.blockShape = BlockShape_Library.allGeneratedVariants[new BlockShape_Library.ShapeRotationKey() { bs = BlockShape_Library.hexCorner, rotationsPerAxis = new Int3(0, 2, 0) }]; continue;
                            }
                            if (neighboursCovering == BlockSides.loo && (neighboursCovering & BlockSides.oll) == 0)
                            {
                                block.blockShape = BlockShape_Library.allGeneratedVariants[new BlockShape_Library.ShapeRotationKey() { bs = BlockShape_Library.hexCorner, rotationsPerAxis = new Int3(0, 3, 0) }]; continue;
                            }
                            if (neighboursCovering == BlockSides.ool && (neighboursCovering & BlockSides.llo) == 0)
                            {
                                block.blockShape = BlockShape_Library.allGeneratedVariants[new BlockShape_Library.ShapeRotationKey() { bs = BlockShape_Library.hexCorner, rotationsPerAxis = new Int3(0, 1, 0) }]; continue;
                            }
                            if (neighboursCovering == BlockSides.ooo && (neighboursCovering & BlockSides.lll) == 0)
                            {
                                block.blockShape = BlockShape_Library.allGeneratedVariants[new BlockShape_Library.ShapeRotationKey() { bs = BlockShape_Library.hexCorner, rotationsPerAxis = new Int3(0, 0, 0) }]; continue;
                            }

                            //45° diagonals
                            //with top
                            if ((neighboursCovering & BlockSides.topFront) == BlockSides.topFront && (neighboursCovering & BlockSides.bottomBack) == 0)
                            {
                                block.blockShape = BlockShape_Library.allGeneratedVariants[new BlockShape_Library.ShapeRotationKey() { bs = BlockShape_Library.shortDiagonal, rotationsPerAxis = new Int3(1, 1, 0) }]; continue;
                            }
                            if ((neighboursCovering & BlockSides.topRight) == BlockSides.topRight && (neighboursCovering & BlockSides.bottomLeft) == 0)
                            {
                                block.blockShape = BlockShape_Library.allGeneratedVariants[new BlockShape_Library.ShapeRotationKey() { bs = BlockShape_Library.shortDiagonal, rotationsPerAxis = new Int3(1, 2, 0) }]; continue;
                            }
                            if ((neighboursCovering & BlockSides.topBack) == BlockSides.topBack && (neighboursCovering & BlockSides.bottomFront) == 0)
                            {
                                block.blockShape = BlockShape_Library.allGeneratedVariants[new BlockShape_Library.ShapeRotationKey() { bs = BlockShape_Library.shortDiagonal, rotationsPerAxis = new Int3(1, 3, 0) }]; continue;
                            }
                            if ((neighboursCovering & BlockSides.topLeft) == BlockSides.topLeft && (neighboursCovering & BlockSides.bottomRight) == 0)
                            {
                                block.blockShape = BlockShape_Library.allGeneratedVariants[new BlockShape_Library.ShapeRotationKey() { bs = BlockShape_Library.shortDiagonal, rotationsPerAxis = new Int3(1, 0, 0) }]; continue;
                            }
                            //with back
                            if ((neighboursCovering & BlockSides.bottomFront) == BlockSides.bottomFront && (neighboursCovering & BlockSides.topBack) == 0)
                            {
                                block.blockShape = BlockShape_Library.allGeneratedVariants[new BlockShape_Library.ShapeRotationKey() { bs = BlockShape_Library.shortDiagonal, rotationsPerAxis = new Int3(3, 1, 0) }]; continue;
                            }
                            if ((neighboursCovering & BlockSides.bottomRight) == BlockSides.bottomRight && (neighboursCovering & BlockSides.topLeft) == 0)
                            {
                                block.blockShape = BlockShape_Library.allGeneratedVariants[new BlockShape_Library.ShapeRotationKey() { bs = BlockShape_Library.shortDiagonal, rotationsPerAxis = new Int3(3, 2, 0) }]; continue;
                            }
                            if ((neighboursCovering & BlockSides.bottomBack) == BlockSides.bottomBack && (neighboursCovering & BlockSides.topFront) == 0)
                            {
                                block.blockShape = BlockShape_Library.allGeneratedVariants[new BlockShape_Library.ShapeRotationKey() { bs = BlockShape_Library.shortDiagonal, rotationsPerAxis = new Int3(3, 3, 0) }]; continue;
                            }
                            if ((neighboursCovering & BlockSides.bottomLeft) == BlockSides.bottomLeft && (neighboursCovering & BlockSides.topRight) == 0)
                            {
                                block.blockShape = BlockShape_Library.allGeneratedVariants[new BlockShape_Library.ShapeRotationKey() { bs = BlockShape_Library.shortDiagonal, rotationsPerAxis = new Int3(3, 0, 0) }]; continue;
                            }
                            //X Z planar
                            if ((neighboursCovering & BlockSides.frontRight) == BlockSides.frontRight && (neighboursCovering & BlockSides.backLeft) == 0)
                            {
                                block.blockShape = BlockShape_Library.allGeneratedVariants[new BlockShape_Library.ShapeRotationKey() { bs = BlockShape_Library.shortDiagonal, rotationsPerAxis = new Int3(0, 2, 0) }]; continue;
                            }
                            if ((neighboursCovering & BlockSides.leftFront) == BlockSides.leftFront && (neighboursCovering & BlockSides.rightBack) == 0)
                            {
                                block.blockShape = BlockShape_Library.allGeneratedVariants[new BlockShape_Library.ShapeRotationKey() { bs = BlockShape_Library.shortDiagonal, rotationsPerAxis = new Int3(0, 1, 0) }]; continue;
                            }
                            if ((neighboursCovering & BlockSides.rightBack) == BlockSides.rightBack && (neighboursCovering & BlockSides.leftFront) == 0)
                            {
                                block.blockShape = BlockShape_Library.allGeneratedVariants[new BlockShape_Library.ShapeRotationKey() { bs = BlockShape_Library.shortDiagonal, rotationsPerAxis = new Int3(0, 3, 0) }]; continue;
                            }
                            if ((neighboursCovering & BlockSides.frontRight) == BlockSides.frontRight && (neighboursCovering & BlockSides.backLeft) == 0)
                            {
                                block.blockShape = BlockShape_Library.allGeneratedVariants[new BlockShape_Library.ShapeRotationKey() { bs = BlockShape_Library.shortDiagonal, rotationsPerAxis = new Int3(0, 2, 0) }]; continue;
                            }
                            if ((neighboursCovering & BlockSides.backLeft) == BlockSides.backLeft && (neighboursCovering & BlockSides.frontRight) == 0)
                            {
                                block.blockShape = BlockShape_Library.allGeneratedVariants[new BlockShape_Library.ShapeRotationKey() { bs = BlockShape_Library.shortDiagonal, rotationsPerAxis = new Int3(0, 0, 0) }]; continue;
                            }
                            if ((neighboursCovering & BlockSides.rightBack) == BlockSides.rightBack && (neighboursCovering & BlockSides.leftFront) == 0)
                            {
                                block.blockShape = BlockShape_Library.allGeneratedVariants[new BlockShape_Library.ShapeRotationKey() { bs = BlockShape_Library.shortDiagonal, rotationsPerAxis = new Int3(0, 3, 0) }]; continue;
                            }
                            if ((neighboursCovering & BlockSides.leftFront) == BlockSides.leftFront && (neighboursCovering & BlockSides.rightBack) == 0)
                            {
                                block.blockShape = BlockShape_Library.allGeneratedVariants[new BlockShape_Library.ShapeRotationKey() { bs = BlockShape_Library.shortDiagonal, rotationsPerAxis = new Int3(0, 1, 0) }]; continue;
                            }
                            if ((neighboursCovering & BlockSides.backLeft) == BlockSides.backLeft && (neighboursCovering & BlockSides.frontRight) == 0)
                            {
                                block.blockShape = BlockShape_Library.allGeneratedVariants[new BlockShape_Library.ShapeRotationKey() { bs = BlockShape_Library.shortDiagonal, rotationsPerAxis = new Int3(0, 0, 0) }]; continue;
                            }
                        }
                    }
                }
            }
        }
#else
        void DealWithTheDiagonals(Chunk c)
        {
            if (directorInstance.diagonalMode == DiagonalToggle.enabledPlanarOnly)
            {
                Parallel.For(0, LandscapeTool.ChunkScale, z =>
                {
                    Parallel.For(0, LandscapeTool.ChunkScale, y =>
                    {
                        Parallel.For(0, LandscapeTool.ChunkScale, x =>
                        {
                            var pos = new Int3(x, y, z);
                            var block = c.blocks[pos.LocalPosToLinearID()];
                            if (block.blockShape == null) return;
                            if (block.blockShape == BlockShape_Library.empty) return;
                            if (block.blockShape.covers == BlockSides.none) return;
                            var neighboursCovering = block.CalcNeighbourCovers(pos, c);

                            if ((neighboursCovering & BlockSides.x) == BlockSides.x || (neighboursCovering & BlockSides.z) == BlockSides.z)
                            {
                                return;
                            }

                            if ((neighboursCovering & BlockSides.front) != 0)
                            {
                                if ((neighboursCovering & BlockSides.right) != 0)
                                {
                                    block.blockShape = BlockShape_Library.allGeneratedVariants[new BlockShape_Library.ShapeRotationKey() { bs = BlockShape_Library.shortDiagonal, rotationsPerAxis = new Int3(0, 2, 0) }];
                                }
                                else if ((neighboursCovering & BlockSides.left) != 0)
                                {
                                    block.blockShape = BlockShape_Library.allGeneratedVariants[new BlockShape_Library.ShapeRotationKey() { bs = BlockShape_Library.shortDiagonal, rotationsPerAxis = new Int3(0, 1, 0) }];
                                }
                            }
                            else if ((neighboursCovering & BlockSides.right) != 0)
                            {
                                if ((neighboursCovering & BlockSides.back) != 0)
                                {
                                    block.blockShape = BlockShape_Library.allGeneratedVariants[new BlockShape_Library.ShapeRotationKey() { bs = BlockShape_Library.shortDiagonal, rotationsPerAxis = new Int3(0, 3, 0) }];
                                }
                                else if ((neighboursCovering & BlockSides.front) != 0)
                                {
                                    block.blockShape = BlockShape_Library.allGeneratedVariants[new BlockShape_Library.ShapeRotationKey() { bs = BlockShape_Library.shortDiagonal, rotationsPerAxis = new Int3(0, 2, 0) }];
                                }
                            }
                            else if ((neighboursCovering & BlockSides.back) != 0)
                            {
                                if ((neighboursCovering & BlockSides.left) != 0)
                                {
                                    block.blockShape = BlockShape_Library.allGeneratedVariants[new BlockShape_Library.ShapeRotationKey() { bs = BlockShape_Library.shortDiagonal, rotationsPerAxis = new Int3(0, 0, 0) }];
                                }
                                else if ((neighboursCovering & BlockSides.right) != 0)
                                {
                                    block.blockShape = BlockShape_Library.allGeneratedVariants[new BlockShape_Library.ShapeRotationKey() { bs = BlockShape_Library.shortDiagonal, rotationsPerAxis = new Int3(0, 3, 0) }];
                                }
                            }
                            else
                            {
                                if ((neighboursCovering & BlockSides.front) != 0)
                                {
                                    block.blockShape = BlockShape_Library.allGeneratedVariants[new BlockShape_Library.ShapeRotationKey() { bs = BlockShape_Library.shortDiagonal, rotationsPerAxis = new Int3(0, 1, 0) }];
                                }
                                else if ((neighboursCovering & BlockSides.back) != 0)
                                {
                                    block.blockShape = BlockShape_Library.allGeneratedVariants[new BlockShape_Library.ShapeRotationKey() { bs = BlockShape_Library.shortDiagonal, rotationsPerAxis = new Int3(0, 0, 0) }];
                                }
                            }

                        });
                    });
                });
            }
            else if (directorInstance.diagonalMode == DiagonalToggle.enabledAllDiagonals)
            {
                Parallel.For(0, LandscapeTool.ChunkScale, z =>
                {
                    Parallel.For(0, LandscapeTool.ChunkScale, y =>
                    {
                        Parallel.For(0, LandscapeTool.ChunkScale, x =>
                        {
                            var pos = new Int3(x, y, z);
                            var block = c.blocks[pos.LocalPosToLinearID()];
                            if (block.blockShape == null) return;
                            if (block.blockShape == BlockShape_Library.empty) return;
                            if (block.blockShape.covers == BlockSides.none) return;
                            var neighboursCovering = block.CalcNeighbourCovers(pos, c,true);
                            //if ((neighboursCovering & BlockSides.x) == BlockSides.x || (neighboursCovering & BlockSides.y) == BlockSides.y || (neighboursCovering & BlockSides.z) == BlockSides.z) return;//no possible diagonal case if opposing sites connect
                            var neighborCoverCount = neighboursCovering.Count();
                            if (neighborCoverCount > 5) return;

                            //hexagonal corners
                            if (neighboursCovering == BlockSides.lll && (neighboursCovering & BlockSides.ooo)==0)
                            {
                                block.blockShape = BlockShape_Library.allGeneratedVariants[new BlockShape_Library.ShapeRotationKey() { bs = BlockShape_Library.hexCorner, rotationsPerAxis = new Int3(1, 2, 0) }]; return;
                            }
                            if (neighboursCovering == BlockSides.llo && (neighboursCovering & BlockSides.ool) == 0)
                            {
                                block.blockShape = BlockShape_Library.allGeneratedVariants[new BlockShape_Library.ShapeRotationKey() { bs = BlockShape_Library.hexCorner, rotationsPerAxis = new Int3(1, 3, 0) }]; return;
                            }
                            if (neighboursCovering == BlockSides.oll && (neighboursCovering & BlockSides.loo) == 0)
                            {
                                block.blockShape = BlockShape_Library.allGeneratedVariants[new BlockShape_Library.ShapeRotationKey() { bs = BlockShape_Library.hexCorner, rotationsPerAxis = new Int3(2, 0, 0) }]; return;
                            }
                            if (neighboursCovering == BlockSides.olo && (neighboursCovering & BlockSides.lol) == 0)
                            {
                                block.blockShape = BlockShape_Library.allGeneratedVariants[new BlockShape_Library.ShapeRotationKey() { bs = BlockShape_Library.hexCorner, rotationsPerAxis = new Int3(1, 0, 0) }]; return;
                            }
                            if (neighboursCovering == BlockSides.lol && (neighboursCovering & BlockSides.olo) == 0)
                            {
                                block.blockShape = BlockShape_Library.allGeneratedVariants[new BlockShape_Library.ShapeRotationKey() { bs = BlockShape_Library.hexCorner, rotationsPerAxis = new Int3(0, 2, 0) }]; return;
                            }
                            if (neighboursCovering == BlockSides.loo && (neighboursCovering & BlockSides.oll) == 0)
                            {
                                block.blockShape = BlockShape_Library.allGeneratedVariants[new BlockShape_Library.ShapeRotationKey() { bs = BlockShape_Library.hexCorner, rotationsPerAxis = new Int3(0, 3, 0) }]; return;
                            }
                            if (neighboursCovering == BlockSides.ool && (neighboursCovering & BlockSides.llo) == 0)
                            {
                                block.blockShape = BlockShape_Library.allGeneratedVariants[new BlockShape_Library.ShapeRotationKey() { bs = BlockShape_Library.hexCorner, rotationsPerAxis = new Int3(0, 1, 0) }]; return;
                            }
                            if (neighboursCovering == BlockSides.ooo && (neighboursCovering & BlockSides.lll) == 0)
                            {
                                block.blockShape = BlockShape_Library.allGeneratedVariants[new BlockShape_Library.ShapeRotationKey() { bs = BlockShape_Library.hexCorner, rotationsPerAxis = new Int3(0, 0, 0) }]; return;
                            }

                            //45° diagonals
                                //with top
                            if ((neighboursCovering & BlockSides.topFront) == BlockSides.topFront && (neighboursCovering & BlockSides.bottomBack) == 0)
                            {
                                block.blockShape = BlockShape_Library.allGeneratedVariants[new BlockShape_Library.ShapeRotationKey() { bs = BlockShape_Library.shortDiagonal, rotationsPerAxis = new Int3(1, 1, 0) }]; return;
                            }
                            if ((neighboursCovering & BlockSides.topRight) == BlockSides.topRight && (neighboursCovering & BlockSides.bottomLeft) == 0)
                            {
                                block.blockShape = BlockShape_Library.allGeneratedVariants[new BlockShape_Library.ShapeRotationKey() { bs = BlockShape_Library.shortDiagonal, rotationsPerAxis = new Int3(1, 2, 0) }]; return;
                            }
                            if ((neighboursCovering & BlockSides.topBack) == BlockSides.topBack && (neighboursCovering & BlockSides.bottomFront) == 0)
                            {
                                block.blockShape = BlockShape_Library.allGeneratedVariants[new BlockShape_Library.ShapeRotationKey() { bs = BlockShape_Library.shortDiagonal, rotationsPerAxis = new Int3(1, 3, 0) }]; return;
                            }
                            if ((neighboursCovering & BlockSides.topLeft) == BlockSides.topLeft && (neighboursCovering & BlockSides.bottomRight) == 0)
                            {
                                block.blockShape = BlockShape_Library.allGeneratedVariants[new BlockShape_Library.ShapeRotationKey() { bs = BlockShape_Library.shortDiagonal, rotationsPerAxis = new Int3(1, 0, 0) }]; return;
                            }
                                //with back
                            if ((neighboursCovering & BlockSides.bottomFront) == BlockSides.bottomFront && (neighboursCovering & BlockSides.topBack) == 0)
                            {
                                block.blockShape = BlockShape_Library.allGeneratedVariants[new BlockShape_Library.ShapeRotationKey() { bs = BlockShape_Library.shortDiagonal, rotationsPerAxis = new Int3(3, 1, 0) }]; return;
                            }
                            if ((neighboursCovering & BlockSides.bottomRight) == BlockSides.bottomRight && (neighboursCovering & BlockSides.topLeft) == 0)
                            {
                                block.blockShape = BlockShape_Library.allGeneratedVariants[new BlockShape_Library.ShapeRotationKey() { bs = BlockShape_Library.shortDiagonal, rotationsPerAxis = new Int3(3, 2, 0) }]; return;
                            }
                            if ((neighboursCovering & BlockSides.bottomBack) == BlockSides.bottomBack && (neighboursCovering & BlockSides.topFront) == 0)
                            {
                                block.blockShape = BlockShape_Library.allGeneratedVariants[new BlockShape_Library.ShapeRotationKey() { bs = BlockShape_Library.shortDiagonal, rotationsPerAxis = new Int3(3, 3, 0) }]; return;
                            }
                            if ((neighboursCovering & BlockSides.bottomLeft) == BlockSides.bottomLeft && (neighboursCovering & BlockSides.topRight) == 0)
                            {
                                block.blockShape = BlockShape_Library.allGeneratedVariants[new BlockShape_Library.ShapeRotationKey() { bs = BlockShape_Library.shortDiagonal, rotationsPerAxis = new Int3(3, 0, 0) }]; return;
                            }
                                //X Z planar
                            if ((neighboursCovering & BlockSides.frontRight) == BlockSides.frontRight && (neighboursCovering & BlockSides.backLeft) == 0)
                            {
                                block.blockShape = BlockShape_Library.allGeneratedVariants[new BlockShape_Library.ShapeRotationKey() { bs = BlockShape_Library.shortDiagonal, rotationsPerAxis = new Int3(0, 2, 0) }]; return;
                            }
                            if ((neighboursCovering & BlockSides.leftFront) == BlockSides.leftFront && (neighboursCovering & BlockSides.rightBack) == 0)
                            {
                                block.blockShape = BlockShape_Library.allGeneratedVariants[new BlockShape_Library.ShapeRotationKey() { bs = BlockShape_Library.shortDiagonal, rotationsPerAxis = new Int3(0, 1, 0) }]; return;
                            }
                            if ((neighboursCovering & BlockSides.rightBack) == BlockSides.rightBack && (neighboursCovering & BlockSides.leftFront) == 0)
                            {
                                block.blockShape = BlockShape_Library.allGeneratedVariants[new BlockShape_Library.ShapeRotationKey() { bs = BlockShape_Library.shortDiagonal, rotationsPerAxis = new Int3(0, 3, 0) }]; return;
                            }
                            if ((neighboursCovering & BlockSides.frontRight) == BlockSides.frontRight && (neighboursCovering & BlockSides.backLeft) == 0)
                            {
                                block.blockShape = BlockShape_Library.allGeneratedVariants[new BlockShape_Library.ShapeRotationKey() { bs = BlockShape_Library.shortDiagonal, rotationsPerAxis = new Int3(0, 2, 0) }]; return;
                            }
                            if ((neighboursCovering & BlockSides.backLeft) == BlockSides.backLeft && (neighboursCovering & BlockSides.frontRight) == 0)
                            {
                                block.blockShape = BlockShape_Library.allGeneratedVariants[new BlockShape_Library.ShapeRotationKey() { bs = BlockShape_Library.shortDiagonal, rotationsPerAxis = new Int3(0, 0, 0) }]; return;
                            }
                            if ((neighboursCovering & BlockSides.rightBack) == BlockSides.rightBack && (neighboursCovering & BlockSides.leftFront) == 0)
                            {
                                block.blockShape = BlockShape_Library.allGeneratedVariants[new BlockShape_Library.ShapeRotationKey() { bs = BlockShape_Library.shortDiagonal, rotationsPerAxis = new Int3(0, 3, 0) }]; return;
                            }
                            if ((neighboursCovering & BlockSides.leftFront) == BlockSides.leftFront && (neighboursCovering & BlockSides.rightBack) == 0)
                            {
                                block.blockShape = BlockShape_Library.allGeneratedVariants[new BlockShape_Library.ShapeRotationKey() { bs = BlockShape_Library.shortDiagonal, rotationsPerAxis = new Int3(0, 1, 0) }]; return;
                            }
                            if ((neighboursCovering & BlockSides.backLeft) == BlockSides.backLeft && (neighboursCovering & BlockSides.frontRight) == 0)
                            {
                                block.blockShape = BlockShape_Library.allGeneratedVariants[new BlockShape_Library.ShapeRotationKey() { bs = BlockShape_Library.shortDiagonal, rotationsPerAxis = new Int3(0, 0, 0) }]; return;
                            }




                            /* //deprecated, less clean and error prone code
                            {//change of simple diagonal. Checking all options
                                if (neighborCoverCount == 2 || neighborCoverCount == 4)
                                {//simplest diagonal but, unlike the other mode, triplanar; For each top and bottom the four front,right,back,left and every of the four its clockwise neighbor
                                    if ((neighboursCovering & BlockSides.top) != 0&& (neighboursCovering & BlockSides.bottom) == 0)
                                    {
                                        if ((neighboursCovering & BlockSides.front) != 0 && (neighboursCovering & BlockSides.back) == 0)
                                        {
                                            block.blockShape = BlockShape_Library.allGeneratedVariants[new BlockShape_Library.ShapeRotationKey() { bs = BlockShape_Library.shortDiagonal, rotationsPerAxis = new Int3(1, 1, 0) }];return;
                                        }
                                        else if ((neighboursCovering & BlockSides.right) != 0 && (neighboursCovering & BlockSides.left) == 0)
                                        {
                                            block.blockShape = BlockShape_Library.allGeneratedVariants[new BlockShape_Library.ShapeRotationKey() { bs = BlockShape_Library.shortDiagonal, rotationsPerAxis = new Int3(1, 2, 0) }]; return;
                                        }
                                        else if ((neighboursCovering & BlockSides.back) != 0 && (neighboursCovering & BlockSides.front) == 0)
                                        {
                                            block.blockShape = BlockShape_Library.allGeneratedVariants[new BlockShape_Library.ShapeRotationKey() { bs = BlockShape_Library.shortDiagonal, rotationsPerAxis = new Int3(1, 3, 0) }]; return;
                                        }
                                        else if( (neighboursCovering & BlockSides.right) == 0)
                                        {
                                            block.blockShape = BlockShape_Library.allGeneratedVariants[new BlockShape_Library.ShapeRotationKey() { bs = BlockShape_Library.shortDiagonal, rotationsPerAxis = new Int3(1, 0, 0) }]; return;
                                        }
                                    }
                                    else if ((neighboursCovering & BlockSides.bottom) != 0 && (neighboursCovering & BlockSides.top) == 0)
                                    {
                                        if ((neighboursCovering & BlockSides.front) != 0 && (neighboursCovering & BlockSides.back) == 0)
                                        {
                                            block.blockShape = BlockShape_Library.allGeneratedVariants[new BlockShape_Library.ShapeRotationKey() { bs = BlockShape_Library.shortDiagonal, rotationsPerAxis = new Int3(3, 1, 0) }]; return;
                                        }
                                        else if ((neighboursCovering & BlockSides.right) != 0 && (neighboursCovering & BlockSides.left) == 0)
                                        {
                                            block.blockShape = BlockShape_Library.allGeneratedVariants[new BlockShape_Library.ShapeRotationKey() { bs = BlockShape_Library.shortDiagonal, rotationsPerAxis = new Int3(3, 2, 0) }]; return;
                                            //Debug.DrawLine((c.key - directorInstance.globalOffset).AsVector3*LandscapeTool.ChunkScale+new Vector3(x,y,z)*LandscapeTool.BlockScale, (c.key - directorInstance.globalOffset).AsVector3 * LandscapeTool.ChunkScale + new Vector3(x+1, y+1, z+1) * LandscapeTool.BlockScale,Color.cyan,1000f,false);
                                        }
                                        else if ((neighboursCovering & BlockSides.back) != 0 && (neighboursCovering & BlockSides.front) == 0)
                                        {
                                            block.blockShape = BlockShape_Library.allGeneratedVariants[new BlockShape_Library.ShapeRotationKey() { bs = BlockShape_Library.shortDiagonal, rotationsPerAxis = new Int3(3, 3, 0) }]; return;
                                        }
                                        else if( (neighboursCovering & BlockSides.right) == 0)
                                        {
                                            block.blockShape = BlockShape_Library.allGeneratedVariants[new BlockShape_Library.ShapeRotationKey() { bs = BlockShape_Library.shortDiagonal, rotationsPerAxis = new Int3(3, 0, 0) }]; return;
                                        }
                                    }
                                    else
                                    {
                                        if ((neighboursCovering & BlockSides.front) != 0 && (neighboursCovering & BlockSides.back) == 0)
                                        {
                                            if ((neighboursCovering & BlockSides.right) != 0 && (neighboursCovering & BlockSides.left) == 0)
                                            {
                                                block.blockShape = BlockShape_Library.allGeneratedVariants[new BlockShape_Library.ShapeRotationKey() { bs = BlockShape_Library.shortDiagonal, rotationsPerAxis = new Int3(0, 2, 0) }]; return;
                                            }
                                            else if ((neighboursCovering & BlockSides.right) == 0)
                                            {
                                                block.blockShape = BlockShape_Library.allGeneratedVariants[new BlockShape_Library.ShapeRotationKey() { bs = BlockShape_Library.shortDiagonal, rotationsPerAxis = new Int3(0, 1, 0) }]; return;
                                            }
                                        }
                                        else if ((neighboursCovering & BlockSides.right) != 0 && (neighboursCovering & BlockSides.left) == 0)
                                        {
                                            if ((neighboursCovering & BlockSides.back) != 0 && (neighboursCovering & BlockSides.front) == 0)
                                            {
                                                block.blockShape = BlockShape_Library.allGeneratedVariants[new BlockShape_Library.ShapeRotationKey() { bs = BlockShape_Library.shortDiagonal, rotationsPerAxis = new Int3(0, 3, 0) }]; return;
                                            }
                                            else if ((neighboursCovering & BlockSides.back) == 0)
                                            {
                                                block.blockShape = BlockShape_Library.allGeneratedVariants[new BlockShape_Library.ShapeRotationKey() { bs = BlockShape_Library.shortDiagonal, rotationsPerAxis = new Int3(0, 2, 0) }]; return;
                                            }
                                        }
                                        else if ((neighboursCovering & BlockSides.back) != 0 && (neighboursCovering & BlockSides.front) == 0)
                                        {
                                            if ((neighboursCovering & BlockSides.left) != 0 && (neighboursCovering & BlockSides.right) == 0)
                                            {
                                                block.blockShape = BlockShape_Library.allGeneratedVariants[new BlockShape_Library.ShapeRotationKey() { bs = BlockShape_Library.shortDiagonal, rotationsPerAxis = new Int3(0, 0, 0) }]; return;
                                            }
                                            else if ((neighboursCovering & BlockSides.left) == 0)
                                            {
                                                block.blockShape = BlockShape_Library.allGeneratedVariants[new BlockShape_Library.ShapeRotationKey() { bs = BlockShape_Library.shortDiagonal, rotationsPerAxis = new Int3(0, 3, 0) }]; return;
                                            }
                                        }
                                        else if( (neighboursCovering & BlockSides.right) == 0)
                                        {
                                            if ((neighboursCovering & BlockSides.front) != 0 && (neighboursCovering & BlockSides.bottom) == 0)
                                            {
                                                block.blockShape = BlockShape_Library.allGeneratedVariants[new BlockShape_Library.ShapeRotationKey() { bs = BlockShape_Library.shortDiagonal, rotationsPerAxis = new Int3(0, 1, 0) }]; return;
                                            }
                                            else if ((neighboursCovering & BlockSides.front) == 0)
                                            {
                                                block.blockShape = BlockShape_Library.allGeneratedVariants[new BlockShape_Library.ShapeRotationKey() { bs = BlockShape_Library.shortDiagonal, rotationsPerAxis = new Int3(0, 0, 0) }]; return;
                                            }
                                        }
                                    }
                                }
                                else if (neighborCoverCount == 3)
                                {//trifold diagonal, every blocks corner
                                    if ((neighboursCovering & BlockSides.top) == BlockSides.top)//top
                                    {
                                        if ((neighboursCovering & BlockSides.right) == BlockSides.right)//right
                                        {
                                            if ((neighboursCovering & BlockSides.front) == BlockSides.front)//front
                                            {
                                                block.blockShape = BlockShape_Library.allGeneratedVariants[new BlockShape_Library.ShapeRotationKey() { bs = BlockShape_Library.hexCorner, rotationsPerAxis = new Int3(1, 2, 0) }]; return;
                                            }
                                            else //back
                                            {
                                                block.blockShape = BlockShape_Library.allGeneratedVariants[new BlockShape_Library.ShapeRotationKey() { bs = BlockShape_Library.hexCorner, rotationsPerAxis = new Int3(1, 3, 0) }]; return;
                                            }
                                        }
                                        else //left
                                        {
                                            if ((neighboursCovering & BlockSides.front) == BlockSides.front)//front
                                            {
                                                block.blockShape = BlockShape_Library.allGeneratedVariants[new BlockShape_Library.ShapeRotationKey() { bs = BlockShape_Library.hexCorner, rotationsPerAxis = new Int3(2, 0, 0) }]; return;
                                            }
                                            else //back
                                            {
                                                block.blockShape = BlockShape_Library.allGeneratedVariants[new BlockShape_Library.ShapeRotationKey() { bs = BlockShape_Library.hexCorner, rotationsPerAxis = new Int3(1, 0, 0) }]; return;
                                            }
                                        }
                                    }
                                    else //bottom
                                    {
                                        if ((neighboursCovering & BlockSides.right) == BlockSides.right)//right
                                        {
                                            if ((neighboursCovering & BlockSides.front) == BlockSides.front)//front
                                            {
                                                block.blockShape = BlockShape_Library.allGeneratedVariants[new BlockShape_Library.ShapeRotationKey() { bs = BlockShape_Library.hexCorner, rotationsPerAxis = new Int3(0, 2, 0) }]; return;
                                            }
                                            else //back
                                            {
                                                block.blockShape = BlockShape_Library.allGeneratedVariants[new BlockShape_Library.ShapeRotationKey() { bs = BlockShape_Library.hexCorner, rotationsPerAxis = new Int3(0, 3, 0) }]; return;
                                            }
                                        }
                                        else //left
                                        {
                                            if ((neighboursCovering & BlockSides.front) == BlockSides.front)//front
                                            {
                                                block.blockShape = BlockShape_Library.allGeneratedVariants[new BlockShape_Library.ShapeRotationKey() { bs = BlockShape_Library.hexCorner, rotationsPerAxis = new Int3(0, 1, 0) }]; return;
                                            }
                                            else //back
                                            {
                                                block.blockShape = BlockShape_Library.allGeneratedVariants[new BlockShape_Library.ShapeRotationKey() { bs = BlockShape_Library.hexCorner, rotationsPerAxis = new Int3(0, 0, 0) }]; return;
                                            }
                                        }
                                    }
                                }
                            }

                            //no chance of simple diagonal. Checking for complex diagonal and continuing.
                            if (x > 0 && y > 0 && z > 0 && x < LandscapeTool.ChunkScale - 1 && y < LandscapeTool.ChunkScale - 1 && z < LandscapeTool.ChunkScale - 1)
                            {//having this instead of the get cube method restricts long diagonals to within one chunk but is sufficient to demonstrate this function and spare code readability and little performance
                                if (c.densityMap[(pos + new Int3(1, 1, 1)).ToLinearChunkScaleIndex()] >= directorInstance.densityToRealityBorder)
                                {
                                    block.blockShape = BlockShape_Library.allGeneratedVariants[new BlockShape_Library.ShapeRotationKey() { bs = BlockShape_Library.longDiagonal, rotationsPerAxis = new Int3(0, 0, 0) }]; return;
                                }
                                else if (c.densityMap[(pos + new Int3(-1, 1, 1)).ToLinearChunkScaleIndex()] >= directorInstance.densityToRealityBorder)
                                {
                                    block.blockShape = BlockShape_Library.allGeneratedVariants[new BlockShape_Library.ShapeRotationKey() { bs = BlockShape_Library.longDiagonal, rotationsPerAxis = new Int3(0, 3, 0) }]; return;
                                }
                                else if (c.densityMap[(pos + new Int3(1, -1, 1)).ToLinearChunkScaleIndex()] >= directorInstance.densityToRealityBorder)
                                {
                                    block.blockShape = BlockShape_Library.allGeneratedVariants[new BlockShape_Library.ShapeRotationKey() { bs = BlockShape_Library.longDiagonal, rotationsPerAxis = new Int3(1, 0, 0) }]; return;
                                }
                                else if (c.densityMap[(pos + new Int3(-1, -1, 1)).ToLinearChunkScaleIndex()] >= directorInstance.densityToRealityBorder)
                                {
                                    block.blockShape = BlockShape_Library.allGeneratedVariants[new BlockShape_Library.ShapeRotationKey() { bs = BlockShape_Library.longDiagonal, rotationsPerAxis = new Int3(1, 3, 0) }]; return;
                                }
                                else if (c.densityMap[(pos + new Int3(1, 1, -1)).ToLinearChunkScaleIndex()] >= directorInstance.densityToRealityBorder)
                                {
                                    block.blockShape = BlockShape_Library.allGeneratedVariants[new BlockShape_Library.ShapeRotationKey() { bs = BlockShape_Library.longDiagonal, rotationsPerAxis = new Int3(0, 1, 0) }]; return;
                                }
                                else if (c.densityMap[(pos + new Int3(-1, 1, -1)).ToLinearChunkScaleIndex()] >= directorInstance.densityToRealityBorder)
                                {
                                    block.blockShape = BlockShape_Library.allGeneratedVariants[new BlockShape_Library.ShapeRotationKey() { bs = BlockShape_Library.longDiagonal, rotationsPerAxis = new Int3(0, 2, 0) }]; return;
                                }
                                else if (c.densityMap[(pos + new Int3(1, -1, -1)).ToLinearChunkScaleIndex()] >= directorInstance.densityToRealityBorder)
                                {
                                    block.blockShape = BlockShape_Library.allGeneratedVariants[new BlockShape_Library.ShapeRotationKey() { bs = BlockShape_Library.longDiagonal, rotationsPerAxis = new Int3(1, 1, 0) }]; return;
                                }
                                else if (c.densityMap[(pos + new Int3(-1, -1, -1)).ToLinearChunkScaleIndex()] >= directorInstance.densityToRealityBorder)
                                {
                                    block.blockShape = BlockShape_Library.allGeneratedVariants[new BlockShape_Library.ShapeRotationKey() { bs = BlockShape_Library.longDiagonal, rotationsPerAxis = new Int3(1, 2, 0) }]; return;
                                }
                            }
                            //*/
                        });
                    });
                });
            }
        }
#endif
    }


}