using System;
using System.Collections.Generic;

using ProjectJuntos.Algol24.Tokens;
using ProjectJuntos.Algol24.NativeFunctions;

namespace ProjectJuntos.Algol24
{
    public class Interpreter : Expr.IVisitor<object>, Stmt.IVisitor<VoidResult>
    {
        public readonly Environment Globals = new Environment();
        private Environment _environment;
        private readonly Dictionary<Expr, int> _locals = new();
        private readonly IErrorHandler _errorHandler;

        private class BreakException : Exception { }

        public Interpreter(IErrorHandler errorHandler)
        {
            _errorHandler = errorHandler;
            _environment = Globals;

            try
            {
                NativeFunctionInvoker.Register(Globals, typeof(NativeFunctions.NativeFunctions));
            }
            catch (MissingMethodException)
            {
                throw new Exception("Error registering native functions");
            }
        }

        public Interpreter() : this(new ErrorHandlerImpl()) { }

        public void Interpret(List<Stmt> statements)
        {
            try
            {
                foreach (var statement in statements)
                {
                    Execute(statement);
                }
            }
            catch (RuntimeError error)
            {
                ConsoleColorUtil.Error(error);
                _errorHandler.RuntimeError(error);
            }
        }

        public void RunTests(List<Stmt> statements)
        {
            try
            {
                AssertionInvoker.Register(Globals, typeof(Assertions));
            }
            catch (MissingMethodException)
            {
                throw new Exception("Error registering assertions.");
            }

            try
            {
                var tests = new Dictionary<string, HashSet<Stmt.Function>>();
                int count = 0;

                foreach (var statement in statements)
                {
                    count++;
                    switch (statement)
                    {
                        case Stmt.Function fun:
                            Execute(statement);
                            if (!tests.ContainsKey(fun.Name.FileName))
                            {
                                tests[fun.Name.FileName] = new HashSet<Stmt.Function>();
                            }
                            tests[fun.Name.FileName].Add(fun);
                            break;
                        case Stmt.Class:
                        case Stmt.Enum:
                        case Stmt.Var:
                            Execute(statement);
                            break;
                    }
                }

                ConsoleColorUtil.Info($"Running {count} tests...");
                foreach (var kvp in tests)
                {
                    if (kvp.Key == "REPL") continue;

                    ConsoleColorUtil.Subheader(kvp.Key);
                    foreach (var fun in kvp.Value)
                    {
                        try
                        {
                            var test = (IPascalCallable)LookupVariable(fun.Name, null);
                            test.Call(this, new List<object>());
                            if (fun.Name?.Literal != null)
                            {
                                var dots = new string('.', 55 - fun.Name.Literal.ToString().Length);
                                ConsoleColorUtil.Info($"Test: {fun.Name.Literal} {dots} [ {ConsoleColorUtil.Green("PASS")} ]");
                            }
                        }
                        catch (RuntimeError error)
                        {
                            var dots = new string('.', 55 - fun.Name.Literal.ToString().Length);
                            ConsoleColorUtil.Info($"Test: {fun.Name.Literal} {dots} [ {ConsoleColorUtil.Red("FAIL")} ]");
                            ConsoleColorUtil.Error(error);
                            _errorHandler.RuntimeError(error);
                        }
                    }
                    ConsoleColorUtil.Info("");
                }
            }
            catch (RuntimeError error)
            {
                ConsoleColorUtil.Error(error);
                _errorHandler.RuntimeError(error);
            }
        }

        private object Evaluate(Expr expr) => expr.Accept(this);
        private void Execute(Stmt stmt) => stmt.Accept(this);

        // public void Resolve(Expr expr, int depth) => _locals[expr] = depth;

        // public void ExecuteBlock(List<Stmt> statements, Environment environment) {
        //     var previous = _environment;
        //     try {
        //         _environment = environment;
        //         foreach (var stmt in statements) {
        //             Execute(stmt);
        //         }
        //     } finally {
        //         _environment = previous;
        //     }
        // }

        public object VisitLiteralExpr(Expr.Literal expr) => expr.Value;

        public object VisitUnaryExpr(Expr.Unary expr)
        {
            var right = Evaluate(expr.Right);

