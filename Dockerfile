FROM ubuntu:latest

EXPOSE 7778

COPY ./build/StandaloneLinux64 .

ENTRYPOINT ["./StandaloneLinux64"]