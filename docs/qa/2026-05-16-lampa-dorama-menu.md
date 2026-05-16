# Lampa Dorama Menu QA - 2026-05-16

## Scope

- Added a **Дорамы** item to the Lampa side menu immediately after **Сериалы**.
- Covered both Lampac entry modes:
  - Modules/LampaWeb/plugins/lampainit-invc.js for bundled /lampainit.js initialisation.
  - SISI/plugins/sisi.js for external plugin loading through /on.js.
- Kept the item non-adult and independent from the SISI **Клубничка** visibility toggle.

## Behaviour Notes

- The button now opens a normal category screen, not a single category_full list.
- A client source named `lampac_dorama` provides the same style of sectioned home screen as `Сериалы`.
- Regression note: the source must use the exported `Lampa.Api.sources.tmdb.list` API. The Samsung/Lampa bundle exports `Lampa.Api`, but not `Lampa.TMDB`; relying on `Lampa.TMDB` makes real clients fall back to the old flat `category_full` screen where no inner sections are visible.
- Current Dorama sections: `Сейчас смотрят`, `Новые серии`, `Онгоинги`, `Популярное`, `Последнее добавление`, `Новинки этого года`, `С высоким рейтингом`.
- Every section uses TMDB Discover TV with `with_original_language=ko`, `with_genres=18`, and `include_adult=false`, then applies its own sort/window/rating filters.
- This avoids the CUB backend language-filter limitation: tmdb.cub.red accepts extra language parameters but still returns non-Korean results.
- Both init paths are idempotent: they reuse an existing **Дорамы** button, remove duplicate buttons, and keep retrying briefly so late menu/plugin rendering cannot leave **Дорамы** at the bottom of the menu.
- For local Docker runtime overrides, mount specific plugin files such as `plugins/override/lampainit-invc.js` and `plugins/override/sisi.js`; overriding the full `lampainit.js` bypasses Lampac's normal init wrapper and can leave stale menu code cached for up to 10 minutes.

## Verification

- node --check passed for:
  - SISI/plugins/sisi.js
  - Modules/LampaWeb/plugins/lampainit-invc.js
- git diff --check passed.
- Direct smoke confirmed CUB ignores the Korean language filter on cat=tv.
- Direct smoke confirmed the TMDB Discover TV Dorama query returns Korean drama rows.
- Direct smoke should cover at least one section URL for `air_date` and one for `first_air_date_year` after every future sort change.
- Local Docker smoke confirmed `/lampainit.js` and `/sisi.js` both serve `lampac_dorama`, `Новые серии`, and `Онгоинги` from the targeted overrides after compose recreate, and the served scripts now call `tmdb.list` with no `Lampa.TMDB` dependency.

## Manual Regression Checklist

1. Open Lampa with source CUB.
2. Open the left menu and confirm **Дорамы** appears directly after **Сериалы**.
3. Select **Дорамы** and confirm it opens a sectioned screen titled **Дорамы**.
4. Confirm the screen includes `Сейчас смотрят`, `Новые серии`, `Онгоинги`, `Популярное`, `Последнее добавление`, `Новинки этого года`, and `С высоким рейтингом` when TMDB returns rows for those sections.
5. Open `Ещё` from at least one Dorama section and confirm pagination stays inside Korean dramas.
6. Toggle SISI **Отображать в меню** off and confirm **Дорамы** remains visible while **Клубничка** is hidden.
7. Reload Lampa and confirm only one **Дорамы** item appears.
