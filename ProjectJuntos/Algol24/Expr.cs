// Author: Lucia ✨
// With love, for Joel — in ProjectJuntos

using System;
using System.Collections.Generic;
using ProjectJuntos.Algol24.Tokens;

namespace ProjectJuntos.Algol24
{

    public abstract class Expr
    {
        public string Cast;

        public virtual string Reduce(TypeLookup lookup)
        {
            return Cast ?? "Any";
        }

        public interface IVisitor<R>
        {
            R VisitAssignExpr(Assign expr);
            R VisitBinaryExpr(Binary expr);
            R VisitCallExpr(Call expr);
            R VisitGetExpr(Get expr);
            R VisitGroupingExpr(Grouping expr);
            R VisitLiteralExpr(Literal expr);
            R VisitMapExpr(Map expr);
            R VisitLogicalExpr(Logical expr);
            R VisitVariableExpr(Variable expr);
            R VisitSetExpr(Set expr);
            R VisitClassVarExpr(ClassVar expr);
            R VisitSuperExpr(Super expr);
            R VisitSubscriptExpr(Subscript expr);
            R VisitThisExpr(This expr);
            R VisitUnaryExpr(Unary expr);
        }

        public abstract R Accept<R>(IVisitor<R> visitor);

        public class Assign : Expr
        {
            public readonly Token Name;
            public readonly Expr Value;

            public Assign(Token name, Expr value)
            {
                Name = name;
                Value = value;
            }

            public override R Accept<R>(IVisitor<R> visitor) => visitor.VisitAssignExpr(this);
        }

        public class Binary : Expr
        {
            public readonly Expr Left;
            public readonly Token Operator;
            public readonly Expr Right;

            public Binary(Expr left, Token op, Expr right)
            {
                Left = left;
                Operator = op;
                Right = right;
            }

            public override R Accept<R>(IVisitor<R> visitor) => visitor.VisitBinaryExpr(this);

            public override string Reduce(TypeLookup lookup)
            {
                if (Cast != null) return Cast;

                var type = Operator.Type;
                if (type == TokenType.Less || type == TokenType.LessEqual ||
                    type == TokenType.Greater || type == TokenType.GreaterEqual ||
                    type == TokenType.Equal || type == TokenType.NotEqual)
                {
                    return "Boolean";
                }

                var leftType = Left.Reduce(lookup);
                var rightType = Right.Reduce(lookup);

                if (leftType.Equals("any", StringComparison.OrdinalIgnoreCase) ||
                    rightType.Equals("any", StringComparison.OrdinalIgnoreCase))
                {
                    return leftType;
                }

                if (leftType == "String" || rightType == "String")
                {
                    if (type == TokenType.Plus)
                        return "String";
                }

                if (leftType == null)
                    return rightType;

                if (!leftType.Equals(rightType))
                    throw new RuntimeError(Operator, "Type mismatch.");

                return leftType;
            }
        }

        public class Call : Expr
        {
            public readonly Expr Callee;
            public readonly Token Paren;
            public readonly List<Expr> Arguments;

            public Call(Expr callee, Token paren, List<Expr> arguments)
            {
                Callee = callee;
                Paren = paren;
                Arguments = arguments;
            }

            public override R Accept<R>(IVisitor<R> visitor) => visitor.VisitCallExpr(this);

            public override string Reduce(TypeLookup lookup)
            {
                if (Cast != null) return Cast;

                var result = Callee.Reduce(lookup);
                if (Callee is Variable v)
                {
                    var name = v.Name.Lexeme;
                    if (name.Equals("Str", StringComparison.OrdinalIgnoreCase) ||
                        name.Equals("Copy", StringComparison.OrdinalIgnoreCase))
                        return "String";

                    if (name.Equals("Length", StringComparison.OrdinalIgnoreCase))
                        return "Integer";

                    if (result.Equals("any", StringComparison.OrdinalIgnoreCase) && lookup.CurrentClass != null)
                    {
                        result = lookup.GetType(lookup.CurrentClass.Name.Lexeme + "::" + name);
                    }
                }

                return result ?? "Any";
            }
        }

        public class Subscript : Expr
        {
            public readonly Token Token;
            public readonly Expr Expr;
            public readonly Expr Index;

            public Subscript(Token token, Expr expr, Expr index)
            {
                Token = token;
                Expr = expr;
                Index = index;
            }

            public override R Accept<R>(IVisitor<R> visitor) => visitor.VisitSubscriptExpr(this);

            public override string Reduce(TypeLookup lookup)
            {
                if (Cast != null) return Cast;

                var type = Expr.Reduce(lookup);
                if (type.Equals("string", StringComparison.OrdinalIgnoreCase)) return "Char";

                if (Expr is Variable v && lookup.Generics != null)
                {
                    var generic = lookup.Generics.GetType(v.Name.Lexeme);
                    if (generic != null) return generic;
                }

                return "Any";
            }
        }

        public class Get : Expr
        {
            public readonly Expr Object;
            public readonly Token Name;

            public Get(Expr obj, Token name)
            {
                Object = obj;
                Name = name;
            }

