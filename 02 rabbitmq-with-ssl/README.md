### proto-rabbitmq-with-ssl

## 02: Server authenticaion (enable SSL)

**Goal: Enable SSL/TLS with RabbitMQ.** Only server authentication. No client authentication yet.


Preliminary remarks:

- All the SSL-handling in RabbitMQ is provided by Erlang runtime. When in doubt about RabbitMQ config properties, it is helpful to check comments of underlying erlang implementation. See http://erlang.org/doc/man/ssl.html for info/comments on parameters.

- 

#### 1. Create CA

Create a certificate authority on the server.

````
// create an RSA private key.
// no use of password for prototype.
// hint: 'openssl genrsa'-command creates RSA keys.
openssl genrsa -out dm.ca.key.pem 2048

// generate (self signed) root CA certificate
// hint: the 'openssl req'-command generates and processes certificate signing requests (CSR).
openssl req -x509 -new -key dm.ca.key.pem -sha256 -days 1825 -out dm.ca.cert.pem
````

#### 2. Create Server Certificate

Create a server certificate that is issued by the self-signed CA.

````
// 1. generate key for server
openssl genrsa -out dm.rabbitmq.key.pem 2048
````

````
// 2. create certificate signing request (csr) 
// this request will be processed by the owner of the ca to generate the certificate.
// specify details for the certificate. 
// important: Common Name (cn) is later used by client to identify server (use IP address or similar).
openssl req -new -key dm.rabbitmq.key.pem -out dm.rabbitmq.csr -days 1825 
            -subj "/C=CH/ST=ZG/L=Rotkreuz/O=Roche/OU=RMD-STING/CN=x800dm.com" -outform PEM  -nodes 
````

````
// 3. sign the request.
// hint: this can be done with "x509" ro "ca" commnad.
openssl x509 -req -in dm.rabbitmq.csr -CA dm.ca.cert.pem -CAkey dm.ca.key.pem 
             -CAcreateserial -extfile v3.ext -days 500 -sha256
             -out dm.rabbitmq.cert.crt 
````

