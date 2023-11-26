using UnityEngine;
namespace ProceduralBlockWorldGeneratorToolPrototyping
{
    public class Quit : MonoBehaviour
    {
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape)) Application.Quit();
        }
    }
}