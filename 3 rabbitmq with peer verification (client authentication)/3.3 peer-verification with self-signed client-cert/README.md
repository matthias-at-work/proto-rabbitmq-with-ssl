### proto-rabbitmq-with-ssl

## 05: Peer-verification - with self-signed client-cert

**Goal**: Enable peer-verification by client-certificate - 
where *the client-certificate is self-signed*.

**Result**: **This doesn't work!**

#### 05.1 Create self-signed client-certificate

````
// create key
openssl genrsa -out instrument.client.key.pem 2048

// create certificate: use also as CA!
openssl req -new -subj /CN=Instrument -key instrument.client.key.pem 
            -out instrument.client.csr.pem

openssl x509 -req -in instrument.client.csr.pem -signkey instrument.client.key.pem 
             -sha256 -days 1000 -extfile v3-extensions-client.ext 
             -out instrument.client.cert.pem 
````
---

#### 05.2 Update RabbitMQ

1. Copy the self-signed `instrument.client.cert.pem` to the DM/server.

2. RabbitMQ has now to trust the DM-CA as well as the (self-signed) client-cert
Ccreate new CA-bundle:
````
cat dm.ca.cert.pem instrument.client.cert.pem > bundled-ca-certs.pem
````

3. Update RabbitMQ configuration
````
{cacertfile,"/home/certificates/bundled-ca-certs.pem"},
````

4. Update Dockerfile:
Make sure bundle is copied into container:
````
COPY dm.server.key.pem dm.server.cert.pem bundled-ca-certs /home/certificates/
````

5. Build image and run container
````
docker build -t rabbitmq-with-mutualauth .
docker container run -d --hostname my-rabbit-host --name my-rabbit-container -p 8080:15672 -p 5671:5671 -p 5672:5672 rabbitmq-with-mutualauth
````

---

#### 05.3 Test with Client

=> Failure with OpenSSL (`openssl s_client`) as well as with .NET-client:
Authentication fails on server with `Bad Certificate`. 
