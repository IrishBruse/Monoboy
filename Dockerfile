FROM mono:latest

ARG MONOGAME_VERSION=3.7.1
ENV MONOGAME_VERSION $MONOGAME_VERSION

# Download monogame from the official repo
RUN apt-get update \
    && apt-get install -y --no-install-recommends wget gtk-sharp3 \
    && wget -O monogame-sdk.run https://github.com/MonoGame/MonoGame/releases/download/v$MONOGAME_VERSION/monogame-sdk.run \
    && chmod +x monogame-sdk.run \
    && yes | ./monogame-sdk.run \
    && apt-get remove -y wget \
    && apt-get autoremove -y \
    && rm -rf /var/lib/apt/lists/* /tmp/* /var/tmp/*
