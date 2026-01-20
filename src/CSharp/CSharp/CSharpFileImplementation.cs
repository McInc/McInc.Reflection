using System.Text;

namespace McInc.CSharp
{
    internal class CSharpFileImplementation: CSharpImplementation,
        ICSharpFile
    {
        public CSharpFileImplementation(StringBuilder builder) : base(builder, 0, false)
        {
        }
    }
}