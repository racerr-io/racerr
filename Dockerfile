FROM ubuntu
COPY ./build/StandaloneLinux64 .
EXPOSE 7778
ENTRYPOINT ["./StandaloneLinux64"]