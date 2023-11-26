using UnityEngine;
namespace ProceduralBlockWorldGeneratorToolPrototyping
{
    public class DontDestroyOnLoad_OnAwake : MonoBehaviour
    {
        void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }
    }
}