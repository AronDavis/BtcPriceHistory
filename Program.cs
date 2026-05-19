using System.Text.Json;

namespace BtcPriceHistory
{
    class Program
    {
        private static readonly DateTime UnixStartDateTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        private const string BaseUrl = "https://api.binance.us/api/v3/klines";
        private const string Symbol = "BTCUSD";
        private const string Interval = "1m";
        private const int Limit = 1000;
        private const string CsvHeader = "OpenTime,Open,High,Low,Close,Volume,CloseTime,QuoteVolume,TradeCount,TakerBuyBaseVolume,TakerBuyQuoteVolume";

        private static readonly string DataDir = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "data");
        private static readonly string CheckpointFile = Path.Combine(DataDir, "last_timestamp.txt");
        private static readonly string StatsFile = Path.Combine(DataDir, "stats.json");

        static string CsvFileForYear(int year) =>
            Path.Combine(DataDir, $"{Symbol}_{Interval}_{year}.csv");

        static async Task Main(string[] args)
        {
            Directory.CreateDirectory(DataDir);

            var startDate = LoadCheckpoint() ?? new DateTime(2019, 6, 6, 0, 0, 0, DateTimeKind.Utc);
            var endDate = DateTime.UtcNow;

            Console.WriteLine($"{(LoadCheckpoint() is null ? "Starting" : "Resuming")} {Symbol} {Interval} pull");
            Console.WriteLine($"From : {startDate:yyyy-MM-dd HH:mm:ss} UTC");
            Console.WriteLine($"To   : {endDate:yyyy-MM-dd HH:mm:ss} UTC");
            Console.WriteLine("-------------------------------------------");

            using var httpClient = new HttpClient();

            int totalNewCandles = 0;
            int requestCount = 0;
            CandleRow? lastCandle = null;
            int currentYear = 0;
            StreamWriter? writer = null;

            try
            {
                long startMs = ToUnixMs(startDate);
                long endMs = ToUnixMs(endDate);
                long currentMs = startMs;

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
                            var row = new CandleRow(candle);
                            int rowYear = DateTime.Parse(row.OpenTime).Year;

                            // Roll to a new file when the year changes
                            if (rowYear != currentYear)
                            {
                                if (writer is not null)
                                {
                                    await writer.FlushAsync();
                                    writer.Dispose();
                                }

                                currentYear = rowYear;
                                var csvPath = CsvFileForYear(currentYear);
                                bool append = File.Exists(csvPath);
                                writer = new StreamWriter(csvPath, append: append);

                                if (!append)
                                    await writer.WriteLineAsync(CsvHeader);

                                Console.WriteLine($"Writing to {Path.GetFileName(csvPath)}");
                            }

                            await writer!.WriteLineAsync(row.ToCsvLine());
                            lastCandle = row;
                        }

                        totalNewCandles += candles.Length;
                        requestCount++;

                        long lastOpenTime = candles[^1][0].GetInt64();
                        currentMs = lastOpenTime + 60_000;

                        SaveCheckpoint(currentMs);

                        if (requestCount % 100 == 0)
                        {
                            await writer!.FlushAsync();
                            Console.WriteLine($"[{requestCount} requests] Up to {FromUnixMs(currentMs):yyyy-MM-dd HH:mm} | {totalNewCandles:N0} candles written...");
                        }

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
            }
            finally
            {
                // Always dispose the writer cleanly, even if we threw
                if (writer is not null)
                {
                    await writer.FlushAsync();
                    writer.Dispose();
                }
            }

            // Writer is fully disposed — safe to read files for stats
            long totalLines = CountAllCsvLines();
            CandleRow? firstCandle = ReadFirstDataCandle(CsvFileForYear(startDate.Year));

            await WriteStatsAsync(
                runAt: endDate,
                newCandles: totalNewCandles,
                totalCandles: totalLines,
                first: firstCandle,
                last: lastCandle,
                rangeStart: startDate,
                rangeEnd: endDate
            );

