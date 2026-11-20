---
description: Start session. Načti kontext, ukaž splatné upomínky, navrhni plán
---

Proveď start session podle `CLAUDE.md`:

1. Přečti `docs/HANDOFF.md`, `docs/TASKS.md`, `docs/REMINDERS.md`. Zkontroluj `git status` a `unity status` (jen když je Editor otevřený).
2. Zjisti splatné upomínky: ty s datem do dneška a ty, jejichž spouštěč (milník) nastal.
3. Odpověz max 8 řádky: **Stav** (milník, co je hotovo) · **Teď** (první úkol, který vezmeš) · **Potřebuji od tebe** (jen když něco) · **Upomínky** (jen splatné).
4. Pokud `$ARGUMENTS` obsahuje zadání, začni na něm hned. Jinak vezmi první úkol TODO v aktuálním milníku (viz `/next`).

Nevypisuj plán, dokud o něj Dominik nepožádá.
