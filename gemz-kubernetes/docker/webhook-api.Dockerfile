FROM mcr.microsoft.com/dotnet/sdk:7.0-jammy AS build
WORKDIR /src
COPY  ./Gemz.Api.StripeWebhook .
RUN mkdir -p /out
RUN dotnet publish ./Gemz.Api.StripeWebhook -c Release -o /out -r linux-x64 --self-contained 

FROM mcr.microsoft.com/dotnet/aspnet:7.0-jammy AS runtime
WORKDIR /app
COPY --from=build /out .
CMD ["./Gemz.Api.StripeWebhook"]
