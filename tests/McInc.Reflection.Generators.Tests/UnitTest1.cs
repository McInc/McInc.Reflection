namespace McInc.Reflection.Generators.Tests;


public class UnitTest1
{
    [Fact]
    public Task GeneratesTypeReflectionCorrectly()
    {
        // The source code to test
        var source = """
                     using McInc.Reflection;

                     namespace Test12345;
                     
                     [GenerateReflection(IncludeInternals = false)]
                     public class SelfId
                     {
                         public int Id;
                         public static int Id2;
                         public readonly int Id3;
                         public static readonly int Id4;
                         public const int CCCC  =111;
                         public int A {get;set;}
                         public int B{get;}
                         public int C{set=> this.Id = value;}
                         public int D{get; init;}
                         public string Gatto{get; set;}
                         internal string Gatto2{get; set;}
                     }
                     """;

        // Pass the source code to our helper and snapshot test the output
        return TestHelper.Verify(source);
    }
}
