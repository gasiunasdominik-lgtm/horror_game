# HANDOFF

Přepisuje `/end`. Krátké. Historie je v gitu.

## Stav
M0 technicky hotový (21. 11. 2026), tag `m0`. Všechny úkoly C (T-003 až T-009, T-011, T-012) jsou DONE. Zbývá T-010: Dominikův playtest + `FEEL:`.

## Hotovo v této session
- T-005: `Vchod.Debug` (jen Editor + Development build): F1 overlay (3 režimy, ve 3. zadání seedu), F2 god mode, F3 teleport (schody → okno → zdroj CO → dveře), F4 přeskládání / Shift+F4 nový seed, F6 time scale, F7 mlha+post, F8 bug report (`BugReports/<čas>/` se screenshot, log, state, note), F9 restart stejný seed. F5 a F10 hlásí „až M1“. `PlaytestLogger` → `PlaytestLogs/session_*.csv`. `-seed N` funguje (ověřeno v buildu).
- T-006: FPS controller (CharacterController, nový Input System v kódu, klávesnice+myš i gamepad), bez skoku, sprint Shift. Interakce E + výzva a zaměřovač (uGUI).
- T-007: greybox moduly generované z kódu (menu `Vchod/Rebuild Greybox Modules`), panelákové dvoubarevné zdi, světlo na spínač 20 s.
- T-008: `LayoutGenerator` + `LayoutValidator`, přeskládání při zhasnutí, hráč zůstává ve stejné hloubce chodby. Sweep 200 seedů × 5 přeskládání zelený.
- T-009: `CoModel`, `SymptomEffects` (prahy 20/40/60/80/100 dle GDD), pípání detektoru podle vzdálenosti (3D zvuk ze zdroje).
- T-011: 49 EditMode testů zelených.
- T-012: Zenbook, 1080p: 66 FPS (studený start) / 98 FPS (teplý), viz `TESTING.md`.
- Rozhodnutí D-017 až D-026 v `DECISIONS.md`.

## Rozpracováno
- nic

## Další krok
1. Dominik: T-010 playtest (10 min) a `FEEL:`.
2. Claude: M1 od T-020 (hasič), podle výsledků playtestu doladit tmu, CO tempo a počet spínačů.

## Dominik: otestuj
Viz seznam „Otestuj“ v poslední zprávě session (5 bodů).

## Otevřené otázky (s výchozími odpověďmi)
- Q: Aktualizovat Unity CLI 1.0.0 → 1.0.1? Výchozí: ano, při příští session (`unity self-update`).
- Q: Merge `feat/m0` do `main` je lokální; push? Výchozí: nepushovat, dokud to neřekneš.

## Poznámky
- Scéna: `Assets/_Project/Scenes/Game.unity`. Zapojení scény se generuje z `Vchod/Rebuild Game Scene Wiring` (spouštět mimo Play Mode). Ruční úpravy scény se při dalším spuštění přepíšou jen u objektů, které builder spravuje.
- Ladění bez kódu: `Assets/_Project/Data/` (`PlayerConfig`, `LightTimerConfig`, `HouseConfig`, `CoConfig`).
- Benchmark: `Builds/Dev/VchodC.exe -seed 4242 -perf 30 -screen-width 1920 -screen-height 1080 -screen-fullscreen 1`.
- Testování přes CLI: simulované klávesy se do hry nedostanou, když Game view nemá fokus; logika cheatů ověřena přímým voláním. Skutečné klávesy F1–F10 potvrdí playtest.
- Prostředí: Claude ve WSL, Unity CLI = `unity.exe`, cesty `C:\...`.
