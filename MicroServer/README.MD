MicroServer 是一款可内嵌的轻量级高性能跨平台 WebAPI 框架，可直接嵌入到各类 .NET 应用中快速搭建轻量级 WebAPI 服务，无需依赖 IIS 或 Kestrel。基于 .NET Standard 2.0 构建，开箱即用、不侵入业务、集成简单，完美支持跨平台及 AOT 编译。

VB.NET示例

```vb
Imports System.Net
Imports MicroServer

Module FastTest
    Private ReadOnly MyAPI As New WebAPIServer
    Sub Main()
        MyAPI.AddRoute("/", AddressOf hello) '添加路由映射
        MyAPI.StartServer() '启动 WebAPI 服务,默认端口8090 传入参数可修改端口
        Console.WriteLine("访问地址：http://127.0.0.1:8090")
        Console.ReadKey()
    End Sub

    Private Async Function hello(request As HttpListenerRequest, response As HttpListenerResponse) As Task
        Await response.WriteAsync(<t>{"code":1,"msg":"Hello WebAPI"}</t>.Value)
    End Function
End Module

```

C#示例

```csharp
using System.Net;
using MicroServer;

namespace FastTestNamespace
{
    public static class FastTest
    {
        private static readonly WebAPIServer MyAPI = new WebAPIServer();

        public static void Main()
        {
            MyAPI.AddRoute("/", Hello);// 添加路由映射
            MyAPI.StartServer(); // 启动服务（默认8090端口）
            
            Console.WriteLine("访问地址：http://127.0.0.1:8090");
            Console.ReadKey();
        }

        // 异步处理方法
        private static async Task Hello(HttpListenerRequest request, HttpListenerResponse response)
        {
          await response.WriteAsync("""{"code":1,"msg":"Hello WebAPI"}""");
        }
    }
}
```