            Console.WriteLine("-------------------------------------------");
            Console.WriteLine($"Done! {totalNewCandles:N0} new candles. {totalLines:N0} total in dataset.");
        }

        // Sum lines across all yearly CSV files
        static long CountAllCsvLines()
        {
            long total = 0;
            foreach (var file in Directory.GetFiles(DataDir, $"{Symbol}_{Interval}_*.csv"))
                total += CountCsvLines(file);
            return total;
        }

        static long CountCsvLines(string path)
        {
            if (!File.Exists(path)) return 0;
            long count = 0;
            using var reader = new StreamReader(path);
            while (reader.ReadLine() != null) count++;
            return Math.Max(0, count - 1); // subtract header
        }

        static CandleRow? ReadFirstDataCandle(string path)
        {
            if (!File.Exists(path)) return null;
            using var reader = new StreamReader(path);
            reader.ReadLine(); // skip header
            var line = reader.ReadLine();
            return line is null ? null : CandleRow.FromCsvLine(line);
        }

        static async Task WriteStatsAsync(DateTime runAt, int newCandles, long totalCandles,
            CandleRow? first, CandleRow? last, DateTime rangeStart, DateTime rangeEnd)
        {
            var stats = new
            {
                last_run_utc = runAt.ToString("yyyy-MM-dd HH:mm:ss"),
                status = "success",
                new_candles = newCandles,
                total_candles = totalCandles,
                range_start = rangeStart.ToString("yyyy-MM-dd HH:mm:ss"),
                range_end = rangeEnd.ToString("yyyy-MM-dd HH:mm:ss"),
                first_candle = first is null ? null : new { first.OpenTime, first.Open, first.High, first.Low, first.Close },
                last_candle = last is null ? null : new { last.OpenTime, last.Open, last.High, last.Low, last.Close }
            };

            var json = JsonSerializer.Serialize(stats, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(StatsFile, json);
            Console.WriteLine($"Stats written to {Path.GetFullPath(StatsFile)}");
        }

        static DateTime? LoadCheckpoint()
        {
            if (!File.Exists(CheckpointFile)) return null;
            var text = File.ReadAllText(CheckpointFile).Trim();
            if (long.TryParse(text, out long ms)) return FromUnixMs(ms);
            return null;
        }

        static void SaveCheckpoint(long unixMs) =>
            File.WriteAllText(CheckpointFile, unixMs.ToString());

        static long ToUnixMs(DateTime dt) =>
            (long)(dt.ToUniversalTime() - UnixStartDateTime).TotalMilliseconds;

        static DateTime FromUnixMs(long ms) =>
            UnixStartDateTime.AddMilliseconds(ms);
    }

    record CandleRow(string OpenTime, string Open, string High, string Low, string Close,
                     string Volume, string CloseTime, string QuoteVol, int TradeCount,
                     string TakerBuyBase, string TakerBuyQuote)
    {
        public CandleRow(JsonElement[] c) : this(
            OpenTime: FromUnixMs(c[0].GetInt64()),
            Open: c[1].GetString()!,
            High: c[2].GetString()!,
            Low: c[3].GetString()!,
            Close: c[4].GetString()!,
            Volume: c[5].GetString()!,
            CloseTime: FromUnixMs(c[6].GetInt64()),
            QuoteVol: c[7].GetString()!,
            TradeCount: c[8].GetInt32(),
            TakerBuyBase: c[9].GetString()!,
            TakerBuyQuote: c[10].GetString()!
        )
        { }

        static string FromUnixMs(long ms) =>
            new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                .AddMilliseconds(ms)
                .ToString("yyyy-MM-dd HH:mm:ss");

        public string ToCsvLine() =>
            $"{OpenTime},{Open},{High},{Low},{Close},{Volume},{CloseTime},{QuoteVol},{TradeCount},{TakerBuyBase},{TakerBuyQuote}";

        public static CandleRow? FromCsvLine(string line)
        {
            var p = line.Split(',');
            if (p.Length < 11) return null;
            return new CandleRow(p[0], p[1], p[2], p[3], p[4], p[5], p[6], p[7],
                int.TryParse(p[8], out int t) ? t : 0, p[9], p[10]);
        }
    }
}