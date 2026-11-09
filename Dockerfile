# ---------- Etapa 1: compilar ----------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS compilar
WORKDIR /src

# Se copian primero los .csproj para aprovechar la cache de paquetes
COPY VetCare.Datos/VetCare.Datos.csproj VetCare.Datos/
COPY VetCare.Negocio/VetCare.Negocio.csproj VetCare.Negocio/
COPY VetCare.Api/VetCare.Api.csproj VetCare.Api/
RUN dotnet restore VetCare.Api/VetCare.Api.csproj

COPY VetCare.Datos/ VetCare.Datos/
COPY VetCare.Negocio/ VetCare.Negocio/
COPY VetCare.Api/ VetCare.Api/
RUN dotnet publish VetCare.Api/VetCare.Api.csproj -c Release -o /app/publicar --no-restore

# ---------- Etapa 2: ejecutar ----------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=compilar /app/publicar .
ENV ASPNETCORE_ENVIRONMENT=Production
ENTRYPOINT ["dotnet", "VetCare.Api.dll"]
