# HANDOFF

Přepisuje `/end`. Krátké. Historie je v gitu.

## Stav
Session 0 hotová (20. 11. 2026). Unity projekt existuje, baseline URP nastavený, tag `m0-start`. Milník M0 začíná.

## Hotovo
- T-003: projekt `Vchod-C` (Unity 6000.3.29f1 LTS, šablona Universal 3D / `urp-blank`), lokální git + Unity `.gitignore` (+ `/BugReports/`, `/PlaytestLogs/`, `/Builds/`), přeneseny `CLAUDE.md`, `.claude/`, `docs/`. Remote `origin` = GitHub `gasiunasdominik-lgtm/horror_game`.
- T-004: baseline v `Assets/Scenes/SampleScene.unity`:
  - Global Volume, profil `Assets/Settings/SampleSceneProfile.asset`: Tonemapping ACES, Bloom 0.9/0.6/0.7, Vignette 0.3, Color Adjustments (kontrast +12, saturace −15), Film Grain Medium1 0.2. Motion Blur vypnutý.
  - Main Camera: perspektivní, post-processing zapnutý, FXAA. Directional Light s měkkými stíny.
  - Kvalita: tier `PC` (PC_RPAsset, HDR) aktivní a výchozí pro Standalone.
  - UI stack: uGUI Canvas `UI` (Screen Space Overlay, škálování 1920×1080) + `EventSystem` s `InputSystemUIInputModule`, TMP Essentials importované.
- Ověřeno: `unity recompile` 0 chyb/0 varování, konzole bez Errorů, screenshot osvětlený (testovací geometrie po screenshotu odstraněna).

## Rozpracováno
- nic

## Další krok
T-005 (debug a QA infrastruktura), pak T-006 až T-009 podle `docs/TASKS.md`.

## Dominik: otestuj
- Otevři projekt v Unity (Editor 6000.3.29f1) → otevře se bez chyb v konzoli, scéna `SampleScene`.

## Otevřené otázky (s výchozími odpověďmi)
- Q: Aktualizovat Unity CLI 1.0.0 → 1.0.1? Výchozí: ano, při příští session (`unity self-update`).

## Poznámky
- Referenční stroj: ASUS Zenbook S14, integrovaná grafika.
- Prostředí: Claude běží ve WSL, Unity CLI je Windows binárka → volá se `unity.exe`, cesty pro Unity ve formátu `C:\...`.
- Unity Cloud nepropojen (`--no-cloud`): při vytváření hlásil `NETWORK_UNREACHABLE`; hra cloud služby nepotřebuje.
- Šablona přidala `com.unity.cinemachine` 3.1.7 a `com.unity.pipeline` 1.0.1 (Pipeline je potřeba pro ovládání Editoru z CLI).
- Windows long paths jsou vypnuté (`unity doctor` varování); zatím bez dopadu.
