﻿#region Copyright notice and license

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
using Grpc.Core.Interceptors;
using Grpc.Net.Client;
using Grpc.Net.ClientFactory;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Net.Http;

namespace GrpcSolution
{
    public class GrpcCallInvokerFactory
    {
        private readonly ILoggerFactory _loggerFactory;

        public GrpcCallInvokerFactory(ILoggerFactory loggerFactory)
        {
            if (loggerFactory == null)
            {
                throw new ArgumentNullException(nameof(loggerFactory));
            }

            _loggerFactory = loggerFactory;
        }

        public CallInvoker CreateCallInvoker(HttpClient httpClient, string name, GrpcClientFactoryOptions clientFactoryOptions)
        {
            if (httpClient == null)
            {
                throw new ArgumentNullException(nameof(httpClient));
            }

            var channelOptions = new GrpcChannelOptions();
            channelOptions.HttpClient = httpClient;
            channelOptions.LoggerFactory = _loggerFactory;

            if (clientFactoryOptions.ChannelOptionsActions.Count > 0)
            {
                foreach (var applyOptions in clientFactoryOptions.ChannelOptionsActions)
                {
                    applyOptions(channelOptions);
                }
            }

            var address = clientFactoryOptions.Address ?? httpClient.BaseAddress;
            if (address == null)
            {

                throw new InvalidOperationException($"Could not resolve the address for gRPC client '{name}'.");
            }

            var channel = GrpcChannel.ForAddress(address, channelOptions);

            var httpClientCallInvoker = channel.CreateCallInvoker();

            var resolvedCallInvoker = clientFactoryOptions.Interceptors.Count == 0
                ? httpClientCallInvoker
                : httpClientCallInvoker.Intercept(clientFactoryOptions.Interceptors.ToArray());

            return resolvedCallInvoker;
        }
    }
}
