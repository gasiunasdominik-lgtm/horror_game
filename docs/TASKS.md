# TASKS

Stavy: `TODO` · `DOING` · `BLOCKED` · `DONE`. Vlastník: **C** = Claude, **D** = Dominik. Priorita: P1 nejvyšší. Claude aktualizuje po každém úkolu.
Max 3 aktivní cíle týdně. Nápady se neimplementují, dokud nejsou přeřazeny do milníku.

## M0 Základ (23. 11. – 29. 11. 2026)

| ID | Úkol | Vl. | Stav | P | Hotovo, když |
|---|---|---|---|---|---|
| T-001 | Nainstalovat Claude Code, mít Unity ID | D | DONE | P1 | Claude Code běží, Unity ID existuje |
| T-002 | (Volitelně) soukromý GitHub repozitář pro zálohu | D | DONE | P3 | Repo existuje, Claude ví URL |
| T-003 | Založit Unity projekt, git, přenést CLAUDE.md, docs, .claude (START_PROMPT Část A) | C | DONE | P1 | Projekt se otevře, první commit a tag `m0-start` |
| T-004 | Baseline URP (Volume, kamera, kvalita, UI stack), ověření screenshotem | C | DONE | P1 | Screenshot osvětlený, bez chyb v konzoli |
| T-005 | Debug a QA infrastruktura: SeedService, DebugOverlay (F1), BugReporter (F8), PlaytestLogger, cheaty F2–F10 | C | DONE | P1 | F8 vytvoří kompletní složku v `BugReports/`, F1 ukáže seed a CO, `-seed N` funguje |
| T-006 | FPS controller + kamera + vstup | C | DONE | P1 | Postava se pohybuje, rozhlíží, skáče/neskáče dle GDD |
| T-007 | Greybox chodba z modulů + schodišťové světlo na časovač | C | TODO | P1 | Tlačítko rozsvítí na ~20 s, světlo zhasne |
| T-008 | ModuleLayout: přeskládání modulů při zhasnutí podle seedu, validace cesty | C | TODO | P1 | Po zhasnutí je dům jiný, stejný seed = stejné rozložení, 200 seedů projde sweepem |
| T-009 | CoModel + SymptomEffects + pípání detektoru podle vzdálenosti ke zdroji | C | TODO | P1 | Hladina CO roste u zdroje, efekty a zvuk se mění podle prahů |
| T-010 | Playtest #1 (10 min) + FEEL hodnocení | D | TODO | P1 | `FEEL:` odpověď v chatu |
| T-011 | EditMode testy: CoModel, SeedService, validace ModuleLayout | C | TODO | P2 | Testy zelené |
| T-012 | Zapsat měření výkonu na Zenbooku do TESTING.md | C | TODO | P2 | Hodnoty FPS zapsané |

## M1 Smyčka (30. 11. – 20. 12. 2026)

| ID | Úkol | Vl. | Stav | P | Hotovo, když |
|---|---|---|---|---|---|
| T-020 | Hasič: model (placeholder), NavMesh, stavový automat (Patrol, Investigate, Chase, Call, Search) | C | TODO | P1 | Hasič hlídá, reaguje na světlo a hluk, honí, ztrácí hráče |
| T-021 | Senzory hasiče (sluch, zrak, světlo) + audio (dech, výstroj, baterka) | C | TODO | P1 | Hráč pozná polohu podle zvuku |
| T-022 | Skrýše (skříň, stůl) a interakce | C | TODO | P1 | Hráč se schová a hasič ho přejde nebo najde |
| T-023 | Okna s čerstvým vzduchem a pokles CO | C | TODO | P1 | U okna CO klesá |
| T-024 | 3 patra z kostek, výtah, schodiště, dveře pokoje | C | TODO | P1 | Dá se dojít od vchodu ke dveřím |
| T-025 | Soused 1 (paní s dortem) jako vzor: dveře, dialog, zkratka nebo past | C | TODO | P1 | Soused reaguje, nabízí zkratku, past je čitelná |
| T-026 | EndingsManager: konec 1 (zhroucení), 2 (evakuace), 3 (čerstvý vzduch) v hrubé verzi | C | TODO | P1 | Všechny tři konce dosažitelné |
| T-027 | Smyčka kola: start, konec, restart se stejným/novým seedem | C | TODO | P1 | Po konci lze začít nové kolo |
| T-028 | Playtest #2 + FEEL, brána G1 | D | TODO | P1 | `FEEL:` odpověď a rozhodnutí G1 |
| T-029 | První krátké video z vývoje | D | TODO | P2 | Video zveřejněné nebo připravené |
| T-030 | Měření výkonu, tag `m1` | C | TODO | P2 | Tag existuje |

## M2 Obsah (21. 12. 2026 – 31. 1. 2027)

| ID | Úkol | Vl. | Stav | P | Hotovo, když |
|---|---|---|---|---|---|
| T-040 | Sousedé 2–3 (pán s rádiem, hádka za dveřmi) | C | TODO | P1 | Funkční, odlišné chování |
| T-041 | Kousky příběhu (15–20) a jejich rozmístění podle seedu | C | TODO | P1 | Sbíratelné, odhalují příběh při opakování |
| T-042 | Low-poly art pass (moduly, rekvizity), jednotný retro filtr | C | TODO | P1 | Hra vypadá jednotně, drží výkon |
| T-043 | Audio pass (ambient, detektor, dech, hlasy sousedů ze zkreslených zvuků) | C | TODO | P1 | Zvuk nese atmosféru |
| T-044 | Menu, pauza, nastavení, uložení nastavení | C | TODO | P2 | Hra se spustí z menu |
| T-045 | Skrytý konec (rozhodnutí v DECISIONS) | C | TODO | P3 | Dosažitelný po sebrání kousků |
| T-046 | Cizí playtest se 3 lidmi, G2 | D | TODO | P1 | 3 FEEL záznamy, rozhodnutí G2 |
| T-047 | Měření výkonu, tag `m2` | C | TODO | P2 | Tag existuje |

## M3 Vertikální řez (1. 2. – 28. 2. 2027)

| ID | Úkol | Vl. | Stav | P | Hotovo, když |
|---|---|---|---|---|---|
| T-060 | Leštění řezu (20 min), ladění CO a hasiče, tutoriální signály | C | TODO | P1 | Cizí hráči se neztrácí |
| T-061 | Steam stránka (text, screenshoty, capsule) | D | TODO | P1 | Stránka připravená k publikaci |
| T-062 | Trailer | D | TODO | P2 | Trailer hotový |
| T-063 | Steam Direct 100 $ (ověř částku), konfigurace aplikace | D | TODO | P2 | Aplikace založená |
| T-064 | Kontrola licencí assetů (ASSETS.md) | C | TODO | P1 | Všechny assety mají licenci |
| T-065 | Rozhodnutí G3 | D | TODO | P1 | Zapsáno v DECISIONS |
| T-066 | Tag `m3` | C | TODO | P2 | Tag existuje |

## Nápady (neimplementovat; Dominik/Claude zde zapisují)
- (zatím prázdné)

## Blokace
- (zatím prázdné)
