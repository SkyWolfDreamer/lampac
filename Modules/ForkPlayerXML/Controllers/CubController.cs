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
                
                string sortBy = "popularity.desc";
                if (sort == "now") sortBy = "first_air_date.desc";
                else if (sort == "top") sortBy = "vote_average.desc&vote_count.gte=50";
                
                string tmdbUrl = $"https://api.themoviedb.org/3/discover/tv?api_key={tmdbKey}&with_original_language=ko&with_genres=18&sort_by={sortBy}&page={page}&language=ru-RU";
                root = await Http.Get<JObject>(tmdbUrl);
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
                title = $"Сортировка: {(string.IsNullOrEmpty(sort) ? "выбрать" : sort.Replace("now_playing", "сейчас смотрят").Replace("now", "новинки").Replace("top", "популярное"))}",
                playlist_url = "submenu",
                submenu = new List<ForkPlaylistItem>()
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
                        title = "Cейчас смотрят",
                        playlist_url = ListUrl(uri, search, cat, "now_playing", page, additionalArgs),
                        logo_30x30 = Icon.Folder
                    }
                },
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

    static string ListUrl(string uri, string search, string cat, string sort, int page, string additionalArgs)
    {
        string url = $"{uri}?search={HttpUtility.UrlEncode(search)}&cat={HttpUtility.UrlEncode(cat)}&sort={HttpUtility.UrlEncode(sort)}&page={page}";
        return url + additionalArgs;
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
