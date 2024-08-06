using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class ChatOllama : LLM
{
    /// <summary>
    /// AI�趨
    /// </summary>
    public string m_SystemSetting = string.Empty;
    /// <summary>
    /// ����ģ��,ģ�������������
    /// </summary>
    public ModelType m_GptModel = ModelType.llama3;

    private void Start()
    {
        //����ʱ�����AI�趨
        m_DataList.Add(new SendData("system", m_SystemSetting));
    }

    /// <summary>
    /// ������Ϣ
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

                // �����ص����ݽ���ΪMessageBack����
                MessageBack _textback = JsonUtility.FromJson<MessageBack>(_msgBack);
                if (_textback != null && _textback.message != null)
                {
                    string _backMsg = _textback.message.content;
                    
                    // ����Ƕ�׵�GameResponse
                    GameResponse response = JsonUtility.FromJson<GameResponse>(_backMsg);
                    Debug.Log("LLM����: " + _backMsg);
                    if (response != null)
                    {
                        Debug.Log("Game Master Guidance: " + response.gamemaster_guidance);
                        Debug.Log("Aegis Reaction: " + response.aegis_reaction);
                        Debug.Log("Clue ID: " + response.clue_id);
                        Debug.Log("Scene ID: " + response.scene_id);
                    }

                    // ��Ӽ�¼
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
            Debug.Log("Ollama��ʱ��" + stopwatch.Elapsed.TotalSeconds);
        }
    }


    #region ���ݶ���

    public enum ModelType
    {
        llama3
    }

    [Serializable]
    public class PostData
    {
        public string model;
        public List<SendData> messages;
        public bool stream = false;//��ʽ
    }
    [Serializable]
    public class MessageBack
    {
        public string created_at;
        public string model;
        public Message message;
        public GameResponse response; // �����һ���������������JSON��Ӧ
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
