using chatbot.ChatDTO;
using chatbot;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using chatbot.Sanayii.Core.Interfaces;

namespace chatbot
{
    public class ChatService : IChatService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly ILogger<ChatService> _logger;

        public ChatService(
            HttpClient httpClient,
            IConfiguration config,
            ILogger<ChatService> logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _apiKey = config["OpenAI:ApiKey"] ?? throw new ArgumentNullException(nameof(config));
            
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ChatResponseDTO> SendMessageAsync(ChatRequestDTO request)
        {
            try
            {
                _logger.LogInformation("Preparing OpenAI request for message: {Message}", request.Message);

                var payload = new
                {
                    model = "gpt-4o-mini",
                    messages = new[]
                    {
                        new
                        {
                            role = "system",
                            content = @"
                            أنت مساعد ذكي داخل تطبيق 'الكترون'، وهو تطبيق للتجارة الإلكترونية متخصص في بيع المنتجات الإلكترونية.
                            مهمتك هي مساعدة المستخدمين على استكشاف المنتجات، فهم المواصفات، مقارنة الأسعار، وتقديم اقتراحات مناسبة.

                            الفئات المتاحة:
                            - هواتف ذكية
                            - لابتوبات
                            - شاشات ذكية
                            - سماعات
                            - ملحقات كمبيوتر

                            أمثلة على أسئلة المستخدم:
                            - عايز موبايل بكاميرا حلوة وسعر متوسط
                            - أرخص لابتوب للألعاب
                            - هل فيه خصومات على الشاشات؟

                            نصائح للرد:
                            - كن مختصرًا وودودًا.
                            - لا تخرج عن نطاق الأجهزة الإلكترونية.
                            - اقترح منتجات بأسعار ومواصفات واضحة.

                            ---

                            You are a smart assistant inside the 'Electron' app, which is an e-commerce platform for electronic products.
                            Your task is to help users explore items, understand features, compare prices, and make smart choices.

                            Available categories:
                            - Smartphones
                            - Laptops
                            - Smart TVs
                            - Headphones
                            - Computer accessories

                            Example questions from users:
                            - Recommend a good phone with strong battery
                            - Cheapest laptop for gaming
                            - Are there any offers on screens?

                            Tips for answering:
                            - Be concise and friendly.
                            - Do not go outside electronics-related topics.
                            - Suggest 2–3 items with brief specs and price.
                            "
                        },
                        
                        // Example user query: "رشح لي لابتوب جيد"
                        new { role = "user", content = "رشح لي لابتوب جيد" },

                        // Assistant response to that query
                        new { role = "assistant", content = @"
أنصحك بلابتوب Lenovo IdeaPad 3i  
يأتي بمعالج Intel Core i5 من الجيل الحادي عشر وكارت شاشة مدمج.  
السعر حوالي 12000 جنيه مصري.

I recommend the Lenovo IdeaPad 3i  
It comes with an Intel Core i5 processor (11th gen) and integrated graphics.  
Price is around 12,000 EGP.
" },

                        // Example user query: "عايز لابتوب مناسب للألعاب"
                        new { role = "user", content = "عايز لابتوب مناسب للألعاب" },

                        // Assistant response to that query
                        new { role = "assistant", content = @"
أفضل لابتوب للألعاب هو MSI GF65 Thin  
يأتي بمعالج Intel Core i7 وكارت شاشة GTX 1660 Ti.  
السعر حوالي 22,000 جنيه مصري.

The best gaming laptop is the MSI GF65 Thin  
It comes with an Intel Core i7 processor and GTX 1660 Ti graphics card.  
Price is around 22,000 EGP.
" },

                        // Message from customer, dynamically using input
                        new { role = "user", content = request.Message }
                    },
                    temperature = 0.7,
                    max_tokens = 1000
                };

                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", _apiKey);

                var jsonPayload = JsonSerializer.Serialize(payload);
                _logger.LogDebug("Sending payload to OpenAI: {Payload}", jsonPayload);

                var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(
                    "https://api.openai.com/v1/chat/completions",
                    content);

                var responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogDebug("Received response from OpenAI: {Response}", responseContent);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("OpenAI API request failed with status {StatusCode}: {Response}",
                        response.StatusCode, responseContent);
                    throw new Exception($"OpenAI API error: {response.StatusCode} - {responseContent}");
                }

                using var jsonDoc = JsonDocument.Parse(responseContent);
                var reply = jsonDoc.RootElement
                    .GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString();

                return new ChatResponseDTO { Reply = reply };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process chat message");
                throw new Exception("Failed to process your request. Please try again later.", ex);
            }
            finally
            {
                _httpClient.DefaultRequestHeaders.Authorization = null;
            }
        }
    }
}
