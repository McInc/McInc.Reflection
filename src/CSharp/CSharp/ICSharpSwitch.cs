using System;

namespace McInc.CSharp
{
    public interface ICSharpSwitch: IDisposable,
        IAppendSingleLineComment,
        IBeginCase,
        IBeginMultilineComment
    {
        
    }
}