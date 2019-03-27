### proto-rabbitmq-with-ssl

## 03: Client authenticaion - with common CA

**Goal**: Enable peer-verification by client-certificate - 
where *the client-certificate is issued by the same CA that also issued the server-certificate* (DM-CA).

#### 03.1 Create Client-key and certificate signing request (csr) 

Do the following on Windows:

````
// generate key
openssl genrsa -out instrument.client.key.pem 2048
````

````
// create certificate signing request (csr) 
openssl req -new -key instrument.client.key.pem 
            -subj "/C=CH/ST=ZH/L=Zurich/O=myO/OU=myOU/CN=myCN" 
            -out instrument.client.csr.pem -outform PEM -nodes
````

---

#### 03.2 Sign CSR by DM-CA 

1. Copy the issued certificate signing request (`instrument.client.csr.pem`) to the DM (server).

2. Sign the certificate with the DM-certificate authority (CA):
   ````
   openssl x509 -req \
                -in instrument.client.csr.pem \
                -CA dm.ca.cert.pem -CAkey dm.ca.key.pem \ 
                -CAcreateserial -extfile v3-extensions-client.ext -days 1000 -sha256 \
                -out instrument.client.cert.pem
   ````

3. Copy the signed certificate (`instrument.client.cert.pem`) and 
   the CA-certificate (`dm.ca.cert.pem`) back to the client (Instrument/Windows).



---

#### 03.3 Enable Mutual-Authentication in RabbitMQ

1. Update the rabbitmq.config to include the following:
````
 {verify,verify_peer},
 {fail_if_no_peer_cert,true},
````
Remarks:
- The `cacertfile` stay as it was as still only one CA is in use for both server and client.

2. Build new image and run:
````
docker build -t rabbitmq-with-mutualauth .
docker container run -d --hostname my-rabbit-host --name my-rabbit-container -p 8080:15672 -p 5671:5671 -p 5672:5672 rabbitmq-with-mutualauth
````

---

#### 03.4 Test with Client

##### 03.4.1 Test with OpenSSL

On windows, use OpenSSL to test the connection:

````
openssl s_client -connect 192.168.56.102:5671 -state -debug
                 -cert "instrument.client.cert.pem" -key "instrument.client.key.pem" -CAfile "dm.ca.cert.pem"   

// or dump info to file:
openssl s_client -connect 192.168.56.102:5671 -state -debug
                 -cert "instrument.client.cert.pem" -key "instrument.client.key.pem" -CAfile "dm.ca.cert.pem"   
                  > connectivity-details.txt         
````

Pay attention to the following section. This is a hint to the client so it knows which client-certificates are accepted by the server.
````
Acceptable client certificate CA names
C = ch, ST = zug, L = zug, O = rmd, OU = rmd, CN = myca
````


##### 03.4.2 Test with .NET Client - Ignore server-certificate

1. Create a pfx/p12 cert for .NET usage.
````
openssl pkcs12 -export 
               -in instrument.client.cert.pem -inkey instrument.client.key.pem 
               -out instrument.client.cert.pfx 
````

2. Use the following connection-factory setting in the .NET client:
````
X509Certificate2 clientCert = new X509Certificate2(@"C:\add-correct-path\instrument.client.cert.pfx", String.Empty);
var factory = new ConnectionFactory
{
    HostName = "192.168.56.102",
    Port = 5671,
    Ssl = new SslOption
    {
        Enabled = true,
        Version = SslProtocols.Tls12,
        ServerName = "x800dm.com",
        // AcceptablePolicyErrors = SslPolicyErrors.RemoteCertificateNameMismatch |
        //                          SslPolicyErrors.RemoteCertificateChainErrors,
        Certs = new X509CertificateCollection(new X509Certificate[] { clientCert })
        // CertificateSelectionCallback = CertificateSelectionCallback,
        // CertPath = @"C:\add-correct-path\instrument.client.cert.pfx"
    }
};
````
Remarks:
- Success (and failure) connection is seen server-side in the rabbitmq logs (from the container).  
- Selecting the correct client-certificate by explicitly setting it using `Certs` worked.
- Check also the properties of the `CertificateSelectionCallback`. The method signature is:
  ````
  private static X509Certificate CertificateSelectionCallback(
    object sender, 
    string targetHost, 
    X509CertificateCollection localCertificates, 
    X509Certificate remoteCertificate, 
    string[] acceptableIssuers)
  ````
  **However**: The objects didn't have the expected properties, e.g. `acceptableIssuers` was empty. 
  The `localCertificates` was populated if the `CertPath` was set.

