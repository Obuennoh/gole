using System.Globalization;
using System.Runtime.Serialization;

namespace Gole.Core;

[DataContract]
public sealed class HydrationState
{
    public const int CurrentVersion = 11;
    public const int DefaultServingMl = 200;
    public const int MinimumServingMl = 1;
    public const int MaximumServingMl = 6000;
    public const int DefaultInterval = 30;
    public const int PhraseInterval = 5;

    [DataMember] public string Name { get; set; } = "Gole";
    [DataMember] public string Color { get; set; } = "#26B9C4";
    [DataMember(EmitDefaultValue = false)]
    public int Accessory
    {
        get; set;
    }
    [DataMember] public List<int> Accessories { get; set; } = [];
    [DataMember]
    public long NextPhrase
    {
        get; set;
    }
    [DataMember] public int Version { get; set; } = CurrentVersion;
    [DataMember]
    public int Vessel
    {
        get; set;
    }
    [DataMember] public int Interval { get; set; } = DefaultInterval;
    [DataMember] public int Goal { get; set; } = 2000;
    [DataMember] public int ServingMl { get; set; } = DefaultServingMl;
    [DataMember]
    public int PersonalityId
    {
        get; set;
    }
    [DataMember] public double Size { get; set; } = 180;
    [DataMember] public double X { get; set; } = -99999;
    [DataMember] public double Y { get; set; } = -99999;
    [DataMember]
    public bool Sound
    {
        get; set;
    }
    [DataMember] public bool Notifications { get; set; } = true;
    [DataMember] public bool Motion { get; set; } = true;
    [DataMember] public bool SpeechSound { get; set; } = true;
    [DataMember]
    public int AlertSoundId
    {
        get; set;
    } = SoundCatalog.DefaultId;
    [DataMember] public int AlertVolume { get; set; } = SoundCatalog.DefaultVolume;
    [DataMember]
    public long LastDrink
    {
        get; set;
    }
    [DataMember]
    public long VisualAnchor
    {
        get; set;
    }
    [DataMember]
    public long NextReminder
    {
        get; set;
    }
    [DataMember]
    public long PauseStart
    {
        get; set;
    }
    [DataMember]
    public long PauseEnd
    {
        get; set;
    }
    [DataMember] public List<Sip> Sips { get; set; } = [];

    public static HydrationState Fresh(DateTime utcNow) => new()
    {
        VisualAnchor = utcNow.Ticks,
        NextReminder = utcNow.AddMinutes(DefaultInterval).Ticks,
        NextPhrase = utcNow.AddMinutes(PhraseInterval).Ticks
    };

    // DataContractJsonSerializer bypasses constructors. Seed optional properties explicitly.
    [OnDeserializing]
    private void InitializeDefaults(StreamingContext _)
    {
        Name = "Gole";
        Color = "#26B9C4";
        Interval = DefaultInterval;
        Goal = 2000;
        ServingMl = DefaultServingMl;
        Size = 180;
        X = Y = -99999;
        Notifications = Motion = SpeechSound = true;
        AlertSoundId = SoundCatalog.DefaultId;
        AlertVolume = SoundCatalog.DefaultVolume;
        Sips = [];
        Accessories = [];
    }

    public void Validate(DateTime utcNow)
    {
        if (Version < 2)
        {
            Interval = DefaultInterval;
            VisualAnchor = utcNow.Ticks;
            NextReminder = utcNow.AddMinutes(Interval).Ticks;
            PauseStart = PauseEnd = 0;
        }
        if (Version < 3 && (Accessories is null || Accessories.Count == 0) && Accessory > 0)
            Accessories = [Accessory];
        Accessories = PetCatalog.Normalize(Accessories);
        Accessory = 0;
        Version = CurrentVersion;
        Name = string.IsNullOrWhiteSpace(Name) ? "Gole" : Name.Trim()[..Math.Min(20, Name.Trim().Length)];
        if (!IsColor(Color))
            Color = "#26B9C4";
        Color = Color.ToUpperInvariant();
        Interval = Math.Clamp(Interval, 5, 180);
        Goal = Math.Clamp(Goal, 250, 6000);
        AlertSoundId = SoundCatalog.Normalize(AlertSoundId);
        PersonalityId = PersonalityCatalog.Normalize(PersonalityId);
        AlertVolume = Math.Clamp(AlertVolume, 0, 100);
        if (ServingMl is < MinimumServingMl or > MaximumServingMl)
            ServingMl = DefaultServingMl;
        Size = double.IsFinite(Size) ? Math.Clamp(Size, 100, 280) : 180;
        Vessel = Math.Clamp(Vessel, 0, PetCatalog.Models.Count - 1);
        if (VisualAnchor <= 0 || VisualAnchor > utcNow.AddMinutes(5).Ticks)
            VisualAnchor = utcNow.Ticks;
        if (NextReminder <= 0 || NextReminder > utcNow.AddDays(2).Ticks)
            NextReminder = utcNow.AddMinutes(Interval).Ticks;
        if (NextPhrase <= 0 || NextPhrase > utcNow.AddMinutes(PhraseInterval * 2).Ticks)
            NextPhrase = utcNow.AddMinutes(PhraseInterval).Ticks;
        if (PauseEnd > utcNow.AddDays(2).Ticks || PauseStart > utcNow.Ticks || PauseStart < 0 || PauseEnd < PauseStart || (PauseEnd > 0 && PauseStart == 0))
            PauseStart = PauseEnd = 0;
        if (LastDrink < 0 || LastDrink > utcNow.AddMinutes(5).Ticks)
            LastDrink = 0;
        Sips ??= [];
        Sips = Sips.Where(s => s is not null && s.Utc > 0 && s.Utc <= utcNow.AddMinutes(5).Ticks && s.Ml is >= MinimumServingMl and <= MaximumServingMl).ToList();
        if (!double.IsFinite(X))
            X = -99999;
        if (!double.IsFinite(Y))
            Y = -99999;
    }

