# HANDOFF

Přepisuje `/end`. Krátké. Historie je v gitu.

## Stav
M1 obsahově hotový na větvi `feat/t020-firefighter` (T-020 až T-027 DONE, 88 EditMode testů zelených). Tag `m1` čeká na platné měření výkonu (T-030). Nic nepushnuto.

## Hotovo v této session
- T-020–T-023: hasič (stavy, smysly, hledání skrýší), zvuk, skrýše skříň/stůl, okna s čerstvým vzduchem, víc spínačů.
- T-024: 3 patra, dvouramenné schodiště, výměna patra na mezipodestě, hasič následuje. Výtah mimo provoz, vchod v přízemí.
- T-025: sousedka s dortem (zkratka, nebo past u zdroje CO).
- T-026: konce 1/2/3, obrazovka konce (Enter nový dům, R stejný dům), F10 náhled, `round_end` v logu.
- T-027: kolo začíná ze tmy s větou o cíli, restart se stejným/novým seedem ověřen.
- Rozhodnutí D-032 až D-047.

## Rozpracováno
- T-030: platné měření 50–56 FPS (1 % low 28–32), cíl 60/45 nesplněn. Tag `m1` až po optimalizaci.

## Další krok
1. Dominik: přepnout Zenbook do režimu výkonu (MyASUS / Windows napájení), restart, pak `! powershell.exe -c "Start-Process 'Builds\Dev\VchodC.exe' -ArgumentList '-seed 4242 -perf 30 -screen-width 1920 -screen-height 1080 -screen-fullscreen 1' -Wait"`, nebo dej vědět a změřím já.
2. Claude: po platném měření tag `m1`, merge `feat/t020-firefighter` → `main`.
3. Dominik: playtest #2 (T-028).

## Otevřené otázky (s výchozími odpověďmi)
- Q: Aktualizovat Unity CLI? Výchozí: ano, při příští session.

## Poznámky
- Scéna: `Assets/_Project/Scenes/Game.unity`. Zapojení generuje `GameSceneBuilder.BuildAll()` (mimo Play Mode).
- Ladění bez kódu: `Assets/_Project/Data/`.
- Profiler přes CLI: `diagnostics profiler_record` → `profiler_session` → `profiler_summary`/`profiler_markers`. Záznamy v `Profiling/` (ignorováno gitem).
- Prostředí: Claude ve WSL, Unity CLI = `unity.exe`, cesty `C:\...`.