            switch (expr.Operator.Type)
            {
                case TokenType.Not:
                    return !IsTruthy(right);

                case TokenType.Minus:
                    CheckNumberOperand(expr.Operator, right);
                    if (right is int i) return -i;
                    else return -(double)right;
            }

            // Unreachable
            return null;
        }

        public VoidResult VisitBreakStmt(Stmt.Break stmt) => throw new BreakException();

        public object VisitGetExpr(Expr.Get expr)
        {
            var obj = Evaluate(expr.Object);
            if (obj is PascalInstance instance)
            {
                return instance.Get(expr.Name);
            }
            throw new RuntimeError(expr.Name, "Only instances have properties.");
        }

        public object VisitGroupingExpr(Expr.Grouping expr)
        {
            return Evaluate(expr.Expression);
        }

        // private object Evaluate(Expr expr)
        // {
        //     return expr.Accept(this);
        // }

        // private void Execute(Stmt stmt)
        // {
        //     stmt.Accept(this);
        // }

        public void Resolve(Expr expr, int depth)
        {
            _locals[expr] = depth;
        }

        public void ExecuteBlock(List<Stmt> statements, Environment environment)
        {
            var previous = _environment;
            try
            {
                _environment = environment;
                foreach (var stmt in statements)
                {
                    Execute(stmt);
                }
            }
            finally
            {
                _environment = previous;
            }
        }

        // public object VisitLiteralExpr(Expr.Literal expr)
        // {
        //     return expr.Value;
        // }

        public object VisitMapExpr(Expr.Map expr)
        {
            var map = new Dictionary<object, object>();
            foreach (var entry in expr.Value)
            {
                var key = Evaluate(entry.Key);
                var value = Evaluate(entry.Value);
                map[key] = value;
            }
            return new PascalMap(map);
        }

        public object VisitLogicalExpr(Expr.Logical expr)
        {
            var left = Evaluate(expr.Left);
            if (expr.Operator.Type == TokenType.Or)
            {
                if (IsTruthy(left)) return left;
            }
            else
            {
                if (!IsTruthy(left)) return left;
            }
            return Evaluate(expr.Right);
        }

        // public object VisitVariableExpr(Expr.Variable expr)
        // {
        //     try
        //     {
        //         return LookupVariable(expr.Name, expr);
        //     }
        //     catch (Exception)
        //     {
        //         try
        //         {
        //             var obj = _environment.Get(new Token(TokenType.This, "this", null, expr.Name.Line, 0, null));
        //             return ((PascalInstance)obj).Get(new Token(TokenType.Identifier, expr.Name.Lexeme, null, expr.Name.Line, 0, null));
        //         }
        //         catch (Exception ex)
        //         {
        //             Console.WriteLine(ex.Message);
        //             throw;
        //         }
        //     }
        // }

        public object VisitVariableExpr(Expr.Variable expr)
        {
            try
            {
                return LookupVariable(expr.Name, expr);
            }
            catch (RuntimeError)
            {
                // Attempt implicit 'this' resolution
                try
                {
                    var thisToken = new Token(TokenType.This, "this", null, expr.Name.Line, 0, null);
                    var instance = _environment.Get(thisToken) as PascalInstance;

                    if (instance != null)
                    {
                        return instance.Get(expr.Name);
                    }
                }
                catch
                {
                    // Swallow to rethrow proper error below
                }

                // If still not found, throw original undefined variable error
                throw new RuntimeError(expr.Name, $"Undefined variable '{expr.Name.Lexeme}'.");
            }
        }

        public object VisitSetExpr(Expr.Set expr)
        {
            var obj = Evaluate(expr.Object);

            if (obj is not PascalInstance instance)
                throw new RuntimeError(expr.Name, "Only instances have fields.");

            var value = Evaluate(expr.Value);
            instance.Set(expr.Name, value);
            return value;
        }

        public object VisitClassVarExpr(Expr.ClassVar expr)
        {
            var obj = Evaluate(expr.Object);

            if (obj is not PascalInstance instance)
                throw new RuntimeError(expr.Name, "Only instances have fields.");

            var value = Evaluate(expr.Value);
            instance.Set(expr.Name, value);
            return value;
        }

