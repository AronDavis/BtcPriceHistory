# BTC Price History

![Status](https://img.shields.io/badge/last%20run-success-brightgreen)

Automated 1-minute OHLCV data for **BTCUSD** pulled from Binance.US, updated nightly via GitHub Actions.

## Last Run

| Field             | Value                              |
|-------------------|------------------------------------|
| 🕐 Run time       | 2026-05-19 22:36:25 UTC                   |
| ✅ Status         | success                         |
| 🕯️ New candles   | 4                    |
| 📦 Total candles  | 2,661,238                  |
| 📅 Pull range     | 2026-05-19 22:33:00 → 2026-05-19 22:36:25    |

## Dataset Coverage

| | OpenTime | Open | High | Low | Close |
|---|---|---|---|---|---|
| **First candle** | 2026-01-01 00:00:00 | 87240.56000000 | 87240.56000000 | 87240.56000000 | 87240.56000000 |
| **Last candle**  | 2026-05-19 22:36:00  | 76720.38000000  | 76720.38000000  | 76692.20000000  | 76692.20000000  |

## Files

| File | Description |
|------|-------------|
| `data/BTCUSD_1m_YEAR.csv` | 1-minute OHLCV data for the year in the file name|
| `data/last_timestamp.txt` | Checkpoint — last pulled Unix ms |
| `data/stats.json` | Raw stats from the last run |

## CSV Schema

```
OpenTime, Open, High, Low, Close, Volume, CloseTime,
QuoteVolume, TradeCount, TakerBuyBaseVolume, TakerBuyQuoteVolume
```

---
*Updated automatically by [GitHub Actions](.github/workflows/update-data.yml)*