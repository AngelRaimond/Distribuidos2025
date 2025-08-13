using System.Net;
 using System.Net.Sockets;
using System.Text;
 namespace ChatP2P;

public class Peer
{
    private readonly TcpListener _tcpListener;
    private TcpClient? _tcpClient;
    private const int Port = 8080;

    public Peer() => _tcpListener = new TcpListener(IPAddress.Any, Port);

    public async Task ConnectToPeer(string ipAddress, string port)
    {
        try
        {
            _tcpClient = new TcpClient(ipAddress, Convert.ToInt32(port));
            Console.WriteLine("Connection established! :D");
            var receiveTask = ReceiveMessage();
            await SendMessage();
            await receiveTask;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Connection closed :( " + ex.Message);
        }
    }

    public async Task StartListening()
    {
        try
        {
            _tcpListener.Start();
            Console.WriteLine("Listening for incoming connections...");
            _tcpClient = await _tcpListener.AcceptTcpClientAsync();
            Console.WriteLine("Connection established! :D");

            var receiveTask = ReceiveMessage();
            await SendMessage();
            await receiveTask;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Connection closed :( " + ex.Message);
        }
    }

    public async Task ReceiveMessage()
    {
        try
        {
            var stream = _tcpClient!.GetStream();
            var reader = new StreamReader(stream, Encoding.UTF8);
            var message = await reader.ReadLineAsync();
            Console.WriteLine($"Peer message: {message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error receiving message: " + ex.Message);
        }
        finally
        {
            Close();
        }
    }

    public async Task SendMessage()
    {
        try
        {
            var stream = _tcpClient!.GetStream();
            var writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };
            var message = "Hola este es mi primer mensaje";
            await writer.WriteLineAsync(message);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error sending message: " + ex.Message);
        }
        finally
        {
            Close();
        }
    }

    private void Close()
    {
        _tcpClient?.Close();
        _tcpListener.Stop();
    }
}
