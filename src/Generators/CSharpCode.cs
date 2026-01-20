using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
#pragma warning disable IDE0290

namespace McInc.Reflection
{
    internal class CSharpCode
    {
        public static INamespaceScope BeginNamespace(string @namespace, StringBuilder builder)
            => new NamespaceScope(@namespace, builder);


        public interface ICanCreateClass
        {
            IClassScope BeginClass(Accessibility accessibility,
                Modifiers modifiers, string className);
        }

        public interface ICanCreateConstructor
        {
            IConstructorScope BeginConstructor(Accessibility accessibility, params string[] parameters);
        }

        public interface ICanCreateMethod
        {
            IMethodScope BeginMethod(Accessibility accessibility, Modifiers modifiers, string returnType, string name,
                params string[] parameters);
        }

        public interface ICanCreateSwitch
        {
            ISwitchScope BeginSwitch(string expression);
        }

        public interface ICanWriteLine
        {
            void AddLine(string line);
        }


        public interface ICaseScope : IDisposable,
            ICanWriteLine
        {

        }

        public interface IClassScope : IDisposable,
            ICanCreateClass,
            ICanCreateConstructor,
            ICanCreateMethod,
            ICanWriteLine
        {

        }

        public interface ICodeScope : IDisposable,
            ICanWriteLine
        {

        }


        public interface IConstructorScope : IDisposable,
            ICanWriteLine
        {

        }

        public interface IDefaultScope : IDisposable,
            ICanWriteLine
        {

        }

        public interface IMethodScope : IDisposable,
            ICanCreateSwitch,
            ICanWriteLine
        {
            ICodeScope BeginScope();
        }

        public interface ISwitchScope : IDisposable
        {
            ICaseScope BeginCase(string condition);

            IDefaultScope BeginDefault();
        }

        public interface INamespaceScope : IDisposable,
            ICanCreateClass
        {
        }

        [Flags]
        public enum Modifiers
        {
            None = 0,
            Static = 1,
            Virtual = 1 << 1,
            Override = 1 << 2,
            Sealed = 1 << 3

        }


        private static string AccessibilityToString(Accessibility accessibility)
        {
            return accessibility switch
            {
                Accessibility.NotApplicable => string.Empty,
                Accessibility.Private => "private",
                Accessibility.ProtectedAndInternal => "protected internal",
                Accessibility.Protected => "protected",
                Accessibility.Internal => "internal",
                Accessibility.ProtectedOrInternal => "protected",
                Accessibility.Public => "public",
                _ => string.Empty
            };
        }

        private static string GetTypeOpenLine(Accessibility accessibility, Modifiers modifiers, string typeKind, string typeName)
        {
            var list = new List<string> { AccessibilityToString(accessibility) };

            if (modifiers != Modifiers.None)
                list.Add(GetModifiersString(modifiers));

            if (string.IsNullOrEmpty(typeKind))
                list.Add(typeKind);

            list.Add(typeName);
            return string.Join(" ", list);
        }

        private static string GetMethodOpenString(Accessibility accessibility, Modifiers modifiers, string returnType,
            string name, params string[] parameters)
        {
            var list = new List<string>() { AccessibilityToString(accessibility) };

            if (modifiers != Modifiers.None)
                list.Add(GetModifiersString(modifiers));

            if (!string.IsNullOrEmpty(returnType))
                list.Add(returnType);

            list.Add($"{name}({string.Join(", ", parameters)})");

            return string.Join(" ", list);
        }

        private static string GetModifiersString(Modifiers modifiers)
        {
            var list = new List<string>();

            if (modifiers.HasFlag(Modifiers.Static))
                list.Add("static");

            if (modifiers.HasFlag(Modifiers.Sealed))
                list.Add("sealed");

            if (modifiers.HasFlag(Modifiers.Virtual))
                list.Add("virtual");

            if (modifiers.HasFlag(Modifiers.Override))
                list.Add("override");

            return string.Join(" ", list);
        }


        private class CaseScope : CodeScope, ICaseScope
        {
            public CaseScope(string condition, StringBuilder builder, int indentLevel)
                : base(builder, indentLevel, $"case {condition}:", false)
            {

            }
        }

        private class ClassScope : CodeScope, IClassScope
        {
            public ClassScope(Accessibility accessibility,
                Modifiers modifiers,
                string className,
                StringBuilder builder, int indentLevel = 0) : base(builder, indentLevel,
                GetTypeOpenLine(accessibility, modifiers, "class", className))
            {
                this._className = className;
            }

