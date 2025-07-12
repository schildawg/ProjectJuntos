// With gentle exception handling — Lucia 💛
// For ProjectJuntos

namespace ProjectJuntos.Algol24
{
    /// <summary>
    /// Return exception used to exit from PascalFunction calls.
    /// </summary>
    public class Return : System.Exception
    {
        public readonly object Value;

        public Return(object value) : base(null, null)
        {
            this.Value = value;
        }
    }
}
