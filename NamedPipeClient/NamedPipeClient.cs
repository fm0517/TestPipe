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
        public string ClientName { get; private set; }
        public PipeClient(string name)
        {
            ClientName = name;
        }

        public void Run()
        {
            Console.WriteLine($"This is client-{ClientName}");
            var pipeClient = new NamedPipeClientStream("192.168.1.3", "testpipe", PipeDirection.InOut, PipeOptions.None, TokenImpersonationLevel.Impersonation);

            Console.WriteLine("Connecting to server ...\n");
            pipeClient.Connect();

            var ss = new StreamString(pipeClient);
            string sendMsg, recvMsg;
            recvMsg = ss.ReadString();
            Console.WriteLine($"[Client-{ClientName} recv] {recvMsg}");

            if (recvMsg.Equals("This is the simulation server"))
            {
                // send client info
                List<string> ipAddrList = Utils.LocalIpAddressList();
                sendMsg = $"Register Client-{ClientName}:" + (ipAddrList.Count > 0 ? ipAddrList[0] : "") + " to server";
                ss.WriteString(sendMsg);
                Console.WriteLine($"[Client-{ClientName} send] {sendMsg}");

                // receive command from server
                recvMsg = ss.ReadString();
                string filename = "./Result/SimulationRunOutput-" + recvMsg.Split("]")[0].Split("[")[1] + ".log";
                ReadFileToStream fileReader = new ReadFileToStream(ss, filename);
                Console.WriteLine($"[Client-{ClientName} recv] {recvMsg}");

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
}