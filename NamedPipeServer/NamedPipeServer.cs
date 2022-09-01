using System;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading;

namespace TestPipe
{
    public class Program
    {
        private static int numThreads = 4;
        public static void Main()
        {
            int i;
            Thread[] servers = new Thread[numThreads];

            Console.WriteLine("\n*** Named pipe server stream with impersonation example ***\n");
            Console.WriteLine("Waiting for client connect...\n");
            for (i = 0; i < numThreads; i++)
            {
                PipeServer pipeServer = new PipeServer();
                servers[i] = new Thread(pipeServer.ServerThread);
                servers[i].Start();
            }
            Thread.Sleep(250);
            while (i > 0)
            {
                for (int j = 0; j < numThreads; j++)
                {
                    if (servers[j] != null)
                    {
                        if (servers[j].Join(250))
                        {
                            Console.WriteLine("Server thread[{0}] finished.", servers[j].ManagedThreadId);
                            servers[j] = null;
                            i--;    // decrement the thread watch count
                        }
                    }
                }
            }
            Console.WriteLine("\nServer threads exhausted, exiting.");
        }

    }

    public class PipeServer
    {
        private int numThreads = 4;

        public void ServerThread(object data)
        {
            NamedPipeServerStream pipeServer = new NamedPipeServerStream("testpipe", PipeDirection.InOut, numThreads);

            int threadId = Thread.CurrentThread.ManagedThreadId;

            // Wait for a client to connect
            pipeServer.WaitForConnection();

            Console.WriteLine("Client connected on thread[{0}].", threadId);
            try
            {
                StreamString ss = new StreamString(pipeServer);
                string sendMsg, recvMsg;

                // send server's signature string
                sendMsg = "I am the one true server!";
                ss.WriteString(sendMsg);
                Console.WriteLine($"[Server({threadId}) send] {sendMsg}");

                // receive client info
                recvMsg = ss.ReadString();
                string [] clientInfo = recvMsg.Split(":");
                string clientId = clientInfo[0].Replace("Client-",string.Empty);
                string clientIP = clientInfo[1];
                Console.WriteLine($"[Server({threadId}) recv] {recvMsg}");

                // send command to each client
                sendMsg = $"Server command client[{clientId}] ro execute simulation.";
                ss.WriteString(sendMsg);
                Console.WriteLine($"[Server({threadId}) send] {sendMsg}");

                // receive response (file content)
                Console.WriteLine($"[Server({threadId}) recv] " + ss.ReadString());
            }
            // Catch the IOException that is raised if the pipe is broken
            // or disconnected.
            catch (IOException e)
            {
                Console.WriteLine("ERROR: {0}", e.Message);
            }
            pipeServer.Close();
        }
    }

    // Defines the data protocol for reading and writing strings on our stream
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

}