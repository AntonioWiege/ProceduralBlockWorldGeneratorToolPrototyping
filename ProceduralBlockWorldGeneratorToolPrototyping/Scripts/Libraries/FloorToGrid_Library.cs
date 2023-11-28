/* Antonio Wiege */
using UnityEngine;
namespace ProceduralBlockWorldGeneratorToolPrototyping
{
    /// <summary>
    /// Floor to Grid related static methods.
    /// </summary>
    public static class FloorToGrid
    {
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
    }
}