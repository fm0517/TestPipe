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

            Console.WriteLine("\n*** Simulation cluster server is running ***\n");
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
}