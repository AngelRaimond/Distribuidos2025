#me he basado en las buenas practicas de multi-stage builds y en usar versiones especificas 
#ademas en la recomendacion de usar contenedores Apline pq son ligeros 
FROM golang:1.21-alpine AS base
WORKDIR /app


FROM golang:1.21-alpine AS build
WORKDIR /src
COPY . .
RUN go build -o /app/hola-mundo main.go


FROM base AS final
WORKDIR /app
COPY --from=build /app/hola-mundo .
CMD ["./hola-mundo"]