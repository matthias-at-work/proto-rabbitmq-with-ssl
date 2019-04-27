FROM rabbitmq:3-management
RUN mkdir -p /home/certificates \
    && chmod 777 /home/certificates
COPY dm.server.key.pem dm.server.cert.pem dm.ca.cert.pem /home/certificates/
RUN chmod 777 /home/certificates/*
COPY rabbitmq.config /etc/rabbitmq/.
