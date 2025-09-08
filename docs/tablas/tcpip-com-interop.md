# TCP/IP y COM Interop en .NET

## Contexto y Propósito

### ¿Qué es?
- **TCP/IP en .NET:** conjunto de protocolos de red que permiten comunicación cliente-servidor mediante sockets (`TcpClient`, `TcpListener`, `SocketAsyncEventArgs`).  
- **COM Interop:** mecanismo que permite a aplicaciones .NET interactuar con librerías heredadas basadas en COM (Component Object Model), muy usadas en sistemas CAD y librerías nativas de Windows.

### ¿Por qué?
Porque los sistemas CAD requieren comunicación en tiempo real vía TCP/IP y suelen integrar componentes antiguos expuestos vía COM. Dominar ambos habilita la interoperabilidad con software legacy y la construcción de soluciones modernas sin romper compatibilidad.

### ¿Para qué?
- **TCP/IP:**  
  - Implementar clientes y servidores concurrentes.  
  - Manejar mensajes binarios o serializados en XML/JSON.  
  - Asegurar comunicación robusta con timeouts, TLS y reconexión.  
- **COM Interop:**  
  - Consumir librerías nativas desde .NET.  
  - Integrar APIs CAD legacy en aplicaciones modernas.  
  - Manejar marshaling seguro y liberar memoria correctamente.  

### Valor agregado desde la experiencia
- Implementar **servidores TCP asíncronos con async/await** para manejar miles de conexiones simultáneas en sistemas municipales.  
- Diseñar protocolos sobre **NetworkStream + JSON/XML** permitió interoperar entre CAD y APIs modernas.  
- Integrar librerías **COM heredadas** en .NET Core mediante `DllImport` y `Runtime.InteropServices`.  
- Resolver memory leaks en **Interop COM** usando `Marshal.ReleaseComObject` mejoró estabilidad en banca.  

# TCP/IP & COM Interop - 🌐 Comunicación y Conectividad en .NET

## 📋 Índice de Contenidos

