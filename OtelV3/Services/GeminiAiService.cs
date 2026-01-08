using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using OtelV3.Models;

namespace OtelV3.Services;

/// <summary>
/// Gemini AI API Servisi - Finansal analiz için
/// </summary>
public class GeminiAiService
{
    private static readonly HttpClient _httpClient = new();
    private const string API_KEY = "AIzaSyDy6mNIdfdYwxMBxa28rhAmsI8gdInrZkE";
    private const string API_URL = "https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent";
    
    public static GeminiAiService Instance { get; } = new();
    
    /// <summary>
    /// Gemini API ile KAPSAMLI finansal analiz yap - TÜM VERİTABANI VERİLERİ
    /// </summary>
    public async Task<GeminiAnalysisResult> AnalyzeComprehensiveHotelDataAsync(ComprehensiveHotelData data)
    {
        try
        {
            var prompt = BuildComprehensiveTurkishPrompt(data);
            System.Diagnostics.Debug.WriteLine($"Gemini Prompt Length: {prompt.Length}");
            var response = await CallGeminiApiAsync(prompt);
            return ParseResponse(response, data.Financial);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Gemini API Error: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Stack: {ex.StackTrace}");
            // Fallback: Basit analiz döndür
            return CreateFallbackResult(data.Financial);
        }
    }
    
    /// <summary>
    /// Eski metod - geriye uyumluluk için
    /// </summary>
    public async Task<GeminiAnalysisResult> AnalyzeFinancialDataAsync(FinancialDataDto data)
    {
        var comprehensive = new ComprehensiveHotelData { Financial = data };
        return await AnalyzeComprehensiveHotelDataAsync(comprehensive);
    }
    
