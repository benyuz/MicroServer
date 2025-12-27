### **MicroServer**

**给你的程序快速加个WebAPI。**

MicroServer 是一款可内嵌的轻量级高性能跨平台 WebAPI 框架，可直接嵌入到各类 .NET 应用中快速搭建轻量级 WebAPI 服务，无需依赖 IIS 或 Kestrel。基于 .NET Standard 2.0 构建，开箱即用、不侵入业务、集成简单，完美支持跨平台及 AOT 编译。

---

### **目录 (Table of Contents)**

- [项目介绍](#项目介绍)
- [核心特点](#核心特点)
- [快速开始](#快速开始)
- [详细文档](#详细文档)
- [贡献指南](#贡献指南)
- [赞助与鸣谢](#赞助与鸣谢)
- [许可证](#许可证)

---

### **项目介绍 (Project Description)**

这是一个为开发者打造的内嵌式高性能跨平台 WebAPI 框架。MicroServer不做大而全的框架，也不做小而全的框架，MicroServer的目标是做WEB请求转发管理和通信连接，让你能够**快速、无痛**地为任何 .NET 程序（无论是 VB.NET 还是 C#）添加 WebAPI 能力，而无需依赖 IIS 等复杂的 Web 服务器，MicroServer内置了 简单token 验证和 JWT 验证，你也可以基于MicroServer的路由映射和中间件二次开发做出更多功能和集成。

**一句话总结：** MicroServer = 一个 DLL + 几行代码 = 你的程序拥有了 WebAPI。

---

### **核心特点 (Key Features)**

*   **🚀 极简部署**：一个 DLL 文件，直接嵌入你的程序，零配置启动。
*   **🌍 跨平台**：基于 .NET Standard 2.0，Windows、Linux 都能跑。
*   **💻 双语言支持**：支持C# 和 VB.NET 开发。
*   **⚡ 高性能**：异步事件驱动架构，处理请求更高效。
*   **🔐 安全可靠**：内置简单 Token 和 JWT 两种验证方式。
*   **📡 功能完备**：支持 HTTP 接口（文字/文件传输）和 WebSocket 客户端。
*   **🛠️ 易于扩展**：灵活的路由和中间件设计，方便自定义。

---

### **快速开始 (Quick Start)**

#### **1. 安装 (Installation)**

最简单的方式是直接在nuget下载安装。

[NuGet Gallery | MicroServer](https://www.nuget.org/packages/MicroServer)

##### 命令行安装
1. NET CLI 命令（推荐，跨平台通用）
  ```bash
   dotnet add package MicroServer
  ```


2. Package Manager 控制台命令（Visual Studio 内使用）
 ```bash
   Install-Package MicroServer
 ```

#### **2. VB.NET 示例 (VB.NET Example)**

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

#### **3. C# 示例 (C# Example)**

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

---

### **详细文档 (Documentation)**


*   **[WebAPI 服务端](docs/webapi-server.md)**
    *   路由管理
    *   响应客户端（文字、文件）
    *   授权验证（Token, JWT）
    *   路由白名单
*   **[WebSocket 客户端](docs/websocket-client.md)**
    *   连接与断开
    *   发送与接收消息
    *   心跳机制
*   **[高级特性](docs/advanced.md)**
    *   灵活映射
    *   中间件
    *   跨平台支持
    *   AOT 编译优化

---

### **贡献指南 (Contributing)**

感谢你的兴趣！欢迎你为 MicroServer 贡献代码、报告 Bug 或提出新功能建议。

---

### **赞助与鸣谢 (Sponsors & Acknowledgements)**

感谢所有为本项目提供支持和灵感的个人与组织。

*   [VB6资源站](http://lydys.cn:1122)

---

### **许可证 (License)**

本项目采用 **MIT 许可证** - 详情请查看 [LICENSE](LICENSE) 文件。

---