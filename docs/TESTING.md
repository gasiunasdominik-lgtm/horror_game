# TESTING — QA systém (rychlé testování, hodnocení, odhalování chyb)

Cíl: každou chybu poznat rychle, reprodukovat ji jedním příkazem a opravit s důkazem. Systém se staví od M0 (úkoly T-005, T-011), protože pozdější doplnění je dražší.

## 1. Brána kvality (každý commit)
1. `unity recompile` bez chyb.
2. Konzole Editoru bez Errorů (`unity logs`; varování se sbírají, ne ignorují).
3. EditMode testy zelené.
4. U herních změn: kontrola v Play Mode a screenshot (`unity command capture screenshot`). Screenshot musí být osvětlený a bez růžových materiálů.
5. `git status` bez `Library/`, `Temp/`, `BugReports/`.

Poznámka k nástrojům: `unity test` odmítne projekt otevřený v Editoru. Dominikův Editor nezavírej. Použij testovací příkazy Editoru přes `unity command` (zjisti je přes `unity command --query test`), nebo testy spusť, když je Editor zavřený.

## 2. Vrstvy testování

| Vrstva | Co | Kdy | Kdo |
|---|---|---|---|
| EditMode | Čistá logika (CoModel, SeedService, validace ModuleLayout, prahy konců) | Každý commit | Claude |
| PlayMode smoke | Načte scénu, 60 s simulace s botem, kontrola výjimek, FPS a validity grafu domu pro N seedů | Před koncem úkolu, před milníkem | Claude |
| Autoplay seed sweep | Přeskládání domu pro seedy 1…200, ověření cesty ke dveřím a oknu | Po změně ModuleLayout | Claude |
| Dominik playtest | 10–15 minut hraní s checklistem a FEEL hodnocením | Minimálně týdně | Dominik |
| Cizí playtest | 3 lidé bez vysvětlování, sleduj, kde váhají | M2, M3 | Dominik |
| Výkon | FPS, frame time, paměť na Zenbooku | Každý milník | Claude + Dominik |

## 3. Debug nástroje ve hře (Editor + Development build)
Jsou součástí M0. Kód v `Vchod.Debug`.

| Klávesa | Funkce |
|---|---|
| F1 | Debug overlay: FPS, seed, hash rozložení, hladina CO, stav hasiče, poloha, čas kola |
| F2 | God mode (zmrazí CO, hasič nechytí) |
| F3 | Teleport na další zónu |
| F4 | Vynucené přeskládání domu (stejný seed i nový) |
| F5 | Přepnutí stavu hasiče (Patrol, Chase, Call, Search) |
| F6 | Time scale 0,25 / 1 / 4 |
| F7 | Přepnutí mlhy a post-processingu (výkonový test) |
| F8 | **Bug report** (viz níže) |
| F9 | Restart kola se stejným seedem |
| F10 | Skok na konkrétní konec (pro test závěrů) |

## 4. Bug report jedním stiskem (F8)
Uloží do `BugReports/<datum-čas>/`:
- `screenshot.png`,
- `log.txt` (posledních 300 řádků konzole),
- `state.json`: verze buildu, git hash, seed, hash rozložení, poloha hráče, hladina CO, stav hasiče, aktivní konec, čas kola, FPS, scéna,
- `note.txt` (Dominik napíše větu, volitelně, nebo hlasovou poznámku vloží do chatu).

`/bug` tuto složku načte, reprodukuje chybu přes seed (`-seed N`) a opraví. Složka `BugReports/` je v `.gitignore` (screenshoty jsou těžké), ale ID chyby v `BUGS.md` na ni odkazuje.

## 5. Playtest log (automaticky)
`PlaytestLogger` zapisuje CSV do `PlaytestLogs/`: začátek kola, seed, každé zhasnutí světla, přeskládání, rozhovory se sousedy, překročení prahů CO, stavy hasiče, konec a čas, ukončení hry. Z toho `/playtest` vytáhne: čas do prvního strachu, kde hráči umírají (kolabují), kde bloudí, které konce a jak často.

## 6. FEEL hodnocení (po každém playtestu, 1 minuta)
Dominik odpoví `FEEL:` a čtyři čísla 1–5 + jedna věta:

| Osa | Otázka |
|---|---|
| Strach | Měl jsem strach v prvních 2 minutách? |
| Srozumitelnost | Věděl jsem, co mám dělat a proč umírám/zlepšuju se? |
| Férovost | Cítil jsem prohru jako svou chybu, ne náhodu? |
| Tempo | Chtěl jsem hrát znovu? |

Playtest log (vyplňuje `/playtest`):