    public static bool IsColor(string? value) => value is { Length: 7 } && value[0] == '#'
        && value.Skip(1).All(char.IsAsciiHexDigit);

    public bool Paused(DateTime utcNow) => PauseEnd > utcNow.Ticks;

    public double Level(DateTime utcNow)
    {
        var ticks = Paused(utcNow) ? PauseStart : utcNow.Ticks;
        var anchor = VisualAnchor;
        if (PauseEnd > 0 && !Paused(utcNow))
            anchor += Math.Max(0, PauseEnd - PauseStart);
        return Math.Clamp(1 - (ticks - anchor) / (double)TimeSpan.FromMinutes(Interval).Ticks, 0, 1);
    }

    public void Resume(DateTime utcNow)
    {
        if (PauseEnd == 0)
            return;
        var end = Math.Min(PauseEnd, utcNow.Ticks);
        VisualAnchor = Math.Min(utcNow.Ticks, VisualAnchor + Math.Max(0, end - PauseStart));
        PauseStart = PauseEnd = 0;
        NextReminder = Math.Max(utcNow.Ticks, VisualAnchor + TimeSpan.FromMinutes(Interval).Ticks);
    }

    public void Pause(DateTime utcNow, int minutes)
    {
        if (minutes is < 1 or > 1440)
            throw new ArgumentOutOfRangeException(nameof(minutes));
        if (PauseEnd != 0)
            Resume(utcNow);
        PauseStart = utcNow.Ticks;
        PauseEnd = utcNow.AddMinutes(minutes).Ticks;
        NextReminder = PauseEnd + TimeSpan.FromMinutes(Interval).Ticks - (PauseStart - VisualAnchor);
        NextReminder = Math.Max(PauseEnd, NextReminder);
    }

    public void SetInterval(DateTime utcNow, int minutes)
    {
        Interval = Math.Clamp(minutes, 5, 180);
        var remaining = Math.Max(0, TimeSpan.FromMinutes(Interval).Ticks - Math.Max(0, (Paused(utcNow) ? PauseStart : utcNow.Ticks) - VisualAnchor));
        NextReminder = (Paused(utcNow) ? PauseEnd : utcNow.Ticks) + remaining;
    }

    public Sip Drink(DateTime utcNow)
    {
        if (ServingMl is < MinimumServingMl or > MaximumServingMl)
            throw new ArgumentOutOfRangeException(nameof(ServingMl));
        if (PauseEnd > 0 && !Paused(utcNow))
            Resume(utcNow);
        var sip = new Sip { Utc = utcNow.Ticks, Ml = ServingMl };
        Sips.Add(sip);
        LastDrink = utcNow.Ticks;
        VisualAnchor = utcNow.Ticks;
        if (Paused(utcNow))
            PauseStart = utcNow.Ticks;
        NextReminder = (Paused(utcNow) ? PauseEnd : utcNow.Ticks) + TimeSpan.FromMinutes(Interval).Ticks;
        NextPhrase = utcNow.AddMinutes(PhraseInterval).Ticks;
        return sip;
    }

    public int Today(DateTime utcNow, TimeZoneInfo? zone = null)
    {
        zone ??= TimeZoneInfo.Local;
        var date = TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utcNow, DateTimeKind.Utc), zone).Date;
        return Sips.Where(s => TimeZoneInfo.ConvertTimeFromUtc(new DateTime(s.Utc, DateTimeKind.Utc), zone).Date == date).Sum(s => s.Ml);
    }
}
