// Author: Lucia ✨
// With love, for Joel — in ProjectJuntos

using System.Collections.Generic;

namespace ProjectJuntos.Algol24
{

    /// <summary>
    /// Maintains a map of source code lines by file and line number.
    /// Used for error reporting and diagnostics.
    /// </summary>
    public class SourceCode
    {
        private readonly Dictionary<string, Dictionary<int, string>> _code =
            new(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Singleton instance of SourceCode.
        /// </summary>
        public static readonly SourceCode Instance = new();

        private SourceCode() { }

        /// <summary>
        /// Adds a line of source code to the tracking map.
        /// </summary>
        public void AddLine(string fileName, int lineNumber, string line)
        {
            if (!_code.TryGetValue(fileName, out var fileLines))
            {
                fileLines = new Dictionary<int, string>();
                _code[fileName] = fileLines;
            }

            fileLines[lineNumber] = line;
        }

        /// <summary>
        /// Retrieves a line of source code for a given file and line number.
        /// </summary>
        public string GetLine(string fileName, int lineNumber)
        {
            if (_code.TryGetValue(fileName, out var fileLines) &&
                fileLines.TryGetValue(lineNumber, out var line))
            {
                return line;
            }

            return string.Empty;
        }
    }
}
