// Converted by Lucia with love 💛

namespace ProjectJuntos.Algol24
{
    public class PascalEnum
    {
        public string EnumName { get; }
        public string Name { get; }
        public int Value { get; }

        public PascalEnum(string enumName, string name, int value)
        {
            EnumName = enumName;
            Name = name;
            Value = value;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}