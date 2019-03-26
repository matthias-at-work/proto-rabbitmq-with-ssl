using System;
using System.IO;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using RabbitMQ.Client;

namespace simple_console
{
    static class Program
    {
        public static void Main()
        {
            var factory = new ConnectionFactory
            {
                HostName = "192.168.56.102",
                Port = 5671,
                // VirtualHost = "default",
                // UserName = "guest",
                // Password = "guest",
                Ssl = new SslOption
                {
                    Enabled = true,
                    Version = SslProtocols.Tls12,
                    // ServerName = "x800dm.com",
                    // AcceptablePolicyErrors = SslPolicyErrors.RemoteCertificateNameMismatch |
                    //                          SslPolicyErrors.RemoteCertificateChainErrors,
                    // CertificateValidationCallback += CertificateValidationCallback
                }
            };

            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: "hello",
                    durable: false,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null);

                string message = "Hello World!";
                var body = Encoding.UTF8.GetBytes(message);

                channel.BasicPublish(exchange: "",
                    routingKey: "hello",
                    basicProperties: null,
                    body: body);

                Console.WriteLine(" [x] Sent {0}", message);
            }

            Console.WriteLine(" Press [enter] to exit.");
            Console.ReadLine();
        }

        private static bool CertificateValidationCallback(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            throw new NotImplementedException();
        }
    }
}




