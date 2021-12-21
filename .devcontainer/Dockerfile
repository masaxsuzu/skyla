FROM mcr.microsoft.com/dotnet/sdk:6.0
ENV DEBIAN_FRONTEND=noninteractive

# Common
RUN apt update && apt install -y make binutils libc6-dev gdb apt-transport-https less ca-certificates curl software-properties-common
# My dot files
RUN git clone https://github.com/masaxsuzu/dotfiles.git ~/dotfiles && bash ~/dotfiles/deploy.sh
