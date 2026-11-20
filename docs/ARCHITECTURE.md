# ARCHITECTURE

Technická struktura pro Claude. Drž to jednoduché; cílem je rychle iterovat a snadno testovat.

## Základ
- Unity (nejnovější LTS), URP, 3D, nový Input System (pokud template jinak, drž se template). Cíl Windows 64-bit.
- Jedna scéna hry (`Game`), plus `Boot` a `MainMenu`. Moduly domu jsou prefaby sestavované za běhu.
- Veškerá herní konfigurace v ScriptableObjectech, ladění bez změny kódu.

## Složky
```
Assets/_Project/
  Scripts/
    Core/        # GameState, SeedService, EventBus, Services
    Player/      # Controller, kamera, interakce
    Enemy/       # Hasič AI (stavový automat), senzory
    World/       # ModuleLayout, ModuleDefinition, Spojky, OknaSVzduchem
    Systems/     # CoModel, SymptomEffects, LightTimer, EndingsManager
    Narrative/   # StoryPiece, NeighborDefinition, Lore
    Audio/       # AudioDirector, Detector, Dech
    UI/          # Menu, pauza, nastavení (UI Toolkit nebo uGUI dle skillu ui)
    Debug/       # DebugOverlay, BugReporter, Cheats, PlaytestLogger (jen Editor a Development build)
  Prefabs/  Scenes/  Art/  Audio/  Settings/  Data/
  Tests/
    EditMode/
    PlayMode/
```

## Assembly definitions
`Vchod.Core`, `Vchod.Gameplay` (Player, Enemy, World, Systems, Narrative, Audio), `Vchod.UI`, `Vchod.Debug` (závisí na ostatních, nic nezávisí na něm), `Vchod.Tests.EditMode`, `Vchod.Tests.PlayMode`. Debug assembly je zkompilován jen v Editoru a Development buildu.

## Klíčové systémy
- **SeedService:** jeden zdroj náhody (`System.Random` s explicitním seedem). Žádné `UnityEngine.Random` v herní logice. Seed lze zadat přes `-seed N` nebo debug overlay. Stejný seed = stejné rozložení a stopy.
- **ModuleLayout:** sestaví patra z modulů podle seedu a pravidel spojek (sockets). Přeskládání se děje při zhasnutí světla. Výstupem je validní graf (existuje cesta ke dveřím pokoje a k alespoň jednomu oknu).
- **LightTimer:** stav světla na schodišti, události `LightOn`, `LightOff`.
- **CoModel:** čistá logika bez závislosti na Unity (testovatelná v EditMode). Vstup: poloha vs. zdroj, okno, čas. Výstup: hodnota 0–100 a prahy.
- **SymptomEffects:** čte hodnotu CO, řídí URP Volume (vignette, zúžení, zkreslení), audio (dolní propust, hučení) a rychlost pohybu.
- **Hasič AI:** stavový automat (Patrol, Investigate, Chase, Call, Search), senzory (sluch, zrak, světlo), NavMesh (skill `initialize-ai-navigation`, NavMesh se při přeskládání znovu staví nebo používá pevné navmesh moduly).
- **EndingsManager:** vyhodnotí konec podle stavu (zhroucení, evakuace, čerstvý vzduch, skrytý).
- **EventBus:** jednoduché události mezi systémy (žádné singletony ve hře kromě služeb v Core).

## Konvence kódu
- C# anglicky, jmenné prostory `Vchod.*`, jeden `MonoBehaviour` na soubor a soubor se jménem třídy (jinak build crashuje).
- Logika oddělená od MonoBehaviour tam, kde se dá (kvůli testům).
- Žádné magické čísla: konfigurace v ScriptableObjectu.
- Žádné `Find` v `Update`. Žádné alokace v `Update` na hot path.
- Veřejné API drž malé, komentuj proč, ne co.
- Debug kód v `Vchod.Debug`, nikdy v produkční cestě.

## Výkon (rozpočet, měř na Zenbooku)
- 60 FPS ve 1080p, frame time ≤ 16,6 ms.
- Max 4 real-time světla v dohledu, ostatní bake nebo emisivní.
- Textury max 512–1024 px, retro filtr (point), mlha omezuje dohled.
- Měření při každém milníku do `docs/TESTING.md` (sekce Výkon).

## Build
Windows x64, IL2CPP nebo Mono podle skillu. Development Build pro testy (Debug povolen), Release pro vydání (Debug vypnut). Buildy do `Builds/` (v `.gitignore`).