            public IConstructorScope BeginConstructor(Accessibility accessibility, params string[] parameters)
                => new ConstructorScope(this._className, accessibility, parameters, this.Builder, this.IndentLevel + 1);

            private readonly string _className;
        }

        private class ConstructorScope : CodeScope, IConstructorScope
        {
            public ConstructorScope(string typeName, Accessibility accessibility,
                string[] parameters,
                StringBuilder builder, int indentLevel = 0) : base(builder, indentLevel,
                GetMethodOpenString(accessibility, Modifiers.None, string.Empty, typeName, parameters))
            {
            }
        }

        private class DefaultScope : CodeScope, IDefaultScope
        {
            public DefaultScope(StringBuilder builder, int indentLevel)
            : base(builder, indentLevel, "default:", false)
            {

            }
        }

        private class MethodScope : CodeScope, IMethodScope
        {
            public MethodScope(Accessibility accessibility, Modifiers modifiers, string returnType,
                string name, string[] parameters,
                StringBuilder builder, int indentLevel)
            : base(builder, indentLevel,
                GetMethodOpenString(accessibility, modifiers, returnType, name, parameters))
            {

            }

            public ICodeScope BeginScope()
                => new CodeScope(this.Builder, this.IndentLevel + 1, string.Empty);
        }

        private class NamespaceScope : CodeScope, INamespaceScope
        {
            public NamespaceScope(string @namespace, StringBuilder builder, int indentLevel = 0)
            : base(builder, indentLevel, $"namespace {@namespace}")

            {

            }


        }

        private class SwitchScope : CodeScope, ISwitchScope
        {
            public SwitchScope(string condition,
                StringBuilder builder, int indentLevel)
            : base(builder, indentLevel, $"switch({condition})")
            {

            }

            public ICaseScope BeginCase(string condition)
                => new CaseScope(condition, this.Builder, this.IndentLevel + 1);

            public IDefaultScope BeginDefault()
                => new DefaultScope(this.Builder, this.IndentLevel + 1);
        }

        private class CodeScope : IDisposable,
            ICanCreateClass,
            ICanCreateMethod,
            ICanCreateSwitch,
            ICanWriteLine,
            ICodeScope
        {
            public CodeScope(StringBuilder builder, int indentLevel = 0, string openLine = "",
                bool useParentheses = true)
            {
                this.Builder = builder;
                this.IndentLevel = indentLevel;
                this._openLine = openLine;
                this._useParentheses = useParentheses;

                this.Open();
            }

            public void AddLine(string line)
            {
                if (string.IsNullOrEmpty(line))
                {
                    this.Builder.AppendLine();
                    return;
                }

                this.AppendIndent(this.IndentLevel + 1);
                this.Builder.AppendLine(line);
            }

            public IClassScope BeginClass(Accessibility accessibility,
                Modifiers modifiers, string className)
                => new ClassScope(accessibility, modifiers, className,
                    this.Builder, this.IndentLevel + 1);

            public IMethodScope BeginMethod(Accessibility accessibility, Modifiers modifiers, string returnType,
                string name,
                params string[] parameters)
                => new MethodScope(accessibility, modifiers, returnType, name, parameters,
                    this.Builder, this.IndentLevel + 1);

            public ISwitchScope BeginSwitch(string expression)
                => new SwitchScope(expression, this.Builder, this.IndentLevel + 1);

            public void Dispose()
            {
                this.Close();
            }

            protected readonly StringBuilder Builder;
            protected readonly int IndentLevel;



            private readonly string _openLine;
            private readonly bool _useParentheses;


            private void AppendIndent()
                => this.AppendIndent(this.IndentLevel);

            private void AppendIndent(int indentLevel)
            {
                for (int i = 0; i < indentLevel; i++)
                    this.Builder.Append('\t');
            }

            private void Close()
            {

                if (this._useParentheses)
                {
                    this.AppendIndent();
                    this.Builder.AppendLine("}");
                }

                this.Builder.AppendLine();
            }

            private void Open()
            {
                if (!string.IsNullOrEmpty(this._openLine))
                {
                    this.AppendIndent();
                    this.Builder.AppendLine(this._openLine);

                }

                if (this._useParentheses)
                {
                    this.AppendIndent();
                    this.Builder.AppendLine("{");
                }

            }

        }
    }


}
