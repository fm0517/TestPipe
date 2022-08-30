using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestPipe
{
    class AnonymousPipeClient
    {
        private PipeStream pipeClient;
        private StreamWriter sw;
        public AnonymousPipeClient(PipeDirection direction, string pipeHandleAsString)
        {
            try
            {
                pipeClient = new AnonymousPipeClientStream(direction, pipeHandleAsString);
                sw = new StreamWriter(pipeClient);
                sw.AutoFlush = true;
                Console.WriteLine("[CLIENT] Current TransmissionMode: {0}.", pipeClient.TransmissionMode);
            }
            catch (Exception e)
            {
                Console.WriteLine("[CLIENT] Construct error: {0}", e.Message);
            }
        }

        public void Send()
        {
            try
            {
                // Send a 'sync message' and wait for client to receive it.
                sw.WriteLine("SYNC");
                sw.Flush();
                pipeClient.WaitForPipeDrain();
                // Send the console input to the client process.
                Console.Write("[CLIENT] Enter text: ");
                sw.WriteLine(Console.ReadLine());
            }
            catch (IOException e)
            {
                Console.WriteLine("[CLIENT] Send error: {0}", e.Message);
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            AnonymousPipeClient client = new AnonymousPipeClient(PipeDirection.Out, args[0]);
            while (true)
            {
                client.Send();
            }
            
            Console.Write("[CLIENT] Press Enter to continue...");
            Console.ReadLine();
        }
    }
}