| Sección | Descripción |
|---------|-------------|
| [🌐 TCP/IP Networking](#tcpip) | Sockets, TCP, UDP en .NET |
| [🔌 Socket Programming](#sockets) | Programación con sockets |
| [📡 HTTP Client/Server](#http) | HttpClient, HttpListener |
| [⚙️ COM Interop](#com) | Integración con componentes COM |
| [🏭 COM+ Services](#complus) | Servicios empresariales |
| [📞 P/Invoke](#pinvoke) | Llamadas a APIs nativas |
| [🔗 WCF Services](#wcf) | Windows Communication Foundation |
| [📨 Message Queuing](#msmq) | MSMQ y colas de mensajes |

---

## 🌐 TCP/IP Networking {#tcpip}

### Conceptos Fundamentales

| Concepto | Descripción | Uso en .NET |
|----------|-------------|-------------|
| **TCP** | Protocolo confiable | `TcpClient`, `TcpListener` |
| **UDP** | Protocolo rápido | `UdpClient` |
| **Socket** | Endpoint de comunicación | `Socket` class |
| **IPEndPoint** | Dirección IP + Puerto | Configuración de conexiones |

### TCP Client/Server Básico

```csharp
// TCP Server
public class TcpServer
{
    private TcpListener _listener;
    private bool _isRunning;
    
    public async Task StartAsync(int port)
    {
        _listener = new TcpListener(IPAddress.Any, port);
        _listener.Start();
        _isRunning = true;
        
        Console.WriteLine($"Servidor iniciado en puerto {port}");
        
        while (_isRunning)
        {
            var client = await _listener.AcceptTcpClientAsync();
            _ = Task.Run(() => HandleClientAsync(client));
        }
    }
    
    private async Task HandleClientAsync(TcpClient client)
    {
        using (client)
        using (var stream = client.GetStream())
        using (var reader = new StreamReader(stream))
        using (var writer = new StreamWriter(stream))
        {
            try
            {
                string message;
                while ((message = await reader.ReadLineAsync()) != null)
                {
                    Console.WriteLine($"Recibido: {message}");
                    await writer.WriteLineAsync($"Echo: {message}");
                    await writer.FlushAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }
    
    public void Stop()
    {
        _isRunning = false;
        _listener?.Stop();
    }
}

// TCP Client
public class TcpClientExample
{
    public async Task ConnectAsync(string host, int port)
    {
        using var client = new TcpClient();
        await client.ConnectAsync(host, port);
        
        using var stream = client.GetStream();
        using var reader = new StreamReader(stream);
        using var writer = new StreamWriter(stream);
        
        // Enviar mensaje
        await writer.WriteLineAsync("Hola Servidor");
        await writer.FlushAsync();
        
        // Leer respuesta
        var response = await reader.ReadLineAsync();
        Console.WriteLine($"Respuesta: {response}");
    }
}
```

---

## 🔌 Socket Programming {#sockets}

### Socket Avanzado con Async/Await

```csharp
public class AdvancedSocketServer
{
    private Socket _listener;
    private bool _isRunning;
    
    public async Task StartAsync(IPEndPoint endPoint)
    {
        _listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        _listener.Bind(endPoint);
        _listener.Listen(100);
        _isRunning = true;
        
        while (_isRunning)
        {
            var client = await AcceptAsync(_listener);
            _ = Task.Run(() => ProcessClientAsync(client));
        }
    }
    
    private async Task<Socket> AcceptAsync(Socket listener)
    {
        var tcs = new TaskCompletionSource<Socket>();
        var args = new SocketAsyncEventArgs();
        
        args.Completed += (s, e) =>
        {
            if (e.SocketError == SocketError.Success)
                tcs.SetResult(e.AcceptSocket);
            else
                tcs.SetException(new SocketException((int)e.SocketError));
        };
        
        if (!listener.AcceptAsync(args))
        {
            // Completó sincrónicamente
            return args.AcceptSocket;
        }
        
        return await tcs.Task;
    }
    
    private async Task ProcessClientAsync(Socket client)
    {
        using (client)
        {
            var buffer = new byte[4096];
            
            while (client.Connected)
            {
                var received = await ReceiveAsync(client, buffer);
                if (received == 0) break;
                
                var message = Encoding.UTF8.GetString(buffer, 0, received);
                var response = $"Echo: {message}";
                var responseBytes = Encoding.UTF8.GetBytes(response);
                
                await SendAsync(client, responseBytes);
            }
        }
    }
    
    private async Task<int> ReceiveAsync(Socket socket, byte[] buffer)
    {
        var tcs = new TaskCompletionSource<int>();
        var args = new SocketAsyncEventArgs();
        args.SetBuffer(buffer, 0, buffer.Length);
        
        args.Completed += (s, e) =>
        {
            if (e.SocketError == SocketError.Success)
                tcs.SetResult(e.BytesTransferred);
            else
                tcs.SetException(new SocketException((int)e.SocketError));
        };
        
        if (!socket.ReceiveAsync(args))
            return args.BytesTransferred;
        
        return await tcs.Task;
    }
    
    private async Task SendAsync(Socket socket, byte[] data)
    {
        var tcs = new TaskCompletionSource<bool>();
        var args = new SocketAsyncEventArgs();
        args.SetBuffer(data, 0, data.Length);
        
        args.Completed += (s, e) =>
        {
            if (e.SocketError == SocketError.Success)
                tcs.SetResult(true);
            else
                tcs.SetException(new SocketException((int)e.SocketError));
        };
        
        if (!socket.SendAsync(args))
            return;
        
        await tcs.Task;
    }
}
```

---

## ⚙️ COM Interop {#com}

### Interoperabilidad COM

```csharp
// Ejemplo con Microsoft Office
[ComImport]
[Guid("000208D5-0000-0000-C000-000000000046")]
[InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
public interface IApplication
{
    [DispId(1)]
    bool Visible { get; set; }
    
    [DispId(2)]
    void Quit();
}

public class ExcelInterop
{
    private dynamic _excelApp;
    
    public void StartExcel()
    {
        Type excelType = Type.GetTypeFromProgID("Excel.Application");
        _excelApp = Activator.CreateInstance(excelType);
        _excelApp.Visible = true;
    }
    
    public void CreateWorkbook()
    {
        var workbook = _excelApp.Workbooks.Add();
        var worksheet = workbook.ActiveSheet;
        
        worksheet.Cells[1, 1] = "Hola desde .NET!";
        worksheet.Cells[1, 2] = DateTime.Now;
    }
    
    public void Cleanup()
    {
        if (_excelApp != null)
        {
            _excelApp.Quit();
            Marshal.ReleaseComObject(_excelApp);
            _excelApp = null;
        }
        
        GC.Collect();
        GC.WaitForPendingFinalizers();
    }
}
```

---

## 📞 P/Invoke {#pinvoke}

### Llamadas a Win32 API

```csharp
public static class Win32API
{
    [DllImport("user32.dll")]
    public static extern int MessageBox(IntPtr hWnd, string text, string caption, uint type);
    
    [DllImport("kernel32.dll")]
    public static extern IntPtr GetCurrentProcess();
    
    [DllImport("kernel32.dll")]
    public static extern bool CloseHandle(IntPtr hObject);
    
    [DllImport("advapi32.dll", SetLastError = true)]
    public static extern bool OpenProcessToken(IntPtr ProcessHandle, uint DesiredAccess, out IntPtr TokenHandle);
    
    public const uint TOKEN_QUERY = 0x0008;
    public const uint TOKEN_ADJUST_PRIVILEGES = 0x0020;
}

// Uso de P/Invoke
public class SystemUtils
{
    public static void ShowMessage(string message, string title = "Información")
    {
        Win32API.MessageBox(IntPtr.Zero, message, title, 0);
    }
    
    public static bool IsProcessElevated()
    {
        IntPtr tokenHandle = IntPtr.Zero;
        try
        {
            IntPtr processHandle = Win32API.GetCurrentProcess();
            if (Win32API.OpenProcessToken(processHandle, Win32API.TOKEN_QUERY, out tokenHandle))
            {
                // Verificar privilegios...
                return true; // Simplificado
            }
            return false;
        }
        finally
        {
            if (tokenHandle != IntPtr.Zero)
                Win32API.CloseHandle(tokenHandle);
        }
    }
}
```

[Continúa con más secciones...]
