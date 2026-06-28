<div align="center">

# 💚 خیریه کوثر — سامانهٔ کمک‌های مردمی

### Kousar Charity — Online Donation Platform

سامانهٔ کامل جمع‌آوری کمک‌های مردمی با صفحهٔ اختصاصی برای هر پروژه، صفحه‌ساز بصری، درگاه پرداخت، و پنل مدیریت کاملاً قابل‌شخصی‌سازی.

![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet&logoColor=white)
![Vue](https://img.shields.io/badge/Vue-3-42b883?logo=vuedotjs&logoColor=white)
![Vite](https://img.shields.io/badge/Vite-6-646CFF?logo=vite&logoColor=white)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16-4169E1?logo=postgresql&logoColor=white)
![Docker](https://img.shields.io/badge/Docker-ready-2496ED?logo=docker&logoColor=white)

</div>

---

## ✨ امکانات

- **پروژه‌های کمک‌مالی** با عنوان/توضیح دو‌زبانه (فارسی/انگلیسی)، تصویر، مبلغ هدف و نوار پیشرفت.
- **صفحهٔ اختصاصی هر پروژه** با **صفحه‌سازِ بصری** (Drag & Drop): عنوان، متن، تصویر، گالری، ویدیو، نقل‌قول، کارت، آمار، دکمهٔ کمک، چند‌ستونه و…
- **بخش ویژه + شمارش معکوس** کاملاً قابل تنظیم (انتخاب واحدها، چیدمان، رنگ، متن نشان و پایان زمان).
- **نوار پیشرفت هوشمند**: کل نوار با پر شدن از یک رنگ به **سبز** تغییر می‌کند (بدون رنگین‌کمان) — رنگ‌ها و حالت قابل تنظیم.
- **چیدمان صفحهٔ اصلی قابل مرتب‌سازی**: ترتیب و نمایش بخش‌ها (متن/مجموع، پروژه‌های ویژه، لیست پروژه‌ها، مشارکت‌کنندگان) از پنل.
- **نمایش مشارکت‌کنندگان** کاملاً داینامیک: نمایش/عدم‌نمایش نام، مبلغ، تاریخ و نام پروژه + عنوان و برچسب ناشناس دلخواه.
- **لوگو و برندینگ** قابل تنظیم (آدرس لوگو، اندازه، نمایش نام و شعار).
- **آپلود تصویر** با کشیدن‌و‌رها و پیش‌نمایش.
- **اشتراک‌گذاری ساده** (واتساپ/تلگرام/کپی) با متن آمادهٔ توکار + قالب قابل ویرایش.
- **هوش مصنوعی (اختیاری، OpenRouter)**: بهبود متن، تولید متن اشتراک، و **ترجمهٔ فارسی → انگلیسی** با انتخاب مدل.
- **درگاه پرداخت زرین‌پال** + پرداخت **رمزارز** + **حالت تست** بدون درگاه واقعی.
- **تأیید پیامکی (OTP)** برای مبالغ بالا و پیامک تشکر (Kavenegar).
- **پنل مدیریت** کامل: داشبورد بلادرنگ (SignalR)، مدیریت پروژه‌ها و کمک‌ها، خروجی CSV/گزارش، و تنظیمات گسترده.
- **چندزبانه و راست‌به‌چپ**، تم تاریک/روشن، و واکنش‌گرا (موبایل‌پسند).

---

## 🧱 پشتهٔ فناوری

| لایه | فناوری |
|------|--------|
| بک‌اند | ASP.NET Core **10**، EF Core 10، SignalR، JWT |
| پایگاه‌داده | **PostgreSQL** (Npgsql) |
| فرانت‌اند | **Vue 3** + Vite 6 + Vue Router + Vue I18n |
| احراز هویت | JWT (BCrypt برای رمزها) |
| استقرار | Docker (مناسب CapRover) |

---

## 📁 ساختار پروژه

```
Charity_Kousar_Donation/                 # ریشهٔ مخزن (شامل .slnx و Dockerfile)
└── Charity_Kousar_Donation/             # پروژهٔ ASP.NET Core
    ├── Controllers/Api/                  # کنترلرهای REST (Campaigns, Donations, Settings, Ai, Upload, ...)
    ├── Services/                         # منطق دامنه (Campaign, Donation, ZarinPal, OpenRouter, Upload, ...)
    ├── Models/ , DTOs/ , Data/           # موجودیت‌ها، DTOها، DbContext + Migrations + DbSeeder
    ├── Hubs/                             # DonationHub (اعلان بلادرنگ)
    ├── wwwroot/                          # خروجی بیلد فرانت‌اند + پوشهٔ uploads
    └── ClientApp/                        # اپ Vue 3
        └── src/ (views, components, composables, utils, i18n)
```

---

## ✅ پیش‌نیازها

- [.NET SDK 10](https://dotnet.microsoft.com/download)
- [Node.js 18+](https://nodejs.org) و npm
- [PostgreSQL 14+](https://www.postgresql.org/) (محلی یا داکر)

> هنگام نخستین اجرا، برنامه به‌صورت خودکار دیتابیس را می‌سازد، مهاجرت‌ها را اعمال می‌کند و دادهٔ اولیه (تنظیمات، کاربر ادمین و یک پروژهٔ نمونه) را seed می‌کند.

---

## 🚀 اجرا در محیط توسعه

### ۱) تنظیم اتصال پایگاه‌داده

رشتهٔ اتصال توسعه در `Charity_Kousar_Donation/appsettings.Development.json` است:

```json
"DefaultConnection": "Host=localhost;Port=5432;Database=kousar_charity;Username=postgres;Password=7082"
```

پسورد/پورت خود را همان‌جا اصلاح کنید، یا بهتر است با **User Secrets** بدون دستکاری فایل تنظیم کنید:

```bash
cd Charity_Kousar_Donation
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=kousar_charity;Username=postgres;Password=YOUR_PASSWORD"
```

> راه‌اندازی سریع Postgres با داکر:
> ```bash
> docker run --name kousar-db -e POSTGRES_PASSWORD=7082 -p 5432:5432 -d postgres:16
> ```

### ۲) اجرای هم‌زمان بک‌اند و فرانت‌اند (پیشنهادی)

دو ترمینال باز کنید:

```bash
# ترمینال ۱ — بک‌اند (API روی https://localhost:7208)
cd Charity_Kousar_Donation
dotnet run --launch-profile https
```

```bash
# ترمینال ۲ — فرانت‌اند (Vite روی http://localhost:5173)
cd Charity_Kousar_Donation/ClientApp
npm install      # فقط بار اول
npm run dev
```

سپس مرورگر را روی **http://localhost:5173** باز کنید. سرور توسعهٔ Vite مسیرهای `/api`، `/d` و `/uploads` را به بک‌اند پراکسی می‌کند.

### ۳) اجرای فقط بک‌اند (سرو کردن نسخهٔ بیلدشدهٔ فرانت)

```bash
cd Charity_Kousar_Donation/ClientApp && npm install && npm run build   # خروجی در wwwroot
cd ..  && dotnet run --launch-profile https
```

حالا کل برنامه روی **https://localhost:7208** در دسترس است.

---

## 🔑 ورود پیش‌فرض ادمین

| مورد | مقدار |
|------|-------|
| آدرس پنل | `/admin` |
| نام کاربری | **`admin`** |
| رمز عبور | **`Admin@12345`** |

> این کاربر فقط در صورتی ساخته می‌شود که هیچ ادمینی وجود نداشته باشد. برای تغییر مقادیر پیش‌فرض، **قبل از نخستین اجرا** آن‌ها را در `appsettings.json` بخش `Seed` یا با متغیر محیطی تنظیم کنید:
>
> ```json
> "Seed": { "AdminUsername": "admin", "AdminPassword": "یک‌رمز‌قوی" }
> ```
>
> پس از ورود نیز می‌توانید از داخل پنل رمز را تغییر دهید.

---

## ⚙️ پیکربندی

### فایل `appsettings.json`

| کلید | توضیح |
|------|--------|
| `ConnectionStrings:DefaultConnection` | رشتهٔ اتصال PostgreSQL |
| `Jwt:Key` | کلید امضای توکن — **حتماً در تولید تغییر دهید** (حداقل ۳۲ کاراکتر) |
| `Jwt:Issuer` / `Jwt:Audience` | صادرکننده/مخاطب توکن |
| `Seed:AdminUsername` / `Seed:AdminPassword` | کاربر ادمین اولیه |

### تنظیمات از داخل پنل (بدون نیاز به کدنویسی)

پنل مدیریت → **تنظیمات** شامل این گروه‌هاست:

- 🎨 **ظاهر، لوگو و رنگ‌ها** — نام/شعار سایت، لوگو و اندازه، رنگ‌های اصلی/تأکید/پس‌زمینه، فوتر.
- 🏠 **چیدمان صفحهٔ اصلی** — ترتیب کادرها + حالت و رنگ نوار پیشرفت.
- ⭐ **بخش ویژه و تایمر** — واحدها، چیدمان، رنگ، متن نشان و پایان زمان.
- 👥 **نمایش مشارکت‌کنندگان** — انتخاب پارامترهای نمایشی و عنوان بخش.
- 💰 **کمک مالی و مبالغ** — حداقل مبلغ، مبالغ پیشنهادی، OTP.
- 📤 **اشتراک‌گذاری** — قالب آماده + فعال/غیرفعال‌سازی AI.
- 💳 **زرین‌پال** / ₿ **رمزارز** / 📱 **پیامک**.
- 🤖 **هوش مصنوعی و ترجمه** — کلید و مدل **OpenRouter** و مدل ترجمه.

> برای فعال‌سازی بهبود متن، تولید متن اشتراک و ترجمهٔ خودکار، کلید [OpenRouter](https://openrouter.ai/keys) را در گروه «هوش مصنوعی» وارد کنید.

---

## 🌐 پورت‌ها

| سرویس | آدرس |
|-------|------|
| فرانت‌اند (Vite dev) | `http://localhost:5173` |
| بک‌اند HTTPS | `https://localhost:7208` |
| بک‌اند HTTP | `http://localhost:5276` |
| داکر (داخل کانتینر) | `8080` (HTTP) / `8081` (HTTPS) |

---

## 🐳 استقرار با Docker

از ریشهٔ مخزن:

```bash
docker build -t kousar-charity .
docker run -d -p 8080:8080 --name kousar \
  -e "ConnectionStrings__DefaultConnection=Host=YOUR_DB_HOST;Port=5432;Database=kousar_charity;Username=postgres;Password=YOUR_PASSWORD" \
  -e "Jwt__Key=YOUR_LONG_RANDOM_SECRET_KEY_AT_LEAST_32_CHARS" \
  kousar-charity
```

- فرانت‌اند (Vue) هنگام `dotnet publish` به‌صورت خودکار با npm بیلد می‌شود (تعریف‌شده در `.csproj`).
- مناسب **CapRover**: کافی است مخزن را متصل کنید؛ Dockerfile بقیه را انجام می‌دهد. متغیرهای محیطی بالا را در تنظیمات اپ قرار دهید.
- تصاویر آپلودشده در `wwwroot/uploads` ذخیره می‌شوند؛ برای ماندگاری، این مسیر را روی یک Volume پایدار mount کنید.

---

## 📡 نگاه کلی به API

| روش | مسیر | توضیح |
|-----|------|--------|
| GET | `/api/campaigns` | فهرست پروژه‌های فعال |
| GET | `/api/campaigns/{slug}` | جزئیات پروژه |
| GET | `/api/settings/public` | تنظیمات عمومی سایت |
| POST | `/api/donations/start` | شروع فرایند کمک |
| POST | `/api/auth/login` | ورود ادمین (JWT) |
| POST | `/api/upload` | آپلود تصویر (ادمین) |
| POST | `/api/ai/translate` | ترجمهٔ متن (ادمین) |
| GET | `/d/{code}` | لینک کوتاه → ریدایرکت به پروژه |
| WS | `/hubs/donations` | اعلان بلادرنگ کمک‌ها (SignalR) |

---

## 🔒 نکات امنیتی برای تولید

- [ ] `Jwt:Key` را به یک مقدار تصادفی و طولانی تغییر دهید.
- [ ] رمز پیش‌فرض ادمین (`Admin@12345`) را عوض کنید.
- [ ] رشتهٔ اتصال و کلیدها را در فایل قرار ندهید؛ از متغیر محیطی/Secret استفاده کنید.
- [ ] حالت تست پرداخت (`payment.bypass.enabled`) را در تولید خاموش کنید.
- [ ] سرویس را پشت HTTPS اجرا کنید.

---

<div align="center">

ساخته‌شده با ❤️ برای کارهای خیر — **خیریه کوثر**

</div>
