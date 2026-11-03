using System;
using System.Diagnostics;
using System.Globalization;
using UnityEngine;

namespace RainbowBlockSaga.Presentation.Scripts.System
{
    // Uses a monotonic clock alongside UTC to reject manual clock jumps while
    // the device stays powered on. No offline clock can prove time after a reset.
    public static class RewardClock
    {
        const string WallKey = "RBS_RewardClockUtc";
        const string MonotonicKey = "RBS_RewardClockMonotonic";
        const double ToleranceSeconds = 120;

        public static bool CanClaimToday(string claimKey)
        {
            if (!Observe()) return false;
            if (!PlayerPrefs.HasKey(claimKey)) return true;
            string storedClaim = PlayerPrefs.GetString(claimKey);
            bool parsed = storedClaim.Contains("T")
                ? DateTime.TryParse(storedClaim, CultureInfo.InvariantCulture,
                    DateTimeStyles.RoundtripKind, out var lastClaim)
                : DateTime.TryParse(storedClaim, CultureInfo.CurrentCulture,
                    DateTimeStyles.AssumeLocal, out lastClaim);
            if (!parsed) return false;
            return DateTime.UtcNow.Date > lastClaim.ToUniversalTime().Date;
        }

        public static void MarkClaimedToday(string claimKey)
        {
            PlayerPrefs.SetString(claimKey, DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture));
            PlayerPrefs.Save();
        }

        static bool Observe()
        {
            double monotonic = (double)Stopwatch.GetTimestamp() / Stopwatch.Frequency;
            DateTime utc = DateTime.UtcNow;
            if (PlayerPrefs.HasKey(WallKey) && PlayerPrefs.HasKey(MonotonicKey) &&
                DateTime.TryParse(PlayerPrefs.GetString(WallKey), CultureInfo.InvariantCulture,
                    DateTimeStyles.RoundtripKind, out var previousUtc) &&
                double.TryParse(PlayerPrefs.GetString(MonotonicKey), NumberStyles.Float,
                    CultureInfo.InvariantCulture, out var previousMonotonic))
            {
                double wallElapsed = (utc - previousUtc.ToUniversalTime()).TotalSeconds;
                double monotonicElapsed = monotonic - previousMonotonic;
                if (wallElapsed < -ToleranceSeconds ||
                    monotonicElapsed >= 0 && wallElapsed > monotonicElapsed + ToleranceSeconds ||
                    monotonicElapsed < 0 && wallElapsed < TimeSpan.FromHours(20).TotalSeconds)
                    return false;
            }

            PlayerPrefs.SetString(WallKey, utc.ToString("o", CultureInfo.InvariantCulture));
            PlayerPrefs.SetString(MonotonicKey, monotonic.ToString("R", CultureInfo.InvariantCulture));
            PlayerPrefs.Save();
            return true;
        }
    }
}
