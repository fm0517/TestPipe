using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestPipe
{
    class AnonymousPipeServer
    {
        public AnonymousPipeServerStream PipeServer { get; private set; }
        private StreamReader sr;
        public AnonymousPipeServer(PipeDirection direction)
        {
            try
            {
                PipeServer = new AnonymousPipeServerStream(direction, HandleInheritability.Inheritable);
                sr = new StreamReader(PipeServer);
                Console.WriteLine("[SERVER] Current TransmissionMode: {0}.", PipeServer.TransmissionMode);
            }
            catch (Exception e)
            {
                Console.WriteLine("[SERVER] Construct error: {0}", e.Message);
            }
        }

        public void Receive()
        {
            while (true)
            {
                string temp;
                bool printed = false;
                // Wait for 'sync message' from the server.
                do
                {
                    temp = sr.ReadLine();
                    if (temp == null || temp.Equals(""))
                    {
                        if (!printed)
                        {
                            Console.WriteLine("[SERVER] Wait for sync...");
                            printed = true;
                        }
                    }
                    temp = temp != null ? temp : "";
                    Thread.Sleep(100);
                }
                while (!temp.StartsWith("SYNC"));
                printed = false;

                // Read the server data and echo to the console.
                temp = sr.ReadLine();
                if (temp != null)
                {
                    Console.WriteLine("[SERVER] Echo: " + temp);
                }
                Thread.Sleep(50);
            }
        }
    }
    class Program
    {
        static void Main()
        {
            AnonymousPipeServer anonymousPipeServer = new AnonymousPipeServer(PipeDirection.In);

            Process pipeClient = new Process();
            pipeClient.StartInfo.FileName = "../../../../AnonymousPipeClient/bin/Debug/net6.0/AnonymousPipeClient.exe";
            // Pass the client process a handle to the server.
            pipeClient.StartInfo.Arguments = anonymousPipeServer.PipeServer.GetClientHandleAsString();
            pipeClient.StartInfo.UseShellExecute = false;
            pipeClient.Start();

            anonymousPipeServer.PipeServer.DisposeLocalCopyOfClientHandle();
            anonymousPipeServer.Receive();

            pipeClient.WaitForExit();
            pipeClient.Close();
            Console.WriteLine("[SERVER] Client quit. Server terminating.");
        }
    }
}
