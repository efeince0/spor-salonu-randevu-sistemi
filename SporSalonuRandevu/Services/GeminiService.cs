using System.Text;
using System.Text.Json; // System.Text.Json kullandığınız için buna sadık kaldım

namespace SporSalonuRandevu.Services
{
    public class GeminiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public GeminiService(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _apiKey = config["Gemini:ApiKey"];
        }

        public async Task<string> DiyetVeEgzersizOlustur(
     int yas, int boy, int kilo, double hedefKilo, double bmi,
     double idealMin, double idealMax, string ekNotlar) // <--- 1. BURAYI EKLEDİK
        {
            // 2. Prompt'u hem tablo isteyecek hem de notları dikkate alacak şekilde güncelliyoruz:
            var prompt = $@"
    Sen uzman bir spor koçu ve diyetisyensin.
    
    Kullanıcı Bilgileri:
    - Yaş: {yas}, Boy: {boy} cm, Kilo: {kilo} kg
    - Hedef: {hedefKilo} kg, BMI: {bmi:F1}
    
    KULLANICININ ÖZEL NOTLARI:
    '{ekNotlar}'
    (Eğer yukarıda bir alerji, sakatlık veya tercih belirtildiyse programı KESİNLİKLE buna göre oluştur.)

    GÖREV:
    Bu kişi için 1 haftalık diyet ve egzersiz programı hazırla.

    ÇIKTI FORMATI (ÇOK ÖNEMLİ):
    - Cevabı sadece **HTML** formatında ver.
    - Markdown (```html ... ```) bloklarını KULLANMA. Sadece saf HTML döndür.
    - Tablolar için Bootstrap class'ı kullan: <table class='table table-bordered table-dark table-striped'>
    - Günleri satır satır tabloya dök.
    - Diyet tablosu ayrı, Egzersiz tablosu ayrı olsun.
    - En sona kısa bir motivasyon notu ekle.
    ";

            var requestBody = new
            {
                contents = new[]
                {
            new { parts = new[] { new { text = prompt } } }
        }
            };

            var jsonContent = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            // Modelin (1.5 Flash veya Pro) URL'i buraya gelecek
            var response = await _httpClient.PostAsync(
      $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-pro:generateContent?key={_apiKey}",
      content
  );

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                return $"API Hatası: {response.StatusCode}. Detay: {errorContent}";
            }

            var jsonResponse = await response.Content.ReadAsStringAsync();

            try
            {
                using var doc = JsonDocument.Parse(jsonResponse);
                var root = doc.RootElement;

                if (root.TryGetProperty("candidates", out var candidates) && candidates.GetArrayLength() > 0)
                {
                    var text = candidates[0]
                        .GetProperty("content")
                        .GetProperty("parts")[0]
                        .GetProperty("text")
                        .GetString();

                    // Gemini bazen inatla ```html ekler, onları temizleyelim ki sayfa bozulmasın
                    return text.Replace("```html", "").Replace("```", "");
                }
            }
            catch
            {
                return "Yanıt işlenirken hata oluştu.";
            }

            return "Yanıt alınamadı.";
        }
    }
}