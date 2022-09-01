using System;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading;

namespace TestPipe
{
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
                sendMsg = "This is the simulation server";
                ss.WriteString(sendMsg);
                //Console.WriteLine($"[Server({threadId}) send] {sendMsg}");

                // receive client info
                recvMsg = ss.ReadString();
                string [] clientInfo = recvMsg.Split(":");
                string clientId = clientInfo[0].Split("-")[1];
                string clientIP = clientInfo[1];
                Console.WriteLine($"Client-{clientId} registed to thread[{threadId}]");
                //Console.WriteLine($"[Server({threadId}) recv] {recvMsg}");

                // send command to each client
                sendMsg = $"Server let Client-[{clientId}] to execute simulation.";
                ss.WriteString(sendMsg);
                //Console.WriteLine($"[Server({threadId}) send] {sendMsg}");

                // receive response (file content)
                Console.WriteLine($"[Server({threadId}) recv] " + ss.ReadString());
            }
            catch (IOException e)
            {
                Console.WriteLine("ERROR: {0}", e.Message);
            }
            pipeServer.Close();
        }
    }
}