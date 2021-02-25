#region Copyright notice and license

// Copyright 2019 The gRPC Authors
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

#endregion

using Grpc.Core;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Threading;

namespace GrpcSolution
{
    public class DefaultClientActivator
    {
        protected readonly static Dictionary<Type, Func<ObjectFactory>> _createActivator = new Dictionary<Type, Func<ObjectFactory>>(); //() => ActivatorUtilities.CreateFactory(typeof(TClient), new Type[] { typeof(CallInvoker), });
        protected readonly IServiceProvider _services;
        private ObjectFactory? _activator;
        private bool _initialized;
        private object? _lock;
        public DefaultClientActivator(IServiceProvider services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            _services = services;
        }
        protected ObjectFactory Activator(Type type)
        {
            if (!_createActivator.ContainsKey(type)) return null;
            return LazyInitializer.EnsureInitialized(
                    ref _activator,
                    ref _initialized,
                    ref _lock,
                    _createActivator[type]);

            // TODO(JamesNK): Compiler thinks activator is nullable
            // Possibly remove in the future when compiler is fixed
            //return activator!;
        }
        public void AddClient(Type type)
        {
            if (type == null) return;
            Func<ObjectFactory> result = () => ActivatorUtilities.CreateFactory(type, new Type[] { typeof(CallInvoker), });
            if (_createActivator.ContainsKey(type))
            {
                _createActivator[type] = result;
            }
            else
            {
                _createActivator.Add(type, result);
            }
        }
        private Object m_lock = new Object();
        public TClient CreateClient<TClient>(CallInvoker callInvoker)
        {
            if (!_createActivator.ContainsKey(typeof(TClient)))
            {
                lock (m_lock)
                {
                    if (!_createActivator.ContainsKey(typeof(TClient)))
                    {
                        AddClient(typeof(TClient));
                    }
                }
            }
            return (TClient)Activator(typeof(TClient))(_services, new object[] { callInvoker });
        }
    }
    // Should be registered as a singleton, so it that it can act as a cache for the Activator.
    public class DefaultClientActivator<TClient> : DefaultClientActivator where TClient : class
    {
        public DefaultClientActivator(IServiceProvider services):base(services)
        {
        }

        public TClient CreateClient(CallInvoker callInvoker)
        {
            return CreateClient<TClient>(callInvoker);
        }
    }
}
