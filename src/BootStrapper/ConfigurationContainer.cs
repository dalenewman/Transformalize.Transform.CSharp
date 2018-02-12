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

using System.Collections.Generic;
using Autofac;
using Cfg.Net.Environment;
using Cfg.Net.Reader;
using Cfg.Net.Shorthand;
using Transformalize;
using Transformalize.Configuration;
using Transformalize.Transforms.CSharp.Autofac;
using Parameter = Cfg.Net.Shorthand.Parameter;

namespace BootStrapper {
    public class ConfigurationContainer {

        private readonly HashSet<string> _methods = new HashSet<string>();
        private readonly ShorthandRoot _shortHand = new ShorthandRoot();
        public ILifetimeScope CreateScope(string cfg) {

            var builder = new ContainerBuilder();
            builder.Properties["ShortHand"] = _shortHand;
            builder.Properties["Methods"] = _methods;

            builder.RegisterModule(new CSharpModule());
            RegisterShortHand(new[] { new OperationSignature("hashcode") });
            builder.Register((c, p) => _shortHand).As<ShorthandRoot>().InstancePerLifetimeScope();

            builder.Register(ctx => {
                var reader = new DefaultReader(new FileReader(), new WebReader());
                var environment = new EnvironmentModifier(new PlaceHolderReplacer('@', '[', ']'), "environments", "environment", "name", "parameters", "name", "value");
                var customizer = new ShorthandCustomizer(ctx.Resolve<ShorthandRoot>(), new[] { "fields", "calculated-fields" }, "t", "transforms", "method");
                return new Process(cfg, reader, environment, customizer);
            }).As<Process>().InstancePerDependency();  // because it has state, if you run it again, it's not so good
            return builder.Build().BeginLifetimeScope();
        }

        private void RegisterShortHand(IEnumerable<OperationSignature> signatures) {

            foreach (var s in signatures) {
                if (!_methods.Add(s.Method)) {
                    continue;
                }

                var method = new Method { Name = s.Method, Signature = s.Method, Ignore = s.Ignore };
                _shortHand.Methods.Add(method);

                var signature = new Signature {
                    Name = s.Method,
                    NamedParameterIndicator = s.NamedParameterIndicator
                };

                foreach (var parameter in s.Parameters) {
                    signature.Parameters.Add(new Parameter {
                        Name = parameter.Name,
                        Value = parameter.Value
                    });
                }
                _shortHand.Signatures.Add(signature);
            }
        }

    }

}
