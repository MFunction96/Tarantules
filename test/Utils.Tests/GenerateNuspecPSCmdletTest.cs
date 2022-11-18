using System.Management.Automation;
using System.Reflection;

namespace Tarantules.Utils.Tests
{
    [TestClass]
    public class GenerateNuspecPSCmdletTest
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        public void OnSuccess()
        {
            var SourceNuspec = "Source.nuspec";
            var ResultNuspec = "Result.nuspec";
            var ExpectedNuspec = "Expected.nuspec";
            if (File.Exists(ResultNuspec)) File.Delete(ResultNuspec);

            try
            {
                using var ps = PowerShell.Create().AddCommand("Import-Module")
                    .AddArgument(Assembly.GetAssembly(typeof(GenerateNuspecPSCmdlet))!.Location);
                ps.Invoke();

                ps.AddCommand("Resolve-Nuspec").AddParameter("SourceNuspec", SourceNuspec)
                    .AddParameter("DestinationNuspec", ResultNuspec).AddParameter("Version", "3.0.7");
                var rev = ps.Invoke();
                foreach (var psObject in rev)
                {
                    this.TestContext.WriteLine(psObject.ToString());
                }

                var result = File.ReadAllText(ResultNuspec);
                var expected = File.ReadAllText(ExpectedNuspec);
                Assert.AreEqual(expected, result);
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }

        }
    }
}