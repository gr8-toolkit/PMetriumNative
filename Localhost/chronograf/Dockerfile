FROM chronograf:1.8.6

RUN apt-get update && apt-get install -y dos2unix

COPY entrypoint1.sh /entrypoint1.sh

RUN dos2unix /entrypoint1.sh
RUN chmod +x entrypoint1.sh
ENTRYPOINT ["/entrypoint1.sh"]