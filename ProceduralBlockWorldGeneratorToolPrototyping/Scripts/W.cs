/* Antonio Wiege */
using System;
using System.Collections.Generic;
using UnityEngine;
namespace ProceduralBlockWorldGeneratorToolPrototyping
{
    /// <summary>
    /// Quick Access Library
    /// Table of Contents:
    ///     FloorToInt
    ///     GridFloor
    ///     GridFloorHit
    ///     ClosestCubeSideNormal
    ///     Verts of Quads to Mesh(turns a bunch of vertices, if they are groups of four, into a mesh of quads, optionally with uv parameteres)
    ///     Bool volume to mesh(turn a 3D boolean array into a voxel mesh)
    ///     Triangle offset(offset e.g.normalized triangle data to fit a certain offset in the vertex collections)
    /// </summary>
    public static class W
    {
        #region Flooring
        //Floor minds putting values from -1 to 0 into the section -1.
        ////Avoiding rounding smth. such as -.5 to 0

        /// <param name="gridSize">must be larger 0</param>
        public static int FloorToInt(float value, float gridSize)
        {
            float r = (value - value % gridSize) / gridSize;
            if (value < 0 && gridSize > value) r -= 1;
            return (int)r;
        }
        /// <param name="gridSize">must be larger 0</param>
        public static int FloorToInt(int value, int gridSize)
        {
            int r = value / gridSize;
            if (value % gridSize != 0 && value < 0 && gridSize > value) r -= 1;
            return r;
        }

        //! Quantization drop coordinates to grid

        public static Int3 GridFloor(Vector3 value, float gridSize)
        {
            return new Int3(FloorToInt(value.x, gridSize), FloorToInt(value.y, gridSize), FloorToInt(value.z, gridSize));
        }
        public static Int3 GridFloor(Int3 value, int gridSize)
        {
            return new Int3(FloorToInt(value.x, gridSize), FloorToInt(value.y, gridSize), FloorToInt(value.z, gridSize));
        }
        public static Vector3 GridFloor(float gridSize, Vector3 value)
        {
            return new Vector3(FloorToInt(value.x, gridSize), FloorToInt(value.y, gridSize), FloorToInt(value.z, gridSize));
        }

        //? Quantize hitMouse minding normal; needs update for more free formed meshes
        public static Vector3 GridFloorHit(RaycastHit hit, float gridSize)
        {
            return GridFloor(gridSize, hit.point - hit.normal * gridSize * .49f);
        }

        //! Any normal to cubeSides normal
        public static Vector3 ClosestCubeSideNormal(Vector3 nrm)
        {
            Vector3 n = Vector3.up;
            var prevDot = Vector3.Dot(nrm, n);
            var curDot = Vector3.Dot(nrm, Vector3.right);
            if (prevDot < curDot)
            {
                n = Vector3.right;
            }
            prevDot = curDot;
            curDot = Vector3.Dot(nrm, Vector3.left);
            if (prevDot < curDot)
            {
                n = Vector3.left;
            }
            prevDot = curDot;
            curDot = Vector3.Dot(nrm, Vector3.forward);
            if (prevDot < curDot)
            {
                n = Vector3.forward;
            }
            prevDot = curDot;
            curDot = Vector3.Dot(nrm, Vector3.back);
            if (prevDot < curDot)
            {
                n = Vector3.back;
            }
            prevDot = curDot;
            curDot = Vector3.Dot(nrm, Vector3.down);
            if (prevDot < curDot)
            {
                n = Vector3.down;
            }
            return n;
        }

        #endregion

