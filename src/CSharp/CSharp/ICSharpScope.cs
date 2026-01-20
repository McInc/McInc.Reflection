using System;

namespace McInc.CSharp
{
    public interface ICSharpScope : IDisposable,
        IAppendLine,
        IAppendSingleLineComment,
        IBeginElse,
        IBeginElseIf,
        IBeginIf,
        IBeginMultilineComment,
        IBeginSwitch
    {
    }
}