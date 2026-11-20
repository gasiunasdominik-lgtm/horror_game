---
description: Zpracuj hlášení chyby. Reprodukuj, oprav, otestuj
argument-hint: 'popis chyby, nebo prázdné pro nejnovější BugReports/'
---

Zpracuj chybu: $ARGUMENTS

1. Zdroj: pokud je zadán popis, použij ho. Pokud ne (nebo navíc), vezmi nejnovější složku v `BugReports/` (screenshot, log, `state.json`, `note.txt`).
2. Zapiš do `docs/BUGS.md`: ID (B-nnn), severity (S1–S4 podle `docs/TESTING.md`), kroky, očekávané vs. skutečné, seed a stav z `state.json`.
3. Reprodukuj přes seed (`-seed <n>`, nebo F9 s uloženým seedem). Když se nedá reprodukovat, napiš co zkusíš a zapiš stav „nereprodukovatelné“. Nehádej opravu.
4. Oprav minimální změnou. Přidej test (EditMode, kde to jde) nebo bod do regresního seznamu v `docs/TESTING.md`.
5. Brána kvality → commit `fix: <B-nnn> <popis>` → stav FIXED v `docs/BUGS.md`.
6. Odpověz: příčina (1 řádek), oprava (1 řádek), **Otestuj** (max 3 body).

S1 (pád, nelze hrát) řeš ihned. S2 a níž zařaď podle priorit milníku.
