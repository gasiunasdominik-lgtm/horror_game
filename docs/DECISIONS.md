# DECISIONS

Zapisuj každé rozhodnutí, které ovlivní hru, nástroje nebo rozsah. Formát: ID, datum, rozhodnutí, důvod, kdo rozhodl (D, C, nebo `default` = výchozí odpověď bez reakce).

| ID | Datum | Rozhodnutí | Důvod | Kdo |
|---|---|---|---|---|
| D-001 | 19. 11. 2026 | Engine Unity (nejnovější LTS), URP | Slabší hardware (Zenbook), C# a Claude spolupráce, nižší vstupní náročnost než Unreal | D |
| D-002 | 19. 11. 2026 | Low-poly / retro vzhled, 1. osoba, PC (Steam) | Výkon a zvládnutelná grafika solo | D |
| D-003 | 19. 11. 2026 | Žánr: krátký horor s pronásledovatelem, jedna lokace (panelák) | Rozsah a znovuhratelnost | D |
| D-004 | 19. 11. 2026 | Příčina divností je fyzická (otrava CO), ne duševní nemoc | Logika, stigmatizace, férovost | D |
| D-005 | 19. 11. 2026 | Pronásledovatel je hasič v dýchacím přístroji (vnímán jako stalker) | Logika odhalení a emoce (strach → úleva) | D |
| D-006 | 19. 11. 2026 | Dům se přeskládá ve tmě (zhasnutí schodišťového světla) | Technické skrytí přechodu a tematická vazba | D |
| D-007 | 19. 11. 2026 | Jazyk komunikace česky, kód a commity anglicky | Efektivita a čitelnost | D |
| D-008 | 19. 11. 2026 | Rozsah: vertikální řez do 28. 2. 2027, brány G1–G3 | Zabránit rozjetí rozsahu | D |
| D-009 | 19. 11. 2026 | Při chybějící odpovědi platí výchozí volba a zapíše se sem | Minimalizace přerušení | D |
| D-010 | | Skrytý konec: obsah a podmínka | Rozhodnout v M2 | otevřeno |
| D-011 | | Definitivní podoba bytu 0 | Rozhodnout v M1/M2 | otevřeno |
| D-012 | 20. 11. 2026 | Unity Editor 6000.3.29f1 (nejnovější LTS k datu založení), šablona Universal 3D (`com.unity.template.urp-blank`) | Zadání: nejnovější LTS; nainstalovaná 6000.6.9f1 je tech stream bez LTS podpory | C |
| D-013 | 20. 11. 2026 | Projekt bez Unity Cloud (`--no-cloud`) | Cloud link při vytváření selhal (`NETWORK_UNREACHABLE`); hra nepotřebuje online služby, záloha přes GitHub | C |
| D-014 | 20. 11. 2026 | Herní UI: uGUI (Canvas + TextMeshPro), EventSystem s `InputSystemUIInputModule` | Výchozí volba skillu `ui` pro nové herní UI; vyzrálé, jednoduché pro menu a HUD | default |
| D-015 | 20. 11. 2026 | Baseline post-processing: ACES, Bloom 0.9/0.6, Vignette 0.3, kontrast +12, saturace −15, Film Grain 0.2 | Tmavší retro tón dle GDD, ale čitelný; hororový tuning až v T-009/M2 | C |
| D-016 | 20. 11. 2026 | Remote `origin` = github.com/gasiunasdominik-lgtm/horror_game, push povolen Dominikem | Záloha | D |
| D-017 | 20. 11. 2026 | Bez skoku; chůze 2,0 m/s, sprint (Shift) 3,6 m/s jen dopředu | GDD skok nezmiňuje; útěk před hasičem potřebuje rychlejší pohyb. Ladí se v `PlayerConfig` | C |
| D-018 | 20. 11. 2026 | Patro = strom chodeb na mřížce 4 m: schodiště (0,0) → hlavní trasa 5–7 buněk → dveře hráče, + 2 boční větve (okno na konci větve), 2+ dveře bytů, jeden byt je zdroj CO | Splňuje GDD (4–6 modulů, přeskládání), validace je jednoduchá a testovatelná | C |
| D-019 | 20. 11. 2026 | Přeskládání při zhasnutí: další rozložení stejného seedu (`seed, "layout", index`); hráč se přesune do buňky ve stejné hloubce chodby, stejná relativní poloha a směr; ve schodišti se nic nemění | Ve tmě působí jako dezorientace (GDD: závrať), hráč nikdy neskončí ve zdi | C |
| D-020 | 20. 11. 2026 | Jediný spínač světla u schodiště, světlo 20 s, svítí celé patro | GDD: tlačítko u schodů; víc spínačů jen podle playtestu (Nápady) | C |
| D-021 | 20. 11. 2026 | CO: start 5, chodba +0,08/s, schodiště −0,3/s, okno −4/s, u zdroje až +2,5/s (≈40 s do kolapsu); detektor = 3D zvuk bytu se zdrojem, interval 0,3–2,2 s podle vzdálenosti | Výchozí ladění; kolo 15–25 min vydrží jen bez zbytečných zastávek u zdroje. Ladí se v `CoConfig` | C |
| D-022 | 20. 11. 2026 | Kolaps (CO 100) v M0 jen zatmí obraz a zablokuje ovládání; konec 1 řeší T-026 | EndingsManager je M1 | C |
| D-023 | 20. 11. 2026 | Debug nástroje se vytvářejí kódem (`[RuntimeInitializeOnLoadMethod]`), namespace `Vchod.DebugTools` (assembly `Vchod.Debug`) | Release build nemá chybějící skripty ve scéně; jméno `Debug` by kolidovalo s `UnityEngine.Debug` | C |
| D-024 | 20. 11. 2026 | Zvuky v M0 syntetizované kódem (pípání, hučení) | Žádné cizí assety ani licence; nahradí audio pass M2 | C |
| D-025 | 20. 11. 2026 | `SampleScene` přesunuta na `Assets/_Project/Scenes/Game.unity`, jediná scéna v buildu; zapojení scény generuje menu `Vchod/Rebuild Game Scene Wiring` | ARCHITECTURE: scéna `Game`; zapojení opakovatelné z kódu | C |
| D-026 | 20. 11. 2026 | Výzva k interakci v češtině („E  Rozsvítit“) jako placeholder | Lokalizace CZ/EN se řeší později (GDD) | C |
| D-027 | 21. 11. 2026 | CO na chodbě +0,08 → +0,04/s (100 za ~40 min), tlak dělá blízkost zdroje | FEEL playtest #1: růst daleko od zdroje působí nefér (pilíř 3). GDD „pomalu na chodbě“ zůstává | C |
| D-028 | 21. 11. 2026 | Práh 60 přidává kymácení hlavy (náklon ±4°) a „dýchání“ FOV (±6°) k zkreslení čočky | FEEL: obraz při 60 nepůsobil jako nevolnost | C |
| D-029 | 21. 11. 2026 | Svítící prvky (spínač, žárovky, okna) jsou Unlit materiály | Nezávislé na osvětlení a na klíčových slovech URP Lit (B-001) | C |
| D-030 | 21. 11. 2026 | Víc spínačů světla (každá 2.–3. buňka chodby) jako součást T-024 v M1 | Nápad z playtestu #1, Dominik potvrdil výchozí odpověď (OK) | D |
| D-031 | 21. 11. 2026 | `main` a tag `m0` pushnuty na GitHub | Výslovný pokyn Dominika | D |
| D-032 | 21. 11. 2026 | Hasič: hlídka 1,2 m/s, honička 3,0 m/s (chůze hráče 2,0, sprint 3,6). Vidí 14 m při světle, 6 m ve tmě, zorné pole 110°, do 2 m cítí hráče kolem dokola. Rozsvícení prozradí polohu spínače, kroky slyší do 4 m (sprint 12 m) | Útěk je možný jen sprintem, který je hlučný: rozhodnutí hráče má cenu (pilíř 3). Ladí se v `FirefighterConfig` / `PlayerConfig` | C |
| D-033 | 21. 11. 2026 | Chycení v M1 zatím jen zablokuje ovládání a pošle `PlayerCaught`; konec 2 (evakuace) napojí T-026 | Konce jsou samostatný úkol | C |
| D-034 | 21. 11. 2026 | NavMesh se peče za běhu po každém přeskládání (`NavMeshSurface` na `House`, kolidery). Hasič zůstává na místě, pokud je na novém patře, jinak se objeví v nejvzdálenější buňce; ve tmě po přeskládání přejde do hledání | Moduly se mění, pevný NavMesh nejde | C |
| D-035 | 21. 11. 2026 | Ve tmě hasič vidí jen kuželem baterky (9 m, 50°), baterka je na hrudi a paprsek dopadá na podlahu; při světle vidí očima (14 m, 110°) | Hráč čte jeho pohled z paprsku, tma je úkryt, rozsvícení riziko (pilíř 2) | C |
| D-036 | 21. 11. 2026 | Zvuky hasiče jsou procedurální placeholdery (dech v masce ve smyčce, kroky s cinkáním výstroje, nesrozumitelné volání přes membránu masky), vše 3D. Při honičce zrychlí dech | Poloha čitelná sluchem bez cizích assetů; nahradí se ve zvukovém průchodu | C |
| D-037 | 21. 11. 2026 | Skrýše: 2–3 na patro (aspoň jedna na hlavní cestě), skříň 60 % / stůl 40 %. Skříň schová úplně, ale dvířka vržou (hluk 6 m). Stůl je tichý, ale hasič tě pod ním vidí do 4 m při světle a do 2,5 m baterkou | Každá skrýš má cenu, volba je čitelná (pilíř 3) | C |
| D-038 | 21. 11. 2026 | Hasič při hledání rozhodne o každé skrýši v okolí jednou: 50 % prohledá (1,5 s, pak otevře), jinak přejde. Skrýš, do které tě viděl vlézt, nebo u které slyšel vrznutí, prohledá vždy | „Přejde nebo najde“ (T-022); opakované losování by hráče našlo skoro vždy | C |
| D-039 | 21. 11. 2026 | Přeskládání při zhasnutí počká, dokud je hráč schovaný, a proběhne po vylezení (pokud je stále tma) | Skrýš nesmí zmizet s hráčem uvnitř | C |
| D-040 | 21. 11. 2026 | Okno se otevírá na E (křídlo dovnitř, vítr ve 3D). Do 1,6 m před otevřeným oknem CO klesá −4/s i u zdroje. Otevření i zavření je hluk 8 m. Po přeskládání jsou okna nová a zavřená | Vzduch má cenu: hasič přijde, a právě tak vzniká konec 3 (T-026) | C |
| D-041 | 21. 11. 2026 | Spínače světla v chodbě: buňka dostane spínač, pokud ho nemá ona ani soused (směrem od schodů), na volné stěně 1,2 m od středu. Žádná buňka není dál než 2 kroky od spínače (validátor) | Playtest #1 (D-030), vychází zhruba každá 2.–3. buňka | C |
| D-042 | 21. 11. 2026 | Výtah je mimo provoz (kánon GDD: hasiči ho vypnuli). Jde přivolat, ale nepřijede | Věrné příběhu, levné, další drobná stopa | C |
| D-043 | 21. 11. 2026 | 3 patra (0 přízemí, 1, 2). Tvoje dveře a zdroj CO jsou jen ve 2. patře (kánon: zdroj je u souseda na tvém patře). Ostatní patra končí dveřmi souseda, detektor tam mlčí | Cíl je jasný (nahoru), CO má jeden původ | C |
| D-044 | 21. 11. 2026 | Ve scéně je vždy jen jedno patro. Dvouramenné schodiště; na mezipodestě se patro vymění a hráč posune o podlaží (2,72 m), geometrie podesty je na obou patrech stejná. Zpět až po sestoupení z podesty | Plynulé, levné na výkon (iGPU), žádné načítání | C |
| D-045 | 21. 11. 2026 | Hasič je vždy na hráčově patře. Když tě honil/hledal, přijde po schodech za 6 s a hledá; jinak se objeví v nejvzdálenější buňce a hlídá | Útěk po schodech dá náskok, ne úplné bezpečí | C |
