using UnityEngine;
using UnityEngine.SceneManagement;

public class ToMenu : MonoBehaviour
{
    public void SwitchToMenu()
    {
        SceneManager.LoadScene(0);
    }
}
