using System;

namespace Gole.Desktop;

internal readonly record struct CharacterPose(double ScaleX, double ScaleY, double Lift, double Lean,
    double LeftArm, double RightArm, bool Blink, double Energy);

internal static class CharacterMotion
{
    public const double CelebrationDuration = 2.4;

    public static CharacterPose Sample(double seconds, double level, bool enabled, bool paused, bool celebrating, double celebrationSeconds)
    {
        level = double.IsFinite(level) ? Math.Clamp(level, 0, 1) : 1;
        double restingLean = level == 0 && !paused ? -7 : 0;
        if (!enabled || paused)
            return new CharacterPose(1, 1, 0, restingLean, 0, 0, false, 0);
        double t = double.IsFinite(seconds) ? Math.Max(0, seconds) : 0;
        double energy = level == 0 ? 0 : .25 + level * .75;
        double breath = Math.Sin(t * (1.6 + energy * .6)) * (.003 + energy * .01);
        double lift = level > .8 ? -Math.Max(0, Math.Sin(t * 1.7)) * 2.8 : 0;
        double lean = restingLean + Math.Sin(t * .95) * (level == 0 ? .4 : .6 + energy);
        if (level > 0 && level <= .4)
            lean += 1.5;
        if (level > 0 && level <= .2)
            lean += Math.Sin(t * 5) * .35;
        double leftArm = Math.Sin(t * 1.6) * energy * 3;
        double rightArm = Math.Sin(t * 1.6 + .6) * energy * 4;
        // A brief wave, with quiet time between gestures.
        double waveTime = t % 11;
        if (level > .6 && waveTime is > 2 and < 3.8)
            rightArm += Math.Sin((waveTime - 2) * Math.PI / 1.8) * Math.Sin((waveTime - 2) * 10) * 15;
        double sx = 1 - breath * .5, sy = 1 + breath;
        if (celebrating && double.IsFinite(celebrationSeconds) && celebrationSeconds >= 0 && celebrationSeconds < CelebrationDuration)
        {
            double c = celebrationSeconds;
            double envelope = Math.Sin(Math.PI * c / CelebrationDuration);
            lift -= Math.Abs(Math.Sin(c * 7)) * 12 * envelope;
            sx += Math.Sin(c * 14) * .027 * envelope;
            sy -= Math.Sin(c * 14) * .027 * envelope;
            lean += Math.Sin(c * 7) * 3 * envelope;
            leftArm -= 16 * envelope;
            rightArm -= 18 * envelope;
        }
        bool blink = t % 4.7 > 4.52;
        return new CharacterPose(sx, sy, lift, Math.Clamp(lean, -8.5, 5), leftArm, rightArm, blink, energy);
    }
}
