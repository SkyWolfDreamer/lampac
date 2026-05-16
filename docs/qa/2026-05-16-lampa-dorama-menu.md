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
- Regression note: the source must not use `Lampa.TMDB`; the Samsung/Lampa bundle exports `Lampa.Api`, but not `Lampa.TMDB`. Dorama pages now fetch TMDB Discover directly through `Lampa.Reguest` and build the full URL with the same protocol/proxy/page contract as the native TMDB source.
- Current Dorama sections: `Сейчас смотрят`, `Новые серии`, `Онгоинги`, `Популярное`, `Последнее добавление`, `Новинки этого года`, `С высоким рейтингом`.
- Every section uses TMDB Discover TV with `with_original_language=ko`, `with_genres=18`, and `include_adult=false`, then applies its own sort/window/rating filters.
- `Сейчас смотрят` uses a current-airing window (`air_date` from the previous 14 days through the next 14 days) sorted by popularity, because TMDB Discover has no filtered `watching now` endpoint.
- `Новые серии` uses already aired episode dates only (`air_date` from the previous 14 days through today) sorted by `air_date.desc`; it must not show future episodes.
- `Онгоинги` is intentionally narrower than TMDB `Returning Series`: it requires `with_status=0|2`, `first_air_date.lte=today`, and an episode `air_date` from today through the next 21 days, so recently completed batches do not stay in ongoing forever.
- `Последнее добавление` and `Новинки этого года` both require `first_air_date.lte=today`, so future/unreleased TMDB entries do not appear as already-added releases.
- This avoids the CUB backend language-filter limitation: tmdb.cub.red accepts extra language parameters but still returns non-Korean results.
- Both init paths are idempotent: they reuse an existing **Дорамы** button, remove duplicate buttons, and keep retrying briefly so late menu/plugin rendering cannot leave **Дорамы** at the bottom of the menu.
- Both init paths now refresh an older in-memory `lampac_dorama` source if the page loaded stale code first, so `/lampainit.js` and `/sisi.js` cannot keep an old source that opens broken **Ещё** pages.
- Dorama section rows preserve their original Discover URL and page metadata for **Ещё**. Rows mark themselves `nomore` when there is no next TMDB page.
- **Ещё** intentionally follows the native **Сериалы/CUB** pattern: category rows keep a section URL, and the list loader opens that same URL with the requested `page` without falling back to page 1. This prevents a section **Ещё** action from reopening the first/full list.
- For local Docker runtime overrides, mount specific plugin files such as `plugins/override/lampainit-invc.js` and `plugins/override/sisi.js`; overriding the full `lampainit.js` bypasses Lampac's normal init wrapper and can leave stale menu code cached for up to 10 minutes.

## Verification

- node --check passed for:
  - SISI/plugins/sisi.js
  - Modules/LampaWeb/plugins/lampainit-invc.js
- git diff --check passed.
- Direct smoke confirmed CUB ignores the Korean language filter on cat=tv.
- Direct smoke confirmed the TMDB Discover TV Dorama query returns Korean drama rows.
- Direct smoke should cover at least one section URL for `air_date` and one for `first_air_date_year` after every future sort change.
- Local Docker smoke confirmed `/lampainit.js` and `/sisi.js` both serve `lampac_dorama`, `Новые серии`, and `Онгоинги` from the targeted overrides after compose recreate, and the served scripts now call `Lampa.Reguest` with no `Lampa.TMDB` dependency.
- Follow-up TV compatibility pass aligned the Dorama direct loader with the built-in TMDB URL contract: `Lampa.Utils.protocol()`, `proxy_tmdb`, a persistent `Lampa.Reguest` instance, and explicit requested page.
- Follow-up filter review smoke confirmed all seven Dorama section queries return rows; `update`, `latest`, and current-year `now` exclude future dates; `ongoing` excludes stale `Returning Series` examples without upcoming episode air dates.
- **Ещё** regression smoke should validate `Новинки этого года` page 2 and one intentionally empty next-page response: the former must return Korean drama rows, while the latter must not silently reopen page 1.

## Manual Regression Checklist

1. Open Lampa with source CUB.
2. Open the left menu and confirm **Дорамы** appears directly after **Сериалы**.
3. Select **Дорамы** and confirm it opens a sectioned screen titled **Дорамы**.
4. Confirm the screen includes `Сейчас смотрят`, `Новые серии`, `Онгоинги`, `Популярное`, `Последнее добавление`, `Новинки этого года`, and `С высоким рейтингом` when TMDB returns rows for those sections.
5. Open `Ещё` from at least `Новинки этого года` and one more Dorama section; confirm it opens Korean drama rows, not Lampa's empty state.
6. Toggle SISI **Отображать в меню** off and confirm **Дорамы** remains visible while **Клубничка** is hidden.
7. Reload Lampa and confirm only one **Дорамы** item appears.
