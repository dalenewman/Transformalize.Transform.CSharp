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
using Transformalize.Contracts;

namespace Transformalize.Transforms.CSharp {
    public class CsharpTransform : BaseTransform {
        private readonly CSharpHost.UserCodeInvoker _userCode;

        public CsharpTransform(IContext context = null) : base(context, null) {
            if (IsMissingContext()) {
                return;
            }

            var name = Utility.GetMethodName(Context);

            if (CSharpHost.Cache.TryGetValue(Context.Process.Name, out var userCodes)) {
                if (userCodes.TryGetValue(name, out _userCode))
                    return;
            }

            Context.Error($"Could not find {name} method in user's code");
            Run = false;
        }

        public override IRow Operate(IRow row) {
            row[Context.Field] = _userCode(row.ToArray());
            return row;
        }

        public override IEnumerable<OperationSignature> GetSignatures() {
            var parameters = new List<OperationParameter> { new OperationParameter("script") };
            return new[] {
                new OperationSignature("cs") {Parameters = parameters},
                new OperationSignature("csharp") {Parameters = parameters}
            };
        }
    }
}
