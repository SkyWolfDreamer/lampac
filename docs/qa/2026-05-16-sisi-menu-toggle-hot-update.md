# SISI Menu Toggle QA - 2026-05-16

## Scope

- Removed the stale "requires restart" wording from the SISI "Отображать в меню" setting.
- Added a storage-change listener for `sisi_show_in_menu`.
- The Lampa menu item with `data-action="sisi"` is now added or removed immediately when the setting changes.
- Guarded against duplicate menu buttons when the setting is toggled back on.

## Verification

- `git diff --check` passed.
- Static inspection confirmed the previous `(требует перезапуска)` text is no longer present in `SISI/plugins/sisi.js`.
- Placeholder-normalized syntax check passed with `node --check` after replacing Lampac template markers such as `{rch_websoket}`, `{historySave}`, and `{push_all}`.
- Manual client regression is still recommended because the real runtime behaviour depends on Lampa's `Storage.listener` event in the client.

## Manual Regression Checklist

1. Open Lampa with the SISI plugin enabled.
2. Go to SISI settings and disable **Отображать в меню**.
3. Confirm **Клубничка** disappears from the main menu without restarting the client/server.
4. Enable **Отображать в меню** again.
5. Confirm **Клубничка** appears once, with no duplicate menu item.
