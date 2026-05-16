# Lampa Dorama Menu QA - 2026-05-16

## Scope

- Added a **Дорамы** item to the Lampa side menu immediately after **Сериалы**.
- Covered both Lampac entry modes:
  - Modules/LampaWeb/plugins/lampainit-invc.js for bundled /lampainit.js initialisation.
  - SISI/plugins/sisi.js for external plugin loading through /on.js.
- Kept the item non-adult and independent from the SISI **Клубничка** visibility toggle.

## Behaviour Notes

- The button opens category_full with source tmdb.
- The URL is discover/tv?with_original_language=ko&with_genres=18&sort_by=popularity.desc.
- This avoids the CUB backend language-filter limitation: tmdb.cub.red accepts extra language parameters but still returns non-Korean results.
- A shared window.lampac_dorama_menu_ready guard prevents duplicates when both init paths are active.

## Verification

- node --check passed for:
  - SISI/plugins/sisi.js
  - Modules/LampaWeb/plugins/lampainit-invc.js
- git diff --check passed.
- Direct smoke confirmed CUB ignores the Korean language filter on cat=tv.
- Direct smoke confirmed the TMDB Discover TV Dorama query returns Korean drama rows.

## Manual Regression Checklist

1. Open Lampa with source CUB.
2. Open the left menu and confirm **Дорамы** appears directly after **Сериалы**.
3. Select **Дорамы** and confirm the screen title is **Дорамы - CUB**.
4. Confirm the visible rows are Korean TV dramas.
5. Toggle SISI **Отображать в меню** off and confirm **Дорамы** remains visible while **Клубничка** is hidden.
6. Reload Lampa and confirm only one **Дорамы** item appears.
