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
