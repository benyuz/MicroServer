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
Module Test
    Private ReadOnly svr As New WebAPI

    Sub Main()
        ' 1. 配置 (可选)
        svr.SimpleToken = "my_secure_token"

        ' 2. 添加路由
        svr.Routes.Add("/api/hello", AddressOf HelloWorld)

        ' 3. 启动服务
        svr.StartServer(8090) ' 监听 8090 端口

        Console.WriteLine("MicroServer 已启动，访问 http://localhost:8090/api/hello")
        Console.ReadKey()
    End Sub

    ' 你的业务逻辑
    Private Async Function HelloWorld(request As HttpListenerRequest, response As HttpListenerResponse) As Task
        Await response.WriteAsync("Hello, MicroServer!")
    End Function
End Module
```

#### **3. C# 示例 (C# Example)**

```csharp
using System;
using System.Threading.Tasks;

class Program
{
    private static readonly WebAPI svr = new WebAPI();

    static void Main()
    {
        // 1. 配置 (可选)
        svr.SimpleToken = "my_secure_token";

        // 2. 添加路由
        svr.Routes.Add("/api/hello", HelloWorld);

        // 3. 启动服务
        svr.StartServer(8090);

        Console.WriteLine("MicroServer 已启动，访问 http://localhost:8090/api/hello");
        Console.ReadKey();
    }

    // 你的业务逻辑
    private static async Task HelloWorld(HttpListenerRequest request, HttpListenerResponse response)
    {
        await response.WriteAsync("Hello, MicroServer!");
    }
}
```

---

### **详细文档 (Documentation)**

这里是存放详细使用指南的地方。随着项目功能的增加，你可以在这里分章节详细介绍。

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

这个模板已经包含了开源项目 README 的所有关键部分。你可以直接复制使用，并根据 MicroServer 的实际情况来填充和修改内容。

需要我帮你把这个模板里的某个部分，比如 **详细文档** 或 **贡献指南**，先写一个初稿出来吗？