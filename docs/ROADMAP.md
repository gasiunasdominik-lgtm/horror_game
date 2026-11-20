# ROADMAP

Čas Dominika: asi 8–10 h týdně, v období zkoušek 4–6 h (ověř termíny, viz REMINDERS R-004). Všechna data jsou odhady a revidují se na bránách rozhodnutí.

## Milníky

| Milník | Termín | Cíl | Hotovo, když |
|---|---|---|---|
| **M0 Základ** | 23. 11. – 29. 11. 2026 | Projekt, nástroje, debug a QA systém, základ pohybu a chodby | Chodbou se dá projít, zhasne světlo, dům se přeskládá, pípání a efekty reagují na CO, F8 vytvoří bug report, EditMode testy zelené |
| **M1 Smyčka** | 30. 11. – 20. 12. 2026 | Hasič AI, skrýše, první soused, 3 patra z kostek, první konec | Celé kolo hratelné od začátku do konce 1, 2 a 3 (greybox), první playtest s hodnocením, první krátké video z vývoje |
| **M2 Obsah** | 21. 12. 2026 – 31. 1. 2027 | Sousedé 1–3, kousky příběhu, low-poly pass, audio pass, menu, nastavení | Čtyři konce, 3 sousedé, hratelné bez Dominikova vysvětlování, test se 3 cizími lidmi |
| **M3 Vertikální řez** | 1. 2. – 28. 2. 2027 | Leštěný 20minutový řez, Steam stránka, trailer | Řez je hratelný na Zenbooku na 60 FPS, Steam stránka je připravená, trailer hotový, rozhodnutí G3 |

Po M3: březen až září 2027 plný obsah, polish, demo, marketing. Odhad vydání: čtvrté čtvrtletí 2027 (orientační, upřesní G3).

## Brány rozhodnutí
- **G1 (konec M1):** je honička a mechanika světla zábavná? Pokud ne, přepracuj jádro, než se přidá obsah. Rozhoduje Dominik podle FEEL hodnocení.
- **G2 (konec M2):** je hra srozumitelná cizím lidem? Pokud ne, upravit tutoriál/signály, ne obsah.
- **G3 (konec M3):** pokračovat, zúžit rozsah, nebo změnit koncept? Vstupy: FEEL hodnocení, výkon, počet wishlistů, množství zbývající práce. Při skluzu o víc než 4 týdny se zúží rozsah (méně sousedů a pater), ne termín.

## Pravidla plánu
- Na každý týden max 3 cíle. Nic dalšího se nezačíná, dokud nejsou hotové.
- Skluz se zapisuje do `HANDOFF.md` a řeší na týdenním review, ne tichým prodlužováním.
- Rozsah roste jen výměnou (nová věc ven = jiná věc pryč).

## Byznys stopa

| Kdy | Co | Kdo |
|---|---|---|
| M1 (do 20. 12.) | První krátké video z vývoje (vlog, krátké klipy) | Dominik |
| od M1 | Jeden klip týdně z aktuálního buildu (YouTube Shorts, TikTok, Instagram) | Dominik, Claude připraví materiály |
| M3 | Steam Direct 100 $ za hru (ověř aktuální částku), store page, trailer, cíl wishlistů | Dominik |
| před prvním příjmem | Živnost jako OSVČ vedlejší (student), konzultace s účetní/ČSSZ, daňové údaje pro Steam | Dominik |
| 2027 | Demo na itch.io a Steamu, přihlášení na Steam Next Fest (ověř termíny) | Dominik, Claude |
| před vydáním | Kontrola licencí assetů (`ASSETS.md`), age rating, content disclaimery | Claude, Dominik |

## Rizika a protiopatření
| Riziko | Protiopatření |
|---|---|
| Perfekcionismus a rozrůstání rozsahu | Out of scope v GDD, pravidlo výměny, týdenní limit 3 cílů |
| Slabý hardware (Zenbook) | Low-poly, mlha, bake světel, měření při každém milníku |
| Grafika a zvuk jako úzké hrdlo | Hotové asset packy s licencemi, jednotný stylizační filtr (retro) |
| Ztráta motivace (zkouškové, trénink) | Krátké kolo práce, vidět pokrok každý týden (build + klip), brána G1 |
| Žánrová konkurence | Odlišení tématem (otrava CO, hasič jako falešný stalker) a emocionálním obratem |
| Chyby a regrese | QA systém z `TESTING.md` od M0 |
