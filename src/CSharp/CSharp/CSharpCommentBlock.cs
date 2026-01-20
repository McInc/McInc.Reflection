using System.Collections.Generic;
using System.Text;

namespace McInc.CSharp
{
    internal class CSharpCommentBlock : CSharpImplementation,
        ICSharpCommentBlock
    {
        public CSharpCommentBlock(string firstComment,
            StringBuilder builder, ushort level = 0) 
            : base(builder, level,
                indentChild: false,
                firstLine: $"/** {firstComment}")
        {
        }

        public override void AppendLine(string line)
            => base.AppendLine($" * {line}");

        protected override IReadOnlyList<string> CloseLines { get; } = new[] { " */" };
        protected override IReadOnlyList<string> OpenLines { get; } = new[] { "/**" };
    }
}