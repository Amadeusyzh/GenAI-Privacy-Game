using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using WebGLSupport;
using UnityEngine.SceneManagement;
using System;
public class ChatSample : MonoBehaviour
{
    /// <summary>
    /// ��������
    /// </summary>
    [SerializeField] private ChatSetting m_ChatSettings;
    #region UI����
    /// <summary>
    /// ����UI��
    /// </summary>
    [SerializeField] private GameObject m_ChatPanel;
    /// <summary>
    /// �������Ϣ
    /// </summary>
    [SerializeField] public InputField m_InputWord;
    /// <summary>
    /// ���ص���Ϣ
    /// </summary>
    [SerializeField] private Text m_TextBack;
    /// <summary>
    /// ��������
    /// </summary>
    [SerializeField] private AudioSource m_AudioSource;
    /// <summary>
    /// ������Ϣ��ť
    /// </summary>
    [SerializeField] private Button m_CommitMsgBtn;
    /// <summary>
    /// Guidence �����ֿ�
    /// </summary>
    [SerializeField] private Text m_GuidenceText;

    #endregion
    // �������ڽ������ص�JSON���ݵ���
    // ��������
    private string gamemasterGuidance;
    private string aegisReaction;
    private int clueId;
    private int sceneId;

    public Material material1; // ��������1
    public Material material2; // ��������2
    public Material material3; // ��������3
    public Material material4; // ��������4
    public Material material5; // ��������5
    public Material material6; // ��������6

    public Material clue1; // ��������1
    public Material clue2; // ��������2
    public Material clue3; // ��������3
    public Material clue4; // ��������4
    public Material clue5; // ��������5
    public Material clue6; // ��������6

    public GameObject quadObject1; // ����Ҫ�������ʵ�Quad����
    public GameObject quadObject2;
    #region ��������
    /// <summary>
    /// ����������
    /// </summary>
    [SerializeField] private Animator m_Animator;
    /// <summary>
    /// ����ģʽ������Ϊfalse,��ͨ�������ϳ�
    /// </summary>
    [Header("�����Ƿ�ͨ�������ϳɲ����ı�")]
    [SerializeField] private bool m_IsVoiceMode = true;
    [Header("��ѡ�򲻷���LLM��ֱ�Ӻϳ���������")]
    [SerializeField] private bool m_CreateVoiceMode = false;

    #endregion

    private void Awake()
    {
        m_CommitMsgBtn.onClick.AddListener(delegate { SendData(); });
        RegistButtonEvent();
        InputSettingWhenWebgl();
    }

    #region ��Ϣ����

    /// <summary>
    /// webglʱ����֧����������
    /// </summary>
    private void InputSettingWhenWebgl()
    {
#if UNITY_WEBGL
        m_InputWord.gameObject.AddComponent<WebGLSupport.WebGLInput>();
#endif
    }


    /// <summary>
    /// ������Ϣ
    /// </summary>
    public void SendData()
    {
        if (m_InputWord.text.Equals(""))
            return;

        if (m_CreateVoiceMode)//�ϳ�����Ϊ����
        {
            CallBack(m_InputWord.text);
            m_InputWord.text = "";
            return;
        }


        //��Ӽ�¼����
        m_ChatHistory.Add(m_InputWord.text);
        //��ʾ��
        string _msg = m_InputWord.text;

        //��������
        m_ChatSettings.m_ChatModel.PostMsg(_msg, CallBack);

        m_InputWord.text = "";
        m_TextBack.text = "Thinking...";

        //�л�˼������
        SetAnimator("state", 1);
    }
    /// <summary>
    /// �����ַ���
    /// </summary>
    /// <param name="_postWord"></param>
    public void SendData(string _postWord)
    {
        if (_postWord.Equals(""))
            return;

        if (m_CreateVoiceMode)//�ϳ�����Ϊ����
        {
            CallBack(_postWord);
            m_InputWord.text = "";
            return;
        }


        //��Ӽ�¼����
        m_ChatHistory.Add(_postWord);
        //��ʾ��
        string _msg = _postWord;

        //��������
        m_ChatSettings.m_ChatModel.PostMsg(_msg, CallBack);

        m_InputWord.text = "";
        m_TextBack.text = "Thinking...";

        //�л�˼������
        SetAnimator("state", 1);
    }

