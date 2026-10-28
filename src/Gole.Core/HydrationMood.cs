namespace Gole.Core;

public enum HydrationMood
{
    Happy, Content, Neutral, Sad, Crying, Dead
}

public static class HydrationExpression
{
    public static HydrationMood AtLevel(double level) => level switch
    {
        > .8 => HydrationMood.Happy,
        > .6 => HydrationMood.Content,
        > .4 => HydrationMood.Neutral,
        > .2 => HydrationMood.Sad,
        > 0 => HydrationMood.Crying,
        _ => HydrationMood.Dead
    };

    public static string Label(HydrationMood mood) => mood switch
    {
        HydrationMood.Happy => "Feliz",
        HydrationMood.Content => "De boa",
        HydrationMood.Neutral => "Sério",
        HydrationMood.Sad => "Triste",
        HydrationMood.Crying => "Chorando",
        _ => "Morto"
    };
}
