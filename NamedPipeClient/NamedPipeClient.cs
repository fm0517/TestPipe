using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Net.Sockets;
using System.Net;
using System.Security.Principal;
using System.Text;
using System.Threading;

namespace TestPipe
{
    public class PipeClient
    {
        private static int numClients = 4;

        public static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                if (args[0] == "spawnclient")
                {
                    string clientId = args[1];
                    int threadId = Thread.CurrentThread.ManagedThreadId;
                    Console.WriteLine($"This is client-{clientId}, ThreadId = {threadId}");
                    var pipeClient = new NamedPipeClientStream(".", "testpipe", PipeDirection.InOut, PipeOptions.None, TokenImpersonationLevel.Impersonation);

                    Console.WriteLine("Connecting to server ...\n");
                    pipeClient.Connect();

                    var ss = new StreamString(pipeClient);
                    string sendMsg, recvMsg;
                    recvMsg = ss.ReadString();
                    Console.WriteLine($"[Client-{clientId}({threadId}) recv] {recvMsg}");

                    if (recvMsg.Equals("I am the one true server!"))
                    {
                        // send client info
                        List<string> ipAddrList = Utils.LocalIpAddressList();
                        sendMsg = $"Client-{clientId}:" + (ipAddrList.Count > 0 ? ipAddrList[0] : "");
                        ss.WriteString(sendMsg);
                        Console.WriteLine($"[Client-{clientId}({threadId}) send] {sendMsg}");

                        // receive command from server
                        recvMsg = ss.ReadString();
                        string filename = "c:/textfile" + recvMsg.Split("]")[0].Split("[")[1] + ".txt";
                        ReadFileToStream fileReader = new ReadFileToStream(ss, filename);
                        Console.WriteLine($"[Client-{clientId}({threadId}) recv] {recvMsg}");

                        // send read file
                        fileReader.Start();
                    }
                    else
                    {
                        Console.WriteLine("Server could not be verified.");
                    }
                    pipeClient.Close();
                    // Give the client process some time to display results before exiting.
                    Thread.Sleep(4000);
                }
            }
            else
            {
                Console.WriteLine("\n*** Named pipe client stream with impersonation example ***\n");
                StartClients();
            }
            Console.ReadLine();
        }

        // Helper function to create pipe client processes
        private static void StartClients()
        {
            string currentProcessName = Environment.CommandLine;

            // Remove extra characters when launched from Visual Studio
            currentProcessName = currentProcessName.Trim('"', ' ');

            currentProcessName = Path.ChangeExtension(currentProcessName, ".exe");
            Process[] plist = new Process[numClients];

            Console.WriteLine("Spawning client processes...\n");

            if (currentProcessName.Contains(Environment.CurrentDirectory))
            {
                currentProcessName = currentProcessName.Replace(Environment.CurrentDirectory, String.Empty);
            }

            // Remove extra characters when launched from Visual Studio
            currentProcessName = currentProcessName.Replace("\\", String.Empty);
            currentProcessName = currentProcessName.Replace("\"", String.Empty);

            int i;
            for (i = 0; i < numClients; i++)
            {
                // Start 'this' program but spawn a named pipe client.
                plist[i] = Process.Start(currentProcessName, $"spawnclient {i + 1}");
            }
            while (i > 0)
            {
                for (int j = 0; j < numClients; j++)
                {
                    if (plist[j] != null)
                    {
                        if (plist[j].HasExited)
                        {
                            Console.WriteLine($"Client process[{plist[j].Id}] has exited.");
                            plist[j] = null;
                            i--;    // decrement the process watch count
                        }
                        else
                        {
                            Thread.Sleep(250);
                        }
                    }
                }
            }
            Console.WriteLine("\nClient processes finished, exiting.");
        }
    }

    // Defines the data protocol for reading and writing strings on our stream.
    public class StreamString
    {
        private Stream ioStream;
        private UnicodeEncoding streamEncoding;

        public StreamString(Stream ioStream)
        {
            this.ioStream = ioStream;
            streamEncoding = new UnicodeEncoding();
        }

        public string ReadString()
        {
            int len = ioStream.ReadByte() * 256;
            len += ioStream.ReadByte();
            byte[] inBuffer = new byte[len];
            ioStream.Read(inBuffer, 0, len);

            return streamEncoding.GetString(inBuffer);
        }

        public int WriteString(string outString)
        {
            byte[] outBuffer = streamEncoding.GetBytes(outString);
            int len = outBuffer.Length;
            if (len > UInt16.MaxValue)
            {
                len = (int)UInt16.MaxValue;
            }
            ioStream.WriteByte((byte)(len / 256));
            ioStream.WriteByte((byte)(len & 255));
            ioStream.Write(outBuffer, 0, len);
            ioStream.Flush();

            return outBuffer.Length + 2;
        }
    }
    // Contains the method executed in the context of the impersonated user
    public class ReadFileToStream
    {
        private string fn;
        private StreamString ss;

        public ReadFileToStream(StreamString str, string filename)
        {
            fn = filename;
            ss = str;
        }

        public void Start()
        {
            string contents = File.ReadAllText(fn);
            ss.WriteString(contents);
        }
    }

    class Utils
    {
        public static List<string> LocalIpAddressList()
        {
            List<string> ips = new List<string>();
            IPHostEntry ipEntry = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in ipEntry.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    ips.Add(ip.ToString());
                }
            }
            return ips;
        }
    }
}