using System.CodeDom.Compiler;
using McInc.Reflection.Utilities;

namespace McInc.Reflection.Extensions;

public static class IndentTextWriterExtensions
{
    extension<TWriter>(TWriter writer) 
        where TWriter: IndentedTextWriter
    {
        public TWriter Append(string text)
        {
            writer.Write(text);
            return writer;
        }

        public TWriter Append(char @char)
        {
            writer.Write(@char);
            return writer;
        }

        public IDisposable BeginIndent()
            => new IndentScope(writer, null);

        public IDisposable BeginBlock()
        {
            writer.WriteLine("{");
            return new IndentScope(writer, () => writer.WriteLine("}"));
        }
    }
}