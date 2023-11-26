/*Antonio Wiege*/
using UnityEngine;
namespace ProceduralBlockWorldGeneratorToolPrototyping
{
    public class Test_Noise : MonoBehaviour
    {
        [Header("To use properly click on the three dots on the right of the component name.\n Execute two methods in order 'Give New Own Material' & 'Compute Noise'")]
        public LandscapeTool LandscapeTool;
        public bool useCPU;
        public NoiseInstanceSetup nis;
        public Texture2D texture;
        public int resolution = 64;
        public float disInt = 5f;
        NoiseHandler noiseHandler;
        Material mat;

        [ContextMenu("Compute All")]
        public void computeAllInScene()
        {
            Test_Noise[] n = FindObjectsByType<Test_Noise>(FindObjectsSortMode.None); ;
            foreach (Test_Noise go in n)
            {
                go.Compute();
            }
        }

        [ContextMenu("Compute Noise")]
        public void Compute()
        {
            if (mat == null)
            {
                mat = new Material(GetComponent<MeshRenderer>().sharedMaterial);
                GetComponent<MeshRenderer>().sharedMaterial = mat;
            }
            if (noiseHandler == null) noiseHandler = new(LandscapeTool);

            if (texture != null) DestroyImmediate(texture);
            texture = new Texture2D(resolution, resolution);
            texture.filterMode = FilterMode.Point;
            nis.spaceDistortion = null;

            ComputeBuffer b = new ComputeBuffer(resolution * resolution, sizeof(float));
            var data = new float[resolution * resolution];
            b.SetData(data);

            data = noiseHandler.Execute(new Int3(nis.position.x, nis.position.y, nis.position.z), nis, b, false, true, resolution, 2, useCPU, data);

            Vector3[] d = new Vector3[data.Length];
            for (int i = 0; i < d.Length; i++)
            {
                d[i] = data[i] * Vector3.one * disInt;
            }
            nis.spaceDistortion = d;
            data = noiseHandler.Execute(new Int3(nis.position.x, nis.position.y, nis.position.z), nis, b, true, true, resolution, 2, useCPU, data);

            b.Dispose();

            for (int i = 0; i < resolution * resolution; i++)
            {
                texture.SetPixel(i % resolution, i / resolution, Color.white * data[i]);
            }
            texture.Apply();
            var g = GetComponent<MeshRenderer>();
            if (g != null)
                GetComponent<MeshRenderer>().sharedMaterial.mainTexture = texture;
        }
        [ContextMenu("Give New Own Material")]
        public void GetNewOwnMaterial()
        {
            GetComponent<MeshRenderer>().sharedMaterial = mat = new Material(GetComponent<MeshRenderer>().sharedMaterial);
        }
    }
}