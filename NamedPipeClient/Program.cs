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
    public class Program
    {
        private static int numClients = 4;

        public static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                string clientName = args[0];
                PipeClient pipeClient = new PipeClient(clientName);
                pipeClient.Run();
            }
            else
            {
                Console.WriteLine("\n*** Simulation cluster client (master) is running ***\n");
                StartClients();
            }
            Console.ReadLine();
        }

        // Helper function to create pipe client processes
        private static void StartClients()
        {
            Console.WriteLine("Spawning client (slave) processes...\n");
            string currentProcessName = Environment.CommandLine;

            currentProcessName = currentProcessName.Trim('"', ' ');
            currentProcessName = Path.ChangeExtension(currentProcessName, ".exe");
            Process[] plist = new Process[numClients];
            if (currentProcessName.Contains(Environment.CurrentDirectory))
            {
                currentProcessName = currentProcessName.Replace(Environment.CurrentDirectory, String.Empty);
            }
            currentProcessName = currentProcessName.Replace("\\", String.Empty);
            currentProcessName = currentProcessName.Replace("\"", String.Empty);

            int i;
            for (i = 0; i < numClients; i++)
            {
                // Start 'this' program but spawn a named pipe client with specific clientName.
                plist[i] = Process.Start(currentProcessName, $"{i + 1}");
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
}