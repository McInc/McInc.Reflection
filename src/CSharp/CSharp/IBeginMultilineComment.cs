namespace McInc.CSharp
{
    public interface IBeginMultilineComment
    {
        ICSharpCommentBlock BeginMultilineComment(string firstComment = "");
    }
}