        #region Verts_of_Quads_to_Mesh
        /// <summary>Turns Vector3[] into Mesh, assuming every four verts represent one new quad; Generating UVs based on (from,to) values
        /// <br>Does no further Mesh operations (no recalc normals, bounds, optimize, etc)</br></summary>
        public static Mesh Verts_of_Quads_to_Mesh(Vector3[] verts, float x_from = 0, float y_from = 0, float x_to = 1, float y_to = 1, bool indexFormat32 = true)
        {
            Mesh m = new Mesh();
            if (indexFormat32) m.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            m.vertices = verts;
            int[] tris = new int[verts.Length / 4 * 6];
            Vector2[] uvs = new Vector2[verts.Length];
            for (int i = 0; i < verts.Length / 4; i++)
            {
                tris[i * 6 + 0] = (i * 4 + 0);
                tris[i * 6 + 1] = (i * 4 + 1);
                tris[i * 6 + 2] = (i * 4 + 2);
                tris[i * 6 + 3] = (i * 4 + 2);
                tris[i * 6 + 4] = (i * 4 + 3);
                tris[i * 6 + 5] = (i * 4 + 0);
                uvs[i * 4 + 0] = (new Vector2(x_from, y_from));
                uvs[i * 4 + 1] = (new Vector2(x_from, y_to));
                uvs[i * 4 + 2] = (new Vector2(x_to, y_to));
                uvs[i * 4 + 3] = (new Vector2(x_to, y_from));
            }
            m.triangles = tris;
            m.uv = uvs;
            return m;
        } 
        /// <summary>Turns Vector3[] into Mesh, assuming every four verts represent one new quad, minding uvs</summary>
        public static Mesh Verts_of_Quads_to_Mesh(Vector3[] verts, Vector2[] uvs, bool indexFormat32 = true)
        {
            Mesh m = new Mesh();
            if (indexFormat32) m.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            m.vertices = verts;
            int[] tris = new int[verts.Length / 4 * 6];
            for (int i = 0; i < verts.Length / 4; i++)
            {
                tris[i * 6 + 0] = (i * 4 + 0);
                tris[i * 6 + 1] = (i * 4 + 1);
                tris[i * 6 + 2] = (i * 4 + 2);
                tris[i * 6 + 3] = (i * 4 + 2);
                tris[i * 6 + 4] = (i * 4 + 3);
                tris[i * 6 + 5] = (i * 4 + 0);
            }
            m.triangles = tris;
            m.uv = uvs;
            return m;
        }
        /// <summary>Turns List of Vector3 into Mesh, assuming every four verts represent one new quad, minding uvs</summary>
        public static Mesh Verts_of_Quads_to_Mesh(List<Vector3> verts, List<Vector2> uvs, bool indexFormat32 = true)
        {
            Mesh m = new Mesh();
            if (indexFormat32) m.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            m.SetVertices(verts);
            int[] tris = new int[verts.Count / 4 * 6];
            for (int i = 0; i < verts.Count / 4; i++)
            {
                tris[i * 6 + 0] = (i * 4 + 0);
                tris[i * 6 + 1] = (i * 4 + 1);
                tris[i * 6 + 2] = (i * 4 + 2);
                tris[i * 6 + 3] = (i * 4 + 2);
                tris[i * 6 + 4] = (i * 4 + 3);
                tris[i * 6 + 5] = (i * 4 + 0);
            }
            m.triangles = tris;
            m.uv = uvs.ToArray();
            return m;
        }
        /// <summary>Turns List of Vector3 into Mesh, assuming every four verts represent one new quad; Generating UVs based on (from,to) values</summary>
        public static Mesh Verts_of_Quads_to_Mesh(List<Vector3> verts, float x_from = 0, float y_from = 0, float x_to = 1, float y_to = 1, bool indexFormat32 = true)
        {
            Mesh m = new Mesh();
            if (indexFormat32) m.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            m.SetVertices(verts);
            int[] tris = new int[verts.Count / 4 * 6];
            Vector2[] uvs = new Vector2[verts.Count];
            for (int i = 0; i < verts.Count / 4; i++)
            {
                tris[i * 6 + 0] = (i * 4 + 0);
                tris[i * 6 + 1] = (i * 4 + 1);
                tris[i * 6 + 2] = (i * 4 + 2);
                tris[i * 6 + 3] = (i * 4 + 2);
                tris[i * 6 + 4] = (i * 4 + 3);
                tris[i * 6 + 5] = (i * 4 + 0);
                uvs[i * 4 + 0] = (new Vector2(x_from, y_from));
                uvs[i * 4 + 1] = (new Vector2(x_from, y_to));
                uvs[i * 4 + 2] = (new Vector2(x_to, y_to));
                uvs[i * 4 + 3] = (new Vector2(x_to, y_from));
            }
            m.triangles = tris;
            m.uv = uvs;
            return m;
        }
        #endregion

