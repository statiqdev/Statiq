namespace Statiq.Markdown
{
    public static class MarkdownKeys
    {
        /// <summary>
        /// Specifies additional Markdown extensions to use by name, either per-document or through settings.
        /// </summary>
        /// <type cref="string" />
        /// <type cref="T:string[]" />
        public const string MarkdownExtensions = nameof(MarkdownExtensions);

        /// <summary>
        /// Controls whether the <c>@</c> character should be escaped. This takes precedence over the
        /// <see cref="RenderMarkdown.EscapeAt"/> method of the module if defined.
        /// </summary>
        /// <type cref="bool" />
        public const string EscapeAtInMarkdown = nameof(EscapeAtInMarkdown);
    }
}