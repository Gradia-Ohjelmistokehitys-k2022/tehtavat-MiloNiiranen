using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace BitcoinAnalyzer
{
    public partial class Form1 : Form
    {
        private readonly BitcoinDataManager _dataManager;
        private List<BitcoinDataModel> data; // Datan tallennus

        public Form1()
        {
            InitializeComponent();
            _dataManager = new BitcoinDataManager();
            this.Load += Form1_Load;
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                // Fetch the current Bitcoin price
                double currentPrice = await _dataManager.FetchCurrentBitcoinPriceAsync();



                // Save today's average price and trading volume to the JSON file
                await _dataManager.AddTodayPriceToJsonAsync();

                // Initialize DateTimePickers
                dateTimePicker1.MinDate = new DateTime(2015, 1, 1);
                dateTimePicker1.MaxDate = DateTime.Now;
                dateTimePicker1.Value = new DateTime(2025, 1, 1);

                dateTimePicker2.MinDate = new DateTime(2015, 1, 1);
                dateTimePicker2.MaxDate = DateTime.Now;
                dateTimePicker2.Value = new DateTime(2025, 1, 9);
            }
            catch (Exception ex)
            {
                // Display an error message if something goes wrong
                labelCurrentPrice.Text = "Error loading current price.";
                MessageBox.Show($"Error during program initialization: {ex.Message}");
            }
        }


        private async void FetchDataButton_Click(object sender, EventArgs e)
        {
            try
            {
                // Haetaan DateTimePickerist‰ p‰iv‰m‰‰r‰t
                DateTime startDate = dateTimePicker1.Value.Date;
                DateTime endDate = dateTimePicker2.Value.Date;

                // Varmistetaan, ett‰ aikav‰li on validi
                if (startDate > endDate)
                {
                    MessageBox.Show("Aloitusp‰iv‰m‰‰r‰n on oltava ennen lopetusp‰iv‰m‰‰r‰‰.");
                    return;
                }

                // Ladataan tiedot v‰limuistista
                data = _dataManager.LoadDataFromJson(startDate, endDate);

                // Tarkista, lˆytyykˆ kaikki tiedot
                if (data == null || !data.Any() || data.Any(d => d.trading_volume == 0))
                {
                    // Jos tietoja ei ole tai kaupank‰yntivolyymi puuttuu, haetaan API:sta
                    await _dataManager.FetchAndSaveDataAsync(startDate, endDate);

                    // Lataa p‰ivitetty data
                    data = _dataManager.LoadDataFromJson(startDate, endDate);
                }

                // Tarkista uudelleen, lˆytyykˆ tietoja
                if (data == null || !data.Any())
                {
                    MessageBox.Show("Tietoja ei ole saatavilla valitulle aikav‰lille.");
                    return;
                }

                // Etsi alhaisin ja korkein hinta
                var lowestPriceData = data.MinBy(d => d.average_price);
                var highestPriceData = data.MaxBy(d => d.average_price);

                // Etsi alhaisin ja korkein volyymi
                var lowestVolumeData = data.MinBy(d => d.trading_volume);
                var highestVolumeData = data.MaxBy(d => d.trading_volume);

                // N‰yt‰ hintatulokset
                if (lowestPriceData != null && highestPriceData != null)
                {
                    labelLowestPriceDate.Text = $"Lowest Price: ${lowestPriceData.average_price:0.00} on {lowestPriceData.date:dd.MM.yyyy}";
                    labelHighestPriceDate.Text = $"Highest Price: ${highestPriceData.average_price:0.00} on {highestPriceData.date:dd.MM.yyyy}";
                }

                // N‰yt‰ volyymitulokset
                if (lowestVolumeData != null && highestVolumeData != null)
                {
                    labelLowestVolume.Text = $"Lowest Volume: ${lowestVolumeData.trading_volume:0.00} on {lowestVolumeData.date:dd.MM.yyyy}";
                    labelHighestVolume.Text = $"Highest Volume: ${highestVolumeData.trading_volume:0.00} on {highestVolumeData.date:dd.MM.yyyy}";
                }

                var (longestBearish, bearishStart, bearishEnd, longestBullish, bullishStart, bullishEnd) = CalculatePriceTrends(data);

                // Show results for longest trends
                labelLongestBearish.Text = $"Longest Bearish Trend: {bearishStart:dd.MM.yyyy} - {bearishEnd:dd.MM.yyyy}";
                labelLongestBullish.Text = $"Longest Bullish Trend: {bullishStart:dd.MM.yyyy} - {bullishEnd:dd.MM.yyyy}";

                var (buyDay, sellDay, maxProfit, sellFirstDay, buyBackDay, maxProfitReverse) = GetBestTradingDays(data);

                // N‰yt‰ kauppatulokset
                labelBestBuySell.Text = $"Buy on {buyDay:dd.MM.yyyy}, Sell on {sellDay:dd.MM.yyyy}, Profit: ${maxProfit:0.00}";
                labelBestSellBuy.Text = $"Sell on {sellFirstDay:dd.MM.yyyy}, Buy on {buyBackDay:dd.MM.yyyy}, Profit: ${maxProfitReverse:0.00}";

                // P‰ivitet‰‰n graafi
                ShowChart(data, startDate, endDate);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Virhe tietojen haussa: {ex.Message}");
            }
        }

        public void ShowChart(List<BitcoinDataModel> data, DateTime startDate, DateTime endDate)
        {
            // Suodata data valitun aikav‰lin mukaan
            var filteredData = data
                .Where(x => x.date.Date >= startDate.Date && x.date.Date <= endDate.Date)
                .ToList();

            // Tarkista, lˆytyykˆ dataa
            if (!filteredData.Any())
            {
                MessageBox.Show("Ei tietoja valitulle aikav‰lille.");
                return;
            }

            // Alustetaan graafimalli
            var model = new PlotModel { Title = "Bitcoin Price Chart" };

            // Luodaan viivadiagrammi ilman pisteit‰
            var lineSeries = new LineSeries
            {
                MarkerType = MarkerType.None, // Ei merkkej‰ viivassa
                StrokeThickness = 2,         // Viivan paksuus
                Color = OxyColors.Blue       // Viivan v‰ri
            };

            // Lis‰t‰‰n data sarjaan
            foreach (var point in filteredData)
            {
                lineSeries.Points.Add(new DataPoint(DateTimeAxis.ToDouble(point.date.Date), point.average_price));
            }

            // Alustetaan akselit
            var dateAxis = new DateTimeAxis
            {
                Position = AxisPosition.Bottom,
                StringFormat = "dd.MM.yyyy",
                Title = "Date",
                Minimum = DateTimeAxis.ToDouble(startDate.Date),
                Maximum = DateTimeAxis.ToDouble(endDate.Date),
                IntervalType = DateTimeIntervalType.Days
            };

            var priceAxis = new LinearAxis
            {
                Position = AxisPosition.Left,
                Title = "Price (USD)"
            };

            model.Axes.Add(dateAxis);
            model.Axes.Add(priceAxis);
            model.Series.Add(lineSeries);

            // P‰ivitet‰‰n PlotView
            plotView1.Model = model;

            // Konsolituloste virheenkorjausta varten
            Console.WriteLine($"Start Date: {startDate}, End Date: {endDate}");
            Console.WriteLine($"Filtered Data Count: {filteredData.Count}");
            foreach (var point in filteredData)
            {
                Console.WriteLine($"P‰iv‰m‰‰r‰: {point.date}, Hinta: {point.average_price}");
            }
        }


        private (int longestBearish, DateTime bearishStart, DateTime bearishEnd, int longestBullish, DateTime bullishStart, DateTime bullishEnd) CalculatePriceTrends(List<BitcoinDataModel> data)
        {
            int currentBearish = 0, longestBearish = 0;
            DateTime bearishStart = DateTime.MinValue, bearishEnd = DateTime.MinValue;
            DateTime tempBearishStart = DateTime.MinValue;

            int currentBullish = 0, longestBullish = 0;
            DateTime bullishStart = DateTime.MinValue, bullishEnd = DateTime.MinValue;
            DateTime tempBullishStart = DateTime.MinValue;

            for (int i = 1; i < data.Count; i++)
            {
                // Check for bearish trend
                if (data[i].average_price < data[i - 1].average_price)
                {
                    if (currentBearish == 0) tempBearishStart = data[i - 1].date;
                    currentBearish++;
                    if (currentBearish > longestBearish)
                    {
                        longestBearish = currentBearish;
                        bearishStart = tempBearishStart;
                        bearishEnd = data[i].date;
                    }
                }
                else
                {
                    currentBearish = 0;
                }

                // Check for bullish trend
                if (data[i].average_price > data[i - 1].average_price)
                {
                    if (currentBullish == 0) tempBullishStart = data[i - 1].date;
                    currentBullish++;
                    if (currentBullish > longestBullish)
                    {
                        longestBullish = currentBullish;
                        bullishStart = tempBullishStart;
                        bullishEnd = data[i].date;
                    }
                }
                else
                {
                    currentBullish = 0;
                }
            }

            return (longestBearish, bearishStart, bearishEnd, longestBullish, bullishStart, bullishEnd);
        }


        private (DateTime buyDay, DateTime sellDay, double maxProfit, DateTime sellFirstDay, DateTime buyBackDay, double maxProfitReverse) GetBestTradingDays(List<BitcoinDataModel> data)
        {
            // Initialize variables for buy-low and sell-high strategy
            double minPrice = double.MaxValue;
            DateTime minPriceDate = DateTime.MinValue;
            double maxProfit = 0;
            DateTime buyDay = DateTime.MinValue;
            DateTime sellDay = DateTime.MinValue;

            // Initialize variables for sell-high and buy-low strategy
            double maxPrice = double.MinValue;
            DateTime maxPriceDate = DateTime.MinValue;
            double maxProfitReverse = 0;
            DateTime sellFirstDay = DateTime.MinValue;
            DateTime buyBackDay = DateTime.MinValue;

            // Iterate over the data to calculate best trading days
            for (int i = 0; i < data.Count; i++)
            {
                // Check for buy-low and sell-high strategy
                if (data[i].average_price < minPrice)
                {
                    minPrice = data[i].average_price;
                    minPriceDate = data[i].date;
                }

                double potentialProfit = data[i].average_price - minPrice;
                if (potentialProfit > maxProfit)
                {
                    maxProfit = potentialProfit;
                    buyDay = minPriceDate;
                    sellDay = data[i].date;
                }

                // Check for sell-high and buy-low strategy
                if (data[i].average_price > maxPrice)
                {
                    maxPrice = data[i].average_price;
                    maxPriceDate = data[i].date;
                }

                double potentialProfitReverse = maxPrice - data[i].average_price;
                if (potentialProfitReverse > maxProfitReverse)
                {
                    maxProfitReverse = potentialProfitReverse;
                    sellFirstDay = maxPriceDate;
                    buyBackDay = data[i].date;
                }
            }

            return (buyDay, sellDay, maxProfit, sellFirstDay, buyBackDay, maxProfitReverse);
        }

    }
}
