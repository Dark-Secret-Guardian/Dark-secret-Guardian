using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Aesheria.AI
{
    // JSON 请求体序列化类（JsonUtility 只支持 [Serializable] 类）
    [Serializable]
    public class ChatMessage
    {
        public string role;
        public string content;
    }

    [Serializable]
    public class ChatRequest
    {
        public string model;
        public List<ChatMessage> messages;
        public int max_tokens;
        public float temperature;
        public bool stream;
    }

    [Serializable]
    public class ChatChoice
    {
        public ChatMessage message;
    }

    [Serializable]
    public class ChatResponse
    {
        public List<ChatChoice> choices;
    }

    /// <summary>
    /// AI 旁白客户端 —— 集成 DeepSeek API（OpenAI 兼容格式）
    /// 支持两种模式：
    /// - Mock 模式：根据当前游戏状态返回预设文本，无需 API 密钥
    /// - 真实模式：调用 DeepSeek API 生成动态诗意旁白
    /// WebGL 构建中因 CORS 限制，建议使用 Mock 模式
    /// </summary>
    public class AIDMClient : MonoBehaviour
    {
        [Header("DeepSeek API Settings")]
        [Tooltip("DeepSeek API 端点")]
        public string apiUrl = "https://api.deepseek.com/v1/chat/completions";
        [Tooltip("你的 API Key")]
        public string apiKey = "sk-96eab29543ae49108be5ccccbf17951b";
        [Tooltip("是否使用模拟响应（避免消耗配额或网络问题）")]
        public bool useMock = false;

        [Header("Request Parameters")]
        public string model = "deepseek-chat";   // 使用的模型名称
        public int maxTokens = 150;               // 最大生成 token 数
        public float temperature = 0.7f;          // 生成温度（越高越随机）

        /// <summary>
        /// 请求 AI 生成旁白
        /// 根据 useMock 标志决定使用模拟响应还是真实 API 请求
        /// </summary>
        /// <param name="userPrompt">用户提示词（包含当前游戏状态）</param>
        /// <param name="onSuccess">成功回调，参数为 AI 返回的文本</param>
        /// <param name="onError">失败回调，参数为错误信息</param>
        public void RequestNarrative(string userPrompt, Action<string> onSuccess, Action<string> onError = null)
        {
            if (useMock)
            {
                StartCoroutine(MockResponseCoroutine(userPrompt, onSuccess, onError));
            }
            else
            {
                StartCoroutine(RealDeepSeekRequest(userPrompt, onSuccess, onError));
            }
        }

        /// <summary>
        /// 请求开局旁白（介绍世界观）
        /// </summary>
        public void RequestOpeningNarration(Action<string> onSuccess, Action<string> onError = null)
        {
            GameManager gm = FindObjectOfType<GameManager>();
            string prompt = "游戏刚刚开始。请用一段神秘的开场白介绍这个世界：灵脉是大地之源，人类开采灵晶换取繁荣，但生态正在崩塌。古树青檀守护灵脉源头，酒馆老板红袖叹息山林凋败，交易所会长墨衡权衡利弊。玩家是一个刚到达这个世界的旅人，命运尚未书写。请用第二人称'你'来叙述，80字以内。";
            RequestNarrative(prompt, onSuccess, onError);
        }

        /// <summary>
        /// 请求中途旁白（提示或误导）
        /// </summary>
        public void RequestMidGameCommentary(Action<string> onSuccess, Action<string> onError = null)
        {
            GameManager gm = FindObjectOfType<GameManager>();
            int p = gm != null ? gm.prosperity : 50;
            int e = gm != null ? gm.ecology : 75;
            int a = gm != null ? gm.awakening : 0;
            int g = gm != null ? gm.guardianship : 0;

            string prompt = $"游戏中途。当前状态——繁荣{p}，生态{e}，觉醒{a}，守护{g}。请以神秘旁白的身份说一句话，可能是提示也可能是误导（真假参半），暗示玩家接下来的方向或暗示某种后果。50字以内，不要解释数值。";
            RequestNarrative(prompt, onSuccess, onError);
        }

        /// <summary>
        /// 真实 DeepSeek API 请求
        /// 使用 UnityWebRequest 发送 POST 请求
        /// 请求格式与 OpenAI Chat Completions API 兼容
        /// </summary>
        private IEnumerator RealDeepSeekRequest(string userPrompt, Action<string> onSuccess, Action<string> onError)
        {
            // 构建请求体
            var requestBody = new ChatRequest
            {
                model = model,
                messages = new List<ChatMessage>
                {
                    new ChatMessage
                    {
                        role = "system",
                        content = "你是一款奇幻叙事RPG《繁荣悖论》的AI主持人（DM）。你的职责：1.游戏开局时介绍世界观背景；2.游戏中途随机出现，给予玩家提示或误导（真假参半）。风格：神秘、诗意、像一个看不见的旁白在耳边低语。每次回复不超过80字，语言简洁有力。不要使用Markdown格式。"
                    },
                    new ChatMessage
                    {
                        role = "user",
                        content = userPrompt
                    }
                },
                max_tokens = maxTokens,
                temperature = temperature,
                stream = false
            };

            // 序列化请求体为 JSON
            string jsonBody = JsonUtility.ToJson(requestBody);

            // 创建并发送 HTTP POST 请求
            using (UnityWebRequest request = new UnityWebRequest(apiUrl, "POST"))
            {
                byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");
                request.SetRequestHeader("Authorization", $"Bearer {apiKey}");

                // 等待请求完成
                yield return request.SendWebRequest();

                // 处理响应
                if (request.result == UnityWebRequest.Result.Success)
                {
                    string jsonResponse = request.downloadHandler.text;
                    // 用 ChatResponse 类反序列化
                    ChatResponse response = JsonUtility.FromJson<ChatResponse>(jsonResponse);
                    if (response != null && response.choices != null && response.choices.Count > 0)
                    {
                        string content = response.choices[0].message.content;
                        onSuccess?.Invoke(content);
                    }
                    else
                    {
                        // 回退到字符串解析
                        string content = ExtractContentFromResponse(jsonResponse);
                        onSuccess?.Invoke(content);
                    }
                }
                else
                {
                    // 请求失败，调用错误回调
                    string errorMsg = $"DeepSeek API Error: {request.error} - {request.downloadHandler.text}";
                    Debug.LogError(errorMsg);
                    onError?.Invoke(errorMsg);
                }
            }
        }

        /// <summary>
        /// 模拟响应协程（无需 API 密钥）
        /// 根据当前生态值和觉醒值返回不同的预设文本
        /// 模拟 0.3 秒延迟以模拟网络请求
        /// </summary>
        private IEnumerator MockResponseCoroutine(string userPrompt, Action<string> onSuccess, Action<string> onError)
        {
            // 模拟网络延迟
            yield return new WaitForSeconds(0.3f);

            // 获取当前游戏状态
            GameManager gm = FindObjectOfType<GameManager>();
            string mockText = "灵脉低语，繁华与枯寂同源。你在选择中听见大地的呼吸。";

            // 根据游戏状态返回不同的预设旁白
            if (gm != null)
            {
                if (gm.ecology >= 70)
                    mockText = "灵脉如泉涌，万物繁盛。但智者见衰微于未萌。";
                else if (gm.ecology <= 30)
                    mockText = "大地龟裂，灵脉残喘。文明的灯火映照着死亡的阴影。";
                else if (gm.awakening >= 60)
                    mockText = "你已看透繁荣的代价，愿平衡之种在你手中萌芽。";
            }

            onSuccess?.Invoke(mockText);
        }

        /// <summary>
        /// 从 DeepSeek API 的 JSON 响应中提取 content 字段
        /// 响应格式与 OpenAI Chat Completions API 一致
        /// </summary>
        private string ExtractContentFromResponse(string json)
        {
            string search = "\"content\":\"";
            int startIndex = json.IndexOf(search);
            if (startIndex == -1) return "无法解析 AI 响应";
            startIndex += search.Length;

            int endIndex = json.IndexOf("\"", startIndex);
            if (endIndex == -1) return "解析失败";

            // 提取内容并处理转义字符
            string content = json.Substring(startIndex, endIndex - startIndex);
            content = content.Replace("\\n", "\n").Replace("\\\"", "\"");
            return content;
        }
    }
}
