FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY dapper_minimal_api.csproj .
RUN dotnet restore "dapper_minimal_api.csproj"
COPY . .
RUN dotnet publish "dapper_minimal_api.csproj" -c Release -o /publish
COPY app.db /publish/

FROM mcr.microsoft.com/dotnet/aspnet:6.0 as final
WORKDIR /app
COPY --from=build /publish .

ENTRYPOINT [ "dotnet", "dapper_minimal_api.dll" ]


#docker images | grep dapper
#docker run --rm -it -p 8000:80 --env-file .env dapper:1.10
#http://localhost:8000/settings
#docker login

#docker build -t dapper:1.12 .
#docker tag 0b82cbac03cb tonydoker/dapper:1.12
#docker push tonydoker/dapper:1.12
