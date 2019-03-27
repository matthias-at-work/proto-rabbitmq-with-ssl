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
            X509Certificate2 clientCert = new X509Certificate2(@"C:\add-correct-path\instrument.client.cert.pfx", String.Empty);
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
                    ServerName = "x800dm",
                    // AcceptablePolicyErrors = SslPolicyErrors.RemoteCertificateNameMismatch |
                    //                          SslPolicyErrors.RemoteCertificateChainErrors,
                    Certs = new X509CertificateCollection(new X509Certificate[] { clientCert })
                    // CertificateSelectionCallback = CertificateSelectionCallback
                    // CertPath = @"C:\add-correct-path\instrument.client.cert.pfx"
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

        private static X509Certificate CertificateSelectionCallback(object sender, string targetHost, X509CertificateCollection localCertificates, X509Certificate remoteCertificate, string[] acceptableIssuers)
        {
            throw new NotImplementedException();
        }
    }
}




