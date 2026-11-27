using System.Collections.Generic;

namespace Vchod.Systems
{
    public enum StoryKind { Note = 0, Photo = 1, Sound = 2 }

    public sealed class StoryEntry
    {
        public readonly int Id;
        public readonly int Tier;
        public readonly StoryKind Kind;
        public readonly string Title;
        public readonly string Text;

        public StoryEntry(int id, int tier, StoryKind kind, string title, string text)
        {
            Id = id;
            Tier = tier;
            Kind = kind;
            Title = title;
            Text = text;
        }
    }

    /// <summary>
    /// The 18 story pieces (GDD: 15–20). Tier 1 is everyday and ambiguous, tier 2 links the oddities,
    /// tier 3 tells the truth (canon in GDD). Nothing is explained by mental illness: the cause is
    /// physical and checkable. The player character stays gender-neutral.
    /// </summary>
    public static class StoryCatalog
    {
        public static readonly IReadOnlyList<StoryEntry> All = new[]
        {
            // Tier 1: an ordinary panelák at night.
            new StoryEntry(1, 1, StoryKind.Note, "Vyhláška u schránek",
                "Upozorňujeme nájemníky, že revize plynových spotřebičů proběhne v pondělí od 8:00. Prosíme o zpřístupnění bytů.\n\nVýbor SVJ\n\n(připsáno propiskou: Byt 2.3 opět nezastižen!)"),
            new StoryEntry(2, 1, StoryKind.Note, "Lístek za dveřmi",
                "Paní Vránová, ten ohřívač u Vás zase bouchá, slyším to přes zeď. Ozvěte se mi prosím.\n\nTomáš, 2.4"),
            new StoryEntry(3, 1, StoryKind.Photo, "Fotka z léta",
                "Trochu rozmazaná fotka vchodu C z léta. Na lavičce před domem sedí paní s dortem na klíně a směje se do objektivu."),
            new StoryEntry(4, 1, StoryKind.Note, "Rozpis úklidu chodby",
                "1. patro: sudé týdny. 2. patro: liché týdny.\n\n(dole, jiným písmem) Kdo zase nechal otevřené okno na chodbě?! Táhne to.\n(pod tím) Já. Bez okna se tu nedá dýchat."),
            new StoryEntry(5, 1, StoryKind.Sound, "Záznamník za dveřmi",
                "[šum, pípnutí] „Mami, to jsem já. Zase tě bolí hlava? Otevři si okno a zavolej mi, jo? ... Mami?“ [pípnutí]"),
            new StoryEntry(6, 1, StoryKind.Note, "Účtenka v kapse",
                "Bistro U Nádraží, 04:12.\nKáva velká, rohlík se šunkou.\n\nNa rubu tvým písmem: druhá noční za sebou. Hlavně se vyspat."),

            // Tier 2: the oddities start to connect.
            new StoryEntry(7, 2, StoryKind.Note, "Utržený návod",
                "...detektor CO. Při spuštění alarmu ihned opusťte prostor, větrejte a volejte 150.\n\nOxid uhelnatý nemá barvu ani zápach. Příznaky otravy: bolest hlavy, závrať, zmatenost, únava, zúžené vidění..."),
            new StoryEntry(8, 2, StoryKind.Photo, "Polaroid na nástěnce",
                "Sousedé na chodbě drží dort se svíčkami a cedulku „Paní Vránové 80!“. Na rubu je datum: před třemi lety. Ten dort voní stejně jako dnes v noci."),
            new StoryEntry(9, 2, StoryKind.Sound, "Interkom u výtahu",
                "[chrčení] „...výtah je mimo provoz. Na pokyn hasičů ho nepoužívejte. Opakuji, výtah je mimo...“ [ticho]"),
            new StoryEntry(10, 2, StoryKind.Note, "Vzkaz na nástěnce",
                "ZTRATIL SE MI MÍČ. Je modrý a trochu splasklý. Kdo ho najde, ať ho vrátí do 1. patra.\n\nKuba (8 let)\n\n(připsáno) Kubo, v noci se nekope do zdi!"),
            new StoryEntry(11, 2, StoryKind.Note, "Zmačkaná lékařská zpráva",
                "...pacientka uvádí opakované bolesti hlavy a únavu, horší večer a doma, lepší po pobytu venku. Doporučeno vyšetření, zvážit vliv prostředí bytu..."),
            new StoryEntry(12, 2, StoryKind.Photo, "Fotka ohřívače",
                "Fotka starého plynového ohřívače v malé koupelně. Nad hořákem je plášť černý od sazí. Někdo k fotce připsal: „Takhle to přece nejde.“"),

            // Tier 3: the truth.
            new StoryEntry(13, 3, StoryKind.Note, "Protokol z revize (kopie)",
                "Byt 2.3, průtokový ohřívač vody: nedostatečný odtah spalin, únik do ventilační šachty.\nDoporučení: ihned odstavit z provozu.\n\nPodpis nájemníka: odmítnut."),
            new StoryEntry(14, 3, StoryKind.Sound, "Vysílačka na podestě",
                "[šum] „Centrálo, tady dvojka. Ve druhém patře naměřeno čtyři sta ppm, šachta je plná. Jeden pohřešovaný, nájemník z 2.1, nebere telefon. Hledám dál.“ [šum]"),
            new StoryEntry(15, 3, StoryKind.Note, "Přepis tísňového volání",
                "02:47, žena, vchod C, 2. patro:\n„Pípá mi to tady a strašně mě bolí hlava. A soused od vedle ještě nepřišel z práce, on chodí pozdě...“"),
            new StoryEntry(16, 3, StoryKind.Photo, "Odložená přilba",
                "Na podestě leží hasičská přilba s čelovkou. Na boku je fixou napsáno HZS a jméno. Vedle ní je dýchací maska. Nic děsivého. Jen těžká."),
            new StoryEntry(17, 3, StoryKind.Note, "Seznam evakuace",
                "Vchod C\n1.1 ✓  1.2 ✓  1.3 ✓  1.4 ✓\n2.2 ✓ (p. Vránová, převoz)  2.3 ✓  2.4 ✓\n2.1 NEZASTIŽEN – noční směna? HLEDÁME."),
            new StoryEntry(18, 3, StoryKind.Note, "Lístek v kapse",
                "Tvým písmem, z minulého týdne:\n„Nezapomenout: zavolat paní Vránové kvůli tomu pípání u ní. Už je to týden.“"),
        };

