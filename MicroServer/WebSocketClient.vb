Imports System.Net.WebSockets
Imports System.Text
Imports System.Threading
Imports System

''' <summary>
''' 简易的 WebSocket 客户端，支持ws和wss协议，支持连接超时设置
''' </summary>
Public Class WebSocketClient
    Private ReadOnly _clientWebSocket As ClientWebSocket
    Private ReadOnly _uri As Uri
    Private ReadOnly _cancellationTokenSource As CancellationTokenSource
    Private _connectTimeout As TimeSpan ' 超时时间

    ''' <summary>
    ''' WebSocket 连接已建立
    ''' </summary>
    Public Event OnConnected As EventHandler
    ''' <summary>
    ''' 收到消息
    ''' </summary>
    Public Event OnMessageReceived As EventHandler(Of String)
    ''' <summary>
    ''' WebSocket 连接已关闭
    ''' </summary>
    Public Event OnDisconnected As EventHandler
    ''' <summary>
    ''' WebSocket 连接失败
    ''' </summary>
    Public Event OnError As EventHandler(Of WebSocketErrorEventArgs)

    ''' <summary>
    ''' 构造函数，支持设置连接超时,默认连接时间为 5 秒
    ''' </summary>
    ''' <param name="uri">连接地址</param>
    ''' <param name="connectTimeoutSeconds">超时间隔，单位：秒，默认 5 秒，可选参数</param>
    Public Sub New(uri As String, Optional connectTimeoutSeconds As Integer = 5)
        _uri = New Uri(uri)
        _clientWebSocket = New ClientWebSocket()
        _cancellationTokenSource = New CancellationTokenSource()
        _connectTimeout = TimeSpan.FromSeconds(connectTimeoutSeconds) ' 转换为 TimeSpan
    End Sub

    ''' <summary>
    ''' 启动 WebSocket 客户端连接
    ''' </summary>
    Public Sub StartConnect()
        ' 使用 Task.Run 执行异步操作，并捕获可能的异常
        Task.Run(Async Function()
                     Try
                         ' 调用 StartAsync 执行连接
                         Await StartAsync()
                     Catch ex As Exception
                         ' 捕获异常并处理（可以记录日志或触发错误事件等）
                         RaiseEvent OnError(Me, New WebSocketErrorEventArgs(WebSocketErrorCode.UnknownError, ex.Message))
                     End Try
                 End Function)
    End Sub

    ''' <summary>
    ''' 停止 WebSocket 客户端连接
    ''' </summary>
    Public Sub StopConnect()
        ' 使用 Task.Run 执行异步操作，并捕获可能的异常
        Task.Run(Async Function()
                     Try
                         ' 调用 StopAsync 执行停止
                         Await StopAsync()
                     Catch ex As Exception
                         ' 捕获异常并处理（可以记录日志或触发错误事件等）
                         RaiseEvent OnError(Me, New WebSocketErrorEventArgs(WebSocketErrorCode.UnknownError, ex.Message))
                     End Try
                 End Function)
    End Sub

    ''' <summary>
    ''' 启动 WebSocket 客户端连接（内部异步方法）
    ''' </summary>
    Private Async Function StartAsync() As Task
        Try
            ' 使用 Task.WhenAny 来处理连接超时
            Dim connectTask As Task = _clientWebSocket.ConnectAsync(_uri, _cancellationTokenSource.Token)
            Dim timeoutTask As Task = Task.Delay(_connectTimeout)

            ' 等待连接任务或超时任务完成
            Dim completedTask = Await Task.WhenAny(connectTask, timeoutTask)

            If completedTask Is timeoutTask Then
                ' 如果超时任务先完成，取消连接操作并抛出超时异常
                _clientWebSocket.Dispose()
                RaiseEvent OnError(Me, New WebSocketErrorEventArgs(WebSocketErrorCode.ConnectionTimeout, "Connection timeout"))
                Return
            End If

            ' 连接完成后，检查 WebSocket 的状态
            If _clientWebSocket.State <> WebSocketState.Open Then
                ' 如果状态不是 Open，说明连接失败
                _clientWebSocket.Dispose()
                RaiseEvent OnError(Me, New WebSocketErrorEventArgs(WebSocketErrorCode.UnknownError, "Failed to establish WebSocket connection"))
                Return
            End If

            ' 连接成功，触发 OnConnected 事件
            RaiseEvent OnConnected(Me, EventArgs.Empty)

            ' 开始接收消息
            Await ReceiveMessagesAsync()
        Catch ex As TimeoutException
            ' 处理超时异常
            RaiseEvent OnError(Me, New WebSocketErrorEventArgs(WebSocketErrorCode.ConnectionTimeout, ex.Message))
        Catch ex As Exception
            ' 其他异常
            RaiseEvent OnError(Me, New WebSocketErrorEventArgs(WebSocketErrorCode.UnknownError, ex.Message))
        End Try
    End Function

    ''' <summary>
    ''' 停止 WebSocket 客户端连接（内部异步方法）
    ''' </summary>
    Private Async Function StopAsync() As Task
        Try
            ' 取消连接并关闭 WebSocket
            _cancellationTokenSource.Cancel()
            If _clientWebSocket.State = WebSocketState.Open Then
                Await _clientWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None)
            End If

            ' 触发 OnDisconnected 事件
            RaiseEvent OnDisconnected(Me, EventArgs.Empty)
        Catch ex As Exception
            ' 关闭连接时发生错误
            RaiseEvent OnError(Me, New WebSocketErrorEventArgs(WebSocketErrorCode.DisconnectError, ex.Message))
        End Try
    End Function

    ''' <summary>
    ''' 发送消息
    ''' </summary>
    ''' <param name="message">要发送的字符串消息，自动使用 UTF-8 编码</param>
    Public Async Function SendMessageAsync(message As String) As Task
        If _clientWebSocket.State = WebSocketState.Open Then
            Try
                Dim buffer As Byte() = Encoding.UTF8.GetBytes(message)
                Await _clientWebSocket.SendAsync(New ArraySegment(Of Byte)(buffer), WebSocketMessageType.Text, True, CancellationToken.None)
            Catch ex As Exception
                ' 捕获发送消息中的错误并触发错误事件
                RaiseEvent OnError(Me, New WebSocketErrorEventArgs(WebSocketErrorCode.SendError, ex.Message))
            End Try
        Else
            ' WebSocket 未连接，触发错误事件
            RaiseEvent OnError(Me, New WebSocketErrorEventArgs(WebSocketErrorCode.NotConnected, "WebSocket is not connected."))
        End If
    End Function

    ''' <summary>
    ''' 接收消息
    ''' </summary>
    ''' <returns>无返回值</returns>
    Private Async Function ReceiveMessagesAsync() As Task
        ' 初始缓冲区大小为 1024 字节
        Dim buffer(1024) As Byte
        Dim messageBuilder As New StringBuilder() ' 用于拼接消息

        While _clientWebSocket.State = WebSocketState.Open
            Try
                ' 接收数据
                Dim result As WebSocketReceiveResult = Await _clientWebSocket.ReceiveAsync(New ArraySegment(Of Byte)(buffer), _cancellationTokenSource.Token)

                ' 如果消息类型是文本消息
                If result.MessageType = WebSocketMessageType.Text Then
                    ' 解码接收到的片段并拼接到消息构建器中
                    messageBuilder.Append(Encoding.UTF8.GetString(buffer, 0, result.Count))

                    ' 检查是否为完整的消息
                    If result.EndOfMessage Then
                        ' 完整的消息已经接收完毕
                        Dim fullMessage As String = messageBuilder.ToString()
                        messageBuilder.Clear() ' 清空消息构建器，准备接收下一个消息

                        ' 触发消息接收事件
                        RaiseEvent OnMessageReceived(Me, fullMessage)
                    End If
                ElseIf result.MessageType = WebSocketMessageType.Close Then
                    ' 处理关闭消息
                    Await StopAsync()
                End If
            Catch ex As Exception
                ' 捕获接收过程中的错误
                RaiseEvent OnError(Me, New WebSocketErrorEventArgs(WebSocketErrorCode.ReceiveError, ex.Message))
                Exit While
            End Try
        End While
    End Function
    ''' <summary>
    ''' 发送 Ping 消息作为心跳包
    ''' </summary>
    ''' <param name="msg">发送的消息，默认：ping</param>
    ''' <returns>如果 Ping 成功，返回 True；否则返回 False</returns>
    Public Async Function SendPingAsync(Optional msg As String = "ping") As Task(Of Boolean)
        If _clientWebSocket.State <> WebSocketState.Open Then
            ' 如果 WebSocket 没有处于打开状态，认为连接无效
            Return False
        End If

        ' 发送一个简单的 ping 消息（可以是空消息）
        Try
            Dim buffer As Byte() = Encoding.UTF8.GetBytes(msg)
            Await _clientWebSocket.SendAsync(New ArraySegment(Of Byte)(buffer), WebSocketMessageType.Text, True, CancellationToken.None)
            Return True
        Catch ex As Exception
            ' 如果发送失败，认为连接无效
            RaiseEvent OnError(Me, New WebSocketErrorEventArgs(WebSocketErrorCode.UnknownError, "Failed to send ping: " & ex.Message))
            Return False
        End Try
    End Function

End Class

''' <summary>
''' WebSocket 错误代码枚举
''' </summary>
Public Enum WebSocketErrorCode
    ConnectionTimeout = 1001 ' 连接超时
    NotConnected = 1002 ' WebSocket 未连接
    SendError = 1003 ' 发送消息时发生错误
    ReceiveError = 1004 ' 接收消息时发生错误
    DisconnectError = 1005 ' 关闭连接时发生错误
    UnknownError = 9999 ' 未知错误
End Enum

''' <summary>
''' WebSocket 错误事件参数类
''' </summary>
Public Class WebSocketErrorEventArgs
    Inherits EventArgs
    Public Property ErrorCode As WebSocketErrorCode
    Public Property ErrorMessage As String
    Public Sub New(errorCode As WebSocketErrorCode, errorMessage As String)
        Me.ErrorCode = errorCode
        Me.ErrorMessage = errorMessage
    End Sub
End Class
