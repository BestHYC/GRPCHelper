using Grpc.Net.Client;
using GrpcService1;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace GrpcClientTest.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IEnumerable<WeatherForecast> Get()
        {
            var rng = new Random();
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            })
            .ToArray();
        }
        /// <summary>
        /// 直接查看单连接在处理程序情况下是否是同一个端口
        /// 
        /// 答案 : 是同一个端口
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        [HttpGet("SingleSamePort")]
        public String SingleSamePort([FromServices] Greeter.GreeterClient client)
        {
            StringBuilder sb = new StringBuilder();
            for(Int32 i = 0; i < 100; i++)
            {
               var result =  client.SayHelloAsync(new HelloRequest() { Name = i.ToString() }).ResponseAsync.Result;
                sb.AppendLine(result.Message);
            }
            return sb.ToString();
        }
        /// <summary>
        /// 同时通过构建方式创建客户端,看是否是同一个端口号
        /// 答案:不是
        /// 原因是:底层源码Action<IServiceProvider, HttpClient> configureTypedClient = (s, httpClient) =>
        /// 每次创建连接会额外创建一个基于Greeter名称的HttpClient,所以这里是有2个
        /// httpClient,一个是Greeter,一个是Greeter1
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        [HttpGet("DoubleSamePort")]
        public String DoubleSamePort([FromServices] Greeter.GreeterClient client,[FromServices]Greeter1.Greeter1Client client1)
        {
            StringBuilder sb = new StringBuilder();
            for (Int32 i = 0; i < 100; i++)
            {
                var result = client.SayHelloAsync(new HelloRequest() { Name = i.ToString() }).ResponseAsync.Result;
                sb.Append(result.Message);
                sb.Append("client1的执行结果");
                var result1 = client1.SayHelloAsync(new HelloRequest() { Name = i.ToString() }).ResponseAsync.Result;
                sb.AppendLine(result1.Message);
            }
            return sb.ToString();
        }
        /// <summary>
        /// 通过构建两个channel方式去创建 连接,是否是相同端口号
        /// 注意,这里与上面那个差别.
        /// 
        /// 答案:是相同端口号
        /// 
        /// 原因是:底层通过创建CreateHttpClient(Default)的形式共同创建一个Client.
        /// 所以使用的相同port
        /// </summary>
        /// <returns></returns>
        [HttpGet("DoubleSamePortByChannel")]
        public String DoubleSamePortByChannel()
        {
            StringBuilder sb = new StringBuilder();
            var channel = GrpcChannel.ForAddress("");
            for (Int32 i = 0; i < 100; i++)
            {
                var client = new Greeter.GreeterClient(channel);
                var result = client.SayHelloAsync(new HelloRequest() { Name = i.ToString() }).ResponseAsync.Result;
                sb.Append(result.Message);
                sb.Append("client1的执行结果");
                var client1 = new Greeter1.Greeter1Client(channel);
                var result1 = client1.SayHelloAsync(new HelloRequest() { Name = i.ToString() }).ResponseAsync.Result;
                sb.AppendLine(result1.Message);
            }
            return sb.ToString();
        }
        /// <summary>
        /// 直接查看单连接在处理程序情况下是否是同一个端口
        /// 
        /// 答案 : 不是同一个端口
        /// 
        /// 因为在每次建立连接时候,都重新创建了新的HttpClient.导致每次连接都是新程序
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        [HttpGet("SingleDifPort")]
        public String SingleDifPort([FromServices]IHttpClientFactory factory)
        {
            StringBuilder sb = new StringBuilder();
            for (Int32 i = 0; i < 100; i++)
            {
                GrpcChannelOptions options = new GrpcChannelOptions()
                {
                    HttpClient = factory.CreateClient(Guid.NewGuid().ToString())
                };
                var channel = GrpcChannel.ForAddress("", options);
                var client = new Greeter.GreeterClient(channel);
                var result = client.SayHelloAsync(new HelloRequest() { Name = i.ToString() }).ResponseAsync.Result;
                sb.AppendLine(result.Message);
            }
            return sb.ToString();
        }
    }
}
