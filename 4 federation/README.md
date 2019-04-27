## 4. Federation

This is the first post in a series of posts on how to setup RabbitMQ federation of exchanges with mutual TLS (server- and client-authentication).

---
#### Info & Reading on Federation over TLS
- Federation is an *erlang-client* (downstream RabbitMQ-broker) calling a (upstream) RabbitMQ-broker. 
  This is not the same as you might be used to from using .NET- or Java-client... 
  => See https://www.rabbitmq.com/ssl.html#erlang-client


- The meaning of some properties in the SSL conifguration depend on wether they are used as sever or client (e.g. `verfiy_peer`).
  I found the docu on openssl (Erlang is based on openssl-library) better readable than the Erlang docu:
  https://www.openssl.org/docs/man1.0.2/man3/SSL_CTX_set_verify.html

- The three important options which must be supplied are (https://www.rabbitmq.com/ssl.html):
  - The `cacertfile` option specifies the certificates of the root Certificate Authorities that we wish to implicitly trust.
  - The `certfile` is the client's own certificate in PEM format
  - The `keyfile` is the client's private key file in PEM format

---

