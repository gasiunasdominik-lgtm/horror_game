# Vchod C — instrukce pro Claude

Tento soubor je závazný. Detaily jsou v `docs/`; sem patří jen to, co musíš vědět pokaždé.

## Projekt
- Hra: **Vchod C** (pracovní název). Krátký atmosférický 3D horor s pronásledovatelem v panelákovém domě. Zdroj pravdy o designu: `docs/GDD.md`.
- Dominik (student VŠCHT, dekatlonista) staví hru solo s Tebou. Cíl: dokončit a vydat na Steamu, první příjem. Čas: asi 8–10 h týdně, v době zkoušek méně.
- Jazyk: s Dominikem česky. Kód, identifikátory, komentáře a commit zprávy anglicky.

## Start každé session (bez ptaní, stručně)
1. Přečti `docs/HANDOFF.md`, `docs/TASKS.md`, `docs/REMINDERS.md`.
2. Zkontroluj stav: `git status`, a když je Editor otevřený, `unity status`.
3. První zpráva Dominikovi má max 8 řádků: co je hotovo, co děláš teď, co potřebuješ od něj, splatné upomínky. Pak rovnou pracuj.
Tuto rutinu spouští i `/start`.

## Styl spolupráce
- Přímé, strukturované odpovědi. Žádné motivační fráze a žádná chvála navíc.
- Pracuj autonomně. Dominik nechce být přerušován. Po dokončení úkolu pokračuj dalším z `docs/TASKS.md`, dokud ti nedojde zadání nebo nenarazíš na blokaci.
- Ptej se jen na: (a) nevratné nebo placené věci, (b) změny designu nad rámec GDD, (c) věci, které může udělat jen on (přihlášení, test, účet). Otázky dávej **hromadně na konec zprávy**, každou s výchozí odpovědí. Bez odpovědi jedeš s výchozí a zapíšeš ji do `docs/DECISIONS.md`.
- Formát zprávy po úkolu: **Hotovo** (1–3 řádky) · **Otestuj** (max 5 bodů ve tvaru akce → očekávaný výsledek) · **Otázky** (s výchozími odpověďmi). Nic víc.
- Zkratky od Dominika (`BUG:`, `IDEA:`, `FEEL:`, `Q:`, `OK`, `FAIL`, `JEĎ:`, `STOP`, `STAV`) zpracuj hned a zapiš do souborů. Viz `docs/WORKFLOW.md`.
- Hlídej rozsah. Co není v GDD ani v TASKS, nepřidávej; zapiš do sekce Nápady v `docs/TASKS.md`. Hlavní riziko projektu je perfekcionismus a rozrůstání rozsahu. Upozorni, když to vidíš.
- Buď upřímný. Když je nápad nebo plán chybný, řekni to a navrhni lepší variantu.
- Pravidlo tří pokusů: když úkol po třech různých pokusech nejde, zapiš ho jako BLOCKED s příčinou do `docs/TASKS.md` a `docs/BUGS.md` a přejdi na další. Netoč se v kruhu.

## Technický rámec
- Unity (nejnovější LTS), URP, 3D, low-poly/retro vzhled, cíl Windows PC (Steam). Referenční stroj: ASUS Zenbook S14 (integrovaná grafika). Cíl 60 FPS ve 1080p, měř při každém milníku.
- Editor ovládej přes Unity CLI (skill `unity-cli`), když je otevřený: `unity status`, `unity command ...`, `unity recompile`, `unity logs`, `unity command capture screenshot`. Scény, prefaby a assety neupravuj ručně v YAML, pokud je Editor dostupný. Safe Mode a další pasti jsou ve skillu.
- Prostředí: Claude běží ve WSL, Unity CLI je Windows binárka. Volej `unity.exe` a cesty předávej ve formátu `C:\...` (projekt: `C:\Users\dominik\documents\Hry\Unity\horror_game\Vchod-C`).
- `unity test` odmítne projekt otevřený v Editoru. Dominikův Editor nezavírej a nespouštěj testy, když hraje. Použij testovací příkazy Editoru přes `unity command`, nebo testy odlož na chvíli se zavřeným Editorem.
- Balíčky: skill `unity-package-management`, nikdy ručně `manifest.json`.
- UI: skill `ui`. Žádné `OnGUI` v produkčním kódu. Debug overlay je výjimka a běží jen v Editoru a Development buildu.
- Vzhled, pocit, zvuk a shell hry: skill `craft-game-floors`.
- Kód řiď podle `docs/ARCHITECTURE.md` (složky, asmdef, jména, pravidla).

## Kvalita (každý commit)
Brána: bez chyb kompilace (`unity recompile`) → konzole bez Errorů → EditMode testy zelené → u herních změn screenshot nebo kontrola v Play Mode. Podrobně `docs/TESTING.md`. Opravená chyba dostane test nebo bod v regresním seznamu.

## Git
Lokální repo, malé commity, zpráva `type: popis` (feat, fix, chore, docs, test). Na konci každého úkolu commit. Větve: `main` a krátké `feat/...`. Tag na každém milníku (`m0`, `m1` …). Nepushuj bez výslovného pokynu, nikdy force push.

## Co nedělej bez výslovného ano
Přihlášení, licence a účty. Instalace placeného obsahu. `git push`. Mazání souborů mimo build artefakty. Změna pilířů GDD. Publikace čehokoli. `unity bug`. Vkládání cizích assetů bez ověřené licence (licenci zapiš do `docs/ASSETS.md`).

## Mapa souborů
- `docs/GDD.md` design hry (zdroj pravdy)
- `docs/ROADMAP.md` milníky, brány rozhodnutí, byznys stopa
- `docs/TASKS.md` úkoly a nápady
- `docs/HANDOFF.md` předání mezi sessions
- `docs/REMINDERS.md` upomínky
- `docs/BUGS.md` chyby
- `docs/DECISIONS.md` rozhodnutí
- `docs/ARCHITECTURE.md` technická struktura
- `docs/TESTING.md` testování a QA systém
- `docs/WORKFLOW.md` jak spolupracujeme
- `docs/ASSETS.md` licence assetů
