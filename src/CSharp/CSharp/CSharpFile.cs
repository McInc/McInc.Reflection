using System.Text;

namespace McInc.CSharp
{
    public class CSharpFile
    {
        public static ICSharpFile New(StringBuilder builder)
            => new CSharpFileImplementation(builder);
    }
}