    private string BuildComprehensiveTurkishPrompt(ComprehensiveHotelData data)
    {
        var prompt = new StringBuilder();
        
        prompt.AppendLine("Sen bir otel yönetim danışmanısın. Aşağıdaki KAPSAMLI OTEL VERİLERİNİ analiz et ve stratejik öneriler sun.");
        prompt.AppendLine();
        
        // 1. FİNANSAL VERİLER
        prompt.AppendLine("=== 1. FİNANSAL DURUM ===");
        prompt.AppendLine($"Toplam Gelir: {data.Financial.TotalRevenue:N0}₺");
        prompt.AppendLine($"Toplam Gider: {data.Financial.TotalExpense:N0}₺");
        prompt.AppendLine($"Net Kar: {data.Financial.NetProfit:N0}₺");
        prompt.AppendLine($"Kar Marjı: %{data.Financial.ProfitMargin:F1}");
        prompt.AppendLine($"Gelir Değişimi: %{data.Financial.RevenueChangePercent:F1}");
        prompt.AppendLine($"Gider Değişimi: %{data.Financial.ExpenseChangePercent:F1}");
        
        if (data.Financial.ExpenseBreakdown != null && data.Financial.ExpenseBreakdown.Count > 0)
        {
            prompt.AppendLine("\nKategori Bazlı Giderler:");
            foreach (var kvp in data.Financial.ExpenseBreakdown)
            {
                var percentage = data.Financial.TotalExpense > 0 
                    ? (kvp.Value / data.Financial.TotalExpense * 100) 
                    : 0;
                prompt.AppendLine($"  • {kvp.Key}: {kvp.Value:N0}₺ (%{percentage:F1})");
            }
        }
        prompt.AppendLine();
        
        // 2. ODA VE DOLULUK VERİLERİ
        prompt.AppendLine("=== 2. ODA YÖNETİMİ ===");
        prompt.AppendLine($"Genel Doluluk Oranı: %{data.Financial.OccupancyRate:F0}");
        prompt.AppendLine($"Toplam Oda: {data.TotalRooms}");
        prompt.AppendLine($"Dolu Odalar: {data.OccupiedRooms}");
        prompt.AppendLine($"Müsait Odalar: {data.AvailableRooms}");
        prompt.AppendLine($"Temizlik Bekleyen: {data.CleaningRooms}");
        prompt.AppendLine($"Bakımda: {data.MaintenanceRooms}");
        prompt.AppendLine();
        
        // 3. REZERVASYON ANALİZİ
        prompt.AppendLine("=== 3. REZERVASYON ANALİTİĞİ ===");
        prompt.AppendLine($"Aktif Rezervasyon: {data.ActiveReservations}");
        prompt.AppendLine($"Bekleyen Rezervasyon: {data.PendingReservations}");
        prompt.AppendLine($"Tamamlanan Rezervasyon: {data.CompletedReservations}");
        prompt.AppendLine($"İptal Edilen: {data.CancelledReservations}");
        
        if (data.CancelledReservations > 0 && data.ActiveReservations > 0)
        {
            var cancelRate = (double)data.CancelledReservations / (data.ActiveReservations + data.CancelledReservations) * 100;
            prompt.AppendLine($"İptal Oranı: %{cancelRate:F1}");
        }
        prompt.AppendLine();
        
        // 4. MÜŞTERİ VERİLERİ
        prompt.AppendLine("=== 4. MÜŞTERİ TABANı ===");
        prompt.AppendLine($"Toplam Müşteri: {data.TotalCustomers}");
        prompt.AppendLine($"Aktif Müşteri (Konaklamada): {data.ActiveCustomers}");
        if (data.TotalCustomers > 0 && data.ActiveReservations > 0)
        {
            var repeatRate = data.ActiveReservations > data.TotalCustomers ? 
                ((double)(data.ActiveReservations - data.TotalCustomers) / data.ActiveReservations * 100) : 0;
            prompt.AppendLine($"Tahmini Tekrar Müşteri Oranı: %{repeatRate:F1}");
        }
        prompt.AppendLine();
        
        // 5. PERSONEL VERİLERİ
        prompt.AppendLine("=== 5. PERSONEL ===");
        prompt.AppendLine($"Toplam Personel: {data.TotalPersonnel}");
        prompt.AppendLine($"Departman Dağılımı: {data.PersonnelByDepartment}");
        prompt.AppendLine();
        
        // GÖREV
        prompt.AppendLine("=== GÖREV ===");
        prompt.AppendLine("Yukarıdaki TÜM verileri göz önünde bulundurarak:");
        prompt.AppendLine("1. Otel performans skoru ver (0-100)");
        prompt.AppendLine("2. Kısa durum değerlendirmesi yap");
        prompt.AppendLine("3. 5 stratejik öneri sun (finansal, operasyonel, pazarlama)");
        prompt.AppendLine("4. Önümüzdeki 7 GÜN için GÜNLÜK gelir ve doluluk tahmini yap (haftalık)");
        prompt.AppendLine("5. Bütçe optimizasyonu önerileri ver");
        prompt.AppendLine("6. Gelir dağılımını kategorilere göre yüzde olarak ver (Oda, Yiyecek-İçecek, Ekstra Hizmetler, Diğer)");
        prompt.AppendLine();
        prompt.AppendLine("YANITINI SADECE JSON FORMATINDA VER:");
        prompt.AppendLine(@"{
  ""score"": 75,
  ""forecast"": ""Durum değerlendirmesi"",
  ""recommendations"": [
    ""Öneri 1"",
    ""Öneri 2"",
    ""Öneri 3"",
    ""Öneri 4"",
    ""Öneri 5""
  ],
  ""weeklyForecast"": [
    { ""day"": ""Pazartesi"", ""revenue"": 15000, ""occupancy"": 75 },
    { ""day"": ""Salı"", ""revenue"": 16000, ""occupancy"": 78 },
    { ""day"": ""Çarşamba"", ""revenue"": 14500, ""occupancy"": 72 },
    { ""day"": ""Perşembe"", ""revenue"": 17000, ""occupancy"": 80 },
    { ""day"": ""Cuma"", ""revenue"": 20000, ""occupancy"": 88 },
    { ""day"": ""Cumartesi"", ""revenue"": 22000, ""occupancy"": 92 },
    { ""day"": ""Pazar"", ""revenue"": 18000, ""occupancy"": 82 }
  ],
  ""revenueDistribution"": [
    { ""category"": ""Oda Geliri"", ""percentage"": 65 },
    { ""category"": ""Yiyecek-İçecek"", ""percentage"": 20 },
    { ""category"": ""Ekstra Hizmetler"", ""percentage"": 10 },
    { ""category"": ""Diğer"", ""percentage"": 5 }
  ],
  ""budgetInsights"": [
    { ""category"": ""Enerji"", ""current"": 25000, ""suggested"": 20000, ""savingTip"": ""Öneri"" },
    { ""category"": ""Personel"", ""current"": 40000, ""suggested"": 38000, ""savingTip"": ""Öneri"" }
  ]
}");
        
        return prompt.ToString();
    }
    
