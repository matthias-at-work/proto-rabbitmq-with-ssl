FROM rabbitmq:3-management
RUN apt-get update \
        && mkdir -p /home/certificates \
        && chmod 777 /home/certificates
COPY dm.rabbitmq.key.pem dm.rabbitmq.cert.pem dm.ca.cert.pem /home/certificates/
RUN chmod 777 /home/certificates/*
COPY rabbitmq.config /etc/rabbitmq/.
