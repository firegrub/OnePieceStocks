FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["OnePieceStocks/OnePieceStocks.csproj", "OnePieceStocks/"]
RUN dotnet restore "OnePieceStocks/OnePieceStocks.csproj"
COPY . .
WORKDIR "/src/OnePieceStocks"
RUN dotnet publish "OnePieceStocks.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:10000
EXPOSE 10000
ENTRYPOINT ["dotnet", "OnePieceStocks.dll"]