namespace Core.Interfaces.Services
{
    public interface IEmailTemplateRenderer
    {
        /// <summary>
        /// Loads EmailTemplates/{templateFileName}, fills in {{Token}} placeholders
        /// from <paramref name="tokens"/>, and returns the result. Values are inserted
        /// as-is (not HTML-escaped) so callers can pass in already-built HTML fragments
        /// (e.g. a rendered body partial) as a token value.
        ///
        /// Supports one simple conditional block form: {{#if Key}}...{{/if}} - the
        /// block is kept only when tokens[Key] is present and non-blank. This is a
        /// small hand-rolled substitution, not a real template engine (no loops) -
        /// enough for these emails without adding a templating library dependency.
        /// </summary>
        string Render(string templateFileName, IReadOnlyDictionary<string, string?> tokens);
    }
}
