using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace TCPwithSSLDiceExamServer
{
    class Program
    {
        private static readonly int PORT = 7000;

        static void Main(string[] args)
        {
            string serverCertificateFile = "C:/Users/asus/Documents/school/3rd semester/Certificates/ServerSSL.cer"; // or ServerSSL.pfx
            X509Certificate serverCertificate = new X509Certificate(serverCertificateFile, "mysecret");
            SslProtocols enabledSSLProtocols = SslProtocols.Tls; //superseeds the former SslProtocols.Ssl3
            IPAddress localAddress = IPAddress.Loopback;
            TcpListener serverSocket = new TcpListener(localAddress, PORT);

            serverSocket.Start();

            Console.WriteLine("TCP Server running on port number: " + PORT);

            while (true)
            {
                try
                {
                    TcpClient client = serverSocket.AcceptTcpClient();
                    Stream unsecureStream = client.GetStream();
                    var userCertificateValidationCallback = new RemoteCertificateValidationCallback(ValidateClientCertificate);
                    SslStream sslStream = new SslStream(unsecureStream, false, userCertificateValidationCallback, null);
                    sslStream.AuthenticateAsServer(serverCertificate, true, enabledSSLProtocols, false);
                    Console.WriteLine("Server authenticated");
                    StreamReader reader = new StreamReader(sslStream);
                    StreamWriter writer = new StreamWriter(sslStream) { AutoFlush = true };

                    Console.WriteLine("Incoming client");
                    Task.Run((() => DoComunicate(client, reader, writer)));
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            }
        }

        private static void DoComunicate(TcpClient client, StreamReader reader, StreamWriter writer)
        {
            //NetworkStream stream = client.GetStream();
            //StreamReader reader = new StreamReader(stream);
            //StreamWriter writer = new StreamWriter(stream);
            //StreamReader reader = new StreamReader(sslStream);
            //StreamWriter writer = new StreamWriter(sslStream) { AutoFlush = true };

            while (true)
            {
                ClassLibraryExamDice.DiceRoll obj = new ClassLibraryExamDice.DiceRoll();

                string request = reader.ReadLine();

                if (request != null)
                {
                    Console.WriteLine("Request: " + request);

                    string response = null;
                    string[] myArray = request.Split(',');

                    if (request.Split(',').Length == 3)
                    {
                        int number = Convert.ToInt32(myArray[2]);
                        int guess = Convert.ToInt32(myArray[1]);
                        response = "Name: " + myArray[0] + ", result: " + obj.ResultMethod(guess, number).ToString();

                        Console.WriteLine("Responce: " + response);
                        writer.WriteLine(response + "\n ");
                        Console.WriteLine();
                        writer.WriteLine();                        
                    }
                    if (request == "STOP")
                    {
                        break;
                    }
                    else
                    {
                        if (request.Split(',').Length != 3)
                        {
                            Console.WriteLine("No such action available");
                            Console.WriteLine();
                            writer.WriteLine("No such action available");
                            writer.WriteLine();
                        }
                    }
                    writer.Flush();
                }

            }
            client.Close();
            Console.WriteLine("Client disconnected.\nWaiting...");
        }

        private static bool ValidateClientCertificate(object sender, X509Certificate clientCertificate,
        X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            Console.WriteLine("Server Sender: " + sender.ToString());
            Console.WriteLine("Server : " + clientCertificate.ToString());
            Console.WriteLine("Server : " + sslPolicyErrors.ToString());
            if (sslPolicyErrors == SslPolicyErrors.None)
            {
                Console.WriteLine("Server validation of client certificate successful.");
                return true;  //false for remote
            }
            Console.WriteLine("Errors in certificate validation:");
            Console.WriteLine(sslPolicyErrors);
            return false;   //true for remote
        }
    }    
}
