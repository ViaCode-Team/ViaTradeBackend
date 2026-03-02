# build-swagger.ps1
dotnet build ./ViaTradeBackend/ViaTradeBackend.csproj -c Debug -p:GenerateSwaggerFile=true
