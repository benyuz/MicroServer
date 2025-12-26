### **MicroServer**

**给你的程序快速加个WebAPI。**

免IIS，单DLL，一个类，几行代码，跨平台，开箱即用。

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

这是一个为开发者打造的内嵌式高性能跨平台 WebAPI 框架。它的核心目标是让你能够**快速、无痛**地为任何 .NET 程序（无论是 VB.NET 还是 C#）添加 WebAPI 能力，而无需依赖 IIS 等复杂的 Web 服务器。

**一句话总结：** MicroServer = 一个 DLL + 几行代码 = 你的程序拥有了 WebAPI。

---

### **核心特点 (Key Features)**

*   **🚀 极简部署**：一个 DLL 文件，直接嵌入你的程序，零配置启动。
*   **🌍 跨平台**：基于 .NET Standard 2.0，Windows、Linux 都能跑。
*   **💻 双语言支持**：原生 VB.NET 开发，完美兼容 C#。
*   **⚡ 高性能**：异步事件驱动架构，处理请求更高效。
*   **🔐 安全可靠**：内置简单 Token 和 JWT 两种验证方式。
*   **📡 功能完备**：支持 HTTP 接口（文字/文件传输）和 WebSocket 客户端。
*   **🛠️ 易于扩展**：灵活的路由和 Handler 设计，方便自定义。

---

### **快速开始 (Quick Start)**

#### **1. 安装 (Installation)**

最简单的方式是直接下载编译好的 DLL 文件。

1.  前往 [Releases](https://gitee.com/jzy168/MicroServer/releases/) 页面。
2.  下载最新版本的 `MicroServer.dll`。
3.  在你的 Visual Studio 项目中，右键 -> **添加** -> **引用** -> 浏览并选择下载的 DLL 文件。

#### **2. VB.NET 示例 (VB.NET Example)**

```vbnet
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
    *   自定义配置
    *   中间件（如果未来支持）
    *   AOT 编译优化

> **提示**：你可以将详细文档放在项目的 `docs` 文件夹中，然后在这里用链接指向它们，保持主 README 的简洁。

---

### **贡献指南 (Contributing)**

感谢你的兴趣！如果你想为 MicroServer 贡献代码、报告 Bug 或提出新功能建议，请阅读我们的 [贡献指南](CONTRIBUTING.md)。

*   **报告 Bug**：请在 [Issues](https://gitee.com/jzy168/MicroServer/issues) 中提交，并附上详细的复现步骤。
*   **提交代码**：欢迎 Fork 本仓库，创建你的特性分支 (`git checkout -b feature/amazing-feature`)，然后提交 Pull Request。

---

### **赞助与鸣谢 (Sponsors & Acknowledgements)**

感谢所有为本项目提供支持和灵感的个人与组织。

*   [VB6资源站](http://lydys.cn:1122)

---

### **许可证 (License)**

本项目采用 **MIT 许可证** - 详情请查看 [LICENSE](LICENSE) 文件。

---