    /// <summary>
    /// AI�ظ�����Ϣ�Ļص�
    /// </summary>
    /// <param name="_response"></param>
    private void CallBack(string _response)
    {


        ResponseData responseData = JsonUtility.FromJson<ResponseData>(_response);

        // �洢�����������
        gamemasterGuidance = responseData.gamemaster_guidance;
        aegisReaction = responseData.aegis_reaction;
        clueId = responseData.clue_id;
        sceneId = responseData.scene_id;

        /*        _response = _response.Trim();*/
        m_TextBack.text = "";

        m_GuidenceText.text = gamemasterGuidance;

        Debug.Log("�յ�AI�ظ���" + _response);
        Debug.Log(sceneId); 
        Debug.Log(clueId);
        //��¼����
        m_ChatHistory.Add(aegisReaction);


        // �л���������scene_id

        if (sceneId != null)
        {
            SwitchBackground(sceneId);
        }

        if (clueId != null)
        {
            SwitchClue(clueId);
        }


        if (!m_IsVoiceMode || m_ChatSettings.m_TextToSpeech == null)
        {
            //��ʼ�����ʾ���ص��ı�
            StartTypeWords(aegisReaction);
            return;
        }
        m_ChatSettings.m_TextToSpeech.Speak(aegisReaction, PlayVoice);
    }
    public void SwitchBackground(int scene_id)
    {
        Material newMaterial = null;

        switch (scene_id)
        {
            case 1:
                newMaterial = material1;
                break;
            case 2:
                newMaterial = material2;
                break;
            case 3:
                newMaterial = material3;
                break;
            case 4:
                newMaterial = material4;
                break;
            case 5:
                newMaterial = material5;
                break;
            case 6:
                newMaterial = material6;
                break;
            default:
                Debug.LogWarning("δ����� scene_id: " + scene_id);
                return; // ���scene_idδ���壬ֱ�ӷ���
        }

        // �����µĲ���
        quadObject1.GetComponent<Renderer>().material = newMaterial;
    }
    public void SwitchClue(int clue_id)
    {
        Material newMaterial = null;

        switch (clue_id)
        {
            case 1:
                newMaterial = clue1;
                break;
            case 2:
                newMaterial = clue2;
                break;
            case 3:
                newMaterial = clue3;
                break;
            case 4:
                newMaterial = clue4;
                break;
            case 5:
                newMaterial = clue5;
                break;
            case 6:
                newMaterial = clue6;
                break;
            default:
                Debug.LogWarning("δ����� clue_id: " + clue_id);
                return; // ���scene_idδ���壬ֱ�ӷ���
        }

        // �����µĲ���
        quadObject2.GetComponent<Renderer>().material = newMaterial;
    }
    #endregion

