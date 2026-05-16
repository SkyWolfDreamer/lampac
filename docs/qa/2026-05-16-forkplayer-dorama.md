# ForkPlayer Dorama QA - 2026-05-16

## Scope

- Reviewed the existing `feat(ForkPlayer): Добавлен раздел Дорамы...` change.
- Kept the ForkPlayer root menu focused on non-adult sections by removing the `Клубничка 18+` menu item from `/fxml`.
- Kept the ForkPlayer XML `Дорамы` item directly after `Сериалы`, matching the Lampa menu order.
- Changed the ForkPlayer `Дорамы` item from one flat list into a nested menu with `Сейчас смотрят`, `Новые серии`, `Онгоинги`, `Популярное`, `Последнее добавление`, `Новинки этого года`, and `С высоким рейтингом`.
- Moved direct TMDB Discover handling into `/fxml/tmdb?cat=dorama` so `CubController` stays focused on CUB-backed lists.
- Hardened Dorama pagination so direct TMDB Discover works with TMDB's 20-item pages instead of CUB's 60-item pages.
- Preserved extra CUB query filters such as `genre` and `without_genres` through cache keys, sorting links, and next-page links.

## Behaviour Notes

- Normal CUB categories still call `http://tmdb.cub.red/` with forwarded extra query params.
- `/fxml/tmdb?cat=dorama` calls TMDB Discover TV directly with `with_original_language=ko`, `with_genres=18`, `include_adult=false`, and the configured `CoreInit.conf.cub.api_key` fallback.
- Dorama sort values now map to direct TMDB filters: `now_playing` -> current-airing popularity window, `update` -> already aired recent episode dates, `ongoing` -> returning/in-production titles with episodes airing from today through the next 21 days, `top` -> popularity, `latest` -> first-air-date descending up to today, `now` -> current-year first-air-date descending up to today, `rated` -> high-rating filter.
- `next_page_url` now uses `search=...` consistently and keeps additional filters instead of switching to the unsupported `query=...` parameter.

## Verification

- `git diff --check` passed.
- Host WSL does not have `dotnet` installed.
- Dockerized module build passed:
  `docker run --rm -v "$PWD":/src -w /src mcr.microsoft.com/dotnet/sdk:10.0 dotnet build Modules/ForkPlayerXML/ForkXML.csproj`.
  The only warning was the existing `Shared/PlaywrightCore/Chromium.cs` obsolete `Devtools` warning.
- Direct upstream smoke checks passed for:
  - TMDB Discover TV Dorama query with `with_original_language=ko` and `with_genres=18`.
  - CUB movie query with forwarded `without_genres=16`.
- Local Docker runtime smoke after compose recreate confirmed:
  - `/fxml` exposes `Дорамы` as a submenu with all 7 sections.
  - `/fxml/tmdb?cat=dorama&sort=rated` returns 20 rows, `Сортировка: с высоким рейтингом`, and a non-empty `next_page_url`.
- Follow-up filter review smoke confirmed `/fxml/tmdb?cat=dorama&sort=now_playing|update|ongoing|top|latest|now|rated` returns rows locally; `ongoing` excludes `Слабый герой` and `Охотничьи псы`.

## Manual Regression Checklist

1. Open `/fxml` in ForkPlayer and confirm `Дорамы` is visible directly after `Сериалы`.
2. Open `Дорамы` and confirm it shows nested sections: `Сейчас смотрят`, `Новые серии`, `Онгоинги`, `Популярное`, `Последнее добавление`, `Новинки этого года`, `С высоким рейтингом`.
3. Confirm `Клубничка 18+` is not visible in the ForkPlayer root menu.
4. Open `/fxml/tmdb?cat=dorama&sort=ongoing` and confirm Korean drama rows are returned, but completed/recently released batches without upcoming episode air dates such as `Слабый герой` and `Охотничьи псы` do not appear.
5. Open `/fxml/tmdb?cat=dorama&sort=latest` and `/fxml/tmdb?cat=dorama&sort=now`; confirm first-air dates do not point into the future.
6. Use the sort submenu on a Dorama list and confirm links still return Dorama rows.
7. Follow `next_page_url` on Dorama results and confirm page 2 loads.
8. Open `/fxml/cub?cat=movie&without_genres=16` and `/fxml/cub?cat=movie&genre=16` to confirm forwarded CUB filters still work.
