using System.Text;

namespace McInc.CSharp
{
    internal class CSharpSwitch : CSharpImplementation,
        ICSharpSwitch
    {
        public CSharpSwitch(string statement, StringBuilder builder, ushort level) :
            base(builder, level, true, $"switch({statement})")
        {
        }
    }
}