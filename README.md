# BTC Price History

![Status](https://img.shields.io/badge/last%20run-success-brightgreen)

Automated 1-minute OHLCV data for **BTCUSD** pulled from Binance.US, updated nightly via GitHub Actions.

## Last Run

| Field             | Value                              |
|-------------------|------------------------------------|
| Time              | 2026-05-28 03:37:42 UTC                   |
| Status            | success                         |
| New candles       | 1,426                    |
| Total candles     | 2,673,059                  |
| Pull range        | 2026-05-27 03:52:00 → 2026-05-28 03:37:42    |

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