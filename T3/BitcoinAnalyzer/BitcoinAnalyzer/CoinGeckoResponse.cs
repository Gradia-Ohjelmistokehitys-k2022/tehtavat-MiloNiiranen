using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace BitcoinAnalyzer
{
    public class CoinGeckoResponse
    {
        public List<List<double>> prices { get; set; } // [timestamp, price]
        public List<List<double>> total_volumes { get; set; } // [timestamp, volume]
    }

}
