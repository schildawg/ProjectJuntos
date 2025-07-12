using System;
using System.Collections.Generic;
using ProjectJuntos.Algol24.Tokens;

namespace ProjectJuntos.Algol24
{

    public class Resolver : Expr.IVisitor<VoidResult>, Stmt.IVisitor<VoidResult>
    {
        private readonly Interpreter _interpreter;
        private readonly Stack<Dictionary<string, bool>> _scopes = new();
        private FunctionType _currentFunction = FunctionType.None;
        private ClassType _currentClass = ClassType.None;

        private enum FunctionType
        {
            None,
            Method,
            Function,
            Initializer
        }

        private enum ClassType
        {
            None,
            Subclass,
            Class
        }

        public Resolver(Interpreter interpreter)
        {
            _interpreter = interpreter;
        }

        public void Resolve(List<Stmt> statements)
        {
            foreach (var statement in statements)
            {
                Resolve(statement);
            }
        }

        public VoidResult VisitBlockStmt(Stmt.Block stmt)
        {
            BeginScope();
            Resolve(stmt.Statements);
            EndScope();
            return null;
        }

        public VoidResult VisitEnumStmt(Stmt.Enum stmt)
        {
            foreach (var value in stmt.Values)
            {
                Declare(value);
                Define(value);
            }
            return null;
        }

        public VoidResult VisitClassStmt(Stmt.Class stmt)
        {
            var enclosingClass = _currentClass;
            _currentClass = ClassType.Class;

            Declare(stmt.Name);
            Define(stmt.Name);

            if (stmt.Superclass != null && stmt.Name.Lexeme == stmt.Superclass.Name.Lexeme)
            {
                Pascal.Error(stmt.Superclass.Name, "A class can't inherit from itself.");
            }

            if (stmt.Superclass != null)
            {
                _currentClass = ClassType.Subclass;
                Resolve(stmt.Superclass);

                BeginScope();
                _scopes.Peek()["super"] = true;
            }

            BeginScope();
            _scopes.Peek()["this"] = true;
            foreach (var method in stmt.Methods)
            {
                var declaration = method.Name.Lexeme == "init" ? FunctionType.Initializer : FunctionType.Method;
                ResolveFunction(method, declaration);
            }
            EndScope();

            if (stmt.Superclass != null) EndScope();
            _currentClass = enclosingClass;

            return null;
        }

        public VoidResult VisitExpressionStmt(Stmt.Expression stmt)
        {
            Resolve(stmt.ExpressionValue);
            return null;
        }

        public VoidResult VisitFunctionStmt(Stmt.Function stmt)
        {
            Declare(stmt.Name);
            Define(stmt.Name);
            ResolveFunction(stmt, FunctionType.Function);
            return null;
        }

        public VoidResult VisitIfStmt(Stmt.If stmt)
        {
            Resolve(stmt.Condition);
            Resolve(stmt.ThenBranch);
            if (stmt.ElseBranch != null) Resolve(stmt.ElseBranch);
            return null;
        }

        public VoidResult VisitTryStmt(Stmt.Try stmt)
        {
            Resolve(stmt.TryBlock);
            foreach (var except in stmt.ExceptMap.Values)
            {
                Resolve(except.Stmt);
            }
            return null;
        }

        public VoidResult VisitPrintStmt(Stmt.Print stmt)
        {
            Resolve(stmt.Expression);
            return null;
        }

        public VoidResult VisitReturnStmt(Stmt.Return stmt)
        {
            if (_currentFunction == FunctionType.None)
            {
                Pascal.Error(stmt.Keyword, "Can't return from top-level code.");
            }

            if (stmt.Value != null)
            {
                if (_currentFunction == FunctionType.Initializer)
                {
                    Pascal.Error(stmt.Keyword, "Can't return a value from an initializer.");
                }
                Resolve(stmt.Value);
            }

            return null;
        }

        public VoidResult VisitRaiseStmt(Stmt.Raise stmt) => null;

        public VoidResult VisitVarStmt(Stmt.Var stmt)
        {
            Declare(stmt.Name);
            if (stmt.Initializer != null)
            {
                Resolve(stmt.Initializer);
            }
            Define(stmt.Name);
            return null;
        }

        public VoidResult VisitWhileStmt(Stmt.While stmt)
        {
            Resolve(stmt.Condition);
            Resolve(stmt.Body);
            return null;
        }

        public VoidResult VisitBreakStmt(Stmt.Break stmt) => null;

        public VoidResult VisitAssignExpr(Expr.Assign expr)
        {
            Resolve(expr.Value);
            ResolveLocal(expr, expr.Name);
            return null;
        }

        public VoidResult VisitBinaryExpr(Expr.Binary expr)
        {
            Resolve(expr.Left);
            Resolve(expr.Right);
            return null;
        }

        public VoidResult VisitCallExpr(Expr.Call expr)
        {
            Resolve(expr.Callee);
            foreach (var argument in expr.Arguments)
            {
                Resolve(argument);
            }
            return null;
        }

