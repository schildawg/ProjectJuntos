using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using ProjectJuntos.Algol24.Tokens;
using ProjectJuntos.Algol24.NativeFunctions;

namespace ProjectJuntos.Algol24
{

    public class Parser
    {
        // private static readonly bool DEBUG = false;
        private readonly bool _synchronize;
        private int _loopDepth = 0;

        private static readonly HashSet<string> Uses = new();
        private readonly List<ParseError> _errors = new();

        private readonly List<Token> _tokens;
        private int _current = 0;


        private bool Check(TokenType type, string lexeme)
        {
            return !IsAtEnd() && Peek().Type == type && Peek().Lexeme.Equals(lexeme, StringComparison.OrdinalIgnoreCase);
        }

        private static readonly HashSet<string> uses = new();
        public readonly List<ParseError> Errors = new();

        private const bool DEBUG = false;

        public Parser(List<Token> tokens, bool synchronize = true)
        {
            this._tokens = tokens;
            this._synchronize = synchronize;
        }

        public class ParseError : Exception
        {
            public Token Token { get; }

            public ParseError(Token token, string message) : base(message)
            {
                Token = token;
            }
        }

        public List<Stmt> Parse()
        {
            var statements = new List<Stmt>();
            while (!IsAtEnd())
            {
                if (Match(TokenType.Uses))
                {
                    statements.AddRange(UsesStatement());
                }
                else
                {
                    statements.Add(Declaration());
                }
            }
            return statements;
        }

        public List<Stmt> ParseWithError()
        {
            try
            {
                return Parse();
            }
            catch (ParseError e)
            {
                _errors.Add(e);
                return new List<Stmt>();
            }
        }

        private List<Stmt> UsesStatement()
        {
            var name = Consume(TokenType.Identifier, "Expected identifier.");
            var fileName = name.Lexeme;
            Consume(TokenType.Semicolon, "Expected ';'");

            if (!uses.Contains(fileName))
            {
                uses.Add(fileName);
                string path = fileName + ".pas";
                string source = File.ReadAllText(path, Encoding.Default);

                var scanner = new Scanner(path, source);
                var tokens = scanner.ScanTokens();

                var parser = new Parser(tokens);
                var result = parser.Parse();

                if (parser.Errors.Count == 0)
                {
                    Console.WriteLine($"✓ {path}");
                }
                else
                {
                    Console.WriteLine($"✗ {path}");
                    foreach (var error in parser.Errors)
                    {
                        Console.WriteLine($"Error at {error.Token.Lexeme}: {error.Message}");
                    }
                }
                return result;
            }

            return new List<Stmt>();
        }

        private Stmt Declaration()
        {
            try
            {
                if (Match(TokenType.Class)) return ClassDeclaration();
                if (Match(TokenType.Function)) return Function("function");
                if (Match(TokenType.Procedure)) return Function("procedure");
                if (Match(TokenType.Var)) return VarDeclaration();
                if (Match(TokenType.Type)) return TypeDeclaration();

                return Statement();
            }
            catch (ParseError error)
            {
                Errors.Add(error);
                if (!_synchronize) throw error;
                Synchronize();
                return null;
            }
        }

        private Stmt ClassDeclaration()
        {
            var name = Consume(TokenType.Identifier, "Expect class name.");
            Expr.Variable superclass = null;

            if (Match(TokenType.LeftParen))
            {
                superclass = new Expr.Variable(Consume(TokenType.Identifier, "Expect superclass name."));
                Consume(TokenType.RightParen, "Expect ')' after superclass name.");
            }
            Consume(TokenType.Semicolon, "Expect ';' after class declaration.");

            var initializers = new List<Expr>();
            var body = new List<Stmt>();
            while (Check(TokenType.Type) || Check(TokenType.Var))
            {
                if (Match(TokenType.Type))
                {
                    while (!Check(TokenType.Begin) && !IsAtEnd())
                    {
                        body.Add(TypeDeclaration());
                    }
                }
                else if (Match(TokenType.Var))
                {
                    foreach (var stmt in VariableSection())
                    {
                        if (stmt is Stmt.Var v)
                        {
                            var type = v.Type ?? "Any";
                            initializers.Add(new Expr.ClassVar(new Expr.This(v.Name), v.Name, type, v.Generic, new Expr.Variable(v.Name)));
                        }
                    }
                }
            }

            Consume(TokenType.Begin, "Expect 'begin' before class body.");
            var methods = new List<Stmt.Function>();

            while (!Check(TokenType.End) && !IsAtEnd())
            {
                Match(TokenType.Function, TokenType.Procedure, TokenType.Constructor);
                methods.Add((Stmt.Function)Function("method"));
            }
            Consume(TokenType.End, "Expect 'end' after class body.");

            return new Stmt.Class(name, superclass, initializers, methods);
        }

