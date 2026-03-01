# ---- build stage ----
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# csproj kopyala + restore
COPY *.csproj ./
RUN dotnet restore

# geri kalan her şeyi kopyala + publish
COPY . ./
RUN dotnet publish -c Release -o /app/publish

# ---- runtime stage ----
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Render portu env ile verir (PORT)
ENV ASPNETCORE_URLS=http://0.0.0.0:${PORT}
EXPOSE 8080

COPY --from=build /app/publish ./

# DLL adını kendi projenin dll'iyle değiştir
ENTRYPOINT ["dotnet", "RANailArt.dll"]