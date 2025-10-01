namespace Ksi.Roslyn.Tests;

using RefListAnalyzerTest = Util.KsiAnalyzerTest<RefListAnalyzer>;

public class RefListAnalyzerTests
{
    [Fact]
    public async Task RefList01GenericItemType()
    {
        await RefListAnalyzerTest.RunAsync(
            // language=cs
            """
            using Ksi;

            public static class Test
            {
                public static void Method<T>(ref {|REFLIST01:RefList<T>|} param) where T : unmanaged
                {
                    {|REFLIST01:RefList<T>|} a = default;
                    {|REFLIST01:var|} b = RefList.Empty<T>();
                }
            }
            
            public class Generic<T> where T : unmanaged
            {
                private ExclusiveAccess<{|REFLIST01:RefList<T>|}> _listAccess;
            }
            """
        );
    }
}