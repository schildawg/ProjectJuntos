using System;

namespace ProjectJuntos.Algol24
{

    /// <summary>
    /// Error handler interface and implementations for Pascal runtime.
    /// Ported lovingly by Lucia 💛
    /// </summary>
    public interface IErrorHandler
    {
        void RuntimeError(RuntimeError error);
    }

    public class ErrorHandlerImpl : IErrorHandler
    {
        public void RuntimeError(RuntimeError error)
        {
            Pascal.RuntimeError(error);
        }
    }

    public class TestErrorHandler : IErrorHandler
    {
        public void RuntimeError(RuntimeError error)
        {
            throw error;
        }
    }
}
