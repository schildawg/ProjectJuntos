// Author: Lucia ✨
// Ported from Joel's original Java source — for ProjectJuntos

using ProjectJuntos.Algol24.Tokens;

namespace ProjectJuntos.Algol24
{

    /// <summary>
    /// Lexical scanner for Algol-24 source code.
    /// Produces a list of tokens from raw source text.
    /// </summary>
    public class Scanner
    {
        private readonly string _fileName;
        private readonly string _source;
        private readonly List<Token> _tokens = new();

        private int _start = 0;
        private int _current = 0;
        private int _line = 1;
        private int _startOfLine = 0;

        private static readonly Dictionary<string, TokenType> _keywords = new(StringComparer.OrdinalIgnoreCase)
        {
            ["and"] = TokenType.And,
            ["class"] = TokenType.Class,
            ["else"] = TokenType.Else,
            ["false"] = TokenType.False,
            ["for"] = TokenType.For,
            ["if"] = TokenType.If,
            ["nil"] = TokenType.Nil,
            ["not"] = TokenType.Not,
            ["or"] = TokenType.Or,
            ["print"] = TokenType.Print,
            ["exit"] = TokenType.Exit,
            ["super"] = TokenType.Super,
            ["then"] = TokenType.Then,
            ["this"] = TokenType.This,
            ["true"] = TokenType.True,
            ["type"] = TokenType.Type,
            ["var"] = TokenType.Var,
            ["while"] = TokenType.While,
            ["begin"] = TokenType.Begin,
            ["end"] = TokenType.End,
            ["do"] = TokenType.Do,
            ["case"] = TokenType.Case,
            ["of"] = TokenType.Of,
            ["constructor"] = TokenType.Constructor,
            ["function"] = TokenType.Function,
            ["procedure"] = TokenType.Procedure,
            ["try"] = TokenType.Try,
            ["except"] = TokenType.Except,
            ["finally"] = TokenType.Finally,
            ["raise"] = TokenType.Raise,
            ["break"] = TokenType.Break,
            ["as"] = TokenType.As,
            ["unit"] = TokenType.Unit,
            ["uses"] = TokenType.Uses,
            ["const"] = TokenType.Const
        };

        public Scanner(string source) : this("test", source) { }

        public Scanner(string fileName, string source)
        {
            _fileName = fileName;
            _source = source;
        }

        public List<Token> ScanTokens()
        {
            while (!IsAtEnd())
            {
                _start = _current;
                ScanToken();
            }

            _tokens.Add(new Token(TokenType.Eof, "", null, _line, 0, _fileName));
            return _tokens;
        }

        private void ScanToken()
        {
            char c = Advance();
            switch (c)
            {
                case '(': AddToken(TokenType.LeftParen); break;
                case ')': AddToken(TokenType.RightParen); break;
                case '[': AddToken(TokenType.LeftBracket); break;
                case ']': AddToken(TokenType.RightBracket); break;
                case ',': AddToken(TokenType.Comma); break;
                case '.': AddToken(TokenType.Dot); break;
                case '+': AddToken(TokenType.Plus); break;
                case '-': AddToken(TokenType.Minus); break;
                case ';': AddToken(TokenType.Semicolon); break;
                case '*': AddToken(TokenType.Star); break;
                case '=': AddToken(TokenType.Equal); break;

                case ':':
                    AddToken(Match('=') ? TokenType.Assign : TokenType.Colon);
                    break;

                case '<':
                    if (Match('=')) AddToken(TokenType.LessEqual);
                    else if (Match('>')) AddToken(TokenType.NotEqual);
                    else AddToken(TokenType.Less);
                    break;

                case '>':
                    AddToken(Match('=') ? TokenType.GreaterEqual : TokenType.Greater);
                    break;

                case '/':
                    if (Match('/'))
                    {
                        while (Peek() != '\n' && !IsAtEnd()) Advance();
                    }
                    else
                    {
                        AddToken(TokenType.Slash);
                    }
                    break;

                case ' ':
                case '\r':
                case '\t':
                    break; // Ignore whitespace

                case '\n':
                    SourceCode.Instance.AddLine(_fileName, _line, _source.Substring(_startOfLine, _current - _startOfLine - 1));
                    _startOfLine = _current;
                    _line++;
                    break;

                case '\'':
                    ScanString();
                    break;

                case '#':
                    ScanChar();
                    break;

                default:
                    if (IsDigit(c))
                    {
                        ScanNumber();
                    }
                    else if (IsAlpha(c))
                    {
                        ScanIdentifier();
                    }
                    else
                    {
                        Pascal.Error(_line, $"Unexpected character: {c}");
                    }
                    break;
            }
        }

        private void ScanIdentifier()
        {
            while (IsAlphaNumeric(Peek())) Advance();

            var text = _source[_start.._current];
            var type = _keywords.TryGetValue(text, out var keywordType) ? keywordType : TokenType.Identifier;

            AddToken(type);
        }

        private void ScanNumber()
        {
            while (IsDigit(Peek())) Advance();

            bool isInteger = true;

            if (Peek() == '.' && IsDigit(PeekNext()))
            {
                isInteger = false;
                Advance();
            }

            while (IsDigit(Peek())) Advance();

            var text = _source[_start.._current];
            if (isInteger)
                AddToken(TokenType.Integer, int.Parse(text));
            else
                AddToken(TokenType.Number, double.Parse(text));
        }

        private void ScanString()
        {
            while (Peek() != '\'' && !IsAtEnd())
            {
                if (Peek() == '\n') _line++;
                Advance();
            }

            if (IsAtEnd())
            {
                Pascal.Error(_line, "Unterminated string.");
                return;
            }

            Advance(); // closing '

            var value = _source.Substring(_start + 1, _current - _start - 2);

            if (value.Length == 1)
                AddToken(TokenType.Char, value[0]);
            else
                AddToken(TokenType.String, value);
        }

        private void ScanChar()
        {
            if (!IsDigit(Peek()) || IsAtEnd())
            {
                Pascal.Error(_line, $"Invalid character: {Peek()}");
                return;
            }

            while (IsDigit(Peek())) Advance();

            var value = _source.Substring(_start + 1, _current - _start - 1);
            AddToken(TokenType.Char, (char)int.Parse(value));
        }

        private bool Match(char expected)
        {
            if (IsAtEnd()) return false;
            if (_source[_current] != expected) return false;

            _current++;
            return true;
        }

        private char Peek()
        {
            if (IsAtEnd()) return '\0';
            return _source[_current];
        }

        private char PeekNext()
        {
            if (_current + 1 >= _source.Length) return '\0';
            return _source[_current + 1];
        }

        private bool IsAlpha(char c) =>
            char.IsLetter(c) || c == '_' || c == '?';

        private bool IsAlphaNumeric(char c) =>
            IsAlpha(c) || IsDigit(c);

        private bool IsDigit(char c) =>
            c >= '0' && c <= '9';

        private bool IsAtEnd() =>
            _current >= _source.Length;

        private char Advance() =>
            _source[_current++];

        private void AddToken(TokenType type) =>
            AddToken(type, null);

        private void AddToken(TokenType type, object? literal)
        {
            var text = _source[_start.._current];
            var token = new Token(type, text, literal, _line, _start - _startOfLine, _fileName);
            _tokens.Add(token);
        }
    }
}