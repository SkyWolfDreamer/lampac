using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json.Linq;
using Shared;
using Shared.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web;

namespace ForkXML;

public class CubController : BaseController
{
    class CubListCache
    {
        public List<TmdbMovie> movies { get; set; }

        public int total_pages { get; set; }
    }

    [HttpGet]
    [Route("fxml/cub")]
    async public Task<ActionResult> Index(string search, string cat, string sort, int page = 1)
    {
        string uri = $"{host}/fxml/cub";
        string additionalArgs = AdditionalArgs();

        string memkey = $"forkxml:list:{search}:{cat}:{sort}:{page}{additionalArgs}";

        if (!memoryCache.TryGetValue(memkey, out CubListCache cache) || cache == null)
        {
            JObject root;
            if (cat == "dorama")
            {
                string tmdbKey = CoreInit.conf.cub?.api_key;
                if (string.IsNullOrEmpty(tmdbKey)) tmdbKey = "4ef0d7355d9ffb5151e987764708ce96";

                root = await Http.Get<JObject>(DoramaDiscoverUrl(tmdbKey, sort, page));
            }
            else
            {
                root = await Http.Get<JObject>("http://tmdb.cub.red/" + $"?query={HttpUtility.UrlEncode(search)}&cat={cat}&sort={sort}&page={page}&results=60{additionalArgs}");
            }
            
            if (root == null || !root.ContainsKey("results"))
                return BadRequest();

            cache = new CubListCache()
            {
                movies = root.Value<JArray>("results")?.ToObject<List<TmdbMovie>>(),
                total_pages = root.Value<int?>("total_pages") ?? 0
            };

            if (cache.movies == null || cache.movies.Count == 0)
                return BadRequest();

            memoryCache.Set(memkey, cache, DateTime.Now.AddMinutes(5));
        }

        var menu = new List<ForkPlaylistItem>();
        var playlists = new List<ForkPlaylistItem>();

        foreach (var movie in cache.movies)
        {
            string title = movie.title ?? movie.name;
            string original_title = movie.original_title ?? movie.original_name;
            string end_title = string.IsNullOrEmpty(original_title) ? title : $"{title} / {original_title}";
            int serial = string.IsNullOrEmpty(movie.title ?? movie.original_title) ? 1 : 0;

            string args = $"id={movie.id}&tmdb_id={movie.id}&imdb_id={movie.imdb_id}&kinopoisk_id={movie.kinopoisk_id}&title={HttpUtility.UrlEncode(title)}&original_title={HttpUtility.UrlEncode(original_title)}&serial={serial}&original_language={movie.original_language}&year={(movie.release_date ?? movie.first_air_date)?.Split("-")?[0]}";

            playlists.Add(new ForkPlaylistItem()
            {
                title = title ?? original_title,
                description = Description(movie, end_title),
                logo_30x30 = Icon.Folder,
                playlist_url = $"{host}/lite/events?{args}",
            });
        }

        if (playlists.Count == 0)
        {
            if (string.IsNullOrWhiteSpace(search))
                return BadRequest();
        }

        if (string.IsNullOrEmpty(search) && sort != "releases")
        {
            menu.Add(new ForkPlaylistItem()
            {
                title = $"Сортировка: {SortTitle(sort)}",
                playlist_url = "submenu",
                submenu = SortMenu(uri, search, cat, page, additionalArgs),
                logo_30x30 = Icon.Filter
            });
        }

        return Json(new
        {
            title = "Lampac",
            align = "left",
            menu = menu,
            channels = playlists,
            next_page_url = HasNextPage(cat, page, playlists.Count, cache.total_pages) ? ListUrl(uri, search, cat, sort, page + 1, additionalArgs) : null
        });
    }

    string AdditionalArgs()
    {
        string additionalArgs = "";

        foreach (var q in Request.Query)
        {
            if (q.Key == "search" || q.Key == "cat" || q.Key == "sort" || q.Key == "page")
                continue;

            foreach (var value in q.Value)
                additionalArgs += $"&{HttpUtility.UrlEncode(q.Key)}={HttpUtility.UrlEncode(value)}";
        }

        return additionalArgs;
    }

    string DoramaDiscoverUrl(string tmdbKey, string sort, int page)
    {
        var now = DateTime.UtcNow;
        var query = new List<string>()
        {
            $"api_key={HttpUtility.UrlEncode(tmdbKey)}",
            "with_original_language=ko",
            "with_genres=18",
            "include_adult=false",
            $"page={page}",
            "language=ru-RU"
        };

        switch (sort)
        {
            case "update":
                query.Add("sort_by=popularity.desc");
                query.Add($"air_date.gte={now.AddDays(-7):yyyy-MM-dd}");
                query.Add($"air_date.lte={now.AddDays(14):yyyy-MM-dd}");
                break;
            case "ongoing":
                query.Add("sort_by=popularity.desc");
                query.Add("with_status=0%7C2");
                query.Add($"first_air_date.lte={now:yyyy-MM-dd}");
                query.Add($"air_date.gte={now:yyyy-MM-dd}");
                query.Add($"air_date.lte={now.AddDays(21):yyyy-MM-dd}");
                break;
            case "top":
                query.Add("sort_by=vote_count.desc");
                query.Add("vote_count.gte=50");
                break;
            case "rated":
                query.Add("sort_by=vote_average.desc");
                query.Add("vote_average.gte=7");
                query.Add("vote_count.gte=50");
                break;
            case "latest":
                query.Add("sort_by=first_air_date.desc");
                break;
            case "now":
                query.Add("sort_by=first_air_date.desc");
                query.Add($"first_air_date_year={now.Year}");
                break;
            case "now_playing":
            default:
                query.Add("sort_by=popularity.desc");
                break;
        }

        string vote = Request.Query["vote"].ToString();
        if (!string.IsNullOrWhiteSpace(vote))
            query.Add($"vote_average.gte={HttpUtility.UrlEncode(vote)}");

        return "https://api.themoviedb.org/3/discover/tv?" + string.Join("&", query);
    }