    #region ��������
    /// <summary>
    /// ����ʶ�𷵻ص��ı��Ƿ�ֱ�ӷ�����LLM
    /// </summary>
    [SerializeField] private bool m_AutoSend = true;
    /// <summary>
    /// ��������İ�ť
    /// </summary>
    [SerializeField] private Button m_VoiceInputBotton;
    /// <summary>
    /// ¼����ť���ı�
    /// </summary>
    [SerializeField] private Text m_VoiceBottonText;
    /// <summary>
    /// ¼������ʾ��Ϣ
    /// </summary>
    [SerializeField] private Text m_RecordTips;
    /// <summary>
    /// �������봦����
    /// </summary>
    [SerializeField] private VoiceInputs m_VoiceInputs;
    /// <summary>
    /// ע�ᰴť�¼�
    /// </summary>
    private void RegistButtonEvent()
    {
        if (m_VoiceInputBotton == null || m_VoiceInputBotton.GetComponent<EventTrigger>())
            return;

        EventTrigger _trigger = m_VoiceInputBotton.gameObject.AddComponent<EventTrigger>();

        //��Ӱ�ť���µ��¼�
        EventTrigger.Entry _pointDown_entry = new EventTrigger.Entry();
        _pointDown_entry.eventID = EventTriggerType.PointerDown;
        _pointDown_entry.callback = new EventTrigger.TriggerEvent();

        //��Ӱ�ť�ɿ��¼�
        EventTrigger.Entry _pointUp_entry = new EventTrigger.Entry();
        _pointUp_entry.eventID = EventTriggerType.PointerUp;
        _pointUp_entry.callback = new EventTrigger.TriggerEvent();

        //���ί���¼�
        _pointDown_entry.callback.AddListener(delegate { StartRecord(); });
        _pointUp_entry.callback.AddListener(delegate { StopRecord(); });

        _trigger.triggers.Add(_pointDown_entry);
        _trigger.triggers.Add(_pointUp_entry);
    }

    /// <summary>
    /// ��ʼ¼��
    /// </summary>
    public void StartRecord()
    {
        m_VoiceBottonText.text = "����¼����...";
        m_VoiceInputs.StartRecordAudio();
    }
    /// <summary>
    /// ����¼��
    /// </summary>
    public void StopRecord()
    {
        m_VoiceBottonText.text = "��ס��ť����ʼ¼��";
        m_RecordTips.text = "¼������������ʶ��...";
        m_VoiceInputs.StopRecordAudio(AcceptClip);
    }

    /// <summary>
    /// ����¼�Ƶ���Ƶ����
    /// </summary>
    /// <param name="_data"></param>
    private void AcceptData(byte[] _data)
    {
        if (m_ChatSettings.m_SpeechToText == null)
            return;

        m_ChatSettings.m_SpeechToText.SpeechToText(_data, DealingTextCallback);
    }

    /// <summary>
    /// ����¼�Ƶ���Ƶ����
    /// </summary>
    /// <param name="_data"></param>
    private void AcceptClip(AudioClip _audioClip)
    {
        if (m_ChatSettings.m_SpeechToText == null)
            return;

        m_ChatSettings.m_SpeechToText.SpeechToText(_audioClip, DealingTextCallback);
    }
    /// <summary>
    /// ����ʶ�𵽵��ı�
    /// </summary>
    /// <param name="_msg"></param>
    private void DealingTextCallback(string _msg)
    {
        m_RecordTips.text = _msg;
        StartCoroutine(SetTextVisible(m_RecordTips));
        //�Զ�����
        if (m_AutoSend)
        {
            SendData(_msg);
            return;
        }

        m_InputWord.text = _msg;
    }

    private IEnumerator SetTextVisible(Text _textbox)
    {
        yield return new WaitForSeconds(3f);
        _textbox.text = "";
    }

    #endregion

    #region �����ϳ�

    private void PlayVoice(AudioClip _clip, string _response)
    {
        m_AudioSource.clip = _clip;
        m_AudioSource.Play();
        Debug.Log("��Ƶʱ����" + _clip.length);
        //��ʼ�����ʾ���ص��ı�
        StartTypeWords(_response);
        //�л���˵������
        SetAnimator("state", 2);
    }

    #endregion

    #region ���������ʾ
    //������ʾ��ʱ����
    [SerializeField] private float m_WordWaitTime = 0.2f;
    //�Ƿ���ʾ���
    [SerializeField] private bool m_WriteState = false;

    /// <summary>
    /// ��ʼ�����ӡ
    /// </summary>
    /// <param name="_msg"></param>
    private void StartTypeWords(string _msg)
    {
        if (_msg == "")
            return;

        m_WriteState = true;
        StartCoroutine(SetTextPerWord(_msg));
    }

