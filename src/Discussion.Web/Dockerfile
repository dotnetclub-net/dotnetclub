FROM mcr.microsoft.com/dotnet/core/aspnet:2.2

ENV ASPNETCORE_URLS http://*:5000
VOLUME /club-data

COPY . /club-app


RUN useradd --uid 5000 --create-home --home /home/clubadm clubadm
RUN chown -R clubadm /home/clubadm
USER clubadm

WORKDIR /club-data
ENTRYPOINT ["dotnet", "/club-app/Discussion.Web.dll", "--webroot", "/club-app/wwwroot"]

