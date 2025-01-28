using System;
using System.Text.Json;

namespace BitcoinAnalyzer
{
    public class BitcoinDataModel
    {
        public DateTime date { get; set; } // Päivämäärä
        public double average_price { get; set; } // Keskihinta
        public double trading_volume { get; set; } // Kaupankäyntivolyymi


        // JSON:in käsittely
        public static BitcoinDataModel FromJson(JsonElement jsonElement)
        {
            return new BitcoinDataModel
            {
                date = DateTime.Parse(jsonElement.GetProperty("date").GetString()),
                average_price = jsonElement.GetProperty("average_price").GetDouble(),
                trading_volume = jsonElement.TryGetProperty("trading_volume", out var volume) ? volume.GetDouble() : 0
            };
        }
    }
}
