FROM node:24-bookworm-slim AS build
WORKDIR /app/frontend
COPY --chown=node:node frontend/package.json frontend/package-lock.json ./
RUN npm ci
COPY --chown=node:node frontend/ ./
COPY --chown=node:node database/ /app/database/
RUN npm run build

FROM node:24-bookworm-slim AS app
WORKDIR /app/frontend
ENV NODE_ENV=production HOSTNAME=0.0.0.0 PORT=3000
COPY --from=build --chown=node:node /app/frontend/.next/standalone ./
COPY --from=build --chown=node:node /app/frontend/.next/static ./.next/static
COPY --from=build --chown=node:node /app/frontend/public ./public
USER node
EXPOSE 3000
CMD ["node","server.js"]

FROM build AS worker
ENV NODE_ENV=production
USER node
CMD ["npm","run","worker"]