| Datum | Build | Strach | Srozum. | Fér. | Tempo | Poznámka |
|---|---|---|---|---|---|---|
| 21. 11. 2026 | m0 (Editor) | – | – | – | 2 | Dominik: „menší napětí s lehkou úzkostí, rozumím nastavení; docela nefér, že CO roste i daleko od zdroje; při 60 obraz nevypadá, že by mi bylo špatně.“ Čísla os kromě tempa nezadal. Nálezy: B-001, B-002 |
| 21. 11. 2026 | m1 (Editor) | ↓ | dobrá | nízká | příliš rychlé | Dominik: „ztratilo to na strašidelnosti, tempo je příliš rychlé, sotva se rozkoukám a už po mě běží, neměl jsem ani pomalejší úvod; férovost skoro není, pravděpodobnost kontroly skrýše je vysoká, je vždy na stejném patře, jediná cesta nahoru, nemám jak utéct; neměl by se spawnovat blízko mě a hned na začátku; srozumitelnost dobrá, ale titulky nestíhám přečíst a nemají efekt.“ Čísla nezadal. G1: honička zatím ne → ladit jádro (T-031–T-035) před obsahem M2 |
| 21. 11. 2026 | main 01b6e19 (Editor) | ↑ (při světle ↓) | dobrá | výrazně lepší, „jak bych si to představoval“ | rychlé, málo náplně | Dominik: „strach je o něco větší, ale během světla mnohem menší; největší strach, když slyším blížit se nepřítele, točí se mi hlava, nebo když nevím, kde jsem; tempo docela rychlé, jen tam zatím není moc co dělat a vidět; férovost mnohem lepší; srozumitelnost dobrá, ale do budoucna namluvený hlas a intuitivnější pochopení ovládání a náplně hry.“ Čísla nezadal. Funguje: zvuk blížícího se hasiče, příznaky CO, dezorientace. Chybí: obsah (M2), onboarding (G2) |

## 7. Severity chyb

| Úroveň | Význam | Reakce |
|---|---|---|
| S1 | Pád, nelze hrát, poškození dat | Ihned |
| S2 | Rozbité jádro (nelze dojít, hasič se zasekne, CO nefunguje) | Do konce milníku, většinou hned |
| S3 | Kosmetika nebo okrajový případ | Zařadit podle priorit |
| S4 | Nápad, vylepšení | Do Nápadů v `TASKS.md` |

## 8. Regresní seznam
Každá opravená chyba, kterou nejde pokrýt testem, dostane bod sem. Dominik ho projde před milníkem (5 minut).
- [x] B-002 (ověřeno 21. 11. 2026): bez rozsvícení je chodba tmavá, ale čitelný tvar chodby, dveře a oranžový spínač.

## 9. Výkon (měření na Zenbooku)
| Milník | Datum | Scéna | FPS (průměr / min) | Frame time | Poznámka |
|---|---|---|---|---|---|
| M0 | 20. 11. 2026 | Game, seed 4242, greybox 1 patro | 66 / 24 (1 % low) | avg 15,2 ms, p99 22,5 ms, max 123 ms | Dev build, 1920×1080 borderless, VSync off, světlo zapnuté. **1. spuštění po buildu** (studená cache shaderů) |
| M0 | 21. 11. 2026 | Game, seed 4242, greybox 1 patro | 98 / 77 (1 % low) | avg 10,2 ms, p99 12,7 ms, max 14 ms | Totéž, 2. spuštění (teplá cache). Rozptyl mezi běhy: napájení/teplota neznámé |
| M1 | 21. 11. 2026 | Game, seed 4242, 3 patra, hasič | ~~5 / 3~~ **neplatné** | avg 189–247 ms | Stroj přiškrcený: CPU 21 % výkonu (1,7 GHz) při 25 % zátěži, na síti. V Editoru i scéna bez domu 15 FPS, takže jde o stav stroje, ne o obsah hry. Profiler: čekání na GPU ~54 ms/snímek. Zopakovat (T-030) |
| M1 | 21. 11. 2026 | Game, seed 4242, 3 patra, hasič | 56 / 32 (1 % low) | avg 18,0 ms, p99 29,6 ms, max 36 ms | Opakování, stroj nepřiškrcený (CPU 133–138 % výkonu), na síti, plán Rovnováha. 1. běh |
| M1 | 21. 11. 2026 | totéž | 50 / 28 (1 % low) | avg 20,0 ms, p99 34,2 ms, max 39 ms | 2. běh. **Pod cílem 60 / 45.** Profil z Editoru: GPU-bound |
| M1 | 21. 11. 2026 | totéž | **134 / 81** (1 % low) | avg 7,4 ms, p99 10,2 ms, max 75 ms (1. snímky) | Po D-048: vypnuté SSAO (plné rozlišení + prepass normál) a nepoužívané kopie opaque/depth textury. 2 běhy 134,9 a 134,1 FPS |

Postup měření: `Builds/Dev/VchodC.exe -seed 4242 -perf 30 -screen-width 1920 -screen-height 1080 -screen-fullscreen 1`. Výsledek v `Builds/Dev/PlaytestLogs/perf_*.txt` a v logu (`[Perf]`). Hráč obchází všechny buňky a otáčí se, světlo trvale zapnuté (nejhorší případ: všechna světla).

Cíl: 60 FPS průměr, min 45. Při propadu: snížit světla, textury, mlhu a dohled, bake.

## 10. Upomínky z testování
Cokoliv, co Claude nestihne nebo odloží (flaky test, varování, TODO v kódu), zapíše jako upomínku do `REMINDERS.md` se spouštěčem (milník nebo datum). Splatné se ukazují při `/start` a `/status`.
