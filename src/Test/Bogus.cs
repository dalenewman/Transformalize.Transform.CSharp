﻿#region license
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

using System.Linq;
using Autofac;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Transformalize.Configuration;
using Transformalize.Containers.Autofac;
using Transformalize.Contracts;
using Transformalize.Providers.Bogus.Autofac;
using Transformalize.Providers.Console;
using Transformalize.Transforms.CSharp.Autofac;

namespace Test {

   [TestClass]
   public class Bogus {

      [TestMethod]
      public void BogusTests() {

         var logger = new ConsoleLogger(LogLevel.Debug);

         using (var outer = new ConfigurationContainer(new CSharpModule()).CreateScope(@"files\bogus-100.xml", logger)) {
            var process = outer.Resolve<Process>();
            using (var inner = new Container(new BogusModule(), new CSharpModule()).CreateScope(process, logger)) {

               var controller = inner.Resolve<IProcessController>();

               controller.Execute();
               var rows = process.Entities.First().Rows;

               Assert.AreEqual(100, rows.Count);
            }
         }



      }
   }
}
