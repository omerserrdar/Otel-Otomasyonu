# 🏨 OtelPaneli - Yeni Nesil Otel Yönetim ve Otomasyon Sistemi

![License](https://img.shields.io/badge/license-MIT-blue.svg)
![Platform](https://img.shields.io/badge/platform-Windows-lightgrey.svg)
![Language](https://img.shields.io/badge/language-C%23%20%7C%20.NET-purple.svg)
![Database](https://img.shields.io/badge/database-SQL%20Server-red.svg)
![AI](https://img.shields.io/badge/AI-Gemini%20Integrated-orange.svg)

**OtelPaneli**, modern otelcilik ihtiyaçları için geliştirilmiş; yapay zeka destekli, ölçeklenebilir ve kullanıcı dostu bir masaüstü otomasyon yazılımıdır. Klasik rezervasyon yönetiminin ötesine geçerek, işletmelere finansal öngörüler sunan bir **Karar Destek Sistemi (DSS)** niteliği taşır.

---

## 🌟 Öne Çıkan Özellikler

* **🎨 Modern UI/UX:** Kart tabanlı (Card-Based) tasarım, Gece/Gündüz modu ve göz yorgunluğunu azaltan renk paleti.
* **🤖 Yapay Zeka (AI) Entegrasyonu:** Geçmiş verileri analiz ederek gelecek haftanın ciro tahminini yapar ve stratejik öneriler sunar.
* **🛏️ Gelişmiş Oda Yönetimi:** Sürükle-bırak mantığına yakın, görsel oda durum takibi (Kirli, Dolu, Müsait, Bakımda).
* **👥 CRM & Misafir İlişkileri:** VIP ve Kara Liste etiketleme sistemi, detaylı misafir konaklama geçmişi.
* **📊 Finansal Analiz:** Anlık gelir-gider takibi, dinamik grafikler ve raporlama.
* **🔒 Güvenli Mimari:** Rol bazlı yetkilendirme (RBAC) ve şifreli veri saklama.

---

## 🏗️ Teknik Mimari

Proje, endüstriyel standartlara uygun **Katmanlı Mimari (N-Tier Architecture)** prensipleri ile geliştirilmiştir.

* **Backend:** C# (.NET Framework / Core)
* **Database:** MS SQL Server (Relational Design)
* **ORM:** Entity Framework / ADO.NET (Repository Pattern)
* **Design Pattern:** Singleton, Factory ve Repository tasarım desenleri.
* **AI Service:** RESTful API üzerinden LLM (Large Language Model) haberleşmesi.
* **UI Libraries:** Modern WinForms / Custom Controls.

---

## 🤖 Yapay Zeka Nasıl Çalışıyor?

Bu proje, sadece veri saklamaz; veriyi **yorumlar**.
1.  **Veri Madenciliği:** Sistem, `Repository` katmanından geçmiş konaklama ve harcama verilerini çeker.
2.  **Prompt Mühendisliği:** Ham veri, yapay zekanın anlayacağı özel prompt formatlarına dönüştürülür.
3.  **API Çağrısı:** Veriler güvenli bir şekilde AI servisine iletilir.
4.  **Karar Destek:** Gelen analiz sonuçları (JSON), yönetici panelinde grafik ve öneri kartlarına dönüştürülür.

---

## 🚀 Kurulum

1.  Bu repoyu klonlayın:
    ```bash
    git clone [https://github.com/kullaniciadi/OtelPaneli.git](https://github.com/kullaniciadi/OtelPaneli.git)
    ```
2.  `Database` klasöründeki `script.sql` dosyasını SQL Server'da çalıştırarak veritabanını oluşturun.
3.  `app.config` (veya `appsettings.json`) dosyasındaki **Connection String** bilgisini kendi sunucunuza göre düzenleyin.
4.  Projeyi Visual Studio ile açın ve **Build** edin.

---

## 👨‍💻 Geliştirici

**Ömer Serdar KAYABAŞI** - *Yazılım Mühendisliği Öğrencisi*

Projeyi beğendiyseniz sağ üst köşeden ⭐️ vermeyi unutmayın!
