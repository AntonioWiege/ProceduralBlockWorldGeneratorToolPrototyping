using UnityEngine;
using UnityEngine.SceneManagement;
namespace ProceduralBlockWorldGeneratorToolPrototyping
{
    public class LoadSceneFromUIScript : MonoBehaviour
    {
        public void LoadScene(string name)
        {
            SceneManager.LoadScene(name);
        }
    }
}