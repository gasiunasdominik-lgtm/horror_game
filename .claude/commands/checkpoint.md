---
description: Brána kvality a commit
argument-hint: 'zpráva commitu (volitelně)'
---

Proveď checkpoint:

1. `unity recompile` bez chyb.
2. Konzole Editoru bez Errorů (`unity logs`).
3. EditMode testy zelené (viz `docs/TESTING.md`; když je Editor otevřený a `unity test` odmítne, použij testovací příkaz Editoru přes `unity command` nebo to zapiš jako výjimku).
4. U herních změn: krátká kontrola v Play Mode a screenshot.
5. `git add -A`, zkontroluj `git status` (žádný `Library/`, `Temp/`, `BugReports/`), commit `type: popis`. Zpráva: $ARGUMENTS (pokud není, odvoď z diffu).

Když brána neprojde, neprováděj commit. Napiš proč a oprav to.
