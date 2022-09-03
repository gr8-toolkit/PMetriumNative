FROM grafana/grafana:8.5.0

USER root
RUN apk update && \
    apk add jq && \
    apk add gettext && \
    apk add curl && \
    apk add dos2unix && \
    rm -rf /var/cache/apk/*

COPY ./dashboards/ /dashboards/
COPY ./datasources/ /datasources/
COPY run1.sh /run1.sh

RUN dos2unix /run1.sh

ENTRYPOINT ["bash","-x","/run1.sh"]