    private IEnumerator SetTextPerWord(string _msg)
    {
        int currentPos = 0;
        while (m_WriteState)
        {
            yield return new WaitForSeconds(m_WordWaitTime);
            currentPos++;
            //������ʾ������
            m_TextBack.text = _msg.Substring(0, currentPos);
/*            m_GuidenceText.text = _msg.Substring(0, currentPos);*/
            m_WriteState = currentPos < _msg.Length;

        }

        //�л����ȴ�����
        SetAnimator("state", 0);
    }

    #endregion

    #region �����¼
    //���������¼
    [SerializeField] private List<string> m_ChatHistory;
    //�����Ѵ�������������
    [SerializeField] private List<GameObject> m_TempChatBox;
    //�����¼��ʾ��
    [SerializeField] private GameObject m_HistoryPanel;
    //�����ı����õĲ�
    [SerializeField] private RectTransform m_rootTrans;
    //������������
    [SerializeField] private ChatPrefab m_PostChatPrefab;
    //�ظ�����������
    [SerializeField] private ChatPrefab m_RobotChatPrefab;
    //������
    [SerializeField] private ScrollRect m_ScroTectObject;
    //��ȡ�����¼
    public void OpenAndGetHistory()
    {
        m_ChatPanel.SetActive(false);
        m_HistoryPanel.SetActive(true);

        ClearChatBox();
        StartCoroutine(GetHistoryChatInfo());
    }
    //����
    public void BackChatMode()
    {
        m_ChatPanel.SetActive(true);
        m_HistoryPanel.SetActive(false);
    }

    //����Ѵ����ĶԻ���
    private void ClearChatBox()
    {
        while (m_TempChatBox.Count != 0)
        {
            if (m_TempChatBox[0])
            {
                Destroy(m_TempChatBox[0].gameObject);
                m_TempChatBox.RemoveAt(0);
            }
        }
        m_TempChatBox.Clear();
    }

    //��ȡ�����¼�б�
    private IEnumerator GetHistoryChatInfo()
    {

        yield return new WaitForEndOfFrame();

        for (int i = 0; i < m_ChatHistory.Count; i++)
        {
            if (i % 2 == 0)
            {
                ChatPrefab _sendChat = Instantiate(m_PostChatPrefab, m_rootTrans.transform);
                _sendChat.SetText(m_ChatHistory[i]);
                m_TempChatBox.Add(_sendChat.gameObject);
                continue;
            }

            ChatPrefab _reChat = Instantiate(m_RobotChatPrefab, m_rootTrans.transform);
            _reChat.SetText(m_ChatHistory[i]);
            m_TempChatBox.Add(_reChat.gameObject);
        }

        //���¼��������ߴ�
        LayoutRebuilder.ForceRebuildLayoutImmediate(m_rootTrans);
        StartCoroutine(TurnToLastLine());
    }

    private IEnumerator TurnToLastLine()
    {
        yield return new WaitForEndOfFrame();
        //�������������Ϣ
        m_ScroTectObject.verticalNormalizedPosition = 0;
    }


    #endregion

    private void SetAnimator(string _para, int _value)
    {
        if (m_Animator == null)
            return;

        m_Animator.SetInteger(_para, _value);
    }

    // �л���ָ���ĳ�������
    public void SwitchScene(string sceneName)
    {
        SceneManager.LoadScene("Level2");
    }
    public Image image1; // ��һ��ͼƬ
    public Image image2; // �ڶ���ͼƬ

    private bool isImage1Active = true;

    // ����ť�����ʱ���ô˷���
    public void SwitchImage()
    {
        if (isImage1Active)
        {
            image1.gameObject.SetActive(false);
            image2.gameObject.SetActive(true);
        }
        else
        {
            image1.gameObject.SetActive(true);
            image2.gameObject.SetActive(false);
        }
        isImage1Active = !isImage1Active;
    }

    [Serializable]
    public class ResponseData
    {
        public string gamemaster_guidance;
        public string aegis_reaction;
        public int clue_id;
        public int scene_id;
    }
}