        private Stmt TypeDeclaration()
        {
            var name = Consume(TokenType.Identifier, "Expect enum name.");
            Consume(TokenType.Equal, "Expect '=' after enum declaration.");
            Consume(TokenType.LeftParen, "Expect '('");

            var parameters = new List<Token>();
            do
            {
                parameters.Add(Consume(TokenType.Identifier, "Expect enum identifier."));
            }
            while (Match(TokenType.Comma));

            Consume(TokenType.RightParen, "Expect ')'");
            Consume(TokenType.Semicolon, "Expect ';'");

            return new Stmt.Enum(name, parameters);
        }

        private Stmt VarDeclaration()
        {
            var name = Consume(TokenType.Identifier, "Expect variable name.");
            var generic = "Any";
            var type = "Any";

            if (Match(TokenType.Colon))
            {
                var token = Consume(TokenType.Identifier, "Expected type.");
                type = token.Lexeme;
                if (type.Equals("list", StringComparison.OrdinalIgnoreCase) && Match(TokenType.Of))
                {
                    generic = Consume(TokenType.Identifier, "Expect generic type.").Lexeme;
                }
            }

            Expr initializer = null;
            if (Match(TokenType.Assign))
            {
                initializer = Expression();
            }

            Consume(TokenType.Semicolon, "Expect ';' after variable declaration.");
            return new Stmt.Var(name, type, generic, initializer);
        }

        private List<Stmt> VariableSection()
        {
            var stmts = new List<Stmt>();
            while (!Check(TokenType.Begin) && !Check(TokenType.Type) && !Check(TokenType.Var) && !IsAtEnd())
            {
                var names = new List<Token> { Consume(TokenType.Identifier, "Expect variable name.") };
                while (Match(TokenType.Comma))
                {
                    names.Add(Consume(TokenType.Identifier, "Expect variable name."));
                }

                var type = "Any";
                var generic = "Any";

                if (Match(TokenType.Colon))
                {
                    var token = Consume(TokenType.Identifier, "Expected type.");
                    type = token.Lexeme;
                    if (type.Equals("list", StringComparison.OrdinalIgnoreCase) && Match(TokenType.Of))
                    {
                        generic = Consume(TokenType.Identifier, "Expect generic type.").Lexeme;
                    }
                }

                Expr initializer = null;
                if (Match(TokenType.Assign))
                {
                    initializer = Expression();
                }

                Consume(TokenType.Semicolon, "Expect ';' after variable declaration.");

                stmts.AddRange(names.Select(name => new Stmt.Var(name, type, generic, initializer)));
            }

            return stmts;
        }

        private Stmt Function(string kind)
        {
            var name = Consume(TokenType.Identifier, $"Expect {kind} name.");
            var parameters = new List<Token>();
            var parameterTypes = new List<Token>();

            if (Match(TokenType.LeftParen))
            {
                if (!Check(TokenType.RightParen))
                {
                    do
                    {
                        parameters.Add(Consume(TokenType.Identifier, "Expect parameter name."));
                        if (Match(TokenType.Colon))
                        {
                            parameterTypes.Add(Consume(TokenType.Identifier, "Expect type."));
                        }
                        else
                        {
                            parameterTypes.Add(new Token(TokenType.Identifier, "Any", null, name.Line, name.Offset, name.FileName));
                        }
                    }
                    while (Match(TokenType.Comma));
                }
                Consume(TokenType.RightParen, "Expect ')' after parameters.");
            }

            var returnType = "Any";
            if (Match(TokenType.Colon))
            {
                if (kind.Equals("procedure", StringComparison.OrdinalIgnoreCase))
                {
                    throw Error(Peek(), "Procedures cannot have return type.");
                }
                returnType = Consume(TokenType.Identifier, "Expected return type.").Lexeme;
            }

            Consume(TokenType.Semicolon, "Expect ';'");

            var body = new List<Stmt>();
            while (Check(TokenType.Type) || Check(TokenType.Var))
            {
                if (Match(TokenType.Type))
                {
                    while (!Check(TokenType.Begin) && !IsAtEnd())
                    {
                        body.Add(TypeDeclaration());
                    }
                }
                else if (Match(TokenType.Var))
                {
                    body.AddRange(VariableSection());
                }
            }

            Consume(TokenType.Begin, $"Expect 'begin' before {kind} body.");
            while (!Check(TokenType.End) && !IsAtEnd())
            {
                body.Add(Declaration());
            }
            Consume(TokenType.End, $"Expect 'end' after {kind} body.");

            return new Stmt.Function(name, new Token(TokenType.Identifier, kind, null, name.Line, name.Offset, name.FileName), returnType, parameters, parameterTypes, body);
        }