        // public object VisitSuperExpr(Expr.Super expr)
        // {
        //     if (!_locals.TryGetValue(expr, out var distance))
        //     {
        //         throw new RuntimeError(expr.Keyword, "Undefined variable 'super?'.");
        //     }

        //     //throw new Exception(_environment.DebugDisplay());

        //     var superclass = (PascalClass)_environment.GetAt(distance, "super");
        //     var obj = (PascalInstance)_environment.GetAt(distance - 1, "this");

        //     var method = superclass.FindMethod(expr.Method.Lexeme);
        //     if (method == null)
        //     {
        //         throw new RuntimeError(expr.Method, $"Undefined property '{expr.Method.Lexeme}'.");
        //     }

        //     return method.Bind(obj);
        // }

        public object VisitSuperExpr(Expr.Super expr)
        {
            int distance = _locals[expr];
            var superclass = (PascalClass)_environment.GetAt(distance, "super");

            // 'this' is always one level nearer than 'super'
            var obj = (PascalInstance)_environment.GetAt(distance - 1, "this");

            var method = superclass.FindMethod(expr.Method.Lexeme);
            if (method == null)
            {
                throw new RuntimeError(expr.Method, $"Undefined property '{expr.Method.Lexeme}'.");
            }

            return method.Bind(obj);
        }


        public object VisitThisExpr(Expr.This expr)
        {
            return LookupVariable(expr.Keyword, expr);
        }

        private object LookupVariable(Token name, Expr expr)
        {
            if (_locals.TryGetValue(expr, out var distance))
            {
                return _environment.GetAt(distance, name.Lexeme);
            }

            try
            {
                return _environment.Get(name);
            }
            catch
            {
                return Globals.Get(name);
            }
        }

        // public object VisitUnaryExpr(Expr.Unary expr)
        // {
        //     var right = Evaluate(expr.Right);

        //     switch (expr.Operator.Type)
        //     {
        //         case TokenType.NOT:
        //             return !IsTruthy(right);
        //         case TokenType.MINUS:
        //             CheckNumberOperand(expr.Operator, right);
        //             return right switch
        //             {
        //                 int i => -i,
        //                 double d => -d,
        //                 _ => throw new RuntimeError(expr.Operator, "Operand must be a number.")
        //             };
        //     }

        //     return null; // Unreachable
        // }

        private void CheckNumberOperand(Token operatorToken, object operand)
        {
            if (operand is double || operand is int || operand is char) return;

            throw new RuntimeError(operatorToken, "Operand must be a number.");
        }

        private void CheckNumberOperands(Token operatorToken, object left, object right)
        {
            if ((left is double && right is double) ||
                (left is int && right is int) ||
                (left is char && right is char)) return;

            throw new RuntimeError(operatorToken, "Operands must be numbers.");
        }

        private bool IsTruthy(object obj)
        {
            return obj switch
            {
                null => false,
                bool b => b,
                int i => i != 0,
                PascalEnum e => e.Value != 0,
                _ => true
            };
        }

        private bool IsEqual(object a, object b)
        {
            if (a == null && b == null) return true;
            if (a == null) return false;
            return a.Equals(b);
        }

        private string Stringify(object obj)
        {
            if (obj == null) return "nil";

            if (obj is double d)
            {
                var text = d.ToString();
                if (text.EndsWith(".0"))
                {
                    return text[..^2];
                }

                return text;
            }

            return obj.ToString();
        }

        public VoidResult VisitBlockStmt(Stmt.Block stmt)
        {
            ExecuteBlock(stmt.Statements, new Environment(_environment));
            return VoidResult.Instance;
        }

