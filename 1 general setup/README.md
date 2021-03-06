### proto-rabbitmq-with-ssl

## 01: General Setup

### 01.1 Environments

**DM: RabbitMQ-broker (server)** (runs in VirtualBox)
 - Ubuntu (18.04.2, Desktop image)
 - Docker (18.09.3)
 - RabbitMQ (tag: 3:management -> rabbtiMQ 3.7.13): 
 - OpenSSL (v1.1.0g 2-Nov-2017)

**Instrument: RabbitMQ-client**
 - Windows 7
 - Visual Studio 2017
 - RabbitMQ .NET client (v5.1.0) (Nuget)
 - OpenSSL (v1.1.1b 26-Feb-2019) (installer for Windows: https://slproweb.com/products/Win32OpenSSL.html)

---

### 01.2 Setting um VM Hints

**TODO**

Remarks:
- Add host-only
- share folder

---

### 01.3 Running RabbitMQ in Docker

Get image:
````
docker image pull rabbitmq:3-management
````

Run container:
````
docker container run -d --hostname my-rabbit-host --name my-rabbit-container -p 8080:15672 -p 5672:5672 rabbitmq:3-management
````

View log-files:
````
docker container logs my-rabbit-container
````

Get into container:
````
docker container exec -it 4e21 /bin/bash
````

Copy file from container to host (and vice versa):
````
docker container cp my-rabbit-container:/etc/rabbitmq/rabbitmq.conf .
````

---

### 01.4 OpenSSL Hints

- Find the location of the OpenSSL configuration file (openssl.cnf)
  ````
  // check the OPENSSLDIR for the cnf file
  openssl version -a
  ````
 
- Show details about commands:
  ````
  // openssl {cmd} -help
  openssl x509 -help
  openssl req -help
  ````

- Show details of a certificate:
  ````
  openssl x509 -in mycertificate.pem -text
  ````