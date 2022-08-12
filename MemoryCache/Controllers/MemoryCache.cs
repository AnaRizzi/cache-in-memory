using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;

namespace MemoryCache.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MemoryCache : ControllerBase
    {
        private readonly IMemoryCache _cache;
        private const string COUNTRIES_KEY = "Countries";

        public MemoryCache(IMemoryCache memoryCache)
        {
            _cache = memoryCache;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            if (_cache.TryGetValue(COUNTRIES_KEY, out object result))
            {
                return Ok(result);
            }
            else
            {
                const string restCountriesUrl = "https://restcountries.com/v2/all";
                using(var client = new HttpClient())
                {
                    var response = await client.GetAsync(restCountriesUrl);
                    var responseData = await response.Content.ReadAsStringAsync();
                    var countries = JsonSerializer.Deserialize<List<Country>>(responseData, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    var memoryCacheEntryOptions = new MemoryCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(3600),
                        SlidingExpiration = TimeSpan.FromSeconds(1200)
                    };

                    _cache.Set(COUNTRIES_KEY, countries, memoryCacheEntryOptions);

                    return Ok(countries);
                }
            }
        }
    }
}