        public VoidResult VisitClassStmt(Stmt.Class stmt)
        {
            object superclass = null;
            if (stmt.Superclass != null)
            {
                superclass = Evaluate(stmt.Superclass);
                if (superclass is not PascalClass)
                {
                    throw new RuntimeError(stmt.Superclass.Name, "Superclass must be a class.");
                }
            }

            _environment.Define(stmt.Name.Lexeme, null);

            if (stmt.Superclass != null)
            {
                _environment = new Environment(_environment);
                _environment.Define("super", superclass);
            }

            var methods = new Dictionary<string, PascalFunction>();
            foreach (var method in stmt.Methods)
            {
                var function = new PascalFunction(method, _environment, method.Name.Lexeme == "Init");

                if (methods.TryGetValue(method.Name.Lexeme, out var first))
                {
                    first.Overloads.Add(function);
                }
                else
                {
                    methods[method.Name.Lexeme] = function;
                }
            }

            var klass = new PascalClass(stmt.Name.Lexeme, (PascalClass?)superclass, methods);

            if (superclass != null)
            {
                _environment = _environment.Enclosing!;
            }

            _environment.Assign(stmt.Name, klass);
            return VoidResult.Instance;
        }

        public VoidResult VisitEnumStmt(Stmt.Enum stmt)
        {
            int count = 0;
            foreach (var value in stmt.Values)
            {
                _environment.Define(value.Lexeme, new PascalEnum(stmt.Name.Lexeme, value.Lexeme, count++));
            }

            return VoidResult.Instance;
        }

        public object VisitAssignExpr(Expr.Assign expr)
        {
            var value = Evaluate(expr.Value);

            if (_locals.TryGetValue(expr, out int distance))
            {
                try
                {
                    _environment.AssignAt(distance, expr.Name, value);
                }
                catch (Exception)
                {
                    try
                    {
                        var instance = (PascalInstance)_environment.Get(new Token(TokenType.This, "this", null, expr.Name.Line, 0, null));
                        instance.Set(new Token(TokenType.Identifier, expr.Name.Lexeme, null, expr.Name.Line, 0, null), value);
                    }
                    catch
                    {
                        throw;
                    }
                }
            }
            else
            {
                try
                {
                    Globals.Assign(expr.Name, value);
                }
                catch (Exception)
                {
                    try
                    {
                        var instance = (PascalInstance)_environment.Get(new Token(TokenType.This, "this", null, expr.Name.Line, 0, null));
                        instance.Set(new Token(TokenType.Identifier, expr.Name.Lexeme, null, expr.Name.Line, 0, null), value);
                    }
                    catch
                    {
                        throw;
                    }
                }
            }

            return value;
        }


        public object VisitBinaryExpr(Expr.Binary expr)
        {
            var left = Evaluate(expr.Left);
            var right = Evaluate(expr.Right);

            switch (expr.Operator.Type)
            {
                case TokenType.Greater:
                    CheckNumberOperands(expr.Operator, left, right);
                    if (left is double)
                        return (double)left > (double)right;
                    else if (left is char)
                        return (char)left >= (char)right;
                    else
                        return (int)left > (int)right;

                case TokenType.GreaterEqual:
                    CheckNumberOperands(expr.Operator, left, right);
                    if (left is double)
                        return (double)left >= (double)right;
                    else if (left is char)
                        return (char)left >= (char)right;
                    else
                        return (int)left >= (int)right;

                case TokenType.Less:
                    CheckNumberOperands(expr.Operator, left, right);
                    if (left is double)
                        return (double)left < (double)right;
                    else if (left is char)
                        return (char)left < (char)right;
                    else
                        return (int)left < (int)right;

                case TokenType.LessEqual:
                    CheckNumberOperands(expr.Operator, left, right);
                    if (left is double)
                        return (double)left <= (double)right;
                    else if (left is char)
                        return (char)left <= (char)right;
                    else
                        return (int)left <= (int)right;

                case TokenType.Minus:
                    CheckNumberOperands(expr.Operator, left, right);
                    if (left is double)
                        return (double)left - (double)right;
                    else
                        return (int)left - (int)right;

                case TokenType.Plus:
                    if (left is double && right is double)
                        return (double)left + (double)right;
                    if (left is int && right is int)
                        return (int)left + (int)right;
                    if (left is string || right is string)
                        return Stringify(left) + Stringify(right);
                    throw new RuntimeError(expr.Operator, "Operands must be two numbers, or two strings.");

                case TokenType.Slash:
                    CheckNumberOperands(expr.Operator, left, right);
                    if (left is double)
                        return (double)left / (double)right;
                    else
                        return (int)left / (int)right;

                case TokenType.Star:
                    CheckNumberOperands(expr.Operator, left, right);
                    if (left is double)
                        return (double)left * (double)right;
                    else
                        return (int)left * (int)right;

                case TokenType.NotEqual:
                    return !IsEqual(left, right);

                case TokenType.Equal:
                    return IsEqual(left, right);
            }

