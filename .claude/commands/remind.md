---
description: Přidej nebo zobraz upomínky
argument-hint: 'datum nebo spouštěč | text; prázdné = zobraz splatné'
---

Upomínky: $ARGUMENTS

- Bez argumentu: vypiš splatné upomínky z `docs/REMINDERS.md` (datum do dneška nebo spouštěč nastal) a stav `open`. Navrhni u každé první krok.
- S argumentem: přidej řádek do `docs/REMINDERS.md` (nové R-nnn, spouštěč, text, vlastník, stav `open`). Když chybí vlastník, odvoď ho (Dominik pro osobní/právní/marketingové věci, Claude pro technické).
- Když upomínku vyřešíš, nastav stav `done` a zapiš datum.
