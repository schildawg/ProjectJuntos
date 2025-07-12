// TypeChecker.cs
using System;
using System.Collections.Generic;

namespace ProjectJuntos.Algol24
{
    public class TypeChecker : Expr.IVisitor<VoidResult>, Stmt.IVisitor<VoidResult>
    {
        private Stmt.Function currentFunction = null;
        private ClassType currentClass = ClassType.None;

        public static readonly TypeLookup Lookup = new TypeLookup();

        public TypeChecker()
        {
            Lookup.Inferred = new TypeLookup();
            Lookup.Parents = new TypeLookup();
            Lookup.Generics = new TypeLookup();
        }

        private enum ClassType
        {
            None,
            Subclass,
            Class
        }

        public void Resolve(List<Stmt> statements)
        {
            foreach (var stmt in statements)
            {
                MapType(stmt);
            }

            foreach (var stmt in statements)
            {
                Resolve(stmt);
            }
        }

        private void MapType(Stmt stmt)
        {
            switch (stmt)
            {
                case Stmt.Enum e:
                    foreach (var value in e.Values)
                    {
                        Lookup.SetType(value.Lexeme, e.Name.Lexeme);
                    }
                    break;

                case Stmt.Function f:
                    Lookup.SetType(f.Name.Lexeme, f.ReturnType);
                    break;

                case Stmt.Class c:
                    Lookup.Parents.SetType(c.Name.Lexeme, c.Superclass == null ? "Any" : c.Superclass.Name.Lexeme);
                    Lookup.SetType(c.Name.Lexeme, c.Name.Lexeme);
                    foreach (var fun in c.Methods)
                    {
                        Lookup.SetType($"{c.Name.Lexeme}::{fun.Name.Lexeme}", fun.ReturnType);
                    }
                    foreach (var expr in c.Initializers)
                    {
                        if (expr is Expr.ClassVar v)
                        {
                            Lookup.Generics.SetType(v.Name.Lexeme, v.Generic);
                        }
                    }
                    break;

                case Stmt.Var v:
                    Lookup.SetType(v.Name.Lexeme, v.Type);
                    Lookup.Generics.SetType(v.Name.Lexeme, v.Generic);
                    break;
            }
        }

        public VoidResult VisitBlockStmt(Stmt.Block stmt)
        {
            Lookup.BeginScope();
            Resolve(stmt.Statements);
            Lookup.EndScope();
            return null;
        }

        public VoidResult VisitEnumStmt(Stmt.Enum stmt) => null;

        public VoidResult VisitClassStmt(Stmt.Class stmt)
        {
            var enclosingClass = currentClass;
            currentClass = ClassType.Class;
            var previousClass = Lookup.CurrentClass;
            Lookup.CurrentClass = stmt;

            if (stmt.Superclass != null)
            {
                currentClass = ClassType.Subclass;
                Resolve(stmt.Superclass);
                Lookup.BeginScope();
            }

            foreach (var initializer in stmt.Initializers)
            {
                Resolve(initializer);
            }

            Lookup.BeginScope();

            foreach (var method in stmt.Methods)
            {
                ResolveFunction(method);
            }

            Lookup.EndScope();
            if (stmt.Superclass != null)
                Lookup.EndScope();

            Lookup.CurrentClass = previousClass;
            currentClass = enclosingClass;
            return null;
        }

        public VoidResult VisitExpressionStmt(Stmt.Expression stmt)
        {
            Resolve(stmt.ExpressionValue);
            return null;
        }

        public VoidResult VisitFunctionStmt(Stmt.Function stmt)
        {
            ResolveFunction(stmt);
            return null;
        }

        public VoidResult VisitIfStmt(Stmt.If stmt)
        {
            Resolve(stmt.Condition);
            Resolve(stmt.ThenBranch);
            if (stmt.ElseBranch != null)
                Resolve(stmt.ElseBranch);
            return null;
        }

        public VoidResult VisitPrintStmt(Stmt.Print stmt) => null;

        public VoidResult VisitReturnStmt(Stmt.Return stmt)
        {
            if (stmt.Value == null) return null;

            if (currentFunction.Type.Lexeme.Equals("procedure", StringComparison.OrdinalIgnoreCase))
                throw new RuntimeError(stmt.Keyword, "Can't return value from procedure.");

            var exitType = stmt.Value.Reduce(Lookup);
            if (exitType == null) return null;

            var returnType = currentFunction.ReturnType;
            if (returnType.Equals("any", StringComparison.OrdinalIgnoreCase)) return null;
            if (exitType.Equals(returnType, StringComparison.OrdinalIgnoreCase)) return null;

            var parent = Lookup.Parents.GetType(exitType);
            while (parent != null)
            {
                if (returnType.Equals(parent, StringComparison.OrdinalIgnoreCase)) return null;
                parent = Lookup.Parents.GetType(parent);
            }

            throw new RuntimeError(stmt.Keyword, "Type mismatch!");
        }

