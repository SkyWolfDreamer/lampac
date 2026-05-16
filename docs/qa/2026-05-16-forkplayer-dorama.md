# ForkPlayer Dorama QA - 2026-05-16

## Scope

- Reviewed the existing `feat(ForkPlayer): Добавлен раздел Дорамы...` change.
- Kept the ForkPlayer root menu focused on non-adult sections by removing the `Клубничка 18+` menu item from `/fxml`.
- Hardened `/fxml/cub?cat=dorama` so direct TMDB Discover pagination works with TMDB's 20-item pages instead of CUB's 60-item pages.
- Preserved extra CUB query filters such as `genre` and `without_genres` through cache keys, sorting links, and next-page links.

## Behaviour Notes

- Normal CUB categories still call `http://tmdb.cub.red/` with forwarded extra query params.
- `cat=dorama` bypasses CUB and calls TMDB Discover TV directly with `with_original_language=ko`, `with_genres=18`, and the configured `CoreInit.conf.cub.api_key` fallback.
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

## Manual Regression Checklist

1. Open `/fxml` in ForkPlayer and confirm `Дорамы` is visible.
2. Confirm `Клубничка 18+` is not visible in the ForkPlayer root menu.
3. Open `/fxml/cub?cat=dorama` and confirm Korean drama rows are returned.
4. Use the sort submenu on the Dorama list and confirm links still return Dorama rows.
5. Follow `next_page_url` on Dorama results and confirm page 2 loads.
6. Open `/fxml/cub?cat=movie&without_genres=16` and `/fxml/cub?cat=movie&genre=16` to confirm forwarded CUB filters still work.
