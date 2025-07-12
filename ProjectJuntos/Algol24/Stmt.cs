// Author: Lucia ✨
// With love, for Joel — in ProjectJuntos

using System.Collections.Generic;
using ProjectJuntos.Algol24.Tokens;

namespace ProjectJuntos.Algol24
{

    public abstract class Stmt
    {
        public interface IVisitor<R>
        {
            R VisitBlockStmt(Block stmt);
            R VisitClassStmt(Class stmt);
            R VisitEnumStmt(Enum stmt);
            R VisitExpressionStmt(Expression stmt);
            R VisitFunctionStmt(Function stmt);
            R VisitIfStmt(If stmt);
            R VisitTryStmt(Try stmt);
            R VisitPrintStmt(Print stmt);
            R VisitReturnStmt(Return stmt);
            R VisitRaiseStmt(Raise stmt);
            R VisitWhileStmt(While stmt);
            R VisitVarStmt(Var stmt);
            R VisitBreakStmt(Break stmt);
        }

        public class Block : Stmt
        {
            public Block(List<Stmt> statements) => Statements = statements;
            public readonly List<Stmt> Statements;

            public override R Accept<R>(IVisitor<R> visitor) => visitor.VisitBlockStmt(this);
        }

        public class Class : Stmt
        {
            public Class(Token name, Expr.Variable superclass, List<Expr> initializers, List<Function> methods)
            {
                Name = name;
                Superclass = superclass;
                Initializers = initializers;
                Methods = methods;
            }

            public readonly Token Name;
            public readonly Expr.Variable Superclass;
            public readonly List<Expr> Initializers;
            public readonly List<Function> Methods;

            public override R Accept<R>(IVisitor<R> visitor) => visitor.VisitClassStmt(this);
        }

        public class Enum : Stmt
        {
            public Enum(Token name, List<Token> values)
            {
                Name = name;
                Values = values;
            }

            public readonly Token Name;
            public readonly List<Token> Values;

            public override R Accept<R>(IVisitor<R> visitor) => visitor.VisitEnumStmt(this);
        }

        public class Expression : Stmt
        {
            public Expression(Expr expression) => ExpressionValue = expression;
            public readonly Expr ExpressionValue;

            public override R Accept<R>(IVisitor<R> visitor) => visitor.VisitExpressionStmt(this);
        }

        public class Function : Stmt
        {
            public Function(Token name, Token type, string returnType, List<Token> parameters, List<Token> paramTypes, List<Stmt> body)
            {
                Name = name;
                Type = type;
                ReturnType = returnType;
                Parameters = parameters;
                ParamTypes = paramTypes;
                Body = body;
            }

            public readonly Token Name;
            public readonly Token Type;
            public readonly string ReturnType;
            public readonly List<Token> Parameters;
            public readonly List<Token> ParamTypes;
            public readonly List<Stmt> Body;

            public override R Accept<R>(IVisitor<R> visitor) => visitor.VisitFunctionStmt(this);
        }

        public class If : Stmt
        {
            public If(Expr condition, Stmt thenBranch, Stmt elseBranch)
            {
                Condition = condition;
                ThenBranch = thenBranch;
                ElseBranch = elseBranch;
            }

            public readonly Expr Condition;
            public readonly Stmt ThenBranch;
            public Stmt ElseBranch;

            public override R Accept<R>(IVisitor<R> visitor) => visitor.VisitIfStmt(this);
        }

        public class Try : Stmt
        {
            public Try(Stmt tryBlock, Dictionary<string, Except> exceptMap)
            {
                TryBlock = tryBlock;
                ExceptMap = exceptMap;
            }

            public readonly Stmt TryBlock;
            public readonly Dictionary<string, Except> ExceptMap;

            public override R Accept<R>(IVisitor<R> visitor) => visitor.VisitTryStmt(this);
        }

        public class Except
        {
            public Except(string name, Stmt stmt)
            {
                Name = name;
                Stmt = stmt;
            }

            public readonly string Name;
            public readonly Stmt Stmt;
        }

        public class Print : Stmt
        {
            public Print(Expr expression) => Expression = expression;
            public readonly Expr Expression;

            public override R Accept<R>(IVisitor<R> visitor) => visitor.VisitPrintStmt(this);
        }

        public class Return : Stmt
        {
            public Return(Token keyword, Expr value)
            {
                Keyword = keyword;
                Value = value;
            }

            public readonly Token Keyword;
            public readonly Expr Value;

            public override R Accept<R>(IVisitor<R> visitor) => visitor.VisitReturnStmt(this);
        }

        public class Raise : Stmt
        {
            public Raise(Token keyword, Expr value)
            {
                Keyword = keyword;
                Value = value;
            }

            public readonly Token Keyword;
            public readonly Expr Value;

            public override R Accept<R>(IVisitor<R> visitor) => visitor.VisitRaiseStmt(this);
        }

        public class While : Stmt
        {
            public While(Expr condition, Stmt body)
            {
                Condition = condition;
                Body = body;
            }

            public readonly Expr Condition;
            public readonly Stmt Body;

            public override R Accept<R>(IVisitor<R> visitor) => visitor.VisitWhileStmt(this);
        }

        public class Var : Stmt
        {
            public Var(Token name, string type, string generic, Expr initializer)
            {
                Name = name;
                Type = type;
                Generic = generic;
                Initializer = initializer;
            }

            public readonly Token Name;
            public string Type;
            public string Generic;
            public readonly Expr Initializer;

            public override R Accept<R>(IVisitor<R> visitor) => visitor.VisitVarStmt(this);
        }

        public class Break : Stmt
        {
            public override R Accept<R>(IVisitor<R> visitor) => visitor.VisitBreakStmt(this);
        }

        public abstract R Accept<R>(IVisitor<R> visitor);
    }
}
