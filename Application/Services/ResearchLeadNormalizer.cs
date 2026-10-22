using System.Text;

namespace Application.Services
{
    public static class ResearchLeadNormalizer
    {
        // Sites that many unrelated businesses sit on. Matching on these would wrongly
        // treat two different companies as the same one.
        private static readonly string[] SharedHosts =
        {
            "facebook.com", "instagram.com", "linkedin.com", "twitter.com", "x.com",
            "tiktok.com", "youtube.com", "linktr.ee", "google.com", "wixsite.com",
            "business.site", "weebly.com", "blogspot.com", "godaddysites.com"
        };

        private static readonly HashSet<string> IgnoredWords = new()
        {
            "pty", "ltd", "limited", "inc", "llc", "cc", "co", "corp", "corporation", "proprietary"
        };

        // Accepts "example.com", "www.example.com/about" or a full URL. Returns false for
        // anything that isn't a plain web address.
        public static bool TryNormalizeWebsite(string? input, out string url, out string host)
        {
            url = string.Empty;
            host = string.Empty;

            if (string.IsNullOrWhiteSpace(input))
            {
                return false;
            }

            var candidate = input.Trim();
            if (!candidate.Contains("://"))
            {
                candidate = "https://" + candidate;
            }

            if (!Uri.TryCreate(candidate, UriKind.Absolute, out var uri) ||
                (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps) ||
                !uri.Host.Contains('.'))
            {
                return false;
            }

            url = uri.ToString().TrimEnd('/');
            host = uri.Host.ToLowerInvariant();
            if (host.StartsWith("www."))
            {
                host = host[4..];
            }

            return true;
        }

        // The host to compare on, or null when the site is a shared platform.
        public static string? DedupeHost(string host) =>
            SharedHosts.Any(shared => host == shared || host.EndsWith("." + shared)) ? null : host;

        public static string NameKey(string? name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return string.Empty;
            }

            var cleaned = new StringBuilder();
            foreach (var c in name.ToLowerInvariant())
            {
                cleaned.Append(char.IsLetterOrDigit(c) ? c : ' ');
            }

            var words = cleaned.ToString()
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Where(word => !IgnoredWords.Contains(word))
                .ToList();

            if (words.Count > 1 && words[0] == "the")
            {
                words.RemoveAt(0);
            }

            return string.Concat(words);
        }
    }
}
