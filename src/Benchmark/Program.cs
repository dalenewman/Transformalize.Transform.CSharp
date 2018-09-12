using Autofac;
using Transformalize.Contracts;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Transformalize.Containers.Autofac;
using Transformalize.Logging;
using Transformalize.Providers.Bogus.Autofac;
using Transformalize.Transforms.CSharp.Autofac;

namespace Benchmark {

    [LegacyJitX64Job]
    public class Benchmarks {

        [Benchmark(Baseline = true, Description = "5000 rows")]
        public void TestRows() {
            using (var outer = new ConfigurationContainer(new CSharpModule()).CreateScope(@"files\bogus.xml?Size=5000")) {
                using (var inner = new TestContainer(new BogusModule(), new CSharpModule()).CreateScope(outer, new NullLogger())) {
                    var controller = inner.Resolve<IProcessController>();
                    controller.Execute();
                }
            }
        }

        [Benchmark(Baseline = false, Description = "5000 rows 1 csharp")]
        public void CSharpRows() {
            using (var outer = new ConfigurationContainer(new CSharpModule()).CreateScope(@"files\bogus-csharp.xml?Size=5000")) {
                using (var inner = new TestContainer(new BogusModule(), new CSharpModule()).CreateScope(outer, new NullLogger())) {
                    var controller = inner.Resolve<IProcessController>();
                    controller.Execute();
                }
            }
        }
    }

    public class Program {
        private static void Main(string[] args) {
            var summary = BenchmarkRunner.Run<Benchmarks>();
        }
    }
}
