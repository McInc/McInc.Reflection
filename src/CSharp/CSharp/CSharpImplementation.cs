using System;
using System.Collections.Generic;
using System.Text;

namespace McInc.CSharp
{
    internal class CSharpImplementation : IDisposable,
        IAppendLine,
        IAppendSingleLineComment,
        IAppendUsing,
        IBeginCase,
        IBeginElse,
        IBeginElseIf,
        IBeginIf,
        IBeginMultilineComment,
        IBeginNamespace,
        IBeginSwitch
        
    {
        public CSharpImplementation(StringBuilder builder, ushort level, bool indentChild, string firstLine = "")
        {
            this.Builder = builder;
            this.ChildLevel = indentChild
                ? (ushort)(level + 1)
                : level;
            this.Level = level;

            this._firstLine = firstLine;
            this.StartLine = GenerateStartLine(level);
            this.ChildStartLine = indentChild
                ? GenerateStartLine(this.ChildLevel)
                : this.StartLine;

            this.Open();
        }

        public virtual void AppendLine(string line)
        {
            if (!string.IsNullOrEmpty(line))
                this.Builder.Append(this.ChildStartLine).AppendLine(line);
            else
                this.Builder.AppendLine();
        }

        public void AppendSingleLineComment(string comment)
            => this.Builder.Append(this.ChildStartLine).AppendLine($"// {comment}");

        public void AppendUsing(string @using)
        {
            if (string.IsNullOrEmpty(@using))
                return;

            this.Builder.Append(this.ChildStartLine).AppendLine($"using {@using};");
        }

        public ICSharpScope BeginCase(string label)
            => new CSharpCase(label, this.Builder, this.ChildLevel);


        public ICSharpScope BeginElse()
            => new CSharpElse(this.Builder, this.ChildLevel);

        public ICSharpScope BeginElseIf(string condition)
            => new CSharpIf(CSharpIfElse.IfElse, condition, this.Builder, this.ChildLevel);

        public ICSharpScope BeginIf(string condition)
            => new CSharpIf(CSharpIfElse.OnlyIf, condition, this.Builder, this.ChildLevel);

        public ICSharpCommentBlock BeginMultilineComment(string firstComment = "")
            => new CSharpCommentBlock(firstComment, this.Builder, this.ChildLevel);

        public ICSharpNamespace BeginNamespace(string @namespace)
            => new CSharpNamespace(@namespace, this.Builder, this.ChildLevel);

        public ICSharpSwitch BeginSwitch(string statement)
            => new CSharpSwitch(statement, this.Builder, this.ChildLevel);

        public void Dispose()
        {
            this.Close();
            
        }

        



        protected static string GenerateStartLine(ushort level)
            => level > 0
                ? new string('\t', level)
                : string.Empty;


        protected readonly StringBuilder Builder;
        protected readonly ushort ChildLevel;
        protected readonly string ChildStartLine;
        protected readonly ushort Level;
        protected readonly string StartLine;

        protected virtual bool AppendNewLineAtClose { get; } = true; 

        protected virtual IReadOnlyList<string> CloseLines { get; } = Array.Empty<string>();
        protected virtual IReadOnlyList<string> OpenLines { get; } = Array.Empty<string>();


        private readonly string _firstLine;

        private void Close()
        {
            foreach (var closeLine in this.CloseLines)
                this.Builder.Append(this.StartLine).AppendLine(closeLine);

            if (this.AppendNewLineAtClose)
                this.Builder.AppendLine();
        }

        private void Open()
        {
            if (!string.IsNullOrEmpty(this._firstLine))
                this.Builder.Append(this.StartLine).AppendLine(this._firstLine);


            if (this.OpenLines.Count == 0)
                return;

            foreach (var openLine in this.OpenLines)
                this.Builder.Append(this.StartLine).AppendLine(openLine);
        }


        
    }
}