        public static StoryEntry ById(int id)
        {
            foreach (var e in All)
                if (e.Id == id) return e;
            return null;
        }
    }

    /// <summary>
    /// Which pieces a round shows. A deeper tier opens after enough pieces of the one before, so
    /// repeated rounds reveal more (GDD). Uncollected pieces first; collected ones only as filler.
    /// </summary>
    public static class StorySelection
    {
        public const int UnlockPerTier = 4;

        public static int OpenTier(ICollection<int> collected)
        {
            var tier = 1;
            while (tier < 3)
            {
                var have = 0;
                foreach (var e in StoryCatalog.All)
                    if (e.Tier == tier && collected.Contains(e.Id)) have++;
                if (have < UnlockPerTier) break;
                tier++;
            }
            return tier;
        }

        public static List<int> Pick(int roundSeed, ICollection<int> collected, int count)
        {
            var tier = OpenTier(collected);
            var fresh = new List<int>();
            var seen = new List<int>();
            foreach (var e in StoryCatalog.All)
            {
                if (e.Tier > tier) continue;
                (collected.Contains(e.Id) ? seen : fresh).Add(e.Id);
            }
            var rng = new System.Random(Core.SeedService.DeriveSeed(roundSeed, "story", 0));
            Shuffle(rng, fresh);
            Shuffle(rng, seen);
            // The newest tier first among the fresh ones, so progress is felt.
            fresh.Sort((a, b) => StoryCatalog.ById(b).Tier.CompareTo(StoryCatalog.ById(a).Tier));
            var result = new List<int>(count);
            foreach (var id in fresh) { if (result.Count < count) result.Add(id); }
            foreach (var id in seen) { if (result.Count < count) result.Add(id); }
            return result;
        }

        private static void Shuffle(System.Random rng, List<int> list)
        {
            for (var i = list.Count - 1; i > 0; i--)
            {
                var j = rng.Next(i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }
    }

    /// <summary>Collected pieces across rounds (PlayerPrefs). Small and versioned by key.</summary>
    public static class StoryProgress
    {
        private const string Key = "vchod.story.v1";
        private static HashSet<int> _cache;

        public static HashSet<int> Collected
        {
            get
            {
                if (_cache != null) return _cache;
                _cache = Parse(UnityEngine.PlayerPrefs.GetString(Key, string.Empty));
                return _cache;
            }
        }

        public static bool Add(int id)
        {
            if (!Collected.Add(id)) return false;
            UnityEngine.PlayerPrefs.SetString(Key, Serialize(_cache));
            UnityEngine.PlayerPrefs.Save();
            return true;
        }

        /// <summary>Debug / tests only.</summary>
        public static void ResetAll()
        {
            _cache = new HashSet<int>();
            UnityEngine.PlayerPrefs.DeleteKey(Key);
        }

        public static HashSet<int> Parse(string s)
        {
            var set = new HashSet<int>();
            if (string.IsNullOrEmpty(s)) return set;
            foreach (var part in s.Split(','))
                if (int.TryParse(part, out var id) && StoryCatalog.ById(id) != null) set.Add(id);
            return set;
        }

        public static string Serialize(IEnumerable<int> ids) => string.Join(",", ids);
    }
}
