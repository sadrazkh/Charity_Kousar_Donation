# تصاویر پیشنهادی پروژه‌ها / Ready-made project images

هر فایل تصویری که در همین پوشه بگذارید، بدون هیچ تغییری در کد:

- در پنل مدیریت → «تصاویر پروژه‌ها» دیده می‌شود،
- و در انتخاب‌گر تصویرِ هر پروژه (دکمهٔ «انتخاب از تصاویر آماده و گالری») قابل انتخاب است.

نکته‌ها:

- فرمت‌های مجاز: `jpg` · `jpeg` · `png` · `webp` · `gif` · `svg`
- نام فایل همان چیزی است که زیر تصویر نمایش داده می‌شود؛ پس نام معنادار بگذارید
  (مثلاً `basket-family.png`، `basket-worker.png`، `basket-health.png`).
- تصویرهای مربعی (مثلاً ۱۰۲۴×۱۰۲۴) بهترین نتیجه را می‌دهند. برای اینکه لبه‌های تصویر
  روی کارت‌ها بریده نشود، در «صفحه اصلی → چیدمان کارت‌ها» گزینهٔ «نمایش کامل تصویر» را انتخاب کنید.
- این پوشه هنگام build فرانت‌اند به `wwwroot/presets` کپی می‌شود و با آدرس `/presets/<filename>` سرو می‌شود.

Any image file placed in this folder automatically appears in the admin “Project images”
screen and in the per-project image picker. The folder is copied to `wwwroot/presets`
by the frontend build and served from `/presets/<filename>`.