        #region Bool volume to mesh

        public static Vector3[] top_VoxelSideMesh = new Vector3[] {
                new Vector3(0, 1 , 0), new Vector3(0, 1 , 1 ), new Vector3(1 , 1 , 1 ), new Vector3(1 , 1 , 0),
            },
            front_VoxelSideMesh = new Vector3[] {
                new Vector3(1 , 0, 1 ), new Vector3(1 , 1 , 1 ), new Vector3(0, 1 , 1 ), new Vector3(0, 0, 1 ),
                },
            right_VoxelSideMesh = new Vector3[] {
                new Vector3(1 , 0, 0), new Vector3(1 , 1 , 0), new Vector3(1 , 1 , 1 ), new Vector3(1 , 0, 1 ),
                },
            back_VoxelSideMesh = new Vector3[] {
                 new Vector3(0, 0, 0), new Vector3(0, 1 , 0), new Vector3(1 , 1 , 0), new Vector3(1 , 0, 0),
                },
            left_VoxelSideMesh = new Vector3[] {
      new Vector3(0, 0, 1 ), new Vector3(0, 1 , 1 ), new Vector3(0, 1 , 0), new Vector3(0, 0, 0),
                },
            bottom_VoxelSideMesh = new Vector3[] {
                  new Vector3(1 , 0, 0), new Vector3(1 , 0, 1 ), new Vector3(0, 0, 1 ), new Vector3(0, 0, 0)
                };
        /// <summary>
        /// Turn a bool[] into a 3D voxel mesh, based on passed sideLength
        /// </summary>
        public static Mesh VoxelCubeMesh_ClosedOff_FromLinear(int dimensionLength, bool[] data, float scale = LandscapeTool.BlockScale)
        {
            List<Vector3> verts = new();
            for (int z = 1; z < dimensionLength - 1; z++)
            {
                for (int y = 1; y < dimensionLength - 1; y++)
                {
                    for (int x = 1; x < dimensionLength - 1; x++)
                    {
                        if (!data[x + y * dimensionLength + z * dimensionLength * dimensionLength]) continue;
                        Vector3 o = new Vector3(x, y, z);
                        if (!data[(x + 1) + (y) * dimensionLength + (z) * dimensionLength * dimensionLength])
                        {
                            for (int i = 0; i < right_VoxelSideMesh.Length; i++)
                            {
                                verts.Add((right_VoxelSideMesh[i] + o) * scale);
                            }
                        }
                        if (!data[(x - 1) + (y) * dimensionLength + (z) * dimensionLength * dimensionLength])
                        {
                            for (int i = 0; i < left_VoxelSideMesh.Length; i++)
                            {
                                verts.Add((left_VoxelSideMesh[i] + o) * scale);
                            }
                        }
                        if (!data[(x) + (y + 1) * dimensionLength + (z) * dimensionLength * dimensionLength])
                        {
                            for (int i = 0; i < top_VoxelSideMesh.Length; i++)
                            {
                                verts.Add((top_VoxelSideMesh[i] + o) * scale);
                            }
                        }
                        if (!data[(x) + (y - 1) * dimensionLength + (z) * dimensionLength * dimensionLength])
                        {
                            for (int i = 0; i < bottom_VoxelSideMesh.Length; i++)
                            {
                                verts.Add((bottom_VoxelSideMesh[i] + o) * scale);
                            }
                        }
                        if (!data[(x) + (y) * dimensionLength + (z + 1) * dimensionLength * dimensionLength])
                        {
                            for (int i = 0; i < front_VoxelSideMesh.Length; i++)
                            {
                                verts.Add((front_VoxelSideMesh[i] + o) * scale);
                            }
                        }
                        if (!data[(x) + (y) * dimensionLength + (z - 1) * dimensionLength * dimensionLength])
                        {
                            for (int i = 0; i < back_VoxelSideMesh.Length; i++)
                            {
                                verts.Add((back_VoxelSideMesh[i] + o) * scale);
                            }
                        }
                    }
                }
            }
            for (int z = 0; z < dimensionLength; z++)
            {
                for (int y = 0; y < dimensionLength; y++)
                {
                    for (int x = 0; x < dimensionLength; x++)
                    {
                        if (!(x == 0 || x == dimensionLength - 1 || y == 0 || y == dimensionLength - 1 || z == 0 || z == dimensionLength - 1)) continue;
                        if (!data[x + y * dimensionLength + z * dimensionLength * dimensionLength]) continue;
                        Vector3 o = new Vector3(x, y, z);
                        if (x > dimensionLength - 2)
                        {
                            for (int i = 0; i < right_VoxelSideMesh.Length; i++)
                            {
                                verts.Add((right_VoxelSideMesh[i] + o) * scale);
                            }
                        }
                        else
                        if (!data[(x + 1) + (y) * dimensionLength + (z) * dimensionLength * dimensionLength])
                        {
                            for (int i = 0; i < right_VoxelSideMesh.Length; i++)
                            {
                                verts.Add((right_VoxelSideMesh[i] + o) * scale);
                            }
                        }
                        if (x < 1)
                        {
                            for (int i = 0; i < left_VoxelSideMesh.Length; i++)
                            {
                                verts.Add((left_VoxelSideMesh[i] + o) * scale);
                            }
                        }
                        else
                        if (!data[(x - 1) + (y) * dimensionLength + (z) * dimensionLength * dimensionLength])
                        {
                            for (int i = 0; i < left_VoxelSideMesh.Length; i++)
                            {
                                verts.Add((left_VoxelSideMesh[i] + o) * scale);
                            }
                        }
                        if (y > dimensionLength - 2)
                        {
                            for (int i = 0; i < top_VoxelSideMesh.Length; i++)
                            {
                                verts.Add((top_VoxelSideMesh[i] + o) * scale);
                            }
                        }
                        else
                        if (!data[(x) + (y + 1) * dimensionLength + (z) * dimensionLength * dimensionLength])
                        {
                            for (int i = 0; i < top_VoxelSideMesh.Length; i++)
                            {
                                verts.Add((top_VoxelSideMesh[i] + o) * scale);
                            }
                        }
                        if (y < 1)
                        {
                            for (int i = 0; i < bottom_VoxelSideMesh.Length; i++)
                            {
                                verts.Add((bottom_VoxelSideMesh[i] + o) * scale);
                            }
                        }
                        else
                        if (!data[(x) + (y - 1) * dimensionLength + (z) * dimensionLength * dimensionLength])
                        {
                            for (int i = 0; i < bottom_VoxelSideMesh.Length; i++)
                            {
                                verts.Add((bottom_VoxelSideMesh[i] + o) * scale);
                            }
                        }
                        if (z > dimensionLength - 2)
                        {
                            for (int i = 0; i < front_VoxelSideMesh.Length; i++)
                            {
                                verts.Add((front_VoxelSideMesh[i] + o) * scale);
                            }
                        }
                        else
                        if (!data[(x) + (y) * dimensionLength + (z + 1) * dimensionLength * dimensionLength])
                        {
                            for (int i = 0; i < front_VoxelSideMesh.Length; i++)
                            {
                                verts.Add((front_VoxelSideMesh[i] + o) * scale);
                            }
                        }
                        if (z < 1)
                        {
                            for (int i = 0; i < back_VoxelSideMesh.Length; i++)
                            {
                                verts.Add((back_VoxelSideMesh[i] + o) * scale);
                            }
                        }
                        else
                        if (!data[(x) + (y) * dimensionLength + (z - 1) * dimensionLength * dimensionLength])
                        {
                            for (int i = 0; i < back_VoxelSideMesh.Length; i++)
                            {
                                verts.Add((back_VoxelSideMesh[i] + o) * scale);
                            }
                        }
                    }
                }
            }
            return Verts_of_Quads_to_Mesh(verts.ToArray());
        }
        /// <summary>
        /// Turn a bool[] into a 3D voxel mesh with hole free surface
        /// </summary>
        public static Mesh VoxelCubeMesh_ClosedOff_From3D(int dimensionLength, bool[,,] data)
        {
            List<Vector3> verts = new();
            for (int z = 1; z < dimensionLength - 1; z++)
            {
                for (int y = 1; y < dimensionLength - 1; y++)
                {
                    for (int x = 1; x < dimensionLength - 1; x++)
                    {
                        if (!data[x, y, z]) continue;
                        Vector3 o = new Vector3(x, y, z);
                        if (!data[x + 1, y, z])
                        {
                            for (int i = 0; i < right_VoxelSideMesh.Length; i++)
                            {
                                verts.Add(right_VoxelSideMesh[i] + o);
                            }
                        }
                        if (!data[x - 1, y, z])
                        {
                            for (int i = 0; i < left_VoxelSideMesh.Length; i++)
                            {
                                verts.Add(left_VoxelSideMesh[i] + o);
                            }
                        }
                        if (!data[x, y + 1, z])
                        {
                            for (int i = 0; i < top_VoxelSideMesh.Length; i++)
                            {
                                verts.Add(top_VoxelSideMesh[i] + o);
                            }
                        }
                        if (!data[x, y - 1, z])
                        {
                            for (int i = 0; i < bottom_VoxelSideMesh.Length; i++)
                            {
                                verts.Add(bottom_VoxelSideMesh[i] + o);
                            }
                        }
                        if (!data[x, y, z + 1])
                        {
                            for (int i = 0; i < front_VoxelSideMesh.Length; i++)
                            {
                                verts.Add(front_VoxelSideMesh[i] + o);
                            }
                        }
                        if (!data[x, y, z - 1])
                        {
                            for (int i = 0; i < back_VoxelSideMesh.Length; i++)
                            {
                                verts.Add(back_VoxelSideMesh[i] + o);
                            }
                        }
                    }
                }
            }
            for (int z = 0; z < dimensionLength; z++)
            {
                for (int y = 0; y < dimensionLength; y++)
                {
                    for (int x = 0; x < dimensionLength; x++)
                    {
                        if (!(x == 0 || x == dimensionLength - 1 || y == 0 || y == dimensionLength - 1 || z == 0 || z == dimensionLength - 1)) continue;
                        if (!data[x, y, z]) continue;
                        Vector3 o = new Vector3(x, y, z);
                        if (x > dimensionLength - 2)
                        {
                            for (int i = 0; i < right_VoxelSideMesh.Length; i++)
                            {
                                verts.Add(right_VoxelSideMesh[i] + o);
                            }
                        }
                        else
                        if (!data[x + 1, y, z])
                        {
                            for (int i = 0; i < right_VoxelSideMesh.Length; i++)
                            {
                                verts.Add(right_VoxelSideMesh[i] + o);
                            }
                        }
                        if (x < 1)
                        {
                            for (int i = 0; i < left_VoxelSideMesh.Length; i++)
                            {
                                verts.Add(left_VoxelSideMesh[i] + o);
                            }
                        }
                        else
                        if (!data[x - 1, y, z])
                        {
                            for (int i = 0; i < left_VoxelSideMesh.Length; i++)
                            {
                                verts.Add(left_VoxelSideMesh[i] + o);
                            }
                        }
                        if (y > dimensionLength - 2)
                        {
                            for (int i = 0; i < top_VoxelSideMesh.Length; i++)
                            {
                                verts.Add(top_VoxelSideMesh[i] + o);
                            }
                        }
                        else
                        if (!data[x, y + 1, z])
                        {
                            for (int i = 0; i < top_VoxelSideMesh.Length; i++)
                            {
                                verts.Add(top_VoxelSideMesh[i] + o);
                            }
                        }
                        if (y < 1)
                        {
                            for (int i = 0; i < bottom_VoxelSideMesh.Length; i++)
                            {
                                verts.Add(bottom_VoxelSideMesh[i] + o);
                            }
                        }
                        else
                        if (!data[x, y - 1, z])
                        {
                            for (int i = 0; i < bottom_VoxelSideMesh.Length; i++)
                            {
                                verts.Add(bottom_VoxelSideMesh[i] + o);
                            }
                        }
                        if (z > dimensionLength - 2)
                        {
                            for (int i = 0; i < front_VoxelSideMesh.Length; i++)
                            {
                                verts.Add(front_VoxelSideMesh[i] + o);
                            }
                        }
                        else
                        if (!data[x, y, z + 1])
                        {
                            for (int i = 0; i < front_VoxelSideMesh.Length; i++)
                            {
                                verts.Add(front_VoxelSideMesh[i] + o);
                            }
                        }
                        if (z < 1)
                        {
                            for (int i = 0; i < back_VoxelSideMesh.Length; i++)
                            {
                                verts.Add(back_VoxelSideMesh[i] + o);
                            }
                        }
                        else
                        if (!data[x, y, z - 1])
                        {
                            for (int i = 0; i < back_VoxelSideMesh.Length; i++)
                            {
                                verts.Add(back_VoxelSideMesh[i] + o);
                            }
                        }
                    }
                }
            }
            return Verts_of_Quads_to_Mesh(verts.ToArray());
        }
        #endregion

        #region Triangle offset
        /*
         Vertex Buffer Offsetting
         */
        // C# should get an update where you can specify the type to have the + operator
        //! Triangle Offset on the original
        public static Vector3[] vertOffsetOrg(Vector3[] tris, Vector3 offset)//reference modified
        {
            for (int i = 0; i < tris.Length; i++)
            {
                tris[i] += offset;
            }
            return tris;
        }
        public static List<Vector3> vertOffsetOrg(List<Vector3> tris, Vector3 offset)//reference modified
        {
            for (int i = 0; i < tris.Count; i++)
            {
                tris[i] += offset;
            }
            return tris;
        }
        //! Triangle Offset on a copy
        public static Vector3[] vertOffsetNew(Vector3[] tris, Vector3 offset)//reference unmodified
        {
            Vector3[] dt = new Vector3[tris.Length];
            tris.CopyTo(dt, 0);
            for (int i = 0; i < dt.Length; i++)
            {
                dt[i] += offset;
            }
            return dt;
        }
        public static List<Vector3> vertOffsetNew(List<Vector3> tris, Vector3 offset)//reference unmodified
        {
            List<Vector3> dt = new List<Vector3>(tris.Count);
            dt.AddRange(tris);
            for (int i = 0; i < dt.Count; i++)
            {
                dt[i] += offset;
            }
            return dt;
        }


        /*
         Triangle Buffer Offsetting
         */
        //! Triangle Offset on the original
        public static int[] triOffsetOrg(int[] tris, int offset)//reference modified
        {
            for (int i = 0; i < tris.Length; i++)
            {
                tris[i] += offset;
            }
            return tris;
        }
        public static List<int> triOffsetOrg(List<int> tris, int offset)//reference modified
        {
            for (int i = 0; i < tris.Count; i++)
            {
                tris[i] += offset;
            }
            return tris;
        }
        //! Triangle Offset on a copy
        public static int[] triOffsetNew(int[] tris, int offset)//reference unmodified
        {
            int[] dt = new int[tris.Length];
            tris.CopyTo(dt, 0);
            for (int i = 0; i < dt.Length; i++)
            {
                dt[i] += offset;
            }
            return dt;
        }
        public static List<int> triOffsetNew(List<int> tris, int offset)//reference unmodified
        {
            List<int> dt = new List<int>(tris.Count);
            dt.AddRange(tris);
            for (int i = 0; i < dt.Count; i++)
            {
                dt[i] += offset;
            }
            return dt;
        }
        #endregion
    }
}