        public Expr Expression()
        {
            var expr = Assignment();
            if (Match(TokenType.As))
            {
                var type = Consume(TokenType.Identifier, "Expect typecast identifier.");
                expr.Cast = type.Lexeme;
            }
            return expr;
        }

        private Expr Assignment()
        {
            var expr = Or();

            if (Match(TokenType.Assign))
            {
                var equals = Previous();
                var value = Assignment();

                if (expr is Expr.Variable variable)
                {
                    return new Expr.Assign(variable.Name, value);
                }
                else if (expr is Expr.Get get)
                {
                    return new Expr.Set(get.Object, get.Name, value);
                }

                throw Error(equals, "Invalid assignment target.");
            }

            return expr;
        }

        private Expr Or()
        {
            var expr = And();

            while (Match(TokenType.Or))
            {
                var operatorToken = Previous();
                var right = And();
                expr = new Expr.Logical(expr, operatorToken, right);
            }

            return expr;
        }

        private Expr And()
        {
            var expr = Equality();

            while (Match(TokenType.And))
            {
                var operatorToken = Previous();
                var right = Equality();
                expr = new Expr.Logical(expr, operatorToken, right);
            }

            return expr;
        }

        private Expr Equality()
        {
            Debug("equality");

            var expr = Comparison();

            while (Match(TokenType.NotEqual, TokenType.Equal))
            {
                var operatorToken = Previous();
                var right = Comparison();
                expr = new Expr.Binary(expr, operatorToken, right);
            }

            return expr;
        }

        private Expr Comparison()
        {
            Debug("comparison");

            var expr = Term();

            while (Match(TokenType.Greater, TokenType.GreaterEqual, TokenType.Less, TokenType.LessEqual))
            {
                var operatorToken = Previous();
                var right = Term();
                expr = new Expr.Binary(expr, operatorToken, right);
            }

            return expr;
        }

        private Expr Term()
        {
            Debug("term");

            var expr = Factor();

            while (Match(TokenType.Minus, TokenType.Plus))
            {
                var operatorToken = Previous();
                var right = Factor();
                expr = new Expr.Binary(expr, operatorToken, right);
            }

            return expr;
        }

        private Expr Factor()
        {
            Debug("factor");

            var expr = Unary();

            while (Match(TokenType.Slash, TokenType.Star))
            {
                var operatorToken = Previous();
                var right = Unary();
                expr = new Expr.Binary(expr, operatorToken, right);
            }

            return expr;
        }

        private Expr Unary()
        {
            Debug("unary");

            if (Match(TokenType.Not, TokenType.Minus))
            {
                var operatorToken = Previous();
                var right = Unary();
                return new Expr.Unary(operatorToken, right);
            }

            return Call();
        }

        private Expr FinishCall(Expr callee)
        {
            var arguments = new List<Expr>();

            if (!Check(TokenType.RightParen))
            {
                do
                {
                    if (arguments.Count >= 255)
                    {
                        throw Error(Peek(), "Can't have more than 255 arguments.");
                    }
                    arguments.Add(Expression());
                }
                while (Match(TokenType.Comma));
            }

            var paren = Consume(TokenType.RightParen, "Expect ')' after arguments.");

            return new Expr.Call(callee, paren, arguments);
        }

        private Expr Call()
        {
            var expr = Primary();

            while (true)
            {
                if (Match(TokenType.LeftParen))
                {
                    expr = FinishCall(expr);
                }
                else if (Match(TokenType.LeftBracket))
                {
                    var subscript = Expression();
                    Consume(TokenType.RightBracket, "Expect ']' after subscript.");

                    expr = new Expr.Subscript(Previous(), expr, subscript);
                }
                else if (Match(TokenType.Dot))
                {
                    var name = Consume(TokenType.Identifier, "Expect property name after '.'");
                    expr = new Expr.Get(expr, name);
                }
                else
                {
                    break;
                }
            }

            return expr;
        }