    static string ListUrl(string uri, string search, string cat, string sort, int page, string additionalArgs)
    {
        string url = $"{uri}?search={HttpUtility.UrlEncode(search)}&cat={HttpUtility.UrlEncode(cat)}&sort={HttpUtility.UrlEncode(sort)}&page={page}";
        return url + additionalArgs;
    }

    static string SortTitle(string sort)
        => sort switch
        {
            "now_playing" => "сейчас смотрят",
            "update" => "новые серии",
            "ongoing" => "онгоинги",
            "top" => "популярное",
            "rated" => "с высоким рейтингом",
            "latest" => "последнее добавление",
            "now" => "новинки этого года",
            _ => "выбрать"
        };

    static List<ForkPlaylistItem> SortMenu(string uri, string search, string cat, int page, string additionalArgs)
    {
        if (cat == "dorama")
        {
            return new List<ForkPlaylistItem>()
            {
                new ForkPlaylistItem()
                {
                    title = "Сейчас смотрят",
                    playlist_url = ListUrl(uri, search, cat, "now_playing", page, additionalArgs),
                    logo_30x30 = Icon.Folder
                },
                new ForkPlaylistItem()
                {
                    title = "Новые серии",
                    playlist_url = ListUrl(uri, search, cat, "update", page, additionalArgs),
                    logo_30x30 = Icon.Folder
                },
                new ForkPlaylistItem()
                {
                    title = "Онгоинги",
                    playlist_url = ListUrl(uri, search, cat, "ongoing", page, additionalArgs),
                    logo_30x30 = Icon.Folder
                },
                new ForkPlaylistItem()
                {
                    title = "Популярное",
                    playlist_url = ListUrl(uri, search, cat, "top", page, additionalArgs),
                    logo_30x30 = Icon.Folder
                },
                new ForkPlaylistItem()
                {
                    title = "Последнее добавление",
                    playlist_url = ListUrl(uri, search, cat, "latest", page, additionalArgs),
                    logo_30x30 = Icon.Folder
                },
                new ForkPlaylistItem()
                {
                    title = "Новинки этого года",
                    playlist_url = ListUrl(uri, search, cat, "now", page, additionalArgs),
                    logo_30x30 = Icon.Folder
                },
                new ForkPlaylistItem()
                {
                    title = "С высоким рейтингом",
                    playlist_url = ListUrl(uri, search, cat, "rated", page, additionalArgs),
                    logo_30x30 = Icon.Folder
                }
            };
        }

        return new List<ForkPlaylistItem>()
        {
            new ForkPlaylistItem()
            {
                title = "Новинки",
                playlist_url = ListUrl(uri, search, cat, "now", page, additionalArgs),
                logo_30x30 = Icon.Folder
            },
            new ForkPlaylistItem()
            {
                title = "Популярное",
                playlist_url = ListUrl(uri, search, cat, "top", page, additionalArgs),
                logo_30x30 = Icon.Folder
            },
            new ForkPlaylistItem()
            {
                title = "Сейчас смотрят",
                playlist_url = ListUrl(uri, search, cat, "now_playing", page, additionalArgs),
                logo_30x30 = Icon.Folder
            }
        };
    }

    static bool HasNextPage(string cat, int page, int count, int totalPages)
    {
        if (cat == "dorama")
            return totalPages > 0 ? page < totalPages : count == 20;

        return count == 60;
    }


    static string Description(TmdbMovie movie, string end_title)
        => $@"<div class=""description"" style=""display: block; top: 38px; max-height: 1042px;""><div id=""title"" style=""color: #699bbb;""><strong>{end_title}</strong></div><br><div id=""cover_div"" style=""float: left; margin: 0px 1.8% 0px 0px;""><img id=""cover_img"" style=""width: 184px; "" src=""http://image.tmdb.org/t/p/w200/{movie.poster_path}""></div><div><strong><span style=""color: #3974d0;"">Выход:</span></strong> {(movie.release_date ?? movie.first_air_date)?.Split("-")[0]}<br><strong><span style=""color: #339966;"">Качество:</span></strong> {movie.release_quality}<br><div id=""footer"" style=""clear: both;  ""><br>{movie.overview}</div></div></div>";
}
