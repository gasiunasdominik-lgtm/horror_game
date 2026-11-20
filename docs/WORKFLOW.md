# WORKFLOW — jak spolupracujeme

Cíl: rychlá komunikace s minimem přerušování. Dominik rozhoduje CO a cítí, jestli to funguje. Claude řeší JAK, ověřuje a hlásí.

## 1. Rytmus
- **Session:** `/start` → zadání `JEĎ: <cíl>` → Claude pracuje autonomně → zpráva „Hotovo · Otestuj · Otázky“ → Dominik zahraje a odpoví zkratkami → `/end`.
- **Týdně (neděle, 15 minut):** `/status`, upomínky, a v pondělí plán týdne (max 3 cíle).
- **Milník:** tag, výkonové měření, playtest, brána rozhodnutí z `ROADMAP.md`.

## 2. Zkratky (Dominik píše, Claude zpracuje a zapíše bez ptaní)

| Zkratka | Význam | Co Claude udělá |
|---|---|---|
| `JEĎ: <cíl>` | Autonomní blok práce | Pracuje, dokud není cíl hotový nebo BLOCKED |
| `STOP` | Přestaň | Dokončí atomický krok, commit, krátké předání |
| `STAV` | Rychlý stav | Totéž co `/status` |
| `OK` | Test prošel | Označí úkol DONE, jede dál |
| `FAIL: <text>` | Test neprošel | Vytvoří bug, opraví, dá nový Otestuj |
| `BUG: <text>` | Chyba | Zapíše do `BUGS.md`, zařadí podle severity |
| `IDEA: <text>` | Nápad | Do sekce Nápady v `TASKS.md`, neimplementuje |
| `FEEL: <text>` | Pocit z hraní (strach, tempo, férovost, srozumitelnost) | Zapíše do Playtest logu v `TESTING.md` |
| `Q: <text>` | Otázka | Odpoví stručně, bez práce navíc |
| `ROZHODNUTO: <text>` | Pevné rozhodnutí | Zapíše do `DECISIONS.md` a aktualizuje GDD |

Hlasové poznámky (DPI) lze vložit tak, jak jsou. Pokud nemají zkratku, Claude je zařadí a jedním řádkem potvrdí, jak je zařadil (kdyby se spletl, Dominik opraví).

## 3. Autonomie (co Claude smí bez ptaní)
**Smí:**
- číst a psát v projektu, vytvářet soubory, skripty, prefaby, scény přes Editor,
- spouštět `unity` příkazy pro stav, překompilaci, logy, screenshoty, testy, `unity command`,
- lokální commity, větve a tagy,
- instalovat UPM balíčky potřebné pro aktuální úkol (přes skill),
- aktualizovat `TASKS`, `BUGS`, `DECISIONS`, `HANDOFF`, `REMINDERS`,
- rozhodovat o implementaci (jak), o ladicích hodnotách a drobných designových detailech uvnitř GDD.

**Nesmí bez ano (viz `CLAUDE.md` a `.claude/settings.json`):** přihlášení, licence, účty, placený obsah, `git push`, mazání mimo build artefakty, změna pilířů GDD, publikace, cizí assety bez licence.

## 4. Jak se minimalizují přerušení
- **Otázky v dávkách na konec zprávy**, každá s výchozí odpovědí. Kdo neodpoví, schvaluje výchozí (zapíše se do `DECISIONS.md`).
- **Oprávnění předem** v `.claude/settings.json` (většina běžných příkazů a editace souborů bez potvrzování).
- **Jedna session = jeden jasný cíl**, Claude jede dál, dokud nenarazí na blokaci.
- **Pravidlo tří pokusů:** po třech různých neúspěšných pokusech úkol označí BLOCKED, zapíše příčinu a pokračuje dalším.
- **Žádné opakování plánu, žádné shrnutí toho, co už je v souborech.** Zpráva = Hotovo · Otestuj · Otázky.
- **Formát Otestuj:** max 5 bodů, každý „akce → očekávaný výsledek“, aby šlo odpovědět `OK` nebo `FAIL`.

## 5. Jak se řeší problémy
1. Chyba najde Dominik (nebo test) → `BUG:` nebo F8 v buildu (vytvoří složku v `BugReports/`).
2. `/bug` ji zpracuje: reprodukce přes seed, oprava, test, commit.
3. Neopravená chyba má v `BUGS.md` stav a severitu. S1 se řeší ihned, S2 do konce milníku.
4. Upomínky (`REMINDERS.md`) hlídají termíny a spouštěče. Claude je ukazuje na začátku session a v `/status`.

## 6. Předání mezi sessions
`/end` přepíše `HANDOFF.md`. `/start` ho čte. Soubor je krátký a zastaralé části mizí, historie je v gitu.

## 7. Pravidla pro Dominika
- Zadávej cíle jednou větou a výsledek, ne postup.
- Nápady piš jako `IDEA:`, ať nerozbijí rozsah.
- Když nevíš, odpověz `OK` (výchozí volba platí).
- Když tě něco vyruší, napiš `STOP`, Claude to bezpečně uloží.

## 8. Pravidla pro Claude
- Řekni jednou, co je špatně, a navrhni alternativu. Neopakuj se.
- Neprovádět víc, než je zadáno; zapsat nápad, nepřidat ho.
- Hlásit skluz a riziko včas, přímo a bez zdůrazňování pozitiv.
- Každé zjištění o hře, které ovlivní design, zapsat do `DECISIONS.md`.