    private async Task<string> CallGeminiApiAsync(string prompt)
    {
        var requestBody = new
        {
            contents = new[]
            {
                new
                {
                    parts = new[]
                    {
                        new { text = prompt }
                    }
                }
            },
            generationConfig = new
            {
                temperature = 0.4,
                topP = 0.95,
                maxOutputTokens = 2048
            }
        };
        
        var json = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        var url = $"{API_URL}?key={API_KEY}";
        var response = await _httpClient.PostAsync(url, content);
        
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new Exception($"Gemini API Error: {response.StatusCode} - {error}");
        }
        
        return await response.Content.ReadAsStringAsync();
    }
    
    private GeminiAnalysisResult ParseResponse(string apiResponse, FinancialDataDto originalData)
    {
        try
        {
            // Gemini response'dan text kısmını çıkar
            using var doc = JsonDocument.Parse(apiResponse);
            var textContent = doc.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString();
            
            if (string.IsNullOrEmpty(textContent))
                return CreateFallbackResult(originalData);
            
            // JSON içeriğini temizle (markdown code block varsa kaldır)
            textContent = textContent.Trim();
            if (textContent.StartsWith("```json"))
                textContent = textContent.Substring(7);
            if (textContent.StartsWith("```"))
                textContent = textContent.Substring(3);
            if (textContent.EndsWith("```"))
                textContent = textContent.Substring(0, textContent.Length - 3);
            textContent = textContent.Trim();
            
            // Parse et
            var result = JsonSerializer.Deserialize<GeminiAnalysisResult>(textContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            
            return result ?? CreateFallbackResult(originalData);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Parse Error: {ex.Message}");
            return CreateFallbackResult(originalData);
        }
    }
    
    private GeminiAnalysisResult CreateFallbackResult(FinancialDataDto data)
    {
        // API çalışmazsa basit hesaplama yap
        var score = data.ProfitMargin > 30 ? 85 : (data.ProfitMargin > 10 ? 65 : 40);
        var baseRevenue = data.TotalRevenue > 0 ? data.TotalRevenue : 100000m;
        
        return new GeminiAnalysisResult
        {
            Score = (int)score,
            Forecast = data.NetProfit > 0 
                ? "Finansal durum olumlu görünüyor. Mevcut performansı koruyun."
                : "Dikkat! Giderler geliri aşıyor. Acil maliyet kontrolü gerekli.",
            Recommendations = new List<string>
            {
                "📊 Mevcut performansı izlemeye devam edin",
                "💡 Enerji tasarrufu önlemlerini değerlendirin",
                "📈 Pazarlama kampanyaları ile doluluk artırın",
                "👥 Personel verimliliğini optimize edin",
                "🏨 Müşteri deneyimini iyileştirin"
            },
            MonthlyForecast = new List<MonthlyForecastItem>
            {
                new() { Month = "Ocak", Revenue = (double)baseRevenue * 1.0, Occupancy = data.OccupancyRate },
                new() { Month = "Şubat", Revenue = (double)baseRevenue * 1.05, Occupancy = data.OccupancyRate + 3 },
                new() { Month = "Mart", Revenue = (double)baseRevenue * 1.08, Occupancy = data.OccupancyRate + 5 }
            },
            BudgetInsights = new List<BudgetInsightItem>
            {
                new() { Category = "Enerji", Current = (double)data.TotalExpense * 0.2, Suggested = (double)data.TotalExpense * 0.15, SavingTip = "LED aydınlatma ve akıllı termostat" },
                new() { Category = "Personel", Current = (double)data.TotalExpense * 0.35, Suggested = (double)data.TotalExpense * 0.32, SavingTip = "Vardiya optimizasyonu" }
            }
        };
    }
}

