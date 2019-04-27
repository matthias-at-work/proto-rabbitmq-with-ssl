# proto-rabbitmq-with-ssl

This series of post describes how to use RabbitMQ (and Federation in special) over TLS.

Technology: RabbitMQ / OpenSSL

Goal: Try out different ways how to authenticate client.

---

#### Table of Contents ####

1. **General setup**
2. **RabbitMQ with SSL** (no client authentication)
3. **RabbitMQ with peer verification** (client authentication)

   3.1 Client-authentication - where client-cert is signed by same CA as server-cert

   3.2 Client-authentication - where client-cert is signed by a different CA 

   3.3 Client-authentication - where client-cert is self-signed (no CA)

4. **Federation**

   4.1 Federation (without TLS)

   4.2 Federation with TLS: Server authenticaiton only

   4.3 Federation over TLS: Peer verification (with client- and server-certificates)

   4.4 Federation over TLS: Peer verification (with self-signed client-cert)

5. **RabbitMQ plugin "Certificate Trust Store"**


---

#### Warning ####

- Be careful when copy&paste code. 
  Some exmaples use code that spans across lines - and this is not well formatted everywhere (e.g. Linux `\` is missing).

- OpenSSL configuration files are not yet properly documented.

---

#### My open tasks ####

- Currently, the OpenSSL configuration file is not properly described. 
  Also, where such files are directly included, my description is insufficient. 
  **Needs improvement!**
  See also https://www.rabbitmq.com/ssl.html#manual-certificate-generation

- Investigate problem with self-signed certificates

- rabbitmq.config file -> use comments to explain stuff: 

  ```` { dafsdf, 12}  %% here a comment ````

- 