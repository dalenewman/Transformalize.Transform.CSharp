using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using Cfg.Net.Shorthand;
using Transformalize.Configuration;
using Transformalize.Contracts;
using Parameter = Cfg.Net.Shorthand.Parameter;

namespace Transformalize.Transforms.CSharp.Autofac {
    public class CSharpModule : Module {

        private HashSet<string> _methods;
        private ShorthandRoot _shortHand;

        protected override void Load(ContainerBuilder builder) {

            var signatures = new CsharpTransform().GetSignatures().ToArray();

            // get methods and shorthand from builder
            _methods = builder.Properties.ContainsKey("Methods") ? (HashSet<string>)builder.Properties["Methods"] : new HashSet<string>();
            _shortHand = builder.Properties.ContainsKey("ShortHand") ? (ShorthandRoot)builder.Properties["ShortHand"] : new ShorthandRoot();

            // register shorthand for configuration container
            RegisterShortHand(signatures);

            // register csharp transform for transformalize container(s)
            if (builder.Properties.ContainsKey("Process")) {
                var process = (Process)builder.Properties["Process"];
                if (process != null) {
                    RegisterTransform(builder, c => {
                        new CSharpHost(c).Start();
                        return new CsharpTransform(c);
                    }, signatures);
                }
            }

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

        private static void RegisterTransform(ContainerBuilder builder, Func<IContext, ITransform> getTransform, IEnumerable<OperationSignature> signatures) {
            foreach (var s in signatures) {
                builder.Register((c, p) => getTransform(p.Positional<IContext>(0))).Named<ITransform>(s.Method);
            }
        }

    }
}
