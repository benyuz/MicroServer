Imports System.IO
Imports System.Net
Imports System.Text
Imports System.Runtime.CompilerServices
Imports System.Web
Imports System.Collections.Specialized


Public Module WebAPIExt
    ''' <summary>
    ''' 异步写入响应内容, 并设置响应头,写入后自动关闭输出流
    ''' </summary>
    ''' <param name="response"></param>
    ''' <param name="content">写入的字符串,默认utf8编码</param>
    ''' <returns></returns>
    <Extension()>
    Public Async Function WriteAsync(response As HttpListenerResponse, content As String, Optional contentType As String = Nothing) As Task
        ' 将字符串转换为字节数组
        Dim buffer As Byte() = Encoding.UTF8.GetBytes(content)
        ' 设置响应头：编码、Content-Type、内容长度
        response.ContentEncoding = Encoding.UTF8
        response.ContentType = If(contentType, WebAPIServer.ContentType.TextPlain)
        response.ContentLength64 = buffer.Length
        ' 异步写入输出流
        Await response.OutputStream.WriteAsync(buffer, 0, buffer.Length)
        If buffer.Length > 4096 Then
            Await response.OutputStream.FlushAsync()
        End If
        ' 关闭响应，自动释放资源
        response.Close()
    End Function

    ''' <summary>
    ''' 获取请求体的字符串内容
    ''' </summary>
    ''' <param name="request"></param>
    ''' <returns></returns>
    <Extension()>
    Public Function GetString(request As HttpListenerRequest) As String
        Using reader As New StreamReader(request.InputStream, Encoding.UTF8)
            Return reader.ReadToEnd()
        End Using
    End Function

    ''' <summary>
    ''' 异步获取请求体的字符串内容(高性能不阻塞)
    ''' </summary>
    ''' <param name="request"></param>
    ''' <returns></returns>
    <Extension()>
    Public Async Function GetStringAsync(request As HttpListenerRequest) As Task(Of String)
        Using reader As New StreamReader(request.InputStream, Encoding.UTF8)
            Return Await reader.ReadToEndAsync()
        End Using
    End Function

    ''' <summary>
    ''' 获取请求的Token,对应请求头中Authorization字段. 检查并去除常见的授权前缀 (如 "Bearer " 或 "Basic ")
    ''' </summary>
    ''' <param name="request"></param>
    ''' <returns></returns>
    <Extension()>
    Public Function GetToken(request As HttpListenerRequest) As String
        If request.Headers("Authorization") IsNot Nothing Then
            Dim authHeader As String = request.Headers("Authorization").Trim()

            ' 检查并去除常见的授权前缀 (如 "Bearer " 或 "Basic ")
            If authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase) Then
                Return authHeader.Substring("Bearer ".Length).Trim()
            ElseIf authHeader.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase) Then
                Return authHeader.Substring("Basic ".Length).Trim()
            Else
                ' 如果没有前缀，返回完整的 Header 值
                Return authHeader
            End If
        End If
        Return ""
    End Function
    ''' <summary>
    ''' 发送文件，扩展名自动识别（zip/json/xml/txt/html/jpg/jpeg/png/gif/bmp），否则以二进制文件标识返回（application/octet-stream）
    ''' </summary>
    ''' <param name="response">响应体</param>
    ''' <param name="filePath">文件完整路径</param>
    ''' <returns></returns>
    <Extension()>
    Public Async Function SendFileAsync(response As HttpListenerResponse, filePath As String) As Task
        ' 检查文件是否存在
        If File.Exists(filePath) Then
            ' 获取文件扩展名
            Dim fileExtension As String = Path.GetExtension(filePath).ToLowerInvariant()
            Dim contentType As String

            ' 根据文件扩展名设置Content-Type
            Select Case fileExtension
                Case ".zip"
                    contentType = "application/zip"
                Case ".json"
                    contentType = "application/json;charset=UTF-8"
                Case ".css"
                    contentType = "text/css;charset=UTF-8"
                Case ".js"
                    contentType = "application/javascript;charset=UTF-8"
                Case ".xml"
                    contentType = "application/xml;charset=UTF-8"
                Case ".txt"
                    contentType = "text/plain;charset=UTF-8"
                Case ".html"
                    contentType = "text/html;charset=UTF-8"
                Case ".jpg", ".jpeg"
                    contentType = "image/jpeg"
                Case ".png"
                    contentType = "image/png"
                Case ".gif"
                    contentType = "image/gif"
                Case ".bmp"
                    contentType = "image/bmp"
                ' 其他常见文件类型
                Case ".pdf"
                    contentType = "application/pdf"
                Case ".csv"
                    contentType = "text/csv;charset=UTF-8"
                Case ".svg"
                    contentType = "image/svg+xml;charset=UTF-8"
                Case Else
                    contentType = "application/octet-stream" ' 默认二进制文件
            End Select

            response.ContentType = contentType

            ' 写入响应头，例如Content-Disposition用于建议浏览器以附件形式下载文件
            response.AddHeader("Content-Disposition", "attachment; filename=""" & Path.GetFileName(filePath) & """")

            ' 设置响应头，例如Content-Length用于指示文件大小
            Dim fileSize As Long = New FileInfo(filePath).Length
            response.ContentLength64 = fileSize

            ' 异步发送文件内容
            Using fileStream As FileStream = File.OpenRead(filePath)
                Await fileStream.CopyToAsync(response.OutputStream)
            End Using

            ' 关闭响应流（这将自动发送HTTP响应）
            response.Close()
        Else
            ' 如果文件不存在，发送错误响应
            response.StatusCode = CType(HttpStatusCode.NotFound, Integer)
            response.StatusDescription = "File Not Found"
            response.Close()
        End If
    End Function

    ''' <summary>
    ''' 解析POST请求体(body)中的参数：application/x-www-form-urlencoded
    ''' </summary>
    ''' <param name="request">当前request请求</param>
    ''' <returns></returns>
    <Extension()>
    Public Function ParsePostQueryString(request As HttpListenerRequest) As NameValueCollection
        Return HttpUtility.ParseQueryString(request.GetString)
    End Function

    ''' <summary>
    ''' 解析GET请求url中的参数
    ''' </summary>
    ''' <param name="request">当前request请求</param>
    ''' <returns></returns>
    <Extension()>
    Public Function ParseGetQueryString(request As HttpListenerRequest) As NameValueCollection
        Return HttpUtility.ParseQueryString(request.Url.Query)
    End Function
End Module
