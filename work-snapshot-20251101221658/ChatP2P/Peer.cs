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
            //Fix: Espera a que termine la recepción (cuando el usuario escriba 'exit')
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
        // Fix: bucle para recibir mensajes continuamente hasta que el usuario escriba 'exit'
        try
        {
            var stream = _tcpClient!.GetStream();
            var reader = new StreamReader(stream, Encoding.UTF8);
            while (true)
            {
                var message = await reader.ReadLineAsync();
                if (message == null || message.ToLower() == "exit")
                {
                    Console.WriteLine("disconnected or typed 'exit'.");
                    break;
                }
                Console.WriteLine($"/n Incoming message: {message}");
            }
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
        // Fix: bucle para enviar mensajes interactivos hasta que el usuario escriba 'exit'
        try
        {
            var stream = _tcpClient!.GetStream();
            var writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };
            while (true)
            {
                Console.Write("You: ");
                var message = Console.ReadLine();
                if (message == null || message.ToLower() == "exit")
                {
                    await writer.WriteLineAsync("exit");
                    break;
                }
                await writer.WriteLineAsync(message);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error sending message: " + ex.Message);
        }
        //Fix: Ya no se cierra aquí, se cierra al terminar la comunicación, el finaly no era necesario
    }
    

    private void Close()
    {
        _tcpClient?.Close();
        _tcpListener.Stop();
    }
}

//TODO:(FIX) Error sending message: Cannot access a disposed object.
            //Object name: 'System.Net.Sockets.NetworkStream'.