### 一、打包镜像

#### 1.WEB服务

```sh
docker build -f Dockerfile_SmartHomeWeb -t smarthomeweb:1.0.0-alpha.1 .
```

#### 2.API服务

```sh
docker build -f Dockerfile_SmartHomeApi -t smarthomeapi:1.0.0-alpha.1 .
```

#### 3.授权服务

```sh
docker build -f Dockerfile_IdentityApi -t identityapi:1.0.0-alpha.1 .
```

#### 4.邮件服务

```sh
docker build -f Dockerfile_MailApi -t mailapi:1.0.0-alpha.1 .
```

### 二、运行镜像

#### 1.WEB服务

```sh
docker run -d --name {实例名} -p 8080:80 --network bridge {镜像:tag}
```

* 开发环境代理：vite.config.ts
* 生产环境代理（nginx）：nginx.conf

#### 2.API服务

```sh
docker run -d --name {实例名} -p 10101:10101 --network bridge {镜像:tag}
```

* 环境变量：

  | Key                          | Type   | Demo                                                         | Description                                    |
  | ---------------------------- | ------ | ------------------------------------------------------------ | ---------------------------------------------- |
  | ASPNETCORE_HTTP_PORTS        | Int16  | 10101                                                        | HTTP服务端口，默认10101                        |
  | App:WorkerId                 | Int32  | 1                                                            | 实例ID，用于雪花ID生成，确保每个实例ID不能重复 |
  | ConnectionStrings:Postgresql | String | Host=127.0.0.1;Port=5432;Username=xxx;Password=xxx;Database=xxx | Postgres数据库连接                             |
  | ConnectionStrings:EventBus   | String | amqp://root:root@127.0.0.1                                   | RabbitMQ连接，用于事件总线                     |

* 授权公钥

  根目录文件：public.pem

#### 3.授权服务

```sh
docker run -d --name {实例名} -p 10103:10103 --network bridge {镜像:tag} -e ConnectionStrings:Mail={邮件服务域名（例如http://mail:10105）}
```

* 环境变量：

  | Key                          | Type   | Demo                                                         | Description                                    |
  | ---------------------------- | ------ | ------------------------------------------------------------ | ---------------------------------------------- |
  | ASPNETCORE_HTTP_PORTS        | Int16  | 10103                                                        | HTTP服务端口，默认10103                        |
  | App:AdminPassword            | String | 123456                                                       | 初始管理员密码                                 |
  | App:WorkerId                 | Int32  | 1                                                            | 实例ID，用于雪花ID生成，确保每个实例ID不能重复 |
  | ConnectionStrings:Postgresql | String | Host=127.0.0.1;Port=5432;Username=xxx;Password=xxx;Database=xxx | Postgres数据库连接                             |
  | ConnectionStrings:Redis      | String | 127.0.0.1                                                    | Redis连接                                      |
  | ConnectionStrings:Mail       | String | http://127.0.01:8080                                         | 邮件服务域名                                   |
  
* 授权私钥（若服务启动时缺失，则会自动在根目录下生成一个密钥对）

  根目录文件：private.pem

#### 4.邮件服务

```sh
docker run -d --name {实例名} -p 10105:10105 --network bridge {镜像:tag}
```

* 配置文件`appsettings.json`的`Email`节点，配置系统可用的邮箱

* 授权公钥

  根目录文件：public.pem