#region Response Models

public class GeminiAnalysisResult
{
    [JsonPropertyName("score")]
    public int Score { get; set; }
    
    [JsonPropertyName("forecast")]
    public string Forecast { get; set; } = string.Empty;
    
    [JsonPropertyName("recommendations")]
    public List<string> Recommendations { get; set; } = new();
    
    [JsonPropertyName("monthlyForecast")]
    public List<MonthlyForecastItem> MonthlyForecast { get; set; } = new();
    
    [JsonPropertyName("weeklyForecast")]
    public List<WeeklyForecastItem> WeeklyForecast { get; set; } = new();
    
    [JsonPropertyName("revenueDistribution")]
    public List<RevenueDistributionItem> RevenueDistribution { get; set; } = new();
    
    [JsonPropertyName("budgetInsights")]
    public List<BudgetInsightItem> BudgetInsights { get; set; } = new();
}

public class WeeklyForecastItem
{
    [JsonPropertyName("day")]
    public string Day { get; set; } = string.Empty;
    
    [JsonPropertyName("revenue")]
    public double Revenue { get; set; }
    
    [JsonPropertyName("occupancy")]
    public double Occupancy { get; set; }
}

public class RevenueDistributionItem
{
    [JsonPropertyName("category")]
    public string Category { get; set; } = string.Empty;
    
    [JsonPropertyName("percentage")]
    public double Percentage { get; set; }
}

public class MonthlyForecastItem
{
    [JsonPropertyName("month")]
    public string Month { get; set; } = string.Empty;
    
    [JsonPropertyName("revenue")]
    public double Revenue { get; set; }
    
    [JsonPropertyName("occupancy")]
    public double Occupancy { get; set; }
}

public class BudgetInsightItem
{
    [JsonPropertyName("category")]
    public string Category { get; set; } = string.Empty;
    
    [JsonPropertyName("current")]
    public double Current { get; set; }
    
    [JsonPropertyName("suggested")]
    public double Suggested { get; set; }
    
    [JsonPropertyName("savingTip")]
    public string SavingTip { get; set; } = string.Empty;
    
    public double SavingAmount => Current - Suggested;
    public double SavingPercent => Current > 0 ? (SavingAmount / Current) * 100 : 0;
}

/// <summary>
/// Tüm otel verilerini içeren kapsamlı veri modeli
/// </summary>
public class ComprehensiveHotelData
{
    public FinancialDataDto Financial { get; set; } = new();
    
    // ODA VERİLERİ
    public int TotalRooms { get; set; }
    public int AvailableRooms { get; set; }
    public int OccupiedRooms { get; set; }
    public int CleaningRooms { get; set; }
    public int MaintenanceRooms { get; set; }
    
    // REZERVASYON VERİLERİ
    public int ActiveReservations { get; set; }
    public int PendingReservations { get; set; }
    public int CompletedReservations { get; set; }
    public int CancelledReservations { get; set; }
    
    // MÜŞTERİ VERİLERİ
    public int TotalCustomers { get; set; }
    public int ActiveCustomers { get; set; }
    
    // PERSONEL VERİLERİ
    public int TotalPersonnel { get; set; }
    public string PersonnelByDepartment { get; set; } = string.Empty;
}

#endregion
