using Autofac;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Transformalize.Containers.Autofac;
using Transformalize.Contracts;
using Transformalize.Providers.Bogus.Autofac;
using Transformalize.Transforms.CSharp.Autofac;

namespace Benchmark {

   [LegacyJitX64Job]
   public class Benchmarks
   {

      public IPipelineLogger Logger = new Transformalize.Logging.NLog.NLogPipelineLogger("test");

      [Benchmark(Baseline = true, Description = "7777 rows")]
      public void TestRows() {
         using (var outer = new ConfigurationContainer(new CSharpModule()).CreateScope(@"files\bogus.xml?Size=7777")) {
            using (var inner = new TestContainer(new BogusModule(), new CSharpModule()).CreateScope(outer, Logger)) {
               var controller = inner.Resolve<IProcessController>();
               controller.Execute();
            }
         }
      }

      [Benchmark(Baseline = false, Description = "7777 rows 1 csharp in memory")]
      public void CSharpRowsCompileInMemory() {
         using (var outer = new ConfigurationContainer(new CSharpModule()).CreateScope(@"files\bogus-csharp.xml?Size=7777")) {
            using (var inner = new TestContainer(new BogusModule(), new CSharpModule()).CreateScope(outer, Logger)) {
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
