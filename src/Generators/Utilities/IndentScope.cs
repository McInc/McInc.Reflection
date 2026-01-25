using System.CodeDom.Compiler;

namespace McInc.Reflection.Utilities;

internal readonly struct IndentScope : IDisposable
{

    public IndentScope(IndentedTextWriter writer, Action? endAction)
    {
        this._writer = writer;
        this._endAction = endAction;
        this._writer.Indent++;
    }

    public void Dispose()
    {
        this._writer.Indent--;
        this._endAction?.Invoke();
    }

    private readonly IndentedTextWriter _writer;
    private readonly Action? _endAction;
}