        public VoidResult VisitRaiseStmt(Stmt.Raise stmt) => null;

        public VoidResult VisitTryStmt(Stmt.Try stmt) => null;

        public VoidResult VisitVarStmt(Stmt.Var stmt)
        {
            if (stmt.Initializer != null)
            {
                var inferredType = stmt.Initializer.Reduce(Lookup);
                if (stmt.Type.Equals("any", StringComparison.OrdinalIgnoreCase))
                {
                    Lookup.Inferred.SetType(stmt.Name.Lexeme, inferredType);
                    return null;
                }

                Lookup.SetType(stmt.Name.Lexeme, stmt.Type);
                if (stmt.Type.Equals(inferredType, StringComparison.OrdinalIgnoreCase)) return null;

                var parent = Lookup.Parents.GetType(inferredType);
                while (parent != null)
                {
                    if (stmt.Type.Equals(parent, StringComparison.OrdinalIgnoreCase)) return null;
                    parent = Lookup.Parents.GetType(parent);
                }

                throw new RuntimeError(stmt.Name, "Type mismatch!");
            }
            return null;
        }

        public VoidResult VisitBreakStmt(Stmt.Break stmt) => null;

        public VoidResult VisitWhileStmt(Stmt.While stmt)
        {
            Resolve(stmt.Condition);
            Resolve(stmt.Body);
            return null;
        }

        public VoidResult VisitAssignExpr(Expr.Assign expr)
        {
            Resolve(expr.Value);

            var expected = Lookup.GetType(expr.Name.Lexeme);
            if (expected == null) return null;

            if (expected.Equals(expr.Cast, StringComparison.OrdinalIgnoreCase)) return null;

            var inferred = expr.Value.Reduce(Lookup);

            if (expected.Equals("any", StringComparison.OrdinalIgnoreCase))
            {
                Lookup.Inferred.SetType(expr.Name.Lexeme, inferred);
                return null;
            }

            if (expected.Equals(inferred, StringComparison.OrdinalIgnoreCase)) return null;

            var parent = Lookup.Parents.GetType(inferred);
            while (parent != null)
            {
                if (expected.Equals(parent, StringComparison.OrdinalIgnoreCase)) return null;
                parent = Lookup.Parents.GetType(parent);
            }

            throw new RuntimeError(expr.Name, "Type mismatch!");
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
            foreach (var arg in expr.Arguments)
                Resolve(arg);
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

        public VoidResult VisitVariableExpr(Expr.Variable expr) => null;

        public VoidResult VisitSetExpr(Expr.Set expr)
        {
            var expected = Lookup.GetType(expr.Name.Lexeme);
            var inferred = expr.Value.Reduce(Lookup);

            Resolve(expr.Value);
            Resolve(expr.Object);

            if (expected == null || expected.Equals("any", StringComparison.OrdinalIgnoreCase)) return null;
            if (expected.Equals(expr.Cast, StringComparison.OrdinalIgnoreCase)) return null;
            if (inferred.Equals("Nil", StringComparison.OrdinalIgnoreCase)) return null;

            if (!expected.Equals(inferred, StringComparison.OrdinalIgnoreCase))
                throw new RuntimeError(expr.Name, "Type mismatch.");

            return null;
        }

        public VoidResult VisitClassVarExpr(Expr.ClassVar expr)
        {
            Lookup.SetType(expr.Name.Lexeme, expr.Type);
            Resolve(expr.Value);
            Resolve(expr.Object);
            return null;
        }

        public VoidResult VisitSuperExpr(Expr.Super expr) => null;

        public VoidResult VisitSubscriptExpr(Expr.Subscript expr)
        {
            Resolve(expr.Expr);
            Resolve(expr.Index);
            return null;
        }

        public VoidResult VisitThisExpr(Expr.This expr) => null;

        private void Resolve(Stmt stmt) => stmt.Accept(this);
        private void Resolve(Expr expr) => expr.Accept(this);

        private void ResolveFunction(Stmt.Function function)
        {
            var enclosing = currentFunction;
            currentFunction = function;

            Lookup.BeginScope();
            Resolve(function.Body);
            Lookup.EndScope();

            currentFunction = enclosing;
        }
    }
}
