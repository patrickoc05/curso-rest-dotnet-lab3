FROM mcr.microsoft.com/dotnet/sdk:5.0-buster-slim AS build-env

COPY ["src/frontend/Frontend.csproj", "src/frontend/"]
RUN dotnet restore "src/frontend/Frontend.csproj"
COPY "src/frontend/" "src/frontend/"
RUN dotnet build "src/frontend/Frontend.csproj" -c Release -o /build

FROM build-env AS publish
RUN dotnet publish "src/frontend/Frontend.csproj" -c Release -o /publish

FROM nginx:alpine AS final
WORKDIR /usr/share/nginx/html
COPY --from=publish /publish/wwwroot /usr/local/webapp/nginx/html
COPY --from=build-env "src/frontend/ngnix.conf" /etc/nginx/nginx.conf
EXPOSE 80