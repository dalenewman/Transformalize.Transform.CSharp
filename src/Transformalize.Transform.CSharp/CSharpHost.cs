#region license
// Transformalize
// Configurable Extract, Transform, and Load
// Copyright 2013-2017 Dale Newman
//  
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//   
//       http://www.apache.org/licenses/LICENSE-2.0
//   
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion
using System;
using System.CodeDom.Compiler;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using Transformalize.Contracts;

namespace Transformalize.Transforms.CSharp {

   public class CSharpHost {

      private static readonly char[] Slash = { '\\' };
      public delegate object UserCodeInvoker(object[] input);

      private readonly IContext _context;
      private readonly CSharpCodeWriter _codeWriter;
      private readonly string _className;

      public static ConcurrentDictionary<string, ConcurrentDictionary<string, UserCodeInvoker>> Cache { get; } = new ConcurrentDictionary<string, ConcurrentDictionary<string, UserCodeInvoker>>();

      public CSharpHost(IContext context, string className = "UserCode") {
         _context = context;
         _codeWriter = new CSharpCodeWriter(context);
         _className = className;
      }

      public bool Start() {

         if (Cache.ContainsKey(_context.Process.Name)) {
            _context.Debug(() => "Using cached user code.");
            return true;
         }

         var timer = new Stopwatch();
         timer.Start();

         var code = _codeWriter.Write(_className);
         var executingAssembly = Assembly.GetExecutingAssembly();

         var libraryName = _className + code.GetHashCode() + "-" + executingAssembly.GetName().Version + ".dll";
         var outputAssembly = Path.Combine(GetTemporaryFolder(_context.Process.Name), libraryName);

         if (File.Exists(outputAssembly)) {
            _context.Debug(() => $"Loading previously compiled code {outputAssembly}");
            _context.Info($"Loading previously compiled code {libraryName}");
            var assembly = Assembly.LoadFile(outputAssembly);
            if (Cache.TryAdd(_context.Process.Name, new ConcurrentDictionary<string, UserCodeInvoker>())) {
               foreach (var method in assembly.GetType(_className).GetMethods(BindingFlags.Static | BindingFlags.Public)) {
#if NETS20
                  Cache[_context.Process.Name].TryAdd(method.Name, (UserCodeInvoker) Delegate.CreateDelegate(typeof(UserCodeInvoker), method));
#else
                  Cache[_context.Process.Name].TryAdd(method.Name, (UserCodeInvoker)DynamicMethodHelper.ConvertFrom(method).CreateDelegate(typeof(UserCodeInvoker)));
#endif
               }
               return true;
            }
            _context.Error($"Couldn't cache C# Methods from {outputAssembly}");
         }

         var codeProvider = new Microsoft.CSharp.CSharpCodeProvider();
         var parameters = new CompilerParameters {
            GenerateInMemory = false,
            OutputAssembly = outputAssembly,
            GenerateExecutable = false,
            IncludeDebugInformation = false,
            TreatWarningsAsErrors = false,
            CompilerOptions = "/optimize"
         };

         parameters.ReferencedAssemblies.Add("System.dll");
         parameters.ReferencedAssemblies.Add("System.Core.dll");
         parameters.ReferencedAssemblies.Add("mscorlib.dll");
         parameters.ReferencedAssemblies.Add(executingAssembly.Location);

         try {
            var result = codeProvider.CompileAssemblyFromSource(parameters, code);
            if (result.Errors.Count > 0) {
               foreach (CompilerError error in result.Errors) {
                  _context.Error($"C# error on line {error.Line}, column {error.Column}.");
                  _context.Error(error.ErrorText);
               }
               Utility.CodeToError(_context, code);
            } else {
               timer.Stop();
               _context.Info($"Compiled user's code in {timer.Elapsed}.");

               if (Cache.TryAdd(_context.Process.Name, new ConcurrentDictionary<string, UserCodeInvoker>())) {
                  foreach (var method in result.CompiledAssembly.GetType(_className).GetMethods(BindingFlags.Static | BindingFlags.Public)) {
#if NETS20
                     Cache[_context.Process.Name].TryAdd(method.Name, (UserCodeInvoker)Delegate.CreateDelegate(typeof(UserCodeInvoker), method));
#else
                     Cache[_context.Process.Name].TryAdd(method.Name, (UserCodeInvoker)DynamicMethodHelper.ConvertFrom(method).CreateDelegate(typeof(UserCodeInvoker)));
#endif
                  }
                  _context.Warn("This process is susceptible to C# code related memory leak.");
                  return true;
               }
               _context.Error("Couldn't cache C# methods.");
               return false;

            }

         } catch (Exception ex) {
            _context.Error("C# Compiler Exception!");
            _context.Error(ex.Message);
            Utility.CodeToError(_context, code);
            return false;
         }
         return true;
      }

      public static string GetTemporaryFolder(string processName) {
         var local = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData).TrimEnd(Slash);

         //i.e. c: no user profile exists
         if (local.Length <= 2) {
            if (AppDomain.CurrentDomain.GetData("DataDirectory") != null) {
               local = AppDomain.CurrentDomain.GetData("DataDirectory").ToString().TrimEnd(Slash);
            }
         }

         var folder = Path.Combine(local, Constants.ApplicationFolder, processName);

         if (!Directory.Exists(folder)) {
            Directory.CreateDirectory(folder);
         }

         return folder;
      }

      public void Dispose() {
         // leave it, you can't recover the memory used by the dynamically generated (and loaded) assembly
      }
   }
}