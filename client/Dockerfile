FROM  mcr.microsoft.com/dotnet/core/sdk:2.2
WORKDIR /app
COPY . /bin
COPY ./crowbar /bin
RUN dotnet restore /bin/crowbar.csproj
