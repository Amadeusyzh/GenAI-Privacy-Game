using UnityEngine;

public class BackgroundSwitcher : MonoBehaviour
{
    public Material material1; // 当前的背景材质
    public Material material2; // 新的背景材质
    private bool isMaterial1Active = true;

    public GameObject quadObject; // 你想要更换材质的Quad对象

    public void SwitchBackground()
    {
        if (isMaterial1Active)
        {
            quadObject.GetComponent<Renderer>().material = material2;
        }
        else
        {
            quadObject.GetComponent<Renderer>().material = material1;
        }
        isMaterial1Active = !isMaterial1Active;
    }
}
