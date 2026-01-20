using McInc.Reflection;

namespace Test12345;

[GenerateReflection]
public struct SelfId
{
    public int Id;
    public static int Id2;
    public readonly int Id3;
    public static readonly int Id4;
    public const int CCCC = 111;
    public int A { get; set; }
    public int B { get; }
    public int C { set => this.Id = value; }
    public int D { get; init; }
}