            // Unreachable
            return null;
        }


        private object LookupCallInternal(Expr expr)
        {
            try
            {
                return Evaluate(expr);
            }
            catch (Exception e)
            {
                if (expr is Expr.Variable variable)
                {
                    try
                    {
                        var obj = _environment.Get(new Token(TokenType.This, "this", null, variable.Name.Line, 0, null));
                        return ((PascalInstance)obj).Get(new Token(TokenType.Identifier, variable.Name.Lexeme, null, variable.Name.Line, 0, null));
                    }
                    catch (Exception)
                    {
                        throw new RuntimeError(new Token(TokenType.This, "this", null, variable.Name.Line, 0, null), $"Undefined variable '{variable.Name.Lexeme}'.");
                    }

                }
                throw;
            }
        }

        // private object LookupCallInternal(Expr expr)
        // {
        //     if (expr is Expr.Variable variable)
        //     {
        //         try
        //         {
        //             return Evaluate(expr);
        //         }
        //         catch (RuntimeError)
        //         {
        //             // If 'this' exists, try resolving as a method on the current instance
        //             if (_environment.Has("this"))
        //             {
        //                 var thisToken = new Token(TokenType.This, "this", null, variable.Name.Line, 0, null);
        //                 var obj = _environment.Get(thisToken);

        //                 if (obj is PascalInstance instance)
        //                 {
        //                     var methodToken = new Token(TokenType.Identifier, variable.Name.Lexeme, null, variable.Name.Line, 0, null);
        //                     return instance.Get(methodToken);
        //                 }
        //             }

        //             throw new RuntimeError(variable.Name, $"Undefined variable '{variable.Name.Lexeme}'.");
        //         }
        //     }

        //     // Non-variable expressions just evaluate normally
        //     return Evaluate(expr);
        // }

        private object LookupCall(Expr expr)
        {
            return LookupCallInternal(expr);
        }

        public string Type(object obj)
        {
            if (obj == null)
                return "Nil";

            var map = new Dictionary<Type, string>
            {
                [typeof(string)] = "String",
                [typeof(int)] = "Integer",
                [typeof(bool)] = "Boolean",
                [typeof(char)] = "Char",
                [typeof(double)] = "Double",
                [typeof(PascalList)] = "List",
                [typeof(PascalStack)] = "Stack"
            };

            if (map.TryGetValue(obj.GetType(), out var name))
                return name;

            if (obj is PascalEnum e)
                return e.EnumName;

            if (obj is PascalInstance instance && instance.Klass != null)
                return instance.Klass.Name;

            return "Any";
        }

        public object VisitCallExpr(Expr.Call expr)
        {
            var callee = LookupCall(expr.Callee);
            var arguments = new List<object>();
            foreach (var arg in expr.Arguments)
                arguments.Add(Evaluate(arg));

            var types = arguments.Select(Type).ToList();

            if (callee is not IPascalCallable function)
                throw new RuntimeError(expr.Paren, "Can only call functions and classes.");

            if (function is PascalFunction fun)
            {
                // Try matching overload
                function = fun.Match(types);

                if (function == null)
                {
                    var parent = fun.GetParent();
                    if (parent != null)
                    {
                        function = parent.Klass.FindMethod(fun.Declaration.Name.Lexeme, types);
                        if (function is PascalFunction boundFunction)
                            function = boundFunction.Bind(parent);
                    }
                }

                // Try walking up environments
                if (function == null)
                    function = _environment.FindFunction(fun.Declaration.Name, types);

                // Try globals
                if (function == null)
                    function = Globals.FindFunction(fun.Declaration.Name, types);

                if (function == null)
                    throw new RuntimeError(expr.Paren, "No matching signature for function.");
            }

            if (function is IAssertion assertion)
            {
                var newArgs = new List<object> { expr };
                newArgs.AddRange(arguments);
                return assertion.Call(this, newArgs);
            }
            else
            {
                if (arguments.Count != function.Arity())
                    throw new RuntimeError(expr.Paren, $"Expected {function.Arity()} arguments but got {arguments.Count}.");

                return function.Call(this, arguments);
            }
        }

