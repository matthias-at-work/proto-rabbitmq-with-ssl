
## 5. Trust-store plugin

This plugin allows the server to whitelist trusted (self-signed) client-certificates.  See:
  - https://www.rabbitmq.com/plugins.html#plugin-directories
  - https://github.com/rabbitmq/rabbitmq-trust-store/

No changes are needed on the client/downstream side.
This is only a re-configuration of the server/upstream.

The benefit of this setup is:
- Client-certificate can be directly listed. Works also for self-signed certificates.
- The list of accepted certificates can be changed at runtime:
    - Periodically (`trust_store.refresh_interval`), or
    - On request (relevant for STING setup when a new Instrument/DM are paired):  
      ````rabbitmqctl eval 'rabbit_trust_store:refresh().'````

---

- ***rabbitmq.config*-file:** 
  ````
  [
   {rabbit,
    [
     {loopback_users, []},
      { ssl_listeners, [5671] },
      { ssl_options, [
        {cacertfile,"/home/certificates/dm.server.cert.pem"},
        {certfile,"/home/certificates/dm.server.cert.pem"},
        {keyfile,"/home/certificates/dm.server.key.pem"},
        {verify,verify_peer},
        {fail_if_no_peer_cert,true},
        {versions, ['tlsv1.2', 'tlsv1.1']}
      ]},
     {default_vhost, "/"},
     {default_user, "guest"},
     {default_pass, "guest"},
     {default_permissions, [".*", ".*", ".*"]},
     {log_levels, [{connection,debug}]}
    ]},
    {rabbitmq_trust_store,
     [
      {directory, "/home/whitelist"},
      {refresh_interval, {seconds, 30}}
     ]}
  ].
  ````
   - *ca-certfile* now only contains DM-CA; no chnage required for new client.
   - configuration of *rabbitmq-trust-store*-plugin: 
     Configure the trust store with a directory of whitelisted certificates and a refresh interval


- ***Dockerfile***:
  ````
  FROM rabbitmq:3-management
  
  RUN rabbitmq-plugins enable rabbitmq_federation && \
      rabbitmq-plugins enable rabbitmq_federation_management && \
      rabbitmq-plugins enable rabbitmq_trust_store
  
  RUN mkdir -p /home/certificates && chmod 777 /home/certificates && \
      mkdir -p /home/whitelist && chmod 777 /home/whitelist
  
  COPY dm.server.key.pem dm.server.cert.pem dm.ca.cert.pem /home/certificates/
  COPY instrument.client.cert.pem /home/whitelist/
  
  RUN chmod 777 /home/certificates/* && chmod 777 /home/whitelist/*
  
  COPY rabbitmq.config /etc/rabbitmq/.
  ````



- **Build & Run the container/server**
  ````
  docker build -t rabbitmq-with-federation . 
  docker container run -d --hostname rabbit-host --name my-rabbit -p 8080:15672 -p 5671:5671 -p 5672:5672 rabbitmq-with-federation 
  ````


- **Check that the server picked up the certificates**:
  ````
  docker container exec my-rabbit rabbitmqctl eval 'io:format(rabbit_trust_store:list()).'
  ````
  - this command lists the currently loaded certificates.

---
