# ForkPlayerXML

Стартовый URL для клиента ForkPlayer: **`http://<хост>:<порт>/fxml`** (типичный порт Lampac — **9118**).

Интеграция с **ForkPlayer**: XML/JSON-плейлисты для **каталога** и **онлайна** через **`EventListener`**, редирект с корня **`/`** на **`/fxml`**, правки модели инициализации клиента (**`rhub`**, **`streamproxy`**).

Пространство имён кода: **`ForkXML`**.

## Назначение

- **Catalog**: `CatalogChannels`, `CatalogList`, `CatalogCard` → **`CatalogAPI`**.
- **Sisi**: `SisiChannels`, `SisiPlaylistResult`, `SisiOnResult` → **`SisiAPI`**.
- **Online**: `OnlineChannels`, `OnlineContentTpl`, `VideoTpl` → **`OnlineAPI`**.
- **Middleware**: если запрос распознан как ForkPlayer (**`Utilities.IsForkPlayer`**) и путь **`/`**, выполняется **редирект** на **`/fxml`** с сохранением query через **`Utilities.ClearArgs`** (обработчик возвращает **`false`** — цепочка прерывается).
- **BadInitialization**: для ForkPlayer в модели инициализации выставляются **`rhub = false`**, **`streamproxy = true`**.

## HTTP

| Маршрут | Назначение |
|---------|------------|
| **GET** `/fxml` | Корневой JSON-плейлист ForkPlayer: пункты «Поиск», «Сейчас смотрят», категории фильмов/сериалов/аниме/дорам, ссылки на **`/fxml/cub`**, **`/fxml/tmdb`** и **`/catalog`**. **«Дорамы»** открываются как вложенное меню с секциями **«Сейчас смотрят»**, **«Новые серии»**, **«Онгоинги»**, **«Популярное»**, **«Последнее добавление»**, **«Новинки этого года»**, **«С высоким рейтингом»**. При включённом **`accsdb`** без авторизованного пользователя возвращается сообщение об ошибке доступа (с **`box_mac`). См. **`ForkController`**. |
| **GET** `/fxml/cub` | Выдача обычного CUB-каталога по параметрам **`search`**, **`cat`**, **`sort`**, **`page`** через **`http://tmdb.cub.red/`**. Дополнительные query-фильтры, например **`genre`** и **`without_genres`**, прокидываются в CUB, участвуют в cache key и сохраняются в sort/next-page ссылках. См. **`CubController`**. |
| **GET** `/fxml/tmdb` | Выдача TMDB-backed списков. Сейчас поддерживает **`cat=dorama`**: прямой TMDB Discover TV с фильтром корейских драм (**`with_original_language=ko`**, **`with_genres=18`**, **`include_adult=false`**) и dorama-specific сортировками **`now_playing`**, **`update`**, **`ongoing`**, **`top`**, **`latest`**, **`now`**, **`rated`**. Для **`ongoing`** требуется серия с **`air_date`** от сегодня до ближайших 21 дней; один только TMDB-статус **`Returning Series`** не считается онгоингом. **`update`**, **`latest`** и **`now`** не показывают будущие релизы. Кеш в памяти ~5 минут. См. **`TmdbController`**. |

Контроллеры наследуют **`BaseController`**.

## Конфигурация

Отдельной секции **`ForkPlayerXML`** в минимальном `ModInit` нет — включение через **`manifest.json`**. Доступ к пунктам главного меню зависит от **`CoreInit.conf.accsdb`** и глобальных настроек каталога.

## Зависимости

- Реализации **`CatalogAPI`**, **`SisiAPI`**, **`OnlineAPI`** в каталоге модуля.
- Внешний HTTP **`tmdb.cub.red`** для обычных списков в **`/fxml/cub`**.
- TMDB API key из **`CoreInit.conf.cub.api_key`** для **`/fxml/tmdb`**; при пустом значении используется встроенный публичный ключ из базовой конфигурации.

## Связь с клиентом

ForkPlayer ожидает ответы в формате, который формируют перечисленные API; без этого модуля соответствующие события не обрабатываются.
