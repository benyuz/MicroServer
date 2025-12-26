MicroServer是一款内嵌式高性能跨平台 WebAPI 框架，基于.NET Standard 2.0 构建，集成简单、不侵业务、开箱即用。免 IIS、单 DLL 即可让程序快速具备 WebAPI 能力，非常适合轻量部署、跨平台及 AOT 编译需求。

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
using MicroServer; // 对应VB的Imports MicroServer

namespace FastTestNamespace // C#需显式声明命名空间
{
    public static class FastTest
    {
        private static readonly WebAPIServer MyAPI = new WebAPIServer();

        public static void Main()
        {
            // 添加路由映射（C#中用委托引用方法）
            MyAPI.AddRoute("/", Hello);
            // 启动服务（默认8090端口）
            MyAPI.StartServer();
            
            Console.WriteLine("访问地址：http://127.0.0.1:8090");
            Console.ReadKey();
        }

        // 异步处理方法（C#的async/await语法）
        private static async Task Hello(HttpListenerRequest request, HttpListenerResponse response)
        {
            // VB的XML字面量<t>...</t>.Value在C#中直接用字符串替代
            await response.WriteAsync("{\"code\":1,\"msg\":\"Hello WebAPI\"}");
        }
    }
}
```

