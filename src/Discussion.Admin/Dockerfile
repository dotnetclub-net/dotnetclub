FROM mcr.microsoft.com/dotnet/core/aspnet:2.1.10-bionic

ENV ASPNETCORE_URLS http://*:5050
VOLUME /club-data

COPY . /app


RUN useradd --uid 5000 --create-home --home /home/clubadm clubadm
RUN chown -R clubadm /home/clubadm
USER clubadm

WORKDIR /app
ENTRYPOINT ["dotnet", "./Discussion.Admin.dll"]




