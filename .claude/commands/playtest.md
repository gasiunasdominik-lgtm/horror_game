---
description: Vyhodnoť playtest (FEEL hodnocení, logy, nálezy)
argument-hint: 'Dominikovy poznámky z hraní (zkratky FEEL, BUG, IDEA)'
---

Vyhodnoť playtest. Poznámky: $ARGUMENTS

1. Vezmi nejnovější CSV v `PlaytestLogs/` (pokud existuje) a poznámky výše.
2. Z `FEEL:` řádků vytáhni hodnocení (strach, srozumitelnost, férovost, tempo, 1–5) a zapiš do tabulky v `docs/TESTING.md` (sekce Playtest log).
3. `BUG:` zapiš do `docs/BUGS.md`, `IDEA:` do sekce Nápady v `docs/TASKS.md`. Neimplementuj nápady.
4. Z logu zjisti: čas do prvního strachu, počet úmrtí/zhroucení a kde, kde hráč trávil čas, kolik kol a jaké konce.
5. Odpověz max 10 řádky: **Co fungovalo · Co ne · Top 3 priority na příští týden (s důvodem)**. Přímo, bez zdůrazňování pozitiv.
