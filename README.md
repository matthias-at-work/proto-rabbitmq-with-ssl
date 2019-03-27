# proto-rabbitmq-with-ssl

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

