using UnityEngine;
using UnityEngine.SceneManagement;

public class ToNavigator : MonoBehaviour
{
    public void LoadNextScene()
    {
        SceneManager.LoadScene(1);
    }
}
