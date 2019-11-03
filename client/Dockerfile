FROM microsoft/dotnetcore-build
WORKDIR /app
COPY . /bin
COPY ./crowbar /bin
RUN dotnet restore
