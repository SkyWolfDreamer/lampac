# Lampa Dorama Plugin QA - 2026-05-17

## Scope

- Added a standalone Lampa plugin at Modules/LampaWeb/plugins/dorama.js.
- Added LampaWeb.initPlugins.dorama so the plugin can be included from both /lampainit.js and /on.js without putting Dorama UI logic inside SISI.
- Added /dorama.js and /dorama/js/{token} routes to serve the plugin with {localhost} replacement.
- The plugin adds Дорамы immediately after Сериалы, registers lampac_dorama, and keeps menu insertion idempotent.

## Behaviour Notes

- lampac_dorama uses TMDB Discover TV filters for Korean dramas: with_original_language=ko, default with_genres=18, and include_adult=false.
- Requests prefer the Lampac TMDB proxy via {localhost}/tmdb/api/3/...; if {localhost} is not replaced, the plugin falls back to the native Lampa TMDB URL pattern.
- The plugin patches Dorama category_full navigation and CUB/TMDB list fallback handling so the native Ещё button cannot route Dorama Discover URLs into the wrong active source.
- SISI files are intentionally untouched by this PR.

## Verification

- git diff --check passed.
- node --check Modules/LampaWeb/plugins/dorama.js passed.
- Dockerized module build passed: docker run --rm -v "$PWD":/src -w /src mcr.microsoft.com/dotnet/sdk:10.0 dotnet build Modules/LampaWeb/LampaWeb.csproj.
- Full Docker image build passed: docker build --progress=plain -t lampac:lampa-dorama-test .
- Runtime Docker smoke passed with compose-style mounted config/passwd: GET /dorama.js returned HTTP 200 and the served script contained lampac_dorama.
- The only warning was the existing Shared/PlaywrightCore/Chromium.cs obsolete Devtools warning.
- Note: naked docker run without mounted init.conf/passwd is not a valid runtime smoke for this image; it starts without the local config files and the HTTP check resets before serving routes.

## Manual Regression Checklist

1. Enable LampaWeb.initPlugins.dorama.
2. Open Lampa through /lampainit.js and confirm Дорамы appears directly after Сериалы.
3. Open Lampa through /on.js and confirm the same Дорамы item appears without requiring SISI.
4. Open Дорамы and confirm the sectioned screen loads rows.
5. Open a section Ещё page and confirm page 2 loads from lampac_dorama rather than CUB.
6. Reload the app and confirm there is only one Дорамы item.
