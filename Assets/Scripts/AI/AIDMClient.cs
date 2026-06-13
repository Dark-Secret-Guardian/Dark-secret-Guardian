using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace Aesheria.AI
{
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
        [Tooltip("你的 API Key（生产环境不要硬编码）")]
        public string apiKey = "YOUR_DEEPSEEK_API_KEY_HERE";
        [Tooltip("是否使用模拟响应（避免消耗配额或网络问题）")]
        public bool useMock = true;

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
                // Mock 模式：使用协程模拟延迟后返回预设文本
                StartCoroutine(MockResponseCoroutine(userPrompt, onSuccess, onError));
            }
            else
            {
                // 真实模式：发送 HTTP 请求到 DeepSeek API
                StartCoroutine(RealDeepSeekRequest(userPrompt, onSuccess, onError));
            }
        }

        /// <summary>
        /// 真实 DeepSeek API 请求
        /// 使用 UnityWebRequest 发送 POST 请求
        /// 请求格式与 OpenAI Chat Completions API 兼容
        /// </summary>
        private IEnumerator RealDeepSeekRequest(string userPrompt, Action<string> onSuccess, Action<string> onError)
        {
            // 构建请求体（匿名对象序列化为 JSON）
            var requestBody = new
            {
                model = model,
                messages = new[]
                {
                    // 系统提示：定义 AI 角色和行为
                    new { role = "system", content = "你是一个旁白助手，为奇幻游戏《艾瑟瑞亚》提供诗意的环境描述或结局演算。语言优美，简洁有力。" },
                    // 用户提示：包含当前游戏状态
                    new { role = "user", content = userPrompt }
                },
                max_tokens = maxTokens,
                temperature = temperature,
                stream = false   // 不使用流式响应
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
                    // 从 JSON 响应中提取 content 字段
                    string content = ExtractContentFromResponse(jsonResponse);
                    onSuccess?.Invoke(content);
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
