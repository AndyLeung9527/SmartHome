import * as signalR from '@microsoft/signalr'

export default function () {
    // const { apiDomain } = useApi()
    const connection = new signalR.HubConnectionBuilder()
        // 身份验证示例 .withUrl("/messageHub", {accessTokenFactory: () => sessionStorage.getItem("token")})
        .withUrl('ws/broadcastHub')
        // .withUrl(`${apiDomain}/broadcastHub`)
        .configureLogging(signalR.LogLevel.Information)
        .build();

    async function start() {
        try {
            await connection.start()
            console.log("'ws/broadcastHub' SignalR connection started")
        } catch (err) {
            console.log(err)
            setTimeout(start, 10000) // 重试连接
        }
    }

    connection.onclose(async error => {
        console.log('error', error)
        // 断线重连，若error为空则是手动断开无需重连
        if (!!error) await start()
    })

    const connect = async () => {
        await start()
    }

    async function send(methodName: string, param: any[]) {
        try {
            await connection.invoke(methodName, param)
        } catch (err) {
            console.error(`Error of SignalR when invoking method '${methodName}':`, err)
        }
    }

    function receivedBroadcast(callback: (msg: string) => void) {
        connection.on('broadcastReceived', callback)
    }

    const disconnect = async () => {
        await connection.stop()
    }

    return { connection, connect, send, disconnect, receivedBroadcast }
}