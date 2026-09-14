using Core.Interfaces.Services;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;

namespace Infrastructure.Services
{
    /// <summary>
    /// Loads .html/.txt files from the EmailTemplates folder and fills in {{Token}}
    /// placeholders. Files are cached in memory after their first read per process.
    /// </summary>
    public class EmailTemplateRenderer : IEmailTemplateRenderer
    {
        private static readonly ConcurrentDictionary<string, string> Cache = new();
        private static readonly Regex ConditionalPattern =
            new(@"\{\{#if (\w+)\}\}(.*?)\{\{/if\}\}", RegexOptions.Singleline | RegexOptions.Compiled);
        private static readonly Regex TokenPattern =
            new(@"\{\{(\w+)\}\}", RegexOptions.Compiled);

        private readonly string _templatesRoot;

        public EmailTemplateRenderer()
        {
            _templatesRoot = ResolveTemplatesRoot();
        }

        public string Render(string templateFileName, IReadOnlyDictionary<string, string?> tokens)
        {
            var raw = LoadTemplate(templateFileName);
            return Apply(raw, tokens);
        }

        private string LoadTemplate(string templateFileName)
        {
            return Cache.GetOrAdd(templateFileName, name =>
            {
                var path = Path.Combine(_templatesRoot, name);

                if (!File.Exists(path))
                {
                    throw new FileNotFoundException(
                        $"Email template '{name}' was not found at '{path}'. " +
                        "See STAFF_PORTAL_SETUP.md (\"Email templates\") for how EmailTemplates/ " +
                        "needs to be deployed alongside the app.",
                        path);
                }

                return File.ReadAllText(path);
            });
        }

        private static string Apply(string template, IReadOnlyDictionary<string, string?> tokens)
        {
            var withConditionalsResolved = ConditionalPattern.Replace(template, match =>
            {
                var key = match.Groups[1].Value;
                var body = match.Groups[2].Value;
                var include = tokens.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value);
                return include ? body : string.Empty;
            });

            return TokenPattern.Replace(withConditionalsResolved, match =>
            {
                var key = match.Groups[1].Value;
                return tokens.TryGetValue(key, out var value) ? value ?? string.Empty : string.Empty;
            });
        }

        private static string ResolveTemplatesRoot()
        {
            var candidates = new[]
            {
                Path.Combine(Directory.GetCurrentDirectory(), "EmailTemplates"),
                Path.Combine(AppContext.BaseDirectory, "EmailTemplates"),
            };

            foreach (var candidate in candidates)
            {
                if (Directory.Exists(candidate)) return candidate;
            }

            // Doesn't exist yet in either place - return the dotnet-run-style path so
            // the FileNotFoundException above points somewhere sensible.
            return candidates[0];
        }

    }
}