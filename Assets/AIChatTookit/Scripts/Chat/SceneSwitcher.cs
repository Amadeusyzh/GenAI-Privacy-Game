using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwitcher : MonoBehaviour
{
    // 这个方法可以在UI按钮的OnClick事件中调用，或者在其他地方调用
    public void SwitchToGame()
    {
        SceneManager.LoadScene("Level1");
    }
}