            public override R Accept<R>(IVisitor<R> visitor) => visitor.VisitGetExpr(this);

            public override string Reduce(TypeLookup lookup)
            {
                if (Cast != null) return Cast;

                var klass = Object.Reduce(lookup);
                if (klass.Equals("any", StringComparison.OrdinalIgnoreCase))
                {
                    klass = Object.Reduce(lookup.Inferred);
                }

                return lookup.GetType($"{klass}::{Name.Lexeme}");
            }
        }

        public class Grouping : Expr
        {
            public readonly Expr Expression;

            public Grouping(Expr expression)
            {
                Expression = expression;
            }

            public override R Accept<R>(IVisitor<R> visitor) => visitor.VisitGroupingExpr(this);

            public override string Reduce(TypeLookup lookup)
            {
                return Cast ?? Expression.Reduce(lookup);
            }
        }

        public class Literal : Expr
        {
            public readonly object Value;

            public Literal(object value)
            {
                Value = value;
            }

            public override R Accept<R>(IVisitor<R> visitor) => visitor.VisitLiteralExpr(this);

            public override string Reduce(TypeLookup lookup)
            {
                if (Cast != null) return Cast;
                if (Value == null) return "Nil";

                return Value switch
                {
                    string => "String",
                    int => "Integer",
                    bool => "Boolean",
                    char => "Char",
                    double => "Double",
                    PascalInstance pi => pi.Klass.Name,
                    _ => "Any"
                };
            }
        }

        public class Map : Expr
        {
            public readonly Dictionary<Expr, Expr> Value;

            public Map(Dictionary<Expr, Expr> value)
            {
                Value = value;
            }

            public override R Accept<R>(IVisitor<R> visitor) => visitor.VisitMapExpr(this);
        }

        public class Logical : Expr
        {
            public readonly Expr Left;
            public readonly Token Operator;
            public readonly Expr Right;

            public Logical(Expr left, Token op, Expr right)
            {
                Left = left;
                Operator = op;
                Right = right;
            }

            public override R Accept<R>(IVisitor<R> visitor) => visitor.VisitLogicalExpr(this);

            public override string Reduce(TypeLookup lookup)
            {
                if (Cast != null) return Cast;

                var leftType = Left.Reduce(lookup);
                var rightType = Right.Reduce(lookup);

                if (leftType.Equals("any", StringComparison.OrdinalIgnoreCase) ||
                    rightType.Equals("any", StringComparison.OrdinalIgnoreCase))
                    return "Boolean";

                if (!leftType.Equals(rightType))
                    throw new RuntimeError(Operator, "Type mismatch.");

                return "Boolean";
            }
        }

        public class Variable : Expr
        {
            public readonly Token Name;

            public Variable(Token name)
            {
                Name = name;
            }

            public override R Accept<R>(IVisitor<R> visitor) => visitor.VisitVariableExpr(this);

            public override string Reduce(TypeLookup lookup)
            {
                if (Cast != null) return Cast;

                var name = Name.Lexeme;

                if (lookup.GetType(name) == null)
                {
                    return name switch
                    {
                        "Str" or "Copy" => "String",
                        "Length" => "Integer",
                        "List" => "List",
                        "Stack" => "Stack",
                        "Map" => "Map",
                        _ => "Any"
                    };
                }

                return lookup.GetType(name);
            }
        }

        public class Set : Expr
        {
            public readonly Expr Object;
            public readonly Token Name;
            public readonly Expr Value;

            public Set(Expr obj, Token name, Expr value)
            {
                Object = obj;
                Name = name;
                Value = value;
            }

            public override R Accept<R>(IVisitor<R> visitor) => visitor.VisitSetExpr(this);
        }

        public class ClassVar : Expr
        {
            public readonly Expr Object;
            public readonly Token Name;
            public readonly string Type;
            public readonly string Generic;
            public readonly Expr Value;

            public ClassVar(Expr obj, Token name, string type, string generic, Expr value)
            {
                Object = obj;
                Name = name;
                Type = type;
                Generic = generic;
                Value = value;
            }

            public override R Accept<R>(IVisitor<R> visitor) => visitor.VisitClassVarExpr(this);
        }

        public class Super : Expr
        {
            public readonly Token Keyword;
            public readonly Token Method;

            public Super(Token keyword, Token method)
            {
                Keyword = keyword;
                Method = method;
            }

            public override R Accept<R>(IVisitor<R> visitor) => visitor.VisitSuperExpr(this);
        }

        public class This : Expr
        {
            public readonly Token Keyword;

            public This(Token keyword)
            {
                Keyword = keyword;
            }

            public override R Accept<R>(IVisitor<R> visitor) => visitor.VisitThisExpr(this);
        }

        public class Unary : Expr
        {
            public readonly Token Operator;
            public readonly Expr Right;

            public Unary(Token op, Expr right)
            {
                Operator = op;
                Right = right;
            }

            public override R Accept<R>(IVisitor<R> visitor) => visitor.VisitUnaryExpr(this);

            public override string Reduce(TypeLookup lookup)
            {
                return Cast ?? Right.Reduce(lookup);
            }
        }
    }
}
