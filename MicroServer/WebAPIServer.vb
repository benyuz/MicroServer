Imports System.Data.Common
Imports System.IO
Imports System.Net
Imports System.Net.Sockets
Imports System.Security.Cryptography
Imports System.Text


Public Class WebAPIServer
    ''' <summary>
    ''' 获取13位时间戳
    ''' </summary>
    ''' <returns></returns>
    Public Function GetTimeStamp() As Long
        Dim epoch As New DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        Return (DateTime.UtcNow - epoch).TotalMilliseconds
    End Function
    ''' <summary>
    ''' 获取10位时间戳
    ''' </summary>
    ''' <returns></returns>
    Public Function GetTimeStamp10() As Long
        Dim epoch As New DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        Return (DateTime.UtcNow - epoch).TotalSeconds
    End Function

    Public Class ContentType
        Public Shared ReadOnly Property ApplicationJson As String = "application/json;charset=UTF-8"
        Public Shared ReadOnly Property ApplicationXml As String = "application/xml;charset=UTF-8"
        Public Shared ReadOnly Property TextPlain As String = "text/plain;charset=UTF-8"
        Public Shared ReadOnly Property TextHtml As String = "text/html;charset=UTF-8"
        Public Shared ReadOnly Property FormUrlEncoded As String = "application/x-www-form-urlencoded"
        Public Shared ReadOnly Property MultipartFormData As String = "multipart/form-data"
    End Class

    ''' <summary>
    ''' WebApi监听实例
    ''' </summary>
    Private Shared ReadOnly Listener As New HttpListener

    ''' <summary>
    ''' 启用https，端口443不能修改。可配合nginx，功能可能还不完善
    ''' </summary>
    Public EnableHttps As Boolean = False

    ''' <summary>
    ''' 请求头中简单的token验证,为空则不需要验证，发起请求时Authorization字段填入token即可
    ''' </summary>
    Public SimpleToken As String

    ''' <summary>
    ''' 启用自带JWT验证
    ''' </summary>
    Public JWT As New JWTclass
    ''' <summary>
    ''' 是否启用websocket支持，注意端口和http端口相同
    ''' </summary>
    Public enableWebSocket As Boolean = False

    ''' <summary>
    ''' 路由->处理程序映射的字典集合 key为路由，value为处理程序.
    ''' </summary>
    Private ReadOnly Routes As New Dictionary(Of String, Func(Of HttpListenerRequest, HttpListenerResponse, Task))
    ''' <summary>
    ''' 路由->允许的请求方法 映射的字典集合 key为路由，value为允许的方法列表。
    ''' 多个方法建议在handler方法中自行判断。
    ''' </summary>
    Private ReadOnly RouteMethods As New Dictionary(Of String, String)
    ''' <summary>
    ''' 路由白名单，白名单内的路由不进行验证
    ''' </summary>
    Private ReadOnly RouteWhiteList As New List(Of String)

    ''' <summary>
    ''' 中间件列表
    ''' </summary>
    Private ReadOnly Middlewares As New List(Of Func(Of HttpListenerRequest, HttpListenerResponse, Task(Of Boolean)))

    ''' <summary>
    ''' 添加路由映射,并可选择限定请求方法
    ''' </summary>
    ''' <param name="path">路由路径</param>
    ''' <param name="handler">具体的处理程序</param>
    ''' <param name="method">(可选)自动限定路由请求方法</param>
    Public Sub AddRoute(path As String, handler As Func(Of HttpListenerRequest, HttpListenerResponse, Task), Optional method As String = Nothing)
        '映射路由
        Routes.Add(path, handler)
        '映射路由请求方法
        If method IsNot Nothing Then
            RouteMethods.Add(path, method.ToUpper)
        End If
    End Sub

    ''' <summary>
    ''' 添加中间件（适配布尔值返回类型）
    ''' </summary>
    ''' <param name="handler">中间件委托</param>
    Public Sub AddMiddleware(handler As Func(Of HttpListenerRequest, HttpListenerResponse, Task(Of Boolean)))
        ' 保留非空判断，避免空委托
        If handler IsNot Nothing Then
            Middlewares.Add(handler)
        End If
    End Sub

    ''' <summary>
    ''' 添加简单token
    ''' </summary>
    ''' <param name="token"></param>
    Public Sub AddSimpleTokenVerify(token As String)
        If String.IsNullOrWhiteSpace(token) Then Return
        SimpleToken = token
        AddMiddleware(AddressOf SimpleTokenVerify)
    End Sub

    ''' <summary>
    ''' 简单Token验证，验证成功继续执行，验证失败自动中断请求
    ''' </summary>
    ''' <param name="request"></param>
    ''' <param name="response"></param>
    ''' <returns>验证通过返回True，验证失败返回False</returns>
    Private Async Function SimpleTokenVerify(request As HttpListenerRequest, response As HttpListenerResponse) As Task(Of Boolean)
        '白名单校验
        If RouteWhiteList.Contains(request.Url.AbsolutePath) Then Return True
        '简单token验证
        Dim token = request.GetToken()
        If token = SimpleToken Then
            Await Task.CompletedTask
            Return True
        Else
            response.StatusCode = 401
            Await response.WriteAsync("Unauthorized")
            Return False
        End If
    End Function

    ''' <summary>
    ''' 添加JWT验证，验证成功继续执行，验证失败自动中断请求
    ''' </summary>
    ''' <param name="pwd">JWT加密密码，采用HS256加密</param>
    Public Sub AddJwtTokenVerify(pwd As String)
        If String.IsNullOrWhiteSpace(pwd) Then Return
        JWT.PassWord = pwd
        AddMiddleware(AddressOf JwtTokenVerify)
    End Sub

    ''' <summary>
    ''' JWT验证
    ''' </summary>
    ''' <param name="request"></param>
    ''' <param name="response"></param>
    ''' <returns>验证通过返回True，急促实行，验证失败返回False</returns>
    Private Async Function JwtTokenVerify(request As HttpListenerRequest, response As HttpListenerResponse) As Task(Of Boolean)
        '白名单校验
        If RouteWhiteList.Contains(request.Url.AbsolutePath) Then Return True
        'JWT验证
        Dim token = request.GetToken()
        If JWT.VerifyToken(token) Then
            Await Task.CompletedTask
            Return True
        Else
            response.StatusCode = 401
            Await response.WriteAsync("Unauthorized")
            Return False
        End If
    End Function

    ''' <summary>
    ''' JWT处理类
    ''' </summary>
    Public Class JWTclass
        Private ReadOnly HEADER As String = "{""alg"":""HS256"",""typ"":""JWT""}"
        ''' <summary>
        ''' JWT加密密码 默认为 mr123456，建议自行修改
        ''' </summary>
        Public PassWord As String = "mr123456"

        ''' <summary>
        ''' 验证token，加密算法HS256，请求时需要在请求头中Authorization字段中设置Bearer 标记
        ''' </summary>
        Friend Function VerifyToken(token As String) As Boolean
            Dim parts As String() = token.Split(".", 3, StringSplitOptions.None)
            If parts.Length <> 3 Then Return False

            Dim recPayload As String = DecodeBase64Url(parts(1))
            Dim recSignature As String = parts(2)

            Dim Signature As String = ComputeHmac256(recPayload, PassWord)
            Return recSignature = Signature
        End Function
        ''' <summary>
        ''' 解码Payload部分,未开启JWT时返回空字符串
        ''' </summary>
        ''' <param name="token"></param>
        ''' <returns></returns>
        Function DecodePayload(token As String) As String
            Dim parts As String() = token.Split("."c)
            ' 解码 Payload 部分 (Base64Url 解码)
            Return DecodeBase64Url(parts(1))
        End Function
        ''' <summary>
        ''' 创建Token，加密算法HS256，先设置PassWord
        ''' </summary>
        ''' <returns>返回签名后的token</returns>
        Function GenerateToken(playload As String) As String
            Dim s, token As String
            s = ComputeHmac256(playload, PassWord)
            token = $"{EnCodeBase64Url(HEADER)}.{EnCodeBase64Url(playload)}.{s}"
            Return token
        End Function
    End Class

    ''' <summary>
    ''' 计算HS256
    ''' </summary>
    ''' <param name="data">待加密的数据</param>
    ''' <param name="key">密钥</param>
    ''' <returns>加密后的值</returns>
    Shared Function ComputeHmac256(data As String, key As String) As String
        Dim keyBytes As Byte() = Encoding.UTF8.GetBytes(key)
        Dim dataBytes As Byte() = Encoding.UTF8.GetBytes(data)

        Using hmac As New HMACSHA256(keyBytes)
            Dim hashBytes As Byte() = hmac.ComputeHash(dataBytes)
            Dim base64 As String = Convert.ToBase64String(hashBytes)
            Dim base64url As String = base64.TrimEnd("=").Replace("+", "-").Replace("/", "_")
            Return base64url
        End Using
    End Function

    ''' <summary>
    ''' Base64Url编码
    ''' </summary>
    ''' <param name="input"></param>
    ''' <returns></returns>
    Public Shared Function EnCodeBase64Url(input As String) As String
        Dim bytes As Byte() = Encoding.UTF8.GetBytes(input)
        Dim base64 As String = Convert.ToBase64String(bytes)
        Dim base64url As String = base64.TrimEnd("=").Replace("+", "-").Replace("/", "_")
        Return base64url
    End Function
    ''' <summary>
    ''' Base64Url解码
    ''' </summary>
    ''' <param name="encodedString"></param>
    ''' <returns></returns>
    Private Shared Function DecodeBase64Url(ByVal encodedString As String) As String
        encodedString = encodedString.Replace("-", "+").Replace("_", "/")
        Dim paddingLength As Integer = 4 - (encodedString.Length Mod 4)
        If paddingLength < 4 Then
            encodedString &= New String("=", paddingLength)
        End If
        Dim bytes As Byte() = Convert.FromBase64String(encodedString)
        Return System.Text.Encoding.UTF8.GetString(bytes)
    End Function

    ''' <summary>
    ''' 获取所有找到的 IPv4 地址的列表
    ''' </summary>
    ''' <returns>IP列表</returns>
    Public Function GetLocalIPAddresses() As List(Of String)
        Dim ipList As New List(Of String)()
        Try
            Dim host As String = Dns.GetHostName() ' 获取本机主机名
            Dim addresses As IPAddress() = Dns.GetHostAddresses(host) ' 获取主机的 IP 地址

            For Each ip As IPAddress In addresses
                If ip.AddressFamily = AddressFamily.InterNetwork Then ' 只添加 IPv4 地址
                    ipList.Add(ip.ToString())
                End If
            Next
        Catch ex As Exception
            Console.WriteLine("获取 IP 地址时出错: " & ex.Message)
        End Try

        Return ipList ' 返回包含所有找到的 IPv4 地址的列表
    End Function
    ''' <summary>
    ''' 开启服务,程序需要管理员权限
    ''' </summary>
    ''' <param name="Port">监听的端口，和IP无关</param>
    Private Async Sub SimpleListenerAsync(Optional Port As Integer = 8090)
        If Not HttpListener.IsSupported Then Return
        ' 监听本机端口
        If EnableHttps Then
            Listener.Prefixes.Add($"https://+:{Port}/")  'https未实现,建议用专业web服务器进行转发
        Else
            Listener.Prefixes.Add($"http://+:{Port}/")
        End If
        Listener.Start()

        While True

            Dim context = Await Listener.GetContextAsync()
            Dim request = context.Request
            Dim Response = context.Response
            Dim path = request.Url.AbsolutePath
            Try

                '检查路由映射是否存在
                If Routes.ContainsKey(path) Then
                    '检查请求方法是否匹配
                    If RouteMethods.ContainsKey(path) AndAlso request.HttpMethod.ToUpperInvariant <> RouteMethods(path) Then
                        Response.StatusCode = 405
                        Response.Headers.Add("Allow", RouteMethods(path))
                        Await Response.WriteAsync("Method Not Allowed")
                        Continue While
                    End If
                Else
                    Response.StatusCode = 404
                    Await Response.WriteAsync("Not Found")
                    Continue While
                End If
                '执行中间件
                For Each middleware In Middlewares
                    Dim result = Await middleware(request, Response)
                    '中间件验证成功（True）继续往下执行，验证失败（False）自动中断请求
                    If result = False Then
                        Response.Abort() '兜底，终止响应
                        Continue While
                    End If
                Next
                '继续执行
                If request.IsWebSocketRequest Then
                    Response.StatusCode = 400
                    Response.StatusDescription = "Bad Request"
                    Await Response.WriteAsync("WebSocket Not Supported")
                Else '处理HTTP请求
                    Await Routes(path)(request, Response)
                End If

            Catch ex As Exception
                ' 记录异常并返回 500 错误
                Response.StatusCode = 500
                Response.StatusDescription = "Internal Server Error"
                Dim unused = Response.WriteAsync("Internal Server Error")
            End Try

        End While
    End Sub

    ''' <summary>
    ''' 启动服务，单独线程。端口默认8090
    ''' </summary>
    ''' <param name="Port">端口，默认8090</param>
    Public Sub StartServer(Optional Port As Integer = 8090)
        Try
            Task.Run(Sub() SimpleListenerAsync(Port))
        Catch ex As Exception
            Console.WriteLine("WebAPI服务启动失败: " & ex.Message)
        End Try
    End Sub

    ''' <summary>
    ''' 停止服务，可能需要几百毫秒释完全放资源
    ''' </summary>
    Public Sub StopServer()
        Try
            Listener.Stop()
            Listener.Close()
            Listener.Abort()
            Console.WriteLine("Server stopped!")
        Finally
            ' 确保在这里释放任何需要的资源  
            ' 注意：HttpListener 不需要显式释放，但如果有其他资源，需要在此处释放
        End Try
    End Sub
End Class
