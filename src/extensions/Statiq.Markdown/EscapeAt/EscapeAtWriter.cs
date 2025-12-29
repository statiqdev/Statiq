using System.IO;
using System.Text;

namespace Statiq.Markdown.EscapeAt
{
    internal class EscapeAtWriter : TextWriter
    {
        // This is a bit of a hack, but to detect if we're in a mailto link, we look for "mailto:" characters
        // and then consider everything until the next quote (single or double) to be an email address.
        private const string MailToPrefix = "mailto:";

        private readonly TextWriter _writer;

        private char _previousChar;

        private bool _processingInstruction;

        private int _mailToPrefixIndex = 0;

        private bool _mailToLink;

        public override Encoding Encoding => _writer.Encoding;

        public EscapeAtWriter(TextWriter writer)
        {
            _writer = writer;
        }

        public override void Write(char value)
        {
            // Are we in a mailto link?
            if (!_mailToLink)
            {
                if (value == MailToPrefix[_mailToPrefixIndex])
                {
                    _mailToPrefixIndex++;
                    if (_mailToPrefixIndex == MailToPrefix.Length)
                    {
                        _mailToLink = true;
                    }
                }
                else
                {
                    _mailToPrefixIndex = 0;
                }
            }
            else if (value == '"' || value == '\'')
            {
                // Exit the mailto link as soon as we see a quote
                _mailToLink = false;
                _mailToPrefixIndex = 0;
            }

            // Escape if we're not in a processing instruction
            if (!_processingInstruction)
            {
                switch (value)
                {
                    case '\\':
                        if (_previousChar == '\\')
                        {
                            // If this is a second, etc. slash, go ahead and write the previous one
                            _writer.Write('\\');
                        }
                        _previousChar = '\\';
                        return;
                    case '@':
                        if (_mailToLink)
                        {
                            // Don't escape @ symbols in mailto links
                            break;
                        }
                        _writer.Write(_previousChar == '\\' ? "@" : "&#64;");
                        _previousChar = '@';
                        return;
                    case '?':
                        if (_previousChar == '<')
                        {
                            _processingInstruction = true;
                        }
                        break;
                    default:
                        if (_previousChar == '\\')
                        {
                            _writer.Write('\\');
                        }
                        break;
                }
            }
            else if (value == '>' && _previousChar == '?')
            {
                _processingInstruction = false;
            }

            _previousChar = value;
            _writer.Write(value);
        }

        public override void Flush()
        {
            if (_previousChar == '\\')
            {
                _writer.Write('\\');
            }
        }
    }
}