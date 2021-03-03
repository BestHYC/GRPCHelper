# GRPCHelper
针对GRPC长连接始终无法释放,导致负载不均衡解决方法.通过维护HttpClient,来实现对单个GRPC连接进行管控
1.修改源码，解决GrpcClient注入问题
2.重写GrpcChannel,使用表达式树生成client，并且满足即可以复用长连接，又可以达到负载目的
3.博客地址：https://www.cnblogs.com/yichaohong/p/14446694.html 共两篇，介绍了Grpc的源码和完善后的代码，并提供了测试api
注意：测试需要抓包，我用的是linux抓包