        private Expr Primary()
        {
            Debug("primary");

            if (Match(TokenType.False)) return new Expr.Literal(false);
            if (Match(TokenType.True)) return new Expr.Literal(true);
            if (Match(TokenType.Nil)) return new Expr.Literal(null);

            if (Match(TokenType.Number, TokenType.Integer, TokenType.String, TokenType.Char))
            {
                return new Expr.Literal(Previous().Literal);
            }

            if (Match(TokenType.Super))
            {
                var keyword = Previous();
                Consume(TokenType.Dot, "Expect '.' after 'super'.");
                var method = Consume(TokenType.Identifier, "Expect superclass method name");
                return new Expr.Super(keyword, method);
            }

            if (Match(TokenType.This)) return new Expr.This(Previous());

            if (Match(TokenType.Identifier))
            {
                return new Expr.Variable(Previous());
            }

            if (Match(TokenType.LeftBracket))
            {
                var map = new Dictionary<Expr, Expr>();
                do
                {
                    var key = Expression();
                    Consume(TokenType.Colon, "Expect ':' after key.");
                    var value = Expression();
                    map[key] = value;
                }
                while (Match(TokenType.Comma));
                Consume(TokenType.RightBracket, "Expect ']' after map.");

                return new Expr.Map(map);
            }

            if (Match(TokenType.LeftParen))
            {
                var expr = Expression();
                Consume(TokenType.RightParen, "Expect ')' after expression.");
                return new Expr.Grouping(expr);
            }

            throw Error(Peek(), "Expect expression.");
        }


        private Token Consume(TokenType type, string message)
        {
            if (Check(type)) return Advance();
            throw Error(Peek(), message);
        }

        private bool Match(params TokenType[] types)
        {
            foreach (var type in types)
            {
                if (Check(type))
                {
                    Advance();
                    return true;
                }
            }
            return false;
        }

        private bool Check(TokenType type)
        {
            if (IsAtEnd()) return false;
            return Peek().Type == type;
        }

        private Token Advance()
        {
            if (!IsAtEnd()) _current++;
            return Previous();
        }

        private bool IsAtEnd()
        {
            return Peek().Type == TokenType.Eof;
        }

        private Token Peek()
        {
            return _tokens[_current];
        }

        private Token Previous()
        {
            return _tokens[_current - 1];
        }

        private ParseError Error(Token token, string message)
        {
            return new ParseError(token, message);
        }

        private void Synchronize()
        {
            Advance();
            while (!IsAtEnd())
            {
                if (Previous().Type == TokenType.Semicolon) return;

                switch (Peek().Type)
                {
                    case TokenType.Class:
                    case TokenType.For:
                    case TokenType.Function:
                    case TokenType.If:
                    case TokenType.Print:
                    case TokenType.Exit:
                    case TokenType.Var:
                    case TokenType.While:
                        return;
                }
                Advance();
            }
        }

        private Stmt Statement()
        {
            if (Match(TokenType.For)) return ForStatement();
            if (Match(TokenType.Break)) return BreakStatement();
            if (Match(TokenType.If)) return IfStatement();
            if (Match(TokenType.Try)) return TryStatement();
            if (Match(TokenType.Case)) return CaseStatement();
            if (Match(TokenType.Print)) return PrintStatement();
            if (Match(TokenType.Exit)) return ExitStatement();
            if (Match(TokenType.Raise)) return RaiseStatement();
            if (Match(TokenType.While)) return WhileStatement();
            if (Match(TokenType.Begin)) return new Stmt.Block(Block());

            return ExpressionStatement();
        }

        private Stmt ForStatement()
        {
            Stmt? initializer;
            if (Match(TokenType.Semicolon))
            {
                initializer = null;
            }
            else if (Match(TokenType.Var))
            {
                initializer = VarDeclaration();
            }
            else
            {
                initializer = ExpressionStatement();
            }

            Expr? condition = null;
            if (!Check(TokenType.Semicolon))
            {
                condition = Expression();
            }
            Consume(TokenType.Semicolon, "Expect ';' after loop condition.");

            Expr? increment = null;
            if (!Check(TokenType.Semicolon))
            {
                increment = Expression();
            }
            Consume(TokenType.Do, "Expect 'do' after for clauses.");

            try
            {
                _loopDepth++;
                var body = Statement();

                if (increment != null)
                {
                    body = new Stmt.Block(new List<Stmt> { body, new Stmt.Expression(increment) });
                }

                condition ??= new Expr.Literal(true);
                body = new Stmt.While(condition, body);

                if (initializer != null)
                {
                    body = new Stmt.Block(new List<Stmt> { initializer, body });
                }

                return body;
            }
            finally
            {
                _loopDepth--;
            }
        }

        private Stmt BreakStatement()
        {
            if (_loopDepth == 0)
            {
                Error(Previous(), "Must be inside a loop to use 'break'.");
            }
            Consume(TokenType.Semicolon, "Expect ';' after 'break'.");
            return new Stmt.Break();
        }

