using System;
using System.Text;

namespace McInc.CSharp
{
    public interface ICSharpNamespace : IDisposable,
        IBeginMultilineComment,
        IAppendSingleLineComment
    {
        
    }

    internal class CSharpNamespace : CSharpScope,
        ICSharpNamespace
    {
        public CSharpNamespace(string @namespace, StringBuilder builder, ushort level = 0) : base(builder, level, 
            $"namespace {@namespace}")
        {
        }
    }
}