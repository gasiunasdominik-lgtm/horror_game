# GDD — Vchod C (zdroj pravdy o hře)

Verze 1 (19. 11. 2026). Změny pilířů a konců jen se souhlasem Dominika. Jiné detaily Claude upřesňuje a zapisuje do `DECISIONS.md`.

## Pitch (1 věta)
Vracíš se pozdě v noci domů do paneláku, ale dům se mění, sousedé jsou divní a za tebou jde postava v masce. Cíl je dostat se do svého pokoje. Pravda se ukáže až na konci.

## Zážitkový cíl
Rychle vyvolat neklid a strach (první 2 minuty), pak emoci obrátit: strach → pochopení → smutek/úleva. Při opakování hráč vidí, že všechno divné bylo předem naznačeno.

## 3 pilíře (rozhodují každé rozhodnutí)
1. **Věř tělu, ne očím.** Příznaky (hučení, zúžený obraz, zpomalení, pípání detektoru) jsou pravdivá informace. Milý soused může být past, děsivá postava pomoc.
2. **Strach se obrací v pochopení.** Každá divnost má lidské vysvětlení. Odhalení přepisuje význam, ale neznehodnocuje zážitek.
3. **Krátké, čitelné, fér kolo.** 15–25 minut, jasná pravidla, náhoda mění trasu, ne spravedlnost.

## Platforma a styl
PC (Windows, Steam). Low-poly / retro vzhled (nízké rozlišení textur, mlha, zrnění). 1. osoba. Jedna lokace. Délka celé hry 1–2 hodiny, kolo 15–25 minut.

## Skutečný příběh (kánon, hráč ho zjišťuje postupně)
1. V bytě souseda na hráčově patře je vadný plynový spotřebič (např. ohřívač vody v koupelně). Vzniká oxid uhelnatý (CO), který nemá barvu ani zápach a šíří se mezi byty ventilačními šachtami.
2. Soused nebo jeho CO detektor spustí tísňové volání. Přijíždí hasiči s měřicí technikou a dýchacími přístroji, vypnou výtah a evakuují dům.
3. Hráč se vrací z noční směny, je už uvnitř, unavený a zmatený. Příznaky bere za únavu.
4. Hasič ho vyhledá a má ho dostat ven. Zmatený hráč před ním utíká, protože v masce a s těžkým dýcháním vypadá jako vetřelec.

Pravidlo: nic se nesmí vysvětlit duševní nemocí. Příčina je fyzická, konkrétní a ověřitelná.

## Mapa „divné → vysvětlení“
- Hádka za dveřmi = televize nebo rádio za zdí.
- Paní s dortem = vůně z jiného bytu a zmatená paměť.
- Kluk s míčem = zvuk potrubí nebo z ulice.
- Měnící se patra = závrať a výpadek schodišťového světla.
- Soused zvoucí „odpočinout si“ = skutečné nebezpečí: byt u zdroje plynu.
- Stalker v masce s těžkým dechem = hasič s dýchacím přístrojem.
- Byt 0 v přízemí = nikdy neexistoval, je to dezorientace (definitivní podoba viz DECISIONS).

## Cíl hráče a smyčka
Dostat se ke dveřím svého pokoje. Dům se pokaždé, když zhasne schodišťové světlo, přeskládá (moduly chodeb a bytů v jiném pořadí). Kolo skončí koncem. Další kolo má jiný seed (rozložení, rozmístění stop a sousedů) a odhalí kousek příběhu, který dřív nebyl vidět.

## Mechaniky

### Schodišťové světlo
Tlačítko rozsvítí světlo na ~20 s (ladit). Světlo = vidíš, ale tvoje poloha je prozrazena (hasič se orientuje podle světla). Tma = skryješ se, ale hůř se orientuješ. Přeskládání domu probíhá při zhasnutí.

### Model CO (hodnota 0–100, ladit)
- Roste v blízkosti zdroje a uvnitř bytů u šachty, pomalu na chodbě, nejméně u otevřeného okna.
- Klesá u otevřeného okna s čerstvým vzduchem (rychle) a na schodišti dál od zdroje (pomalu).
- Prahy (orientační): 20 náznak hučení v uších · 40 zúžený obraz, pomalejší otáčení · 60 viditelná zkreslení geometrie, dolní propust zvuku · 80 skoro tma, chůze −30 % · 100 kolaps (špatný konec).
- Pípání detektoru sílí směrem ke zdroji (fér orientační signál).

### Hasič (pronásledovatel)
Stavy: Hlídá (Patrol) → Zaslechl (Investigate: jde za hlukem a světlem) → Pronásleduje (Chase) → Volá (Call: volá hráče zkresleným hlasem) → Hledá (Search) po ztrátě. Slyší kroky, zvonek a rozsvícení světla. Neubližuje: chycení znamená evakuaci (konec 2). Strach vzniká z vnímání, ne z násilí. Audio: těžké dýchání v masce, cinkání výstroje, baterka.

### Sousedé
Nejsou nepřátelé. Každý je zbytek reality zkreslený otravou. Někteří pomáhají (otevřený byt, zkratka), jiní lákají dovnitř (nebezpečné byty u zdroje). Rozlišení je fér podle zvuku, chování a příznaků. Pět postav: paní s dortem, pán s rádiem, hádka za dveřmi, kluk s míčem, byt 0.

### Interakce a skrýše
Skříně a stoly jako skrýše, okna k větrání (čerstvý vzduch), dveře (klepat, vstoupit), sběr kousků příběhu (poznámky, fotky, zvuky). Žádný inventář mimo drobných předmětů.

## Konce
1. **Zhroucení:** dojdeš do pokoje, který je u zdroje šachty, a omdlíš (špatný).
2. **Evakuace:** nechá ses odvést hasičem nebo tě chytil (dobrý, hořký).
3. **Čerstvý vzduch:** vydržíš u okna, dokud tě hasič nenajde (nejlepší).
4. **Skrytý (rozhodne se později):** po sebrání všech kousků příběhu.

## Znovuhratelnost
Náhodný seed (pořadí modulů, rozmístění stop a sousedů), nové kousky příběhu při dalších hrách, 3 hlavní konce, odemykání skrytého konce. Seed je vždy reprodukovatelný (kvůli chybám a testování).

## Rozsah
Vchod, výtah, 3–4 patra, sklep/střecha, 5 bytů, 4–6 modulů chodeb, 15–20 kousků příběhu, 3–4 konce. Kolo 15–25 minut.

## Nebudeme dělat (Out of scope)
Multiplayer. Otevřený svět. Voice acting (jen zkreslené zvuky a hlasy z balíčků). Krev a brutalita. Proceduralita nad rámec míchání modulů a seedu. Složitý inventář a crafting. Mobilní a konzolové porty (později, pokud dává smysl). Lokalizace do dalších jazyků v první verzi (kromě češtiny a angličtiny rozhodne se později).

## Poznámky k obsahu
Téma otravy plynem je realistické. Vyvarovat se zobrazení duševních nemocí a samotných škod jako zábavy. Konec 3 nese poselství, že pomoc přichází zvenčí a stojí za to jí důvěřovat.

## Výkon
Cíl: 60 FPS ve 1080p na ASUS Zenbook S14 (integrovaná grafika), mlha a omezený dohled jako optimalizace, nízký počet světel, bake kde to jde.

## Otevřené designové otázky
- Jak zajistit napětí, když chycení hasičem není trest (návrh: tlak z rostoucího CO a rizika „špatných“ bytů, ne z fyzické hrozby).
- Rozsah skrytého konce.
- Definitivní podoba bytu 0.
