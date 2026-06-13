using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace Aesheria.AI
{
    public class AIDMClient : MonoBehaviour
    {
        [Header("DeepSeek API Settings")]
        [Tooltip("DeepSeek API 端点")]
        public string apiUrl = "https://api.deepseek.com/v1/chat/completions";
        [Tooltip("你的 API Key（生产环境不要硬编码）")]
        public string apiKey = "YOUR_DEEPSEEK_API_KEY_HERE";
        [Tooltip("是否使用模拟响应（避免消耗配额或网络问题）")]
        public bool useMock = true;

        [Header("Request Parameters")]
        public string model = "deepseek-chat";
        public int maxTokens = 150;
        public float temperature = 0.7f;

        /// <summary>
        /// 请求 AI 生成旁白
        /// </summary>
        /// <param name="userPrompt">用户提示词</param>
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

        // 真实 DeepSeek API 请求
        private IEnumerator RealDeepSeekRequest(string userPrompt, Action<string> onSuccess, Action<string> onError)
        {
            var requestBody = new
            {
                model = model,
                messages = new[]
                {
                    new { role = "system", content = "你是一个旁白助手，为奇幻游戏《艾瑟瑞亚》提供诗意的环境描述或结局演算。语言优美，简洁有力。" },
                    new { role = "user", content = userPrompt }
                },
                max_tokens = maxTokens,
                temperature = temperature,
                stream = false
            };

            string jsonBody = JsonUtility.ToJson(requestBody);
            using (UnityWebRequest request = new UnityWebRequest(apiUrl, "POST"))
            {
                byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");
                request.SetRequestHeader("Authorization", $"Bearer {apiKey}");

                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    string jsonResponse = request.downloadHandler.text;
                    string content = ExtractContentFromResponse(jsonResponse);
                    onSuccess?.Invoke(content);
                }
                else
                {
                    string errorMsg = $"DeepSeek API Error: {request.error} - {request.downloadHandler.text}";
                    Debug.LogError(errorMsg);
                    onError?.Invoke(errorMsg);
                }
            }
        }

        // 模拟响应（无需 API 密钥，用于快速测试）
        private IEnumerator MockResponseCoroutine(string userPrompt, Action<string> onSuccess, Action<string> onError)
        {
            yield return new WaitForSeconds(0.3f);

            GameManager gm = FindObjectOfType<GameManager>();
            string mockText = "灵脉低语，繁华与枯寂同源。你在选择中听见大地的呼吸。";

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

        // 从 DeepSeek 响应中提取 content 字段（与 OpenAI 格式一致）
        private string ExtractContentFromResponse(string json)
        {
            string search = "\"content\":\"";
            int startIndex = json.IndexOf(search);
            if (startIndex == -1) return "无法解析 AI 响应";
            startIndex += search.Length;

            int endIndex = json.IndexOf("\"", startIndex);
            if (endIndex == -1) return "解析失败";

            string content = json.Substring(startIndex, endIndex - startIndex);
            content = content.Replace("\\n", "\n").Replace("\\\"", "\"");
            return content;
        }
    }
}
