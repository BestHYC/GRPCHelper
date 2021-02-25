using Grpc.Net.Client;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Net.Http;
using System.Reflection;
using System.Text;

namespace GRPCHelper
{
    public class GrpcConnectionClient : IDisposable
    {
        //一个HttpClient有多少连接的数量
        //每次获取客户端就遍历,获取到使用连接数小于10的使用
        private static Dictionary<String, Int32> m_httpclients = new Dictionary<String, Int32>();
        private IHttpClientFactory m_httpclientfactory;
        private String m_currentname;
        public GrpcConnectionClient(IHttpClientFactory httpClientFactory)
        {
            m_httpclientfactory = httpClientFactory;
        }
        private static Object m_lock = new Object();
        public T GetHttpClient<T>()
        {
            lock (m_lock)
            {
                HttpClient client = null;
                foreach (var item in m_httpclients)
                {
                    if (item.Value < 10)
                    {
                        m_currentname = item.Key;
                        break;
                    }
                }
                if (String.IsNullOrWhiteSpace(m_currentname))
                {
                    String guid = Guid.NewGuid().ToString();
                    m_currentname = guid;
                    m_httpclients.Add(guid, 0);
                }
                m_httpclients[m_currentname] += 1;
                client = m_httpclientfactory.CreateClient(m_currentname);
                GrpcChannelOptions options = new GrpcChannelOptions()
                {
                    HttpClient = client
                };
                var channel = GrpcChannel.ForAddress("http://localhost:6001", options);
                var client1 = GetFunc<T>(channel);
                return client1;
            }
        }
        private static Dictionary<String, Func<GrpcChannel, Object>> m_expression = new Dictionary<String, Func<GrpcChannel, Object>>();
        private T GetFunc<T>(GrpcChannel channel)
        {
            String name = typeof(T).FullName;
            if (m_expression.ContainsKey(name)) return (T)m_expression[name].Invoke(channel);
            var argumentType = new[] { typeof(GrpcChannel) };
            var constructor = typeof(T).GetConstructor(
                BindingFlags.Instance | BindingFlags.Public,
                null,
                argumentType,
                null);
            var param = Expression.Parameter(typeof(GrpcChannel), "channel");
            var constructorCallExpression = Expression.New(constructor, param);
            var constructorCallingLambda = Expression
              .Lambda<Func<GrpcChannel, Object>>(constructorCallExpression, param).Compile();
            m_expression.Add(name, constructorCallingLambda);
            return (T)constructorCallingLambda(channel);
        }
        public void Dispose()
        {
            lock (m_lock)
            {
                if (m_currentname == null) return;
                if (m_httpclients.TryGetValue(m_currentname, out Int32 num))
                {
                    if (num <= 0) return;
                    m_httpclients[m_currentname] = num - 1;
                }
            }
        }
    }
}
