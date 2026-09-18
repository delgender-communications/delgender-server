namespace Application.Utilities
{
    public static class UserAgentParser
    {
        public static string Describe(string? userAgent)
        {
            if (string.IsNullOrWhiteSpace(userAgent))
            {
                return "Unknown device";
            }

            var ua = userAgent;

            var device = ua switch
            {
                _ when ua.Contains("iPad") => "iPad",
                _ when ua.Contains("iPhone") => "iPhone",
                _ when ua.Contains("Android") => "Android device",
                _ when ua.Contains("Macintosh") || ua.Contains("Mac OS X") => "Mac",
                _ when ua.Contains("Windows") => "Windows PC",
                _ when ua.Contains("Linux") => "Linux PC",
                _ => "Unknown device"
            };

            var browser = ua switch
            {
                _ when ua.Contains("EdgA") || ua.Contains("Edg/") || ua.Contains("Edge/") => "Edge",
                _ when ua.Contains("OPR/") || ua.Contains("Opera") => "Opera",
                _ when ua.Contains("CriOS") || ua.Contains("Chrome/") => "Chrome",
                _ when ua.Contains("FxiOS") || ua.Contains("Firefox/") => "Firefox",
                _ when ua.Contains("Safari/") => "Safari",
                _ => null
            };

            return browser is null ? device : $"{device} · {browser}";
        }
    }
}
