using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using qckdev.Net.Http.Test.Common;

namespace qckdev.Net.Http.Test.Net35
{
    [TestClass]
    public sealed class TestAssemblySetup
    {
        [AssemblyInitialize]
        public static void AssemblyInitialize(TestContext context)
        {
            var settings = Helpers.GetSettings();
            LocalTestServiceManager.StartIfNeeded(settings, AppDomain.CurrentDomain.BaseDirectory);
        }

        [AssemblyCleanup]
        public static void AssemblyCleanup()
        {
            LocalTestServiceManager.StopIfOwned();
        }
    }
}