        public object VisitSubscriptExpr(Expr.Subscript expr)
        {
            var target = Evaluate(expr.Expr);
            var indexValue = Evaluate(expr.Index);

            if (target is string s)
            {
                int index = Convert.ToInt32(indexValue);
                return s[index];
            }
            else if (target is PascalList list)
            {
                int index = Convert.ToInt32(indexValue);
                try
                {
                    return list.List[index];
                }
                catch (Exception e)
                {
                    throw new RuntimeError(expr.Token, e.Message);
                }
            }
            // else if (target is PascalStack stack)
            // {
            //     int index = Convert.ToInt32(indexValue);
            //     try
            //     {
            //         return stack.Stack[index];
            //     }
            //     catch (Exception e)
            //     {
            //         throw new RuntimeError(expr.Token, e.Message);
            //     }
            // }

            throw new RuntimeError(expr.Token, "Subscript target should be an ordinal.");
        }

        public VoidResult VisitExpressionStmt(Stmt.Expression stmt)
        {
            Evaluate(stmt.ExpressionValue);
            return null!;
        }

        public VoidResult VisitFunctionStmt(Stmt.Function stmt)
        {
            var function = new PascalFunction(stmt, _environment, false);

            if (_environment.Values.ContainsKey(stmt.Name.Lexeme))
            {
                var value = _environment.Values[stmt.Name.Lexeme];
                throw new RuntimeError(stmt.Name, $"Variable already exists! {value.GetType()}");
            }

            _environment.Define(stmt.Name.Lexeme, function);
            return null!;
        }

        public VoidResult VisitIfStmt(Stmt.If stmt)
        {
            if (IsTruthy(Evaluate(stmt.Condition)))
            {
                Execute(stmt.ThenBranch);
            }
            else if (stmt.ElseBranch != null)
            {
                Execute(stmt.ElseBranch);
            }

            return null!;
        }

        public VoidResult VisitTryStmt(Stmt.Try stmt)
        {
            try
            {
                Execute(stmt.TryBlock);
            }
            catch (Return r)
            {
                throw;
            }
            catch (RuntimeError e)
            {
                var exMessage = e.Message;
                var value = e.Value ?? exMessage;

                string name;
                if (value is PascalInstance pi)
                {
                    name = pi.Klass.Name;
                }
                else
                {
                    name = value.GetType().Name;
                }

                if (!stmt.ExceptMap.TryGetValue(name, out var except))
                {
                    stmt.ExceptMap.TryGetValue("default", out except);
                }

                if (except != null)
                {
                    _environment.Define(except.Name, value);
                    Execute(except.Stmt);
                }
            }

            return null!;
        }

        public VoidResult VisitPrintStmt(Stmt.Print stmt)
        {
            var value = Evaluate(stmt.Expression);
            Console.WriteLine(Stringify(value));
            return null!;
        }

        public VoidResult VisitReturnStmt(Stmt.Return stmt)
        {
            object? value = null;
            if (stmt.Value != null)
            {
                value = Evaluate(stmt.Value);
            }

            throw new Return(value);
        }


        public VoidResult VisitRaiseStmt(Stmt.Raise stmt)
        {
            object? value = null;
            if (stmt.Value != null)
            {
                value = Evaluate(stmt.Value);
            }

            var ex = new RuntimeError(stmt.Keyword, value?.ToString() ?? "Exception");
            ex.Value = value;

            throw ex;
        }

        public VoidResult VisitWhileStmt(Stmt.While stmt)
        {
            try
            {
                while (IsTruthy(Evaluate(stmt.Condition)))
                {
                    Execute(stmt.Body);
                }
            }
            catch (BreakException)
            {
                // Swallow break
            }

            return null!;
        }

        public VoidResult VisitVarStmt(Stmt.Var stmt)
        {
            object? value = null;
            if (stmt.Initializer != null)
            {
                value = Evaluate(stmt.Initializer);
            }

            _environment.Define(stmt.Name.Lexeme, value);
            return null!;
        }

    }
}