        public VoidResult VisitGetExpr(Expr.Get expr)
        {
            Resolve(expr.Object);
            return null;
        }

        public VoidResult VisitGroupingExpr(Expr.Grouping expr)
        {
            Resolve(expr.Expression);
            return null;
        }

        public VoidResult VisitLiteralExpr(Expr.Literal expr) => null;

        public VoidResult VisitMapExpr(Expr.Map expr) => null;

        public VoidResult VisitLogicalExpr(Expr.Logical expr)
        {
            Resolve(expr.Left);
            Resolve(expr.Right);
            return null;
        }

        public VoidResult VisitUnaryExpr(Expr.Unary expr)
        {
            Resolve(expr.Right);
            return null;
        }

        public VoidResult VisitVariableExpr(Expr.Variable expr)
        {
            if (_scopes.Count > 0 && _scopes.Peek().TryGetValue(expr.Name.Lexeme, out bool isDefined) && !isDefined)
            {
                Pascal.Error(expr.Name, "Can't read local variable in its own initializer.");
            }

            ResolveLocal(expr, expr.Name);
            return null;
        }

        public VoidResult VisitSetExpr(Expr.Set expr)
        {
            Resolve(expr.Value);
            Resolve(expr.Object);
            return null;
        }

        public VoidResult VisitClassVarExpr(Expr.ClassVar expr)
        {
            Resolve(expr.Value);
            Resolve(expr.Object);
            return null;
        }

        public VoidResult VisitSuperExpr(Expr.Super expr)
        {
            if (_currentClass == ClassType.None)
            {
                Pascal.Error(expr.Keyword, "Can't use 'super' outside a class.");
            }
            else if (_currentClass != ClassType.Subclass)
            {
                Pascal.Error(expr.Keyword, "Can't use 'super' in a class with no superclass.");
            }

            ResolveLocal(expr, expr.Keyword);
            return null;
        }

        public VoidResult VisitSubscriptExpr(Expr.Subscript expr)
        {
            Resolve(expr.Expr);
            Resolve(expr.Index);
            return null;
        }

        public VoidResult VisitThisExpr(Expr.This expr)
        {
            if (_currentClass == ClassType.None)
            {
                Pascal.Error(expr.Keyword, "Can't use 'this' outside a class.");
                return null;
            }

            ResolveLocal(expr, expr.Keyword);
            return null;
        }

        private void Resolve(Stmt stmt) => stmt.Accept(this);
        private void Resolve(Expr expr) => expr.Accept(this);

        private void ResolveFunction(Stmt.Function function, FunctionType type)
        {
            var enclosingFunction = _currentFunction;
            _currentFunction = type;

            BeginScope();
            foreach (var param in function.Parameters)
            {
                Declare(param);
                Define(param);
            }
            Resolve(function.Body);
            EndScope();

            _currentFunction = enclosingFunction;
        }

        private void BeginScope() => _scopes.Push(new Dictionary<string, bool>());
        private void EndScope() => _scopes.Pop();

        private void Declare(Token name)
        {
            if (_scopes.Count == 0) return;

            var scope = _scopes.Peek();
            if (scope.ContainsKey(name.Lexeme))
            {
                Pascal.Error(name, "Already a variable with this name in this scope.");
            }

            scope[name.Lexeme] = false;
        }

        private void Define(Token name)
        {
            if (_scopes.Count == 0) return;
            _scopes.Peek()[name.Lexeme] = true;
        }

        // private void ResolveLocal(Expr expr, Token name)
        // {
        //     for (int i = _scopes.Count - 1; i >= 0; i--)
        //     {
        //         if (_scopes.ToArray()[i].ContainsKey(name.Lexeme))
        //         {
        //             _interpreter.Resolve(expr, _scopes.Count - 1 - i);
        //             return;
        //         }
        //     }
        // }

        // private void ResolveLocal(Expr expr, Token name)
        // {
        //     var scopesArray = _scopes.ToArray(); // take snapshot once
        //     for (int i = 0; i < scopesArray.Length; i++)
        //     {
        //         if (scopesArray[i].ContainsKey(name.Lexeme))
        //         {
        //             _interpreter.Resolve(expr, i);  // i is correct distance from current
        //             return;
        //         }
        //     }
        // }

        private void ResolveLocal(Expr expr, Token name)
        {
            int depth = 0;
            foreach (var scope in _scopes)
            {
                if (scope.ContainsKey(name.Lexeme))
                {
                    _interpreter.Resolve(expr, depth);
                    return;
                }
                depth++;
            }
        }

        // private void ResolveLocal(Expr expr, Token name)
        // {
        //     int depth = 0;
        //     foreach (var scope in _scopes)
        //     {
        //         if (scope.ContainsKey(name.Lexeme))
        //         {
        //             _interpreter.Resolve(expr, depth);
        //             return;
        //         }
        //         depth++;
        //     }
        // }
    }
}
