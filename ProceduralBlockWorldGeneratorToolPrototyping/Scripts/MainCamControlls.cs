/*Antonio Wiege*/
using System.Collections.Generic;
using UnityEngine;
namespace ProceduralBlockWorldGeneratorToolPrototyping
{
    public class MainCamControlls : MonoBehaviour
    {
        new Camera camera;
        public float speed = 10f;
        public float sensitivityX = 10, sensitivityY = 10;
        public bool alreadyCalledThisFrame = false;
        private Vector3 cameraAngles;
        private float rotX = 0f, rotY = 0f;
        public List<LandscapeTool> landscapeTools = new List<LandscapeTool>();
        private void Awake()
        {
            camera = GetComponent<Camera>();
        }
        private void Update()
        {
            alreadyCalledThisFrame = false;
        }
        /// <summary>
        /// Called from LandscapeTool
        /// </summary>
        public void CustomUpdate()
        {

            //Scroll to adjust movement speed (disabled to have scroll control the brush radius/scale)
            //speed = Mathf.Clamp(speed + Input.GetAxisRaw("Mouse ScrollWheel") * speed, 0.1f, 1000f);

            //Multiply speed activation
            if (Input.GetKey(KeyCode.Space)) speed *= 10f;
            if (Input.GetKey(KeyCode.Tab)) speed /= 10f;

            //Move 3D via QWEASD
            float perpendicularInputAxis = ((Input.GetKey(KeyCode.E)) ? 1 : 0) - ((Input.GetKey(KeyCode.Q)) ? 1 : 0);//explicitely to avoid setting up new input axis in unity inspector
            Vector3 inputDirectionRaw = new Vector3(Input.GetAxisRaw("Horizontal"), perpendicularInputAxis, Input.GetAxisRaw("Vertical"));
            camera.transform.position += camera.transform.TransformDirection(inputDirectionRaw) * Time.deltaTime * speed;

            //Reset rotation to Z axis world forward
            camera.transform.LookAt(camera.transform.position + Vector3.forward);
            cameraAngles = camera.transform.rotation.eulerAngles;

            //Mouse Look Rotation Clamped
            rotX = (rotX + Input.GetAxisRaw("Mouse X") * sensitivityX) % 360.0f;
            rotY = Mathf.Clamp(rotY - Input.GetAxisRaw("Mouse Y") * sensitivityY, -90, 90);
            camera.transform.eulerAngles = new Vector3(rotY, rotX, 0f);

            //Multiply speed deactivation
            if (Input.GetKey(KeyCode.Space)) speed /= 10f;
            if (Input.GetKey(KeyCode.Tab)) speed *= 10f;
            alreadyCalledThisFrame = true;

            //Take Screenshot
            /*if (Input.GetKey(KeyCode.KeypadEnter))
            {
                var nameID = System.DateTime.Now.Ticks.ToString();
                ScreenCapture.CaptureScreenshot(Application.dataPath + "/Imgs/Pic" + nameID + ".png", ScreenCapture.StereoScreenCaptureMode.BothEyes);
            }*/

            GlobalOffsetAdjustment();
        }

        /// <summary>
        /// Calculate offset to clamp camera into high precision space; and call to apply.
        /// </summary>
        public void GlobalOffsetAdjustment()
        {
            if (landscapeTools.Count == 0) return;
            var offsetPostChunkCount = landscapeTools[0].offsetAfterChunkCount;
            var relativePos = (transform.position - landscapeTools[0].transform.position) / (LandscapeTool.ChunkScale * LandscapeTool.BlockScale);
            Vector3 offset = Vector3.zero;
            while (relativePos.x > offsetPostChunkCount)
            {
                relativePos -= Vector3.right * offsetPostChunkCount;
                offset -= Vector3.right * offsetPostChunkCount;
            }
            while (relativePos.x < -offsetPostChunkCount)
            {
                relativePos += Vector3.right * offsetPostChunkCount;
                offset += Vector3.right * offsetPostChunkCount;
            }
            while (relativePos.y > offsetPostChunkCount)
            {
                relativePos -= Vector3.up * offsetPostChunkCount;
                offset -= Vector3.up * offsetPostChunkCount;
            }
            while (relativePos.y < -offsetPostChunkCount)
            {
                relativePos += Vector3.up * offsetPostChunkCount;
                offset += Vector3.up * offsetPostChunkCount;
            }
            while (relativePos.z > offsetPostChunkCount)
            {
                relativePos -= Vector3.forward * offsetPostChunkCount;
                offset -= Vector3.forward * offsetPostChunkCount;
            }
            while (relativePos.z < -offsetPostChunkCount)
            {
                relativePos += Vector3.forward * offsetPostChunkCount;
                offset += Vector3.forward * offsetPostChunkCount;
            }
            OffsetGlobal(offset);
        }
        /// <summary>
        /// Apply some offset globally
        /// </summary>
        void OffsetGlobal(Vector3 offset)
        {//change move all child objects, skip if camera if it happens to be one
            if (landscapeTools[0].camera.transform.parent == landscapeTools[0].transform) landscapeTools[0].camera.transform.parent = null;//as to not offset the camera twice
            transform.position += offset * LandscapeTool.ChunkScale * LandscapeTool.BlockScale;
            foreach (var directorInstance in landscapeTools)
            {
                directorInstance.globalOffset += offset.ToInt3();
                foreach (Transform tsfm in directorInstance.transform)
                {
                    tsfm.position += offset * LandscapeTool.ChunkScale * LandscapeTool.BlockScale;
                }
            }
        }
    }

}