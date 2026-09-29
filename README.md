kv.server

if you want to start project:
1)start docker and check work postgres(bd) and rabbit
2)Change directory project \kv.server\KV.Server\src\KV.Server.Web then input "dotnet run"
first dounload kv.server.web
3)Change directory project \kv.server\KV.Server\src\KV.Server.PublicWeb\ then input "dotnet run"
4)if need to add packege to libs folder need input to terminal "abp install-libs"

Для использования пкетов из репозитория @appricot необходимо добавить следующий код в фаил .npmrc находящийся в паке пользовотеля системы:

//gitlab.com/api/v4/packages/npm/:_authToken=${ ваш сгенерированный токен GitLab }
@appricot:registry=https://gitlab.com/api/v4/packages/npm/

