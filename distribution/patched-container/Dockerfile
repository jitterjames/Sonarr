FROM lscr.io/linuxserver/sonarr:4.0.19.2979-ls319

ARG TARGETARCH

RUN rm -rf /app/sonarr/bin/*

COPY ${TARGETARCH}/ /app/sonarr/bin/

LABEL org.opencontainers.image.title="Sonarr with unaired-release protection" \
      org.opencontainers.image.description="Sonarr 4.0.19.2979 with an optional rule that rejects releases before episode air dates" \
      org.opencontainers.image.source="https://github.com/jitterjames/Sonarr"
