using UnityEngine;

public class BackgroundSwitcher : MonoBehaviour
{
    public Material material1; // ��ǰ�ı�������
    public Material material2; // �µı�������
    private bool isMaterial1Active = true;

    public GameObject quadObject; // ����Ҫ�������ʵ�Quad����

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
