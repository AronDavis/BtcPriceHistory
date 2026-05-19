# BTC Price History

{{STATUS_BADGE}}

Automated 1-minute OHLCV data for **BTCUSD** pulled from Binance.US, updated nightly via GitHub Actions.

## Last Run

| Field             | Value                              |
|-------------------|------------------------------------|
| 🕐 Run time       | {{LAST_RUN}} UTC                   |
| ✅ Status         | {{STATUS}}                         |
| 🕯️ New candles   | {{NEW_CANDLES}}                    |
| 📦 Total candles  | {{TOTAL_CANDLES}}                  |
| 📅 Pull range     | {{RANGE_START}} → {{RANGE_END}}    |

## Dataset Coverage

| | OpenTime | Open | High | Low | Close |
|---|---|---|---|---|---|
| **First candle** | {{FIRST_TIME}} | {{FIRST_OPEN}} | {{FIRST_HIGH}} | {{FIRST_LOW}} | {{FIRST_CLOSE}} |
| **Last candle**  | {{LAST_TIME}}  | {{LAST_OPEN}}  | {{LAST_HIGH}}  | {{LAST_LOW}}  | {{LAST_CLOSE}}  |

## Files

| File | Description |
|------|-------------|
| `data/BTCUSD_1m_2019.csv` | 1-minute OHLCV — 2019 |
| `data/BTCUSD_1m_2020.csv` | 1-minute OHLCV — 2020 |
| `data/BTCUSD_1m_2021.csv` | 1-minute OHLCV — 2021 |
| `data/BTCUSD_1m_2022.csv` | 1-minute OHLCV — 2022 |
| `data/BTCUSD_1m_2023.csv` | 1-minute OHLCV — 2023 |
| `data/BTCUSD_1m_2024.csv` | 1-minute OHLCV — 2024 |
| `data/BTCUSD_1m_2025.csv` | 1-minute OHLCV — 2025 (appended nightly) |
| `data/last_timestamp.txt` | Checkpoint — last pulled Unix ms |
| `data/stats.json` | Raw stats from the last run |

## CSV Schema

```
OpenTime, Open, High, Low, Close, Volume, CloseTime,
QuoteVolume, TradeCount, TakerBuyBaseVolume, TakerBuyQuoteVolume
```

---
*Updated automatically by [GitHub Actions](.github/workflows/update-data.yml)*