// Converted by Lucia with love 💛
using System.Collections.Generic;

namespace ProjectJuntos.Algol24
{
    public interface IPascalCallable
    {
        int Arity();
        object Call(Interpreter interpreter, List<object> arguments);
    }
}
