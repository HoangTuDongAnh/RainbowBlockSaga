namespace RainbowBlockSaga.Presentation.Scripts.Utils
{
    public static class TimeUtils
    {
        public static string GetTimeString(int hours, int minutes, int seconds)
        {
            // if hours more than 24 return days and hours
            if (hours >= 24)
            {
                var days = hours / 24;
                var h = hours % 24;
                return $"{days}d {h:D2}h";
            }

            return $"{hours:D2}:{minutes:D2}:{seconds:D2}";
        }

        public static string GetTimeString(float time)
        {
            var hours = (int)time / 3600;
            var minutes = (int)(time % 3600) / 60;
            var seconds = (int)time % 60;
            return GetTimeString(hours, minutes, seconds);
        }

        public static string GetTimeString(float time, float activeTimeLimit, bool descendant = true)
        {
            return GetTimeString(descendant ? activeTimeLimit - time % activeTimeLimit : time % activeTimeLimit);
        }

        public static float GetTimeInSeconds(string timeString)
        {
            var time = timeString.Split(':');
            if (time.Length == 3)
            {
                return GetTimeInSeconds(int.Parse(time[0]), int.Parse(time[1]), int.Parse(time[2]));
            }

            return 0;
        }

        public static float GetTimeInSeconds(int hours, int minutes, int seconds)
        {
            return hours * 3600 + minutes * 60 + seconds;
        }
    }
}