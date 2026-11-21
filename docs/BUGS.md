# BUGS

Severity: S1 pád/nehratelné · S2 rozbité jádro · S3 kosmetika/okrajové · S4 nápad (patří do TASKS).
Stavy: `OPEN` · `REPRO` (reprodukováno) · `NOREPRO` · `FIXED` (čeká na ověření Dominikem) · `VERIFIED`.

## Šablona
```
### B-nnn — krátký název
- Severity: S?
- Stav: OPEN
- Kdy/kde: build, milník, složka v BugReports/ (pokud je)
- Seed: ...
- Kroky: 1) ... 2) ...
- Očekáváno: ...
- Skutečnost: ...
- Příčina: (doplní Claude)
- Oprava: commit, test nebo bod v regresním seznamu
```

## Záznamy

### B-001 — Spínač světla nejde zapnout
- Severity: S2
- Stav: VERIFIED (21. 11. 2026)
- Kdy/kde: m0, playtest #1 (T-010), Editor
- Seed: libovolný
- Kroky: 1) Play 2) najít spínač ve tmě 3) E
- Očekáváno: spínač ve tmě svítí, E rozsvítí chodbu
- Skutečnost: spínač je ve tmě neviditelný, hráč ho nenajde
- Příčina: URP Lit materiál vytvořený kódem ztratil klíčové slovo `_EMISSION`, takže spínač, žárovky ani okna nesvítily. Raycast a E fungovaly (ověřeno).
- Oprava: svítící materiály (`M_LightButton`, `M_LampOn`, `M_WindowNight`) přes Unlit shader s HDR barvou; větší kolizní box spínače (0,45 × 0,5 m); dosah interakce 2,2 → 2,6 m. Test `GreyboxMaterialTests`.

### B-002 — Bez světla příliš velká tma
- Severity: S3
- Stav: VERIFIED (21. 11. 2026)
- Kdy/kde: m0, playtest #1 (T-010)
- Kroky: 1) Play, nerozsvěcet
- Očekáváno: hůř se orientuješ, ale chodba je čitelná (GDD)
- Skutečnost: skoro černý obraz
- Příčina: ambient 0,05 + tonemapping ACES a kontrast potlačily tmavé tóny
- Oprava: ambient 0,05 → 0,20 (modravý), okna slabě svítí. Bod v regresním seznamu `TESTING.md`.

