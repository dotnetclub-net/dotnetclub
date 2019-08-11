FROM mcr.microsoft.com/dotnet/core/aspnet:2.2

ENV ASPNETCORE_URLS http://*:5000

COPY . /club-app
COPY ./appsettings.json /club-data/


RUN useradd --uid 5000 --create-home --home /home/clubadm clubadm
RUN chown -R clubadm /home/clubadm
RUN chown -R clubadm /club-data
USER clubadm

WORKDIR /club-data
ENTRYPOINT ["dotnet", "/club-app/Discussion.Web.dll", "--webroot", "/club-app/wwwroot"]

