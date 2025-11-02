FROM ruby:3.2-alpine
WORKDIR /app
RUN apk add --no-cache build-base mysql-client mariadb-connector-c-dev yaml-dev
COPY . .
RUN gem install bundler && bundle config set without 'development test' && bundle install --jobs 4
COPY docker-entrypoint.sh /app/docker-entrypoint.sh
RUN sed -i 's/\r$//' /app/docker-entrypoint.sh && chmod 755 /app/docker-entrypoint.sh
EXPOSE 4567
ENTRYPOINT ["/app/docker-entrypoint.sh"]