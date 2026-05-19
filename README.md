# BTC Price History

![Status](https://img.shields.io/badge/last%20run-pending-lightgrey)

Automated 1-minute OHLCV data for **BTCUSD** pulled from Binance.US, updated nightly via GitHub Actions.

## Last Run

*No runs yet — badge and table will populate after the first workflow execution.*

## Dataset Coverage

*Will appear after first run.*

## Files

| File | Description |
|------|-------------|
| `data/BTCUSD_1m.csv` | Full 1-minute OHLCV history |
| `data/last_timestamp.txt` | Checkpoint — last pulled Unix ms |
| `data/stats.json` | Raw stats from the last run |

## CSV Schema

```
OpenTime, Open, High, Low, Close, Volume, CloseTime,
QuoteVolume, TradeCount, TakerBuyBaseVolume, TakerBuyQuoteVolume
```

---
*Updated automatically by [GitHub Actions](.github/workflows/update-data.yml)*