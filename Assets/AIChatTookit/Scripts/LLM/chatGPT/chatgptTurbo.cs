using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class chatgptTurbo : LLM
{
    public chatgptTurbo()
    {
        url = "https://api.openai.com/v1/chat/completions";
    }

    /// <summary>
    /// api key
    /// </summary>
    [SerializeField] private string api_key;
    /// <summary>
    /// AI设定
    /// </summary>
    public string m_SystemSetting = string.Empty;
    /// <summary>
    /// gpt-3.5-turbo
    /// </summary>
    public string m_gptModel = "gpt-4o";


    private void Start()
    {
        //运行时，添加AI设定
        m_DataList.Add(new SendData("system", m_SystemSetting));
    }

    // 新增字段用于存储解析后的数据
    public string gamemasterGuidance { get; private set; }
    public string aegisReaction { get; private set; }
    public int? clueId { get; private set; }
    public int? sceneId { get; private set; }

    /// <summary>
    /// 发送消息
    /// </summary>
    /// <returns></returns>
    public override void PostMsg(string _msg, Action<string> _callback)
    {
        base.PostMsg(_msg, _callback);
    }

    /// <summary>
    /// 调用接口
    /// </summary>
    /// <param name="_postWord"></param>
    /// <param name="_callback"></param>
    /// <returns></returns>
    public override IEnumerator Request(string _postWord, System.Action<string> _callback)
    {
        stopwatch.Restart();
        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            PostData _postData = new PostData
            {
                model = m_gptModel,
                messages = m_DataList
            };

            string _jsonText = JsonUtility.ToJson(_postData).Trim();
            byte[] data = System.Text.Encoding.UTF8.GetBytes(_jsonText);
            request.uploadHandler = (UploadHandler)new UploadHandlerRaw(data);
            request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();

            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", string.Format("Bearer {0}", api_key));

            yield return request.SendWebRequest();

            if (request.responseCode == 200)
            {


                string _msgBack = request.downloadHandler.text;
                MessageBack _textback = JsonUtility.FromJson<MessageBack>(_msgBack);
                if (_textback != null && _textback.choices.Count > 0)
                {

                    string _backMsg = _textback.choices[0].message.content;
                    int jsonStartIndex = _backMsg.IndexOf('{');
                    int jsonEndIndex = _backMsg.LastIndexOf('}');
                    string cleanJson = _backMsg.Substring(jsonStartIndex, jsonEndIndex - jsonStartIndex + 1);
                    // 解析纯JSON内容
                    ResponseData responseData = JsonUtility.FromJson<ResponseData>(cleanJson);

                    // 存储解析后的数据
                    gamemasterGuidance = responseData.gamemaster_guidance;
                    aegisReaction = responseData.aegis_reaction;
                    clueId = responseData.clue_id.HasValue ? responseData.clue_id : null;
                    sceneId = responseData.scene_id.HasValue ? responseData.scene_id : null;

                    //添加记录
                    m_DataList.Add(new SendData("assistant", cleanJson));
                    _callback(cleanJson);
                }

            }
            else
            {
                string _msgBack = request.downloadHandler.text;
                Debug.LogError(_msgBack);
            }
            /*
                        if (request.responseCode == 200)
                        {
                            string _msgBack = request.downloadHandler.text;

                            // 检查并去除多余的文本
                            int jsonStartIndex = _msgBack.IndexOf('{');
                            int jsonEndIndex = _msgBack.LastIndexOf('}');
                            if (jsonStartIndex >= 0 && jsonEndIndex >= 0)
                            {
                                // 提取纯JSON部分
                                string cleanJson = _msgBack.Substring(jsonStartIndex, jsonEndIndex - jsonStartIndex + 1);
                                if (_textback != null && _textback.choices.Count > 0)
                                {

                                    string _backMsg = _textback.choices[0].message.content;
                                    //添加记录
                                    m_DataList.Add(new SendData("assistant", _backMsg));
                                    _callback(_backMsg);
                                }

                                }
                                else
                                {
                                    string _msgBack = request.downloadHandler.text;
                                    Debug.LogError(_msgBack);
                                }
                            try
                                {
                                    // 解析纯JSON内容
                                    ResponseData responseData = JsonUtility.FromJson<ResponseData>(cleanJson);

                                    // 存储解析后的数据
                                    gamemasterGuidance = responseData.gamemaster_guidance;
                                    aegisReaction = responseData.aegis_reaction;
                                    clueId = responseData.clue_id.HasValue ? responseData.clue_id : null;
                                    sceneId = responseData.scene_id.HasValue ? responseData.scene_id : null;

                                    // 回调
                                    _callback(cleanJson);
                                }
                                catch (Exception ex)
                                {
                                    Debug.LogError("Error parsing JSON response: " + ex.Message);
                                }
                            }
                            else
                            {
                                Debug.LogError("No valid JSON found in the response.");
                            }
                        }
                        else
                        {
                            string _msgBack = request.downloadHandler.text;
                            Debug.LogError(_msgBack);
                        }*/
            stopwatch.Stop();
            Debug.Log("chatgpt耗时："+ stopwatch.Elapsed.TotalSeconds);
        }
    }

    #region 数据包

    [Serializable]
    public class PostData
    {
        [SerializeField]public string model;
        [SerializeField] public List<SendData> messages;
        [SerializeField] public float temperature = 0.7f;
    }

    [Serializable]
    public class MessageBack
    {
        public string id;
        public string created;
        public string model;
        public List<MessageBody> choices;
    }
    [Serializable]
    public class MessageBody
    {
        public Message message;
        public string finish_reason;
        public string index;
    }
    [Serializable]
    public class Message
    {
        public string role;
        public string content;
    }

    // 新增用于解析返回的JSON数据的类
    [Serializable]
    public class ResponseData
    {
        public string gamemaster_guidance;
        public string aegis_reaction;
        public int? clue_id;
        public int? scene_id;
    }

    #endregion

}
