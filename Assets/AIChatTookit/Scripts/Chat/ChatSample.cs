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
    /// 聊天配置
    /// </summary>
    [SerializeField] private ChatSetting m_ChatSettings;
    #region UI定义
    /// <summary>
    /// 聊天UI窗
    /// </summary>
    [SerializeField] private GameObject m_ChatPanel;
    /// <summary>
    /// 输入的信息
    /// </summary>
    [SerializeField] public InputField m_InputWord;
    /// <summary>
    /// 返回的信息
    /// </summary>
    [SerializeField] private Text m_TextBack;
    /// <summary>
    /// 播放声音
    /// </summary>
    [SerializeField] private AudioSource m_AudioSource;
    /// <summary>
    /// 发送信息按钮
    /// </summary>
    [SerializeField] private Button m_CommitMsgBtn;
    /// <summary>
    /// Guidence 的文字框
    /// </summary>
    [SerializeField] private Text m_GuidenceText;

    #endregion
    // 新增用于解析返回的JSON数据的类
    // 声明变量
    private string gamemasterGuidance;
    private string aegisReaction;
    private int clueId;
    private int sceneId;

    public Material material1; // 背景材质1
    public Material material2; // 背景材质2
    public Material material3; // 背景材质3
    public Material material4; // 背景材质4
    public Material material5; // 背景材质5
    public Material material6; // 背景材质6

    public Material clue1; // 背景材质1
    public Material clue2; // 背景材质2
    public Material clue3; // 背景材质3
    public Material clue4; // 背景材质4
    public Material clue5; // 背景材质5
    public Material clue6; // 背景材质6

    public GameObject quadObject1; // 你想要更换材质的Quad对象
    public GameObject quadObject2;
    #region 参数定义
    /// <summary>
    /// 动画控制器
    /// </summary>
    [SerializeField] private Animator m_Animator;
    /// <summary>
    /// 语音模式，设置为false,则不通过语音合成
    /// </summary>
    [Header("设置是否通过语音合成播放文本")]
    [SerializeField] private bool m_IsVoiceMode = true;
    [Header("勾选则不发送LLM，直接合成输入文字")]
    [SerializeField] private bool m_CreateVoiceMode = false;

    #endregion

    private void Awake()
    {
        m_CommitMsgBtn.onClick.AddListener(delegate { SendData(); });
        RegistButtonEvent();
        InputSettingWhenWebgl();
    }

    #region 消息发送

    /// <summary>
    /// webgl时处理，支持中文输入
    /// </summary>
    private void InputSettingWhenWebgl()
    {
#if UNITY_WEBGL
        m_InputWord.gameObject.AddComponent<WebGLSupport.WebGLInput>();
#endif
    }


    /// <summary>
    /// 发送信息
    /// </summary>
    public void SendData()
    {
        if (m_InputWord.text.Equals(""))
            return;

        if (m_CreateVoiceMode)//合成输入为语音
        {
            CallBack(m_InputWord.text);
            m_InputWord.text = "";
            return;
        }


        //添加记录聊天
        m_ChatHistory.Add(m_InputWord.text);
        //提示词
        string _msg = m_InputWord.text;

        //发送数据
        m_ChatSettings.m_ChatModel.PostMsg(_msg, CallBack);

        m_InputWord.text = "";
        m_TextBack.text = "Thinking...";

        //切换思考动作
        SetAnimator("state", 1);
    }
    /// <summary>
    /// 带文字发送
    /// </summary>
    /// <param name="_postWord"></param>
    public void SendData(string _postWord)
    {
        if (_postWord.Equals(""))
            return;

        if (m_CreateVoiceMode)//合成输入为语音
        {
            CallBack(_postWord);
            m_InputWord.text = "";
            return;
        }


        //添加记录聊天
        m_ChatHistory.Add(_postWord);
        //提示词
        string _msg = _postWord;

        //发送数据
        m_ChatSettings.m_ChatModel.PostMsg(_msg, CallBack);

        m_InputWord.text = "";
        m_TextBack.text = "Thinking...";

        //切换思考动作
        SetAnimator("state", 1);
    }

    /// <summary>
    /// AI回复的信息的回调
    /// </summary>
    /// <param name="_response"></param>
    private void CallBack(string _response)
    {


        ResponseData responseData = JsonUtility.FromJson<ResponseData>(_response);

        // 存储解析后的数据
        gamemasterGuidance = responseData.gamemaster_guidance;
        aegisReaction = responseData.aegis_reaction;
        clueId = responseData.clue_id;
        sceneId = responseData.scene_id;

        /*        _response = _response.Trim();*/
        m_TextBack.text = "";

        m_GuidenceText.text = gamemasterGuidance;

        Debug.Log("收到AI回复：" + _response);
        Debug.Log(sceneId); 
        Debug.Log(clueId);
        //记录聊天
        m_ChatHistory.Add(aegisReaction);


        // 切换背景根据scene_id

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
            //开始逐个显示返回的文本
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
                Debug.LogWarning("未定义的 scene_id: " + scene_id);
                return; // 如果scene_id未定义，直接返回
        }

        // 设置新的材质
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
                Debug.LogWarning("未定义的 clue_id: " + clue_id);
                return; // 如果scene_id未定义，直接返回
        }

        // 设置新的材质
        quadObject2.GetComponent<Renderer>().material = newMaterial;
    }
    #endregion

    #region 语音输入
    /// <summary>
    /// 语音识别返回的文本是否直接发送至LLM
    /// </summary>
    [SerializeField] private bool m_AutoSend = true;
    /// <summary>
    /// 语音输入的按钮
    /// </summary>
    [SerializeField] private Button m_VoiceInputBotton;
    /// <summary>
    /// 录音按钮的文本
    /// </summary>
    [SerializeField] private Text m_VoiceBottonText;
    /// <summary>
    /// 录音的提示信息
    /// </summary>
    [SerializeField] private Text m_RecordTips;
    /// <summary>
    /// 语音输入处理类
    /// </summary>
    [SerializeField] private VoiceInputs m_VoiceInputs;
    /// <summary>
    /// 注册按钮事件
    /// </summary>
    private void RegistButtonEvent()
    {
        if (m_VoiceInputBotton == null || m_VoiceInputBotton.GetComponent<EventTrigger>())
            return;

        EventTrigger _trigger = m_VoiceInputBotton.gameObject.AddComponent<EventTrigger>();

        //添加按钮按下的事件
        EventTrigger.Entry _pointDown_entry = new EventTrigger.Entry();
        _pointDown_entry.eventID = EventTriggerType.PointerDown;
        _pointDown_entry.callback = new EventTrigger.TriggerEvent();

        //添加按钮松开事件
        EventTrigger.Entry _pointUp_entry = new EventTrigger.Entry();
        _pointUp_entry.eventID = EventTriggerType.PointerUp;
        _pointUp_entry.callback = new EventTrigger.TriggerEvent();

        //添加委托事件
        _pointDown_entry.callback.AddListener(delegate { StartRecord(); });
        _pointUp_entry.callback.AddListener(delegate { StopRecord(); });

        _trigger.triggers.Add(_pointDown_entry);
        _trigger.triggers.Add(_pointUp_entry);
    }

    /// <summary>
    /// 开始录制
    /// </summary>
    public void StartRecord()
    {
        m_VoiceBottonText.text = "正在录音中...";
        m_VoiceInputs.StartRecordAudio();
    }
    /// <summary>
    /// 结束录制
    /// </summary>
    public void StopRecord()
    {
        m_VoiceBottonText.text = "按住按钮，开始录音";
        m_RecordTips.text = "录音结束，正在识别...";
        m_VoiceInputs.StopRecordAudio(AcceptClip);
    }

    /// <summary>
    /// 处理录制的音频数据
    /// </summary>
    /// <param name="_data"></param>
    private void AcceptData(byte[] _data)
    {
        if (m_ChatSettings.m_SpeechToText == null)
            return;

        m_ChatSettings.m_SpeechToText.SpeechToText(_data, DealingTextCallback);
    }

    /// <summary>
    /// 处理录制的音频数据
    /// </summary>
    /// <param name="_data"></param>
    private void AcceptClip(AudioClip _audioClip)
    {
        if (m_ChatSettings.m_SpeechToText == null)
            return;

        m_ChatSettings.m_SpeechToText.SpeechToText(_audioClip, DealingTextCallback);
    }
    /// <summary>
    /// 处理识别到的文本
    /// </summary>
    /// <param name="_msg"></param>
    private void DealingTextCallback(string _msg)
    {
        m_RecordTips.text = _msg;
        StartCoroutine(SetTextVisible(m_RecordTips));
        //自动发送
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

    #region 语音合成

    private void PlayVoice(AudioClip _clip, string _response)
    {
        m_AudioSource.clip = _clip;
        m_AudioSource.Play();
        Debug.Log("音频时长：" + _clip.length);
        //开始逐个显示返回的文本
        StartTypeWords(_response);
        //切换到说话动作
        SetAnimator("state", 2);
    }

    #endregion

    #region 文字逐个显示
    //逐字显示的时间间隔
    [SerializeField] private float m_WordWaitTime = 0.2f;
    //是否显示完成
    [SerializeField] private bool m_WriteState = false;

    /// <summary>
    /// 开始逐个打印
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
            //更新显示的内容
            m_TextBack.text = _msg.Substring(0, currentPos);
/*            m_GuidenceText.text = _msg.Substring(0, currentPos);*/
            m_WriteState = currentPos < _msg.Length;

        }

        //切换到等待动作
        SetAnimator("state", 0);
    }

    #endregion

    #region 聊天记录
    //保存聊天记录
    [SerializeField] private List<string> m_ChatHistory;
    //缓存已创建的聊天气泡
    [SerializeField] private List<GameObject> m_TempChatBox;
    //聊天记录显示层
    [SerializeField] private GameObject m_HistoryPanel;
    //聊天文本放置的层
    [SerializeField] private RectTransform m_rootTrans;
    //发送聊天气泡
    [SerializeField] private ChatPrefab m_PostChatPrefab;
    //回复的聊天气泡
    [SerializeField] private ChatPrefab m_RobotChatPrefab;
    //滚动条
    [SerializeField] private ScrollRect m_ScroTectObject;
    //获取聊天记录
    public void OpenAndGetHistory()
    {
        m_ChatPanel.SetActive(false);
        m_HistoryPanel.SetActive(true);

        ClearChatBox();
        StartCoroutine(GetHistoryChatInfo());
    }
    //返回
    public void BackChatMode()
    {
        m_ChatPanel.SetActive(true);
        m_HistoryPanel.SetActive(false);
    }

    //清空已创建的对话框
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

    //获取聊天记录列表
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

        //重新计算容器尺寸
        LayoutRebuilder.ForceRebuildLayoutImmediate(m_rootTrans);
        StartCoroutine(TurnToLastLine());
    }

    private IEnumerator TurnToLastLine()
    {
        yield return new WaitForEndOfFrame();
        //滚动到最近的消息
        m_ScroTectObject.verticalNormalizedPosition = 0;
    }


    #endregion

    private void SetAnimator(string _para, int _value)
    {
        if (m_Animator == null)
            return;

        m_Animator.SetInteger(_para, _value);
    }

    // 切换到指定的场景名称
    public void SwitchScene(string sceneName)
    {
        SceneManager.LoadScene("Level2");
    }
    public Image image1; // 第一张图片
    public Image image2; // 第二张图片

    private bool isImage1Active = true;

    // 当按钮被点击时调用此方法
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

