using System;

namespace McInc.CSharp
{
    public interface ICSharpFile : IDisposable,
        IAppendSingleLineComment,
        IAppendUsing,
        IBeginMultilineComment,
        IBeginNamespace
    {

    }
}