Remarks about the signing step:
- "-CAcreateserial" -> also creates a file with "{ca}.srl" with the last used serialnumber
- We need x509 **v3** certificates ( see https://www.openssl.org/docs/manmaster/man5/x509v3_config.html and https://tools.ietf.org/html/rfc5280)
- "-extfile v3.ext" -> The content of the file v3.ext is the following  

**TODO: should aslo specify -config req.cnf **

````
authorityKeyIdentifier=keyid,issuer
basicConstraints=CA:FALSE
keyUsage = digitalSignature, nonRepudiation, keyEncipherment, dataEncipherment
extendedKeyUsage = serverAuth, clientAuth, codeSigning, emailProtection
````


````
// 4. convert crt to pem  (RabbitMQ requires pem)
openssl x509 -inform pem -in dm.rabbitmq.cert.crt -out dm.rabbitmq.cert.pem
````

Remark: Key or certs are in PEM format when the file's content is readable (base64) and begins with `-----BEGIN`.

---

#### 3. Create rabbitMQ docker-image with SSL enabled

There are different ways how to achieve a container with ssl enabled and the necessary security keys accessible:

1. Copy necessary config and keys/certs into an existing container. (--> docker container cp)
2. Create a new image with correct config and keys/certs. (-> docker build)
3. Use the existing image and specify dedicated volumes for all files that need to be overridden. (Use docker-compose to automate this)
 
I go with approach 2.

##### 3.1 Create a new rabbitMQ configuration file:

Name the file `rabbitmq.config`:
````
[
 {rabbit,
  [
   {loopback_users, []},
    { ssl_listeners, [5671] },
    { ssl_options, [
      {cacertfile,"/home/certificates/dm.ca.cert.pem"},
      {certfile,"/home/certificates/dm.rabbitmq.cert.pem"},
      {keyfile,"/home/certificates/dm.rabbitmq.key.pem"},
      {verify,verify_peer},
      {fail_if_no_peer_cert,false},
      {versions, ['tlsv1.2', 'tlsv1.1']}
    ]},
   {default_vhost, "/"},
   {default_user, "guest"},
   {default_pass, "guest"},
   {default_permissions, [".*", ".*", ".*"]},
   {log_levels, [{connection,debug}]}	
  ]}
].
````
Remarks:
- For info on config: https://www.rabbitmq.com/ssl.html
- I'm using old RabbitMQ-configuration format (hence file ends in `.config`). Using new style didn't work for me. Didn't investigate why.
- The CA certificate is required! The server-certificate alone is not sufficient. 
- With this configuration, port 5672 is still accessible/operatable without tls. 


##### Dockerfile:

Create the following dockerfile (name it `Dockerfile`):

**TODO: Why apt-get update**
````
FROM rabbitmq:3-management

RUN apt-get update \
        && mkdir -p /home/certificates \
        && chmod 777 /home/certificates
COPY dm.rabbitmq.key.pem dm.rabbitmq.cert.pem dm.ca.cert.pem /home/certificates/
RUN chmod 777 /home/certificates/*

COPY rabbitmq.config /etc/rabbitmq/.
````

Remarks:
- The path to the rabbitmq-configuration (above: `/etc/rabbitmq/.`) can be read from the log-files: it lists the path at the beginning.
- Just placing a 'rabbitmq.config' seems to overrule the configuration 'rabbitmq.conf' (new style) that is used by default by the container. 
- Not sure about access-rights. Used 777 to be on the safe side...  

##### 3.3 Build & Run image

Build:
````
docker build -t rabbitmq-with-ssl .
````

Run:
````
docker container run -d --hostname my-rabbit-host --name my-rabbit-container -p 8080:15672 -p 5671:5671 -p 5672:5672 rabbitmq-with-ssl
````

Others:
````
docker container logs my-rabbit-container
docker container stop my-rabbit-container
docker container rm my-rabbit-container
````

---

#### 4. Test with Client

##### 4.1 Test with OpenSSL

On windows, use OpenSSL to test the connection:

````
// check connectivity (port 5671: SSL):
openssl s_client -connect 192.168.56.102:5671 

// or with full datail:
openssl s_client -connect 192.168.56.102:5671 -state -debug
````


##### 4.2 Test with .NET Client - Ignore server-certificate

Use the following connection-factory setting in the .NET client:
````
var factory = new ConnectionFactory
{
    HostName = "192.168.56.102",
    Port = 5671,
    Ssl = new SslOption
    {
        Enabled = true,
        Version = SslProtocols.Tls12,
        AcceptablePolicyErrors = SslPolicyErrors.RemoteCertificateNameMismatch |
                                 SslPolicyErrors.RemoteCertificateChainErrors,
    }
};
````
Remarks:
- The .NET client also has the option to specify a `CertificateValidationCallback` in the SSL option.
Add&debug this callback to check that the right server certificate is received.
- https://www.rabbitmq.com/releases/rabbitmq-dotnet-client/v3.3.0/rabbitmq-dotnet-client-3.3.0-api-guide.pdf


##### 4.2 Test with .NET Client - Valide server-certificate

Install the DM-CA to the trusted root certificate authorities in Windows:
 
````
// 1. Convert to crt file
openssl x509 -outform der -in "dm.ca.cert.pem" -out "dm.ca.crt"
````

````
// 2. Install certificate [Via UI -> Pseudo instructions]:
run mmc
add snap-in: Certificates > Local computer
select "Trusted Root Certificate Authorities"
actions -> import the crt of the CA
````

Now use the following connection-factory setting in the .NET client:
````
var factory = new ConnectionFactory
{
    HostName = "192.168.56.102",
    Port = 5671,
    Ssl = new SslOption
    {
        Enabled = true,
        Version = SslProtocols.Tls12,
        ServerName = "x800dm.com",
    }
};
````
Reamrks:
- Installing the cert to the trusted CAs got rid of the `SslPolicyErrors.RemoteCertificateChainErrors` error.
- Adding `ServerName` gets rid of the 'SslPolicyErrors.RemoteCertificateNameMismatch` error -
  **if** the server-name matches the CN of the server-certificate.


