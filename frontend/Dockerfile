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

FROM node:24-bookworm-slim AS worker-deps
WORKDIR /app/frontend
COPY frontend/package.json frontend/package-lock.json ./
RUN npm ci --omit=dev

FROM node:24-bookworm-slim AS worker
WORKDIR /app/frontend
ENV NODE_ENV=production
COPY --from=worker-deps --chown=node:node /app/frontend/node_modules ./node_modules
COPY --chown=node:node frontend/package.json ./
COPY --chown=node:node frontend/tsconfig.json ./
COPY --chown=node:node frontend/src ./src
COPY --chown=node:node frontend/scripts ./scripts
COPY --chown=node:node database/ /app/database/
USER node
CMD ["npm","run","worker"]
