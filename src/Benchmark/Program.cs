using Autofac;
using BootStrapper;
using Transformalize.Contracts;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Attributes.Jobs;
using BenchmarkDotNet.Running;
using Transformalize.Logging;

namespace Benchmark {

    
    [LegacyJitX64Job, LegacyJitX86Job]
    public class Benchmarks {

        [Benchmark(Baseline = true, Description = "500 test rows")]
        public void TestRows() {
            using (var outer = new ConfigurationContainer().CreateScope(@"files\bogus.xml?Size=500")) {
                using (var inner = new TestContainer().CreateScope(outer, new NullLogger())) {
                    var controller = inner.Resolve<IProcessController>();
                    controller.Execute();
                }
            }
        }

        [Benchmark(Baseline = false, Description = "500 test rows with 3 csharp transforms")]
        public void CSharpRows() {
            using (var outer = new ConfigurationContainer().CreateScope(@"files\bogus-csharp.xml?Size=500")) {
                using (var inner = new TestContainer().CreateScope(outer, new NullLogger())) {
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
