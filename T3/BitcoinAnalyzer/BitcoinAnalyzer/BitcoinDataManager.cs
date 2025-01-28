using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace BitcoinAnalyzer
{
    public class BitcoinDataManager
    {
        private const string ApiBaseUrl = "https://api.coingecko.com/api/v3/coins/bitcoin/market_chart/range";
        private static readonly string CacheFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bitcoinData.json");
        private readonly HttpClient _httpClient = new HttpClient();

        private string BuildApiUrl(DateTime startDate, DateTime endDate)
        {
            long startTimestamp = ((DateTimeOffset)startDate).ToUnixTimeSeconds();
            long endTimestamp = ((DateTimeOffset)endDate).ToUnixTimeSeconds();
            return $"{ApiBaseUrl}?vs_currency=usd&from={startTimestamp}&to={endTimestamp}";
        }

        public List<BitcoinDataModel> LoadDataFromJson(DateTime startDate, DateTime endDate)
        {
            if (!File.Exists(CacheFilePath)) return new List<BitcoinDataModel>();

            string jsonData = File.ReadAllText(CacheFilePath);

            var allData = JsonSerializer.Deserialize<List<BitcoinDataModel>>(jsonData);
            return allData
                ?.Where(d => d.date >= startDate && d.date <= endDate)
                .ToList() ?? new List<BitcoinDataModel>();
        }

        public async Task FetchAndSaveDataAsync(DateTime startDate, DateTime endDate)
        {
            // Lataa nykyinen JSON-data
            List<BitcoinDataModel> existingData = File.Exists(CacheFilePath)
                ? JsonSerializer.Deserialize<List<BitcoinDataModel>>(File.ReadAllText(CacheFilePath))
                : new List<BitcoinDataModel>();

            // Suodata puuttuvat päivämäärät
            var missingDates = Enumerable.Range(0, (endDate - startDate).Days + 1)
                .Select(offset => startDate.AddDays(offset))
                .Where(date => !existingData.Any(d => d.date.Date == date.Date))
                .ToList();

            // Jos ei ole puuttuvia päivämääriä, lopetetaan
            if (!missingDates.Any())
            {
                Console.WriteLine("Kaikki pyydetyt päivämäärät löytyvät jo tiedostosta.");
                return;
            }

            // Määritä API-kutsu puuttuville päiville
            string url = BuildApiUrl(missingDates.First(), missingDates.Last());
            HttpResponseMessage response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            string json = await response.Content.ReadAsStringAsync();

            // CoinGeckon vastaus
            var apiData = JsonSerializer.Deserialize<CoinGeckoResponse>(json);

            if (apiData == null || apiData.prices == null || apiData.total_volumes == null)
            {
                Console.WriteLine("API-vastaus ei sisällä tarvittavia tietoja.");
                return;
            }

            // Laske keskimääräiset hinnat ja volyymit BTC-yksiköissä päivittäin
            var newEntries = apiData.prices
                .GroupBy(p => DateTimeOffset.FromUnixTimeMilliseconds((long)p[0]).DateTime.Date)
                .Select(g =>
                {
                    var averagePrice = g.Average(p => p[1]); // Keskimääräinen hinta USD/BTC
                    var totalVolumeUsd = apiData.total_volumes
                        .Where(v => DateTimeOffset.FromUnixTimeMilliseconds((long)v[0]).DateTime.Date == g.Key)
                        .Sum(v => v[1]); // Päivän kaupankäyntivolyymi USD:ssa

                    return new BitcoinDataModel
                    {
                        date = g.Key,
                        average_price = averagePrice,
                        trading_volume = averagePrice > 0 ? totalVolumeUsd / averagePrice : 0 // BTC-volyymi
                    };
                })
                .ToList();

            // Lisää uudet tiedot ja tallenna
            existingData.AddRange(newEntries);
            existingData = existingData.OrderBy(d => d.date).ToList();

            File.WriteAllText(CacheFilePath, JsonSerializer.Serialize(existingData, new JsonSerializerOptions { WriteIndented = true }));
            Console.WriteLine("Puuttuvat päivämäärät haettu ja tallennettu.");
        }


        public async Task<(double price, double volume)> GetCurrentPriceAndVolumeAsync()
        {
            try
            {
                string url = "https://api.coingecko.com/api/v3/coins/bitcoin/market_chart/range";
                DateTime today = DateTime.UtcNow.Date;
                long startTimestamp = ((DateTimeOffset)today).ToUnixTimeSeconds();
                long endTimestamp = ((DateTimeOffset)today.AddDays(1)).ToUnixTimeSeconds();

                var response = await _httpClient.GetAsync($"{url}?vs_currency=usd&from={startTimestamp}&to={endTimestamp}");
                response.EnsureSuccessStatusCode();

                string json = await response.Content.ReadAsStringAsync();
                var data = JsonSerializer.Deserialize<CoinGeckoResponse>(json);

                // Get today's price and volume
                double price = data.prices.LastOrDefault()?.ElementAtOrDefault(1) ?? 0;
                double volume = data.total_volumes.LastOrDefault()?.ElementAtOrDefault(1) ?? 0;

                return (price, volume);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching current price and volume: {ex.Message}");
                return (0, 0); // Return default values on failure
            }
        }


        public async Task<double> FetchCurrentBitcoinPriceAsync()
        {
            string url = "https://api.coingecko.com/api/v3/simple/price?ids=bitcoin&vs_currencies=usd";

            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                string json = await response.Content.ReadAsStringAsync();
                var data = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, double>>>(json);

                // Tarkistetaan, sisältääkö vastaus Bitcoin-hinnan
                if (data != null && data.ContainsKey("bitcoin") && data["bitcoin"].ContainsKey("usd"))
                {
                    return data["bitcoin"]["usd"];
                }
                else
                {
                    throw new Exception("Bitcoin-hintaa ei löytynyt API-vastauksesta.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Virhe nykyisen hinnan haussa: {ex.Message}");
                return 0; // Palautetaan 0 virhetilanteessa
            }
        }


        public async Task AddTodayPriceToJsonAsync()
        {
            DateTime today = DateTime.Today;

            // Load existing data
            List<BitcoinDataModel> existingData = File.Exists(CacheFilePath)
                ? JsonSerializer.Deserialize<List<BitcoinDataModel>>(File.ReadAllText(CacheFilePath))
                : new List<BitcoinDataModel>();

            // Check if today's data already exists
            if (existingData.Any(d => d.date.Date == today))
            {
                Console.WriteLine("Today's data already exists in the JSON.");
                return;
            }

            // Fetch today's price and volume
            var (price, volume) = await GetCurrentPriceAndVolumeAsync();

            if (price == 0 || volume == 0)
            {
                Console.WriteLine("Unable to fetch today's data from the API.");
                return;
            }

            // Add today's data
            var todayData = new BitcoinDataModel
            {
                date = today,
                average_price = price,
                trading_volume = volume
            };

            existingData.Add(todayData);

            // Save the updated data
            File.WriteAllText(CacheFilePath, JsonSerializer.Serialize(existingData, new JsonSerializerOptions { WriteIndented = true }));
            Console.WriteLine("Today's price and volume have been saved to the JSON file.");
        }


        public async Task<(DateTime LowestVolumeDate, double LowestVolume, DateTime HighestVolumeDate, double HighestVolume)> GetVolumeExtremesFromApiAsync(DateTime startDate, DateTime endDate)
        {
            // Rakennetaan URL API-kutsulle
            string url = BuildApiUrl(startDate, endDate);

            // Lähetetään API-pyyntö
            HttpResponseMessage response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            string json = await response.Content.ReadAsStringAsync();

            // Deserialisoidaan CoinGecko-vastaus
            var apiData = JsonSerializer.Deserialize<CoinGeckoResponse>(json);

            // Tarkistetaan, löytyykö dataa
            if (apiData == null || apiData.total_volumes == null || !apiData.total_volumes.Any())
            {
                throw new Exception("Ei volyymitietoja saatavilla annetulta aikaväliltä.");
            }

            // Muodostetaan volyymidata päivämäärien mukaan
            var volumeData = apiData.total_volumes
                .Select(v => new
                {
                    Date = DateTimeOffset.FromUnixTimeMilliseconds((long)v[0]).DateTime.Date,
                    Volume = v[1]
                })
                .ToList();

            // Lasketaan pienin ja suurin volyymi
            var lowestVolume = volumeData.OrderBy(v => v.Volume).First();
            var highestVolume = volumeData.OrderByDescending(v => v.Volume).First();

            return (lowestVolume.Date, lowestVolume.Volume, highestVolume.Date, highestVolume.Volume);
        }

        public async Task<List<BitcoinDataModel>> FetchDataWithVolumes(List<DateTime> dates, string apiUrl)
        {
            var bitcoinDataList = new List<BitcoinDataModel>();

            foreach (var date in dates)
            {
                try
                {
                    // Hae olemassa oleva data JSON:sta
                    var jsonData = await FetchJsonForDate(apiUrl, date);

                    // Muodosta BitcoinDataModel-objekti
                    var data = BitcoinDataModel.FromJson(jsonData);

                    // Jos kaupankäyntivolyymi puuttuu, tarkistetaan JSON-tieto uudelleen
                    if (data.trading_volume == 0 && jsonData.TryGetProperty("trading_volume", out var volumeProperty))
                    {
                        data.trading_volume = volumeProperty.GetDouble();
                    }

                    bitcoinDataList.Add(data);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Virhe tietojen haussa päivämäärälle {date.ToShortDateString()}: {ex.Message}");
                    // Jatka silmukkaa, vaikka yksi päivä epäonnistuisi
                    continue;
                }
            }

            return bitcoinDataList;
        }


        public async Task<JsonElement> FetchJsonForDate(string apiUrl, DateTime date)
        {
            using var client = new HttpClient();

            // Oletetaan, että API käyttää päivämäärää query-parametrina
            var response = await client.GetAsync($"{apiUrl}?date={date:yyyy-MM-dd}");
            response.EnsureSuccessStatusCode();

            var jsonString = await response.Content.ReadAsStringAsync();
            var jsonDocument = JsonDocument.Parse(jsonString);

            return jsonDocument.RootElement;
        }

        public async Task GetVolumeDataAsync(DateTime startDate, DateTime endDate)
        {
            // Rakennetaan URL API-kutsulle
            string url = BuildApiUrl(startDate, endDate);

            // Lähetetään API-pyyntö
            HttpResponseMessage response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            string json = await response.Content.ReadAsStringAsync();

            // Deserialisoidaan CoinGecko-vastaus
            var apiData = JsonSerializer.Deserialize<CoinGeckoResponse>(json);

            // Tarkistetaan, löytyykö dataa
            if (apiData == null || apiData.total_volumes == null || !apiData.total_volumes.Any())
            {
                Console.WriteLine("Ei volyymitietoja.");
                return;
            }

            // Muodostetaan volyymidata
            var volumeData = apiData.total_volumes
                .Select(v => new
                {
                    Date = DateTimeOffset.FromUnixTimeMilliseconds((long)v[0]).DateTime.Date,
                    Volume = v[1] == null ? 0 : v[1] // Jos volume on null, aseta oletusarvoksi 0
                })
                .ToList();

            // Tulostetaan volyymit
            foreach (var data in volumeData)
            {
                Console.WriteLine($"Päivämäärä: {data.Date.ToShortDateString()}, Volyymi: {data.Volume}");
            }
        }

    }
}
