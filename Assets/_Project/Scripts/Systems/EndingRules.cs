using Vchod.Core;

namespace Vchod.Systems
{
    /// <summary>Which ending a round-ending event means (GDD: Konce 1–3). Pure, tested.</summary>
    public static class EndingRules
    {
        /// <summary>Caught by the firefighter: at an open window it is fresh air (3), anywhere else evacuation (2).</summary>
        public static EndingId ForCatch(bool atOpenWindow) => atOpenWindow ? EndingId.FreshAir : EndingId.Evacuation;

        public static EndingId ForCollapse() => EndingId.Collapse;
    }

    /// <summary>Ending screen texts. Gender-neutral on purpose: the player character has no set gender.</summary>
    public static class EndingTexts
    {
        public static string Title(EndingId e) => e switch
        {
            EndingId.Collapse => "Konec 1 · Zhroucení",
            EndingId.Evacuation => "Konec 2 · Evakuace",
            EndingId.FreshAir => "Konec 3 · Čerstvý vzduch",
            _ => string.Empty,
        };

        public static string Body(EndingId e, CollapseCause cause) => e switch
        {
            EndingId.Collapse => (cause switch
            {
                CollapseCause.Home => "Klíč, zámek, tma předsíně. Jen si na chvíli lehnout.",
                CollapseCause.Neighbour => "Dort voní sladce. Židle je měkká. Jen na chvíli zavřít oči.",
                _ => "Hučení v uších se změní v ticho. Podlaha je najednou blízko.",
            }) + "\n\nOxid uhelnatý nemá barvu ani zápach. Detektor pípal celou dobu.",
            EndingId.Evacuation =>
                "Ruka v rukavici, maska, těžký dech. A hlas: „Mám tu člověka, vyvádím.“\n\n" +
                "Venku pod dekou to dochází: v masce nebyl vetřelec. Byl to hasič.",
            EndingId.FreshAir =>
                "Studený vzduch pročistí hlavu. Když se ve tmě objeví baterka, poprvé je jasné, komu patří.\n\n" +
                "Pomoc přišla zvenčí. Stálo za to jí věřit.",
            _ => string.Empty,
        };
    }
}
