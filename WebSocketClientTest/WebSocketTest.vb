Imports MicroServer
Module WebSocketTest

    ' 声明 WebSocket 客户端
    Private wsClient As WebSocketClient

    Sub Main()
        ' 初始化 WebSocket 客户端并指定 WebSocket 服务器地址
        ' wsClient = New WebSocketClient("wss://echo.websocket.org/")
        wsClient = New WebSocketClient("ws://127.0.0.1:8888/")

        ' 订阅事件
        AddHandler wsClient.OnConnected, AddressOf OnConnected
        AddHandler wsClient.OnMessageReceived, AddressOf OnMessageReceived
        AddHandler wsClient.OnDisconnected, AddressOf OnDisconnected
        AddHandler wsClient.OnError, AddressOf OnError

        ' 启动 WebSocket 客户端
        StartWebSocket()

        ' 让用户输入消息并发送
        While True
            Console.WriteLine("请输入消息发送到 WebSocket（输入 'exit' 退出）：")
            Dim message As String = Console.ReadLine()
            If message.ToLower() = "exit" Then
                Exit While
            End If
            wsClient.SendMessageAsync(message)
        End While

        ' 停止 WebSocket 客户端连接
        StopWebSocket()

        Console.WriteLine("按任意键退出...")
        Console.ReadKey()
    End Sub

    ' 启动 WebSocket 客户端并连接到服务器
    Private Sub StartWebSocket()
        Console.WriteLine("正在连接到 WebSocket 服务器...")
        wsClient.StartConnect()
    End Sub

    ' 停止 WebSocket 客户端
    Private Sub StopWebSocket()
        Console.WriteLine("正在断开连接...")
        wsClient.StopConnect()
    End Sub

    ' 连接成功事件
    Private Sub OnConnected(sender As Object, e As EventArgs)
        Console.WriteLine("WebSocket 连接成功！")
    End Sub

    ' 收到消息事件
    Private Sub OnMessageReceived(sender As Object, message As String)
        Console.WriteLine()
        Console.WriteLine()
        Console.WriteLine($"收到消息: {message}")
    End Sub

    ' 连接断开事件
    Private Sub OnDisconnected(sender As Object, e As EventArgs)
        Console.WriteLine("WebSocket 连接已断开！")
    End Sub

    ' 错误事件
    Private Sub OnError(sender As Object, e As WebSocketErrorEventArgs)
        Console.WriteLine($"错误！ Code: {e.ErrorCode}, Message: {e.ErrorMessage}")
    End Sub
End Module
