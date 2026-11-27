# HANDOFF

Přepisuje `/end`. Krátké. Historie je v gitu.

## Stav
M2 rozpracované (27. 11. 2026): G1 prošla (D-055). Hotovo T-039 (světlo jako kompromis), T-040 (sousedé 2–3), T-041 (18 kousků příběhu), T-044 (menu, pauza, nastavení). 118 EditMode testů zelených. Výkon na baterii 198–218 FPS. `main` pushnutý.

## Hotovo v této session
- T-020–T-023: hasič (stavy, smysly, hledání skrýší), zvuk, skrýše skříň/stůl, okna s čerstvým vzduchem, víc spínačů.
- T-024: 3 patra, dvouramenné schodiště, výměna patra na mezipodestě, hasič následuje. Výtah mimo provoz, vchod v přízemí.
- T-025: sousedka s dortem (zkratka, nebo past u zdroje CO).
- T-026: konce 1/2/3, obrazovka konce (Enter nový dům, R stejný dům), F10 náhled, `round_end` v logu.
- T-027: kolo začíná ze tmy s větou o cíli, restart se stejným/novým seedem.
- T-030: SSAO a nepoužívané kopie textur vypnuty (D-048), 50 → 134 FPS.
- Rozhodnutí D-032 až D-048.

## Rozpracováno
- nic

## Další krok
1. Dominik: zahrát přes menu (build `Builds/Dev/VchodC.exe` nebo scéna Menu v Editoru), FEEL.
2. Claude: T-042 art pass (procedurálně, bez cizích assetů), pak T-043 audio pass (potřebuje souhlas s balíčky a licencemi).
3. Rozhodnout skrytý konec (T-045).

## Otevřené otázky (s výchozími odpověďmi)
- Q: Hasič startuje v nejhlubší buňce patra hráče. Když hráč hned rozsvítí, může ho chytit do ~10 s. Výchozí: nechat a rozhodnout podle playtestu #2.
- Q: Aktualizovat Unity CLI? Výchozí: ano, při příští session.

## Poznámky
- Scéna: `Assets/_Project/Scenes/Game.unity`. Zapojení generuje `GameSceneBuilder.BuildAll()` (mimo Play Mode).
- Ladění bez kódu: `Assets/_Project/Data/`.
- Benchmark: `Builds/Dev/VchodC.exe -seed 4242 -perf 30 -screen-width 1920 -screen-height 1080 -screen-fullscreen 1`. Před měřením ověř, že stroj není přiškrcený (CPU výkon > 100 %).
- Profiler přes CLI: `diagnostics profiler_record` → `profiler_session` → `profiler_summary`/`profiler_markers`. Záznamy v `Profiling/` (ignorováno gitem).
- Prostředí: Claude ve WSL, Unity CLI = `unity.exe`, cesty `C:\...`.
