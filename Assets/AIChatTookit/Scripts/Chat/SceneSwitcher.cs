using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwitcher : MonoBehaviour
{
    // �������������UI��ť��OnClick�¼��е��ã������������ط�����
    public void SwitchToGame()
    {
        SceneManager.LoadScene("Level1");
    }
}
