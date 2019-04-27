# proto-rabbitmq-with-ssl

This series of post describes how to use RabbitMQ (and Federation in special) over TLS.

Technology: RabbitMQ / OpenSSL

Goal: Try out different ways how to authenticate client.

Contents:
01. General setup
02. Server-authentication (enable ssl)
03. Client-authentication - where client-cert is signed by same CA as server-cert
04. Client-authentication - where client-cert is signed by dedicated CA 
05. Client-authentication - where client-cert is self-signed (no CA) => **FAILURE**
06. Client-authentication - Using RabbitMQ plugin "Certificate Trust Store"
07. Use client-certificate for RabbitMQ user-authentication
10. Federation (without TLS)
11. Federation with TLS: Server authenticaiton only
12. Federation over TLS: Mutual authentication with client-cert signed by server-CA
13. Federation over TLS: Mutual authentication with self-signed client-cert


---
#### Warning ####

- Be careful when copy&paste code. 
  Some exmaples use code that spans across lines - and this is not well formatted everywhere (e.g. Linux `\` is missing).

---

#### My open tasks ####

- Currently, the OpenSSL configuration file is not properly described. 
  Also, where such files are directly included, my description is insufficient. 
  **Needs improvement!**
  See also https://www.rabbitmq.com/ssl.html#manual-certificate-generation

- Investigate problem with self-signed certificates

- rabbitmq.config file -> use comments: 

  ```` { dafsdf, 12}  %% here a comment ````

- 