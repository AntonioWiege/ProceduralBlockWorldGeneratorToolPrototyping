/*Antonio Wiege*/
using System.Collections.Generic;
using UnityEngine;
namespace ProceduralBlockWorldGeneratorToolPrototyping
{

    public static class VoxelizeByCollision
    {
        public static float resolution = LandscapeTool.BlockScale;
        public static float debug_size_mul = 1f;
        /// <summary>
        /// Voxelizes by Sphere Checks. Some inaccuracy confirmed.
        /// </summary>
        /// <param name="body"> gameObject to check (should have colliders active within passed bounds)</param>
        /// <param name="checkWithinTheseBounds"> usually a colliders or mesh bounds, sometimes the combined largest extend, but that is not calculated here</param>
        /// <param name="relative">default true reset point_in_Biome_Value_Space and rotation during check, otherwise voxelize relative to world however currently transformed</param>
        public static List<Vector3> Run(GameObject body, Bounds checkWithinTheseBounds = default(Bounds), bool relative = false)
        {
            List<Vector3> result = new();

            if (resolution < 0.01f) resolution = LandscapeTool.BlockScale;

            GameObject original_transform = new("don't touch. technical transform var in use for VoxelizeByCollision.");
            GameObject original_rotation = new("don't touch. technical transform var in use for VoxelizeByCollision.");

            Bounds b = checkWithinTheseBounds;
            if (b == default(Bounds))
            {
                b = body.GetComponent<Collider>().bounds;
            }
            if (!relative)
            {
                Vector3 grid_center = Int3.ToInt3(b.center / resolution).AsVector3 * resolution;
                b.Expand((b.center - grid_center).magnitude);
                b.center = grid_center;
            }
            Transform t = body.transform;

            Vector3 pos = Vector3.zero;
            Quaternion rot = Quaternion.identity;
            Vector3 scale = Vector3.one;

            if (relative)
            {
                pos = t.position;
                rot = t.rotation;
                scale = t.localScale;

                original_transform.transform.SetPositionAndRotation(pos, rot); original_transform.transform.localScale = scale;
                original_rotation.transform.rotation = rot;

                t.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
                t.localScale = Vector3.one;
            }

            LayerMask lm = body.layer;
            //Should be some temporary layer to selectively have the one object in collision, but for general work without setup using "Water" should suffice
            body.layer = LayerMask.NameToLayer("Water"); //may not be exclusive due to multi threading which is why we use overlapSphere instead of SphereCheck

            Collider[] colliders;

            //get the closest radius that is larger than the magnitude and a multiple of block_size 
            /// to cover the bounds with sphere
            int octaves = closestOfSqrt((b.max - b.center).magnitude);

            colliders = Physics.OverlapSphere(b.center, cubeOctSize(octaves) * .5f * Mathf.Sqrt(2f), ~body.layer);
            if (colliders.Length > 0)
            {
                foreach (var item in colliders)
                {
                    if (item.gameObject == body)
                    {
                        NestedOctave(result, body, b.center, octaves, relative, original_rotation.transform, original_transform.transform);
                    }
                }
            }

            body.layer = lm;


            if (relative)
            {
                t.SetPositionAndRotation(pos, rot);
                t.localScale = scale;
            }

            Object.DestroyImmediate(original_rotation);
            Object.DestroyImmediate(original_transform);

#if !Deactivate_Debugging
            foreach (var item in result)
            {
                Debug.DrawLine(body.transform.TransformPoint(item), body.transform.TransformPoint(item) + Vector3.right * resolution * .5f * Mathf.Sqrt(2f), Color.blue * .5f);
            }
#endif

            return result;
        }

        static void NestedOctave(List<Vector3> result, GameObject body, Vector3 center, int octave, bool relative, Transform rotationT, Transform transformT)
        {
            octave--;

            float d = cubeOctSize(octave);

            for (int z = 0; z < 2; z++)
            {
                for (int y = 0; y < 2; y++)
                {
                    for (int x = 0; x < 2; x++)
                    {
                        Vector3 p = Vector3.zero;
                        //position collision check center
                        if (relative)
                        {
                            p = center + (rotationT.TransformPoint(new Vector3(x - 1, y - 1, z - 1) * d + Vector3.one * d * .5f));
                        }
                        else
                        {
                            p = center + new Vector3(x - 1, y - 1, z - 1) * d + Vector3.one * d * .5f;
                        }
                        Collider[] colliders;
                        // In SphereCast it is which layers are included. In SphereOverlap which are ignored.
                        colliders = Physics.OverlapSphere(p, d * .5f * Mathf.Sqrt(2f), ~body.layer);

                        if (colliders.Length > 0)//alloc version needed => generates garbage
                        {
                            foreach (var item in colliders)
                            {
                                if (item.gameObject == body)
                                {
#if !Deactivate_Debugging&&!Deactivate_Gizmos
                                    Debug.DrawLine(p, p + Vector3.right * d * .5f * Mathf.Sqrt(2f) * debug_size_mul, Color.green * .86f + Color.red);
#endif

                                    if (octave > 1)
                                    {
                                        //each collision of larger scale gets subdivided into a new octree octave, to repeat the collision checks in finer resolution, where necessary
                                        NestedOctave(result, body, p, octave, relative, rotationT, transformT);
                                    }
#if !Deactivate_Debugging&&!Deactivate_Gizmos
                                    else
                                    {
                                        //if final resolution and target size reached, add position as voxel entry to the result container.
                                        result.Add(transformT.InverseTransformPoint(p));

                                        Debug.DrawLine(p, p + Vector3.right * resolution * .5f * Mathf.Sqrt(2f), Color.blue * .5f);

                                    }
#endif
                                    break;
                                }
#if !Deactivate_Debugging&&!Deactivate_Gizmos
                                else
                                {
                                    Debug.DrawLine(p, p + Vector3.right * d * .5f * Mathf.Sqrt(2f) * debug_size_mul, Color.red + Color.blue * .68f);
                                }
#endif
                            }
                        }

#if !Deactivate_Debugging&&!Deactivate_Gizmos
                        else
                        {
                            Debug.DrawLine(p, p + Vector3.right * d * .5f * Mathf.Sqrt(2f) * debug_size_mul, Color.green * 0.86f + Color.blue);
                        }
#endif
                    }
                }
            }
        }

        static int closestOfSqrt(float i)
        {
            int c = 1;
            while (i > cubeOctSize(c))
            {
                c++;
            }
            return c + 1;
        }

        static float cubeOctSize(int s)
        {
            return resolution * Mathf.Pow(2, s) * .5f;
        }
    }
}