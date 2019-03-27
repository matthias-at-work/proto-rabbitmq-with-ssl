### proto-rabbitmq-with-ssl

## 04: Client authenticaion - with self-signed client-cert

**Goal**: Enable peer-verification by client-certificate - 
where *the client-certificate is issued by a dedicated Instrument-CA* (which has nothing shared with the server/DM-CA).

#### 04.1 Create instrument-CA

Do the following on Instrument/Windows (analog to DM-CA described earlier):

````
openssl genrsa -out instrument.ca.key.pem 2048
openssl req -x509 -new -key instrument.ca.key.pem -sha256 -days 1000 -out instrument.ca.cert.pem
````
---

#### 04.2 Create Client-certificate signed by instrument-CA

Do the following on Instrument/Windows (analog to DM-Server described earlier):

````
openssl genrsa -out instrument.client.key.pem 2048

openssl req -new -key instrument.client.key.pem 
            -subj "/C=CH/ST=ZH/L=Zurich/O=Roche/OU=RMD-STING/CN=Instrument" 
            -out instrument.client.csr.pem -outform PEM -nodes  

openssl x509 -req -in instrument.client.csr.pem -CA instrument.ca.cert.pem -CAkey instrument.ca.key.pem 
             -CAcreateserial -extfile v3-extensions-client.ext -days 1000 -sha256
             -out instrument.client.cert.pem 
````
---

#### 04.3 Update RabbitMQ

3.1 Copy the `instrument.ca.cert.pem` to the DM/server.

3.2 RabbitMQ has now to trust both CAs (DM-CA and Instrument-CA).
For this, **create a CA-bundle** that contains both CAs.
The most common way of appending several certificates to one another and use in a single Certificate Authority bundle file is 
to simply concatenate them: 
````
// on the DM/server: 
cat dm.ca.cert.pem instrument.ca.cert.pem > bundled-ca-certs.pem
````

3.3 Update RabbitMQ configuration
````
{cacertfile,"/home/certificates/bundled-ca-certs.pem"},
````

3.4 Update Dockerfile:
Make sure bundle is copied into container:
````
COPY dm.server.key.pem dm.server.cert.pem bundled-ca-certs /home/certificates/
````

3.5 Build image and run container
````
docker build -t rabbitmq-with-mutualauth .
docker container run -d --hostname my-rabbit-host --name my-rabbit-container -p 8080:15672 -p 5671:5671 -p 5672:5672 rabbitmq-with-mutualauth
````

---

#### 04.4 Test with Client

##### 04.4.1 Test with OpenSSL

On windows, use OpenSSL to test the connection (the `CAfile`-param needs adjustment:

````
openssl s_client -connect 192.168.56.102:5671 -state -debug
                 -cert "instrument.client.cert.pem" -key "instrument.client.key.pem" -CAfile "instrument.ca.cert.pem"   
````

This time, the server is ready to accept client-certificates from both CA:
````
Acceptable client certificate CA names
C = ch, ST = zug, L = zug, O = rmd, OU = rmd, CN = myca
C = ch, ST = zh, L = zurich, O = home, OU = home unit, CN = instrument
````

The rabbitMQ-log file shows that the connection succeeded.


##### 04.4.2 Test with .NET Client - Ignore server-certificate

1. Create a pfx/p12 cert for .NET usage.
````
openssl pkcs12 -export 
               -in instrument.client.cert.pem -inkey instrument.client.key.pem 
               -out instrument.client.cert.pfx 
````

2. Use the the same .NET client as in last example
   => The call *succeeds* as before...
