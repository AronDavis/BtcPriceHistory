# BTC Price History

![Status](https://img.shields.io/badge/last%20run-success-brightgreen)

Automated 1-minute OHLCV data for **BTCUSD** pulled from Binance.US, updated nightly via GitHub Actions.

## Last Run

| Field             | Value                              |
|-------------------|------------------------------------|
| Time              | 2026-11-18 06:07:32 UTC                   |
| Status            | success                         |
| New candles       | 1,398                    |
| Total candles     | 2,923,226                  |
| Pull range        | 2026-11-17 06:50:00 → 2026-11-18 06:07:32    |

## Data Files

| File | Description |
|------|-------------|
| `BTCUSD_1m_{YEAR}.csv` | 1-minute OHLCV data for `{YEAR}` |
| `last_timestamp.txt` | Checkpoint — last pulled Unix ms |
| `stats.json` | Raw stats from the last run |

## CSV Schema

```
OpenTime, Open, High, Low, Close, Volume, CloseTime,
QuoteVolume, TradeCount, TakerBuyBaseVolume, TakerBuyQuoteVolume
```

---
*Updated automatically by [GitHub Actions](.github/workflows/update-data.yml)*