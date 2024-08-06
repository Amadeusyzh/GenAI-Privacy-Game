using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class ChatOllama : LLM
{
    /// <summary>
    /// AI设定
    /// </summary>
    public string m_SystemSetting = string.Empty;
    /// <summary>
    /// 设置模型,模型类型自行添加
    /// </summary>
    public ModelType m_GptModel = ModelType.llama3;

    private void Start()
    {
        //运行时，添加AI设定
        m_DataList.Add(new SendData("system", m_SystemSetting));
    }

    /// <summary>
    /// 发送消息
    /// </summary>
    /// <returns></returns>
    public override IEnumerator Request(string _postWord, System.Action<string> _callback)
    {
        stopwatch.Restart();
        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            PostData _postData = new PostData
            {
                model = m_GptModel.ToString(),
                messages = m_DataList
            };

            string _jsonText = JsonUtility.ToJson(_postData);
            byte[] data = System.Text.Encoding.UTF8.GetBytes(_jsonText);
            request.uploadHandler = (UploadHandler)new UploadHandlerRaw(data);
            request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();

            request.SetRequestHeader("Content-Type", "application/json");
            //request.SetRequestHeader("Authorization", string.Format("Bearer {0}", api_key));

            yield return request.SendWebRequest();

            if (request.responseCode == 200)
            {
                string _msgBack = request.downloadHandler.text;

                // 将返回的内容解析为MessageBack对象
                MessageBack _textback = JsonUtility.FromJson<MessageBack>(_msgBack);
                if (_textback != null && _textback.message != null)
                {
                    string _backMsg = _textback.message.content;
                    
                    // 解析嵌套的GameResponse
                    GameResponse response = JsonUtility.FromJson<GameResponse>(_backMsg);
                    Debug.Log("LLM生成: " + _backMsg);
                    if (response != null)
                    {
                        Debug.Log("Game Master Guidance: " + response.gamemaster_guidance);
                        Debug.Log("Aegis Reaction: " + response.aegis_reaction);
                        Debug.Log("Clue ID: " + response.clue_id);
                        Debug.Log("Scene ID: " + response.scene_id);
                    }

                    // 添加记录
                    m_DataList.Add(new SendData("assistant", _backMsg));
                    _callback("Game Master Guidance:"+ response.gamemaster_guidance+"\n"+ "\n" + "Aegis:"+response.aegis_reaction);
                }
            }
            else
            {
                string _msgBack = request.downloadHandler.text;
                Debug.LogError(_msgBack);
            }

            stopwatch.Stop();
            Debug.Log("Ollama耗时：" + stopwatch.Elapsed.TotalSeconds);
        }
    }


    #region 数据定义

    public enum ModelType
    {
        llama3
    }

    [Serializable]
    public class PostData
    {
        public string model;
        public List<SendData> messages;
        public bool stream = false;//流式
    }
    [Serializable]
    public class MessageBack
    {
        public string created_at;
        public string model;
        public Message message;
        public GameResponse response; // 添加这一行来包含解析后的JSON响应
    }

    [Serializable]
    public class Message
    {
        public string role;
        public string content;
    }

    [Serializable]
    public class GameResponse
    {
        public string gamemaster_guidance;
        public string aegis_reaction;
        public int? clue_id;
        public int? scene_id;
    }
    #endregion

}
