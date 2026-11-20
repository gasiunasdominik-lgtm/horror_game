# DECISIONS

Zapisuj každé rozhodnutí, které ovlivní hru, nástroje nebo rozsah. Formát: ID, datum, rozhodnutí, důvod, kdo rozhodl (D, C, nebo `default` = výchozí odpověď bez reakce).

| ID | Datum | Rozhodnutí | Důvod | Kdo |
|---|---|---|---|---|
| D-001 | 19. 11. 2026 | Engine Unity (nejnovější LTS), URP | Slabší hardware (Zenbook), C# a Claude spolupráce, nižší vstupní náročnost než Unreal | D |
| D-002 | 19. 11. 2026 | Low-poly / retro vzhled, 1. osoba, PC (Steam) | Výkon a zvládnutelná grafika solo | D |
| D-003 | 19. 11. 2026 | Žánr: krátký horor s pronásledovatelem, jedna lokace (panelák) | Rozsah a znovuhratelnost | D |
| D-004 | 19. 11. 2026 | Příčina divností je fyzická (otrava CO), ne duševní nemoc | Logika, stigmatizace, férovost | D |
| D-005 | 19. 11. 2026 | Pronásledovatel je hasič v dýchacím přístroji (vnímán jako stalker) | Logika odhalení a emoce (strach → úleva) | D |
| D-006 | 19. 11. 2026 | Dům se přeskládá ve tmě (zhasnutí schodišťového světla) | Technické skrytí přechodu a tematická vazba | D |
| D-007 | 19. 11. 2026 | Jazyk komunikace česky, kód a commity anglicky | Efektivita a čitelnost | D |
| D-008 | 19. 11. 2026 | Rozsah: vertikální řez do 28. 2. 2027, brány G1–G3 | Zabránit rozjetí rozsahu | D |
| D-009 | 19. 11. 2026 | Při chybějící odpovědi platí výchozí volba a zapíše se sem | Minimalizace přerušení | D |
| D-010 | | Skrytý konec: obsah a podmínka | Rozhodnout v M2 | otevřeno |
| D-011 | | Definitivní podoba bytu 0 | Rozhodnout v M1/M2 | otevřeno |
| D-012 | 20. 11. 2026 | Unity Editor 6000.3.29f1 (nejnovější LTS k datu založení), šablona Universal 3D (`com.unity.template.urp-blank`) | Zadání: nejnovější LTS; nainstalovaná 6000.6.9f1 je tech stream bez LTS podpory | C |
| D-013 | 20. 11. 2026 | Projekt bez Unity Cloud (`--no-cloud`) | Cloud link při vytváření selhal (`NETWORK_UNREACHABLE`); hra nepotřebuje online služby, záloha přes GitHub | C |
| D-014 | 20. 11. 2026 | Herní UI: uGUI (Canvas + TextMeshPro), EventSystem s `InputSystemUIInputModule` | Výchozí volba skillu `ui` pro nové herní UI; vyzrálé, jednoduché pro menu a HUD | default |
| D-015 | 20. 11. 2026 | Baseline post-processing: ACES, Bloom 0.9/0.6, Vignette 0.3, kontrast +12, saturace −15, Film Grain 0.2 | Tmavší retro tón dle GDD, ale čitelný; hororový tuning až v T-009/M2 | C |
| D-016 | 20. 11. 2026 | Remote `origin` = github.com/gasiunasdominik-lgtm/horror_game, push povolen Dominikem | Záloha | D |
