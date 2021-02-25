using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace GRPCHelper
{
    /// <summary>
    /// 1.创建GrpcClient
    /// 2.1s钟请求超过10次 就新增一个HttpClient
    /// 3.每个HttpClient可循环使用10次,后面必须释放到0后才允许重复利用,而不是到9又会涨到10
    /// </summary>
    public class GrpcHelper
    {
        private IHttpClientFactory m_httpclientfactory;
        public GrpcHelper(IHttpClientFactory httpClientFactory)
        {
            m_httpclientfactory = httpClientFactory;
        }
        public GrpcConnectionClient CreateClientFactory()
        {
            return new GrpcConnectionClient(m_httpclientfactory);
        }
    }
}