        private Stmt IfStatement()
        {
            var condition = Expression();
            Consume(TokenType.Then, "Expect 'then' after if condition.");

            var thenBranch = Statement();
            Stmt? elseBranch = null;

            if (Match(TokenType.Else))
            {
                elseBranch = Statement();
            }

            return new Stmt.If(condition, thenBranch, elseBranch);
        }

        private Stmt TryStatement()
        {
            var tryStmts = new List<Stmt>();
            while (!Check(TokenType.Except) && !IsAtEnd())
            {
                tryStmts.Add(Statement());
            }
            var tryBlock = new Stmt.Block(tryStmts);

            var exceptMap = new Dictionary<string, Stmt.Except>();

            Consume(TokenType.Except, "Expect 'except' after try block.");

            while (Check(TokenType.Identifier, "on"))
            {
                Consume(TokenType.Identifier, "Expected 'on'");

                var variable = Consume(TokenType.Identifier, "Expected variable name.");
                string? type = null;
                if (Match(TokenType.Colon))
                {
                    type = Consume(TokenType.Identifier, "Expected type.").Lexeme;
                }
                Consume(TokenType.Do, "Expected 'do'.");
                var stmt = Statement();
                exceptMap[type ?? "Any"] = new Stmt.Except(variable.Lexeme, stmt);
            }

            var exceptStmts = new List<Stmt>();
            while (!Check(TokenType.End) && !IsAtEnd())
            {
                exceptStmts.Add(Statement());
            }

            exceptMap["default"] = new Stmt.Except("Any", new Stmt.Block(exceptStmts));
            Consume(TokenType.End, "Expect 'end' after except block.");

            return new Stmt.Try(tryBlock, exceptMap);
        }

        private Stmt CaseStatement()
        {
            var left = Expression();
            Consume(TokenType.Of, "Expect 'of' after case condition.");

            Stmt.If? top = null;
            Stmt.If? current = null;

            do
            {
                var right = Expression();
                Expr condition = new Expr.Binary(left, new Token(TokenType.Equal, "=", null, Previous().Line, Previous().Offset, Previous().FileName), right);

                while (Match(TokenType.Comma))
                {
                    right = Expression();
                    var additional = new Expr.Binary(left, new Token(TokenType.Equal, "=", null, Previous().Line, Previous().Offset, Previous().FileName), right);
                    condition = new Expr.Logical(condition, new Token(TokenType.Or, "or", null, Previous().Line, Previous().Offset, Previous().FileName), additional);
                }

                Consume(TokenType.Colon, "Expect ':' after condition.");
                var stmt = Statement();

                var ifStmt = new Stmt.If(condition, stmt, null);
                if (top == null)
                {
                    top = ifStmt;
                }
                else
                {
                    current!.ElseBranch = ifStmt;
                }

                current = ifStmt;
            }
            while (!Match(TokenType.Else, TokenType.End));

            if (Previous().Type == TokenType.Else)
            {
                current!.ElseBranch = Statement();
                Consume(TokenType.End, "Expected 'end'.");
            }

            return top!;
        }

        private Stmt PrintStatement()
        {
            var value = Expression();
            Consume(TokenType.Semicolon, "Expect ';' after value.");
            return new Stmt.Print(value);
        }

        private Stmt ExitStatement()
        {
            var keyword = Previous();
            Expr? value = null;

            if (!Check(TokenType.Semicolon))
            {
                value = Expression();
            }

            Consume(TokenType.Semicolon, "Expect ';' after exit value.");
            return new Stmt.Return(keyword, value);
        }

        private Stmt RaiseStatement()
        {
            var keyword = Previous();
            Expr? value = null;

            if (!Check(TokenType.Semicolon))
            {
                value = Expression();
            }

            Consume(TokenType.Semicolon, "Expect ';' after exit value.");
            return new Stmt.Raise(keyword, value);
        }

        private Stmt ExpressionStatement()
        {
            var value = Expression();

            Consume(TokenType.Semicolon, "Expect ';' after value.");

            return new Stmt.Expression(value);
        }

        private Stmt WhileStatement()
        {
            var condition = Expression();
            Consume(TokenType.Do, "Expect 'do' after condition.");

            try
            {
                _loopDepth++;
                var body = Statement();
                return new Stmt.While(condition, body);
            }
            finally
            {
                _loopDepth--;
            }
        }

        private List<Stmt> Block()
        {
            var statements = new List<Stmt>();

            while (!Check(TokenType.End) && !IsAtEnd())
            {
                statements.Add(Declaration());
            }

            Consume(TokenType.End, "Expect 'end' after block.");
            return statements;
        }


        private void Debug(string message)
        {
            if (DEBUG)
            {
                Console.WriteLine(message);
            }
        }
    }
}

