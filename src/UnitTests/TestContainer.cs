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
using System.Collections.Generic;
using System.Linq;
using Autofac;
using Transformalize;
using Transformalize.Context;
using Transformalize.Contracts;
using Transformalize.Impl;
using Transformalize.Nulls;
using Transformalize.Provider.Internal;
using Transformalize.Providers.Console;
using Transformalize.Transforms;
using Transformalize.Transforms.CSharp.Autofac;
using Transformalize.Transforms.System;
using Process = Transformalize.Configuration.Process;

namespace UnitTests {
    public class TestContainer {

        public ILifetimeScope CreateScope(ILifetimeScope scope) {

            var logger = new ConsoleLogger(LogLevel.Debug);

            var process = scope.Resolve<Process>();

            var builder = new ContainerBuilder();
            builder.Properties["Process"] = process;

            builder.Register((ctx) => process).As<Process>();
            builder.RegisterInstance(logger).As<IPipelineLogger>().SingleInstance();
            builder.RegisterModule(new CSharpModule());
            RegisterTransform(builder, c => new HashcodeTransform(c), new[] { new OperationSignature("hashcode") });

            // Process Context
            builder.Register<IContext>((ctx, p) => new PipelineContext(logger, process)).As<IContext>();

            // Process Output Context
            builder.Register(ctx => {
                var context = ctx.Resolve<IContext>();
                return new OutputContext(context);
            }).As<OutputContext>();

            // Connection and Process Level Output Context
            foreach (var connection in process.Connections) {

                builder.Register(ctx => new ConnectionContext(ctx.Resolve<IContext>(), connection)).Named<IConnectionContext>(connection.Key);

                if (connection.Name != "output")
                    continue;

                // register output for connection
                builder.Register(ctx => {
                    var context = ctx.ResolveNamed<IConnectionContext>(connection.Key);
                    return new OutputContext(context);
                }).Named<OutputContext>(connection.Key);

            }

            // Entity Context and RowFactory
            foreach (var entity in process.Entities) {
                builder.Register<IContext>((ctx, p) => new PipelineContext(ctx.Resolve<IPipelineLogger>(), process, entity)).Named<IContext>(entity.Key);

                builder.Register(ctx => {
                    var context = ctx.ResolveNamed<IContext>(entity.Key);
                    return new InputContext(context);
                }).Named<InputContext>(entity.Key);

                builder.Register<IRowFactory>((ctx, p) => new RowFactory(p.Named<int>("capacity"), entity.IsMaster, false)).Named<IRowFactory>(entity.Key);

                builder.Register(ctx => {
                    var context = ctx.ResolveNamed<IContext>(entity.Key);
                    return new OutputContext(context);
                }).Named<OutputContext>(entity.Key);

                var connection = process.Connections.First(c => c.Name == entity.Connection);
                builder.Register(ctx => new ConnectionContext(ctx.Resolve<IContext>(), connection)).Named<IConnectionContext>(entity.Key);

            }

            // Entity input
            foreach (var entity in process.Entities) {

                builder.RegisterType<NullInputProvider>().Named<IInputProvider>(entity.Key);

                // READER
                builder.Register<IRead>(ctx => {
                    var input = ctx.ResolveNamed<InputContext>(entity.Key);
                    var rowFactory = ctx.ResolveNamed<IRowFactory>(entity.Key, new NamedParameter("capacity", input.RowCapacity));

                    return new InternalReader(input, rowFactory);
                }).Named<IRead>(entity.Key);

            }

            // PROCESS OUTPUT CONTROLLER
            builder.Register<IOutputController>(ctx => new NullOutputController()).As<IOutputController>();

            // Entity Output
            foreach (var entity in process.Entities) {

                builder.Register<IOutputController>(ctx => new NullOutputController()).Named<IOutputController>(entity.Key);
                builder.Register<IOutputProvider>(ctx => new InternalOutputProvider(ctx.ResolveNamed<OutputContext>(entity.Key), ctx.ResolveNamed<IWrite>(entity.Key))).Named<IOutputProvider>(entity.Key);

                // WRITER
                builder.Register<IWrite>(ctx => new InternalWriter(ctx.ResolveNamed<OutputContext>(entity.Key))).Named<IWrite>(entity.Key);
            }

            // entity pipelines
            foreach (var entity in process.Entities) {
                builder.Register(ctx => {
                    var context = ctx.ResolveNamed<IContext>(entity.Key);
                    var outputController = ctx.IsRegisteredWithName<IOutputController>(entity.Key) ? ctx.ResolveNamed<IOutputController>(entity.Key) : new NullOutputController();
                    IPipeline pipeline = new DefaultPipeline(outputController, context);

                    // TODO: rely on IInputProvider's Read method instead (after every provider has one)
                    pipeline.Register(ctx.IsRegisteredWithName(entity.Key, typeof(IRead)) ? ctx.ResolveNamed<IRead>(entity.Key) : null);
                    pipeline.Register(ctx.IsRegisteredWithName(entity.Key, typeof(IInputProvider)) ? ctx.ResolveNamed<IInputProvider>(entity.Key) : null);

                    // transforms
                    if (!process.ReadOnly) {
                        pipeline.Register(new SetSystemFields(new PipelineContext(ctx.Resolve<IPipelineLogger>(), process, entity)));
                    }

                    pipeline.Register(new IncrementTransform(context));
                    pipeline.Register(new LogTransform(context));
                    pipeline.Register(new DefaultTransform(context, context.GetAllEntityFields().Where(f => !f.System)));
                    pipeline.Register(TransformFactory.GetTransforms(ctx, context, entity.GetAllFields().Where(f => f.Transforms.Any())));

                    if (!process.ReadOnly) {
                        pipeline.Register(new StringTruncateTransfom(new PipelineContext(ctx.Resolve<IPipelineLogger>(), process, entity)));
                    }

                    pipeline.Register(new LogTransform(context));

                    // writer, TODO: rely on IOutputProvider instead
                    pipeline.Register(ctx.IsRegisteredWithName(entity.Key, typeof(IWrite)) ? ctx.ResolveNamed<IWrite>(entity.Key) : null);
                    pipeline.Register(ctx.IsRegisteredWithName(entity.Key, typeof(IOutputProvider)) ? ctx.ResolveNamed<IOutputProvider>(entity.Key) : null);

                    // updater
                    pipeline.Register(process.ReadOnly || !ctx.IsRegisteredWithName(entity.Key, typeof(IUpdate)) ? new NullUpdater() : ctx.ResolveNamed<IUpdate>(entity.Key));

                    return pipeline;

                }).Named<IPipeline>(entity.Key);
            }


            // process pipeline
            builder.Register(ctx => {

                var calc = process.ToCalculatedFieldsProcess();
                var entity = calc.Entities.First();

                var context = new PipelineContext(ctx.Resolve<IPipelineLogger>(), calc, entity);

                context.Debug(() => $"Registering {process.Pipeline} pipeline.");
                var outputController = ctx.IsRegistered<IOutputController>() ? ctx.Resolve<IOutputController>() : new NullOutputController();
                IPipeline pipeline = new DefaultPipeline(outputController, context);

                // no updater necessary
                pipeline.Register(new NullUpdater(context, false));

                if (!process.CalculatedFields.Any()) {
                    pipeline.Register(new NullReader(context, false));
                    pipeline.Register(new NullWriter(context, false));
                    return pipeline;
                }

                // register transforms
                pipeline.Register(new IncrementTransform(context));
                pipeline.Register(new LogTransform(context));
                pipeline.Register(new DefaultTransform(new PipelineContext(ctx.Resolve<IPipelineLogger>(), calc, entity), entity.CalculatedFields));
                pipeline.Register(TransformFactory.GetTransforms(ctx, context, entity.CalculatedFields));
                pipeline.Register(new StringTruncateTransfom(new PipelineContext(ctx.Resolve<IPipelineLogger>(), calc, entity)));

                pipeline.Register(new NullReader(context));
                pipeline.Register(new NullWriter(context));

                return pipeline;
            }).As<IPipeline>();

            // process controller
            builder.Register<IProcessController>(ctx => {

                var pipelines = new List<IPipeline>();

                // entity-level pipelines
                foreach (var entity in process.Entities) {
                    var pipeline = ctx.ResolveNamed<IPipeline>(entity.Key);

                    pipelines.Add(pipeline);
                    if (entity.Delete && process.Mode != "init") {
                        pipeline.Register(ctx.ResolveNamed<IEntityDeleteHandler>(entity.Key));
                    }
                }

                // process-level pipeline for process level calculated fields
                if (ctx.IsRegistered<IPipeline>()) {
                    pipelines.Add(ctx.Resolve<IPipeline>());
                }

                var context = ctx.Resolve<IContext>();

                var controller = new ProcessController(pipelines, context);

                // actions
                foreach (var action in process.Actions.Where(a => a.GetModes().Any(m => m == process.Mode || m == "*"))) {
                    if (action.Before) {
                        controller.PreActions.Add(ctx.ResolveNamed<IAction>(action.Key));
                    }
                    if (action.After) {
                        controller.PostActions.Add(ctx.ResolveNamed<IAction>(action.Key));
                    }
                }

                return controller;
            }).As<IProcessController>();


            return builder.Build().BeginLifetimeScope();

        }

        private static void RegisterTransform(ContainerBuilder builder, Func<IContext, ITransform> getTransform, IEnumerable<OperationSignature> signatures) {
            foreach (var s in signatures) {
                builder.Register((c, p) => getTransform(p.Positional<IContext>(0))).Named<ITransform>(s.Method);
            }
        }

    }
}