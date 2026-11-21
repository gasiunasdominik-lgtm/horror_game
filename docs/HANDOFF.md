# HANDOFF

Přepisuje `/end`. Krátké. Historie je v gitu.

## Stav
M1 hotový (21. 11. 2026): tag `m1` na `main`. T-020 až T-027 a T-030 DONE, 88 EditMode testů zelených. Výkon na Zenbooku 1080p: 134 FPS (1 % low 81). `main` a `m1` pushnuty.

## Hotovo v této session
- T-020–T-023: hasič (stavy, smysly, hledání skrýší), zvuk, skrýše skříň/stůl, okna s čerstvým vzduchem, víc spínačů.
- T-024: 3 patra, dvouramenné schodiště, výměna patra na mezipodestě, hasič následuje. Výtah mimo provoz, vchod v přízemí.
- T-025: sousedka s dortem (zkratka, nebo past u zdroje CO).
- T-026: konce 1/2/3, obrazovka konce (Enter nový dům, R stejný dům), F10 náhled, `round_end` v logu.
- T-027: kolo začíná ze tmy s větou o cíli, restart se stejným/novým seedem.
- T-030: SSAO a nepoužívané kopie textur vypnuty (D-048), 50 → 134 FPS.
- Rozhodnutí D-032 až D-048.

## Rozpracováno
- Větev `feat/t031-pacing` (nemergnuto): po FEEL #2 hotové T-031 (90 s klid, příchod ≥ 10 m), T-032 (hasič prochází patra), T-033 (skrýše 20 %), T-035 (titulky + hlas). Čeká T-034 (druhá cesta mezi patry, otázka níže). 97 testů zelených.

## Další krok
1. Dominik: playtest #2 (T-028) a FEEL hodnocení, brána G1.
2. Dominik: T-029 video.
3. Claude: po playtestu zpracovat nálezy, pak M2.

## Otevřené otázky (s výchozími odpověďmi)
- Q: T-034 druhá cesta mezi patry? Výchozí: požární dveře na konci chodby na každém patře (dnes je má jen byt sousedky), vedou na nouzové schodiště o patro výš/níž.
- Q: Hasič startuje v nejhlubší buňce patra hráče. Když hráč hned rozsvítí, může ho chytit do ~10 s. Výchozí: nechat a rozhodnout podle playtestu #2.
- Q: Aktualizovat Unity CLI? Výchozí: ano, při příští session.

## Poznámky
- Scéna: `Assets/_Project/Scenes/Game.unity`. Zapojení generuje `GameSceneBuilder.BuildAll()` (mimo Play Mode).
- Ladění bez kódu: `Assets/_Project/Data/`.
- Benchmark: `Builds/Dev/VchodC.exe -seed 4242 -perf 30 -screen-width 1920 -screen-height 1080 -screen-fullscreen 1`. Před měřením ověř, že stroj není přiškrcený (CPU výkon > 100 %).
- Profiler přes CLI: `diagnostics profiler_record` → `profiler_session` → `profiler_summary`/`profiler_markers`. Záznamy v `Profiling/` (ignorováno gitem).
- Prostředí: Claude ve WSL, Unity CLI = `unity.exe`, cesty `C:\...`.
