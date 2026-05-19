using System.Text.Json;

namespace BtcPriceHistory
{
    class Program
    {
        private static readonly DateTime UnixStartDateTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        // Binance public API - no API key required for historical klines
        private const string BaseUrl = "https://api.binance.us/api/v3/klines";
        private const string Symbol = "BTCUSD";
        private const string Interval = "1m";
        private const int Limit = 1000; // Max per request

        static async Task Main(string[] args)
        {
            // Configure date range
            var startDate = new DateTime(2017, 8, 17, 0, 0, 0, DateTimeKind.Utc); // Binance launch date
            var endDate = DateTime.UtcNow;
            var outputFile = $"C:\\Users\\aront\\Downloads\\{Symbol}_{Interval}_{startDate:yyyyMMdd}_to_{endDate:yyyyMMdd}.csv";
            Console.WriteLine($"Pulling {Symbol} {Interval} data from {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}");
            Console.WriteLine($"Output: {outputFile}");
            Console.WriteLine("-------------------------------------------");

            using var httpClient = new HttpClient();
            using var writer = new StreamWriter(outputFile, append: false);

            // Write CSV header
            await writer.WriteLineAsync("OpenTime,Open,High,Low,Close,Volume,CloseTime,QuoteVolume,TradeCount,TakerBuyBaseVolume,TakerBuyQuoteVolume");

            long startMs = ToUnixMs(startDate);
            long endMs = ToUnixMs(endDate);
            long currentMs = startMs;

            int totalCandles = 0;
            int requestCount = 0;

            while (currentMs < endMs)
            {
                try
                {
                    var url = $"{BaseUrl}?symbol={Symbol}&interval={Interval}&startTime={currentMs}&endTime={endMs}&limit={Limit}";
                    var response = await httpClient.GetStringAsync(url);
                    var candles = JsonSerializer.Deserialize<JsonElement[][]>(response);

                    if (candles == null || candles.Length == 0)
                        break;

                    foreach (var candle in candles)
                    {
                        long openTime = candle[0].GetInt64();
                        string open = candle[1].GetString()!;
                        string high = candle[2].GetString()!;
                        string low = candle[3].GetString()!;
                        string close = candle[4].GetString()!;
                        string volume = candle[5].GetString()!;
                        long closeTime = candle[6].GetInt64();
                        string quoteVol = candle[7].GetString()!;
                        int tradeCount = candle[8].GetInt32();
                        string takerBuyBase = candle[9].GetString()!;
                        string takerBuyQuote = candle[10].GetString()!;

                        var openDt = FromUnixMs(openTime).ToString("yyyy-MM-dd HH:mm:ss");
                        var closeDt = FromUnixMs(closeTime).ToString("yyyy-MM-dd HH:mm:ss");

                        await writer.WriteLineAsync(
                            $"{openDt},{open},{high},{low},{close},{volume},{closeDt},{quoteVol},{tradeCount},{takerBuyBase},{takerBuyQuote}"
                        );
                    }

                    totalCandles += candles.Length;
                    requestCount++;

                    // Advance to the next batch (last candle's open time + 1 minute)
                    long lastOpenTime = candles[^1][0].GetInt64();
                    currentMs = lastOpenTime + 60_000;

                    // Progress update every 100 requests
                    if (requestCount % 100 == 0)
                    {
                        var progress = FromUnixMs(currentMs);
                        Console.WriteLine($"[{requestCount} requests] Up to {progress:yyyy-MM-dd HH:mm} | {totalCandles:N0} candles written...");
                    }

                    // Respect Binance rate limits: ~1200 requests/min allowed, stay conservative
                    await Task.Delay(50);
                }
                catch (HttpRequestException ex)
                {
                    Console.WriteLine($"HTTP error: {ex.Message} — retrying in 5s...");
                    await Task.Delay(5000);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Unexpected error: {ex.Message}");
                    break;
                }
            }

            Console.WriteLine("-------------------------------------------");
            Console.WriteLine($"Done! {totalCandles:N0} candles written to {outputFile}");
        }

        static long ToUnixMs(DateTime dt)
        {
            return (long)(dt.ToUniversalTime() - UnixStartDateTime).TotalMilliseconds;
        }

        static DateTime FromUnixMs(long ms)
        {
            return UnixStartDateTime.AddMilliseconds(ms);
        }
    }
}