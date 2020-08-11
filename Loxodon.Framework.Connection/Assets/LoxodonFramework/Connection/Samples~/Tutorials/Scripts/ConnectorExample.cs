﻿/*
 * MIT License
 *
 * Copyright (c) 2018 Clark Yang
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy of 
 * this software and associated documentation files (the "Software"), to deal in 
 * the Software without restriction, including without limitation the rights to 
 * use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies 
 * of the Software, and to permit persons to whom the Software is furnished to do so, 
 * subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all 
 * copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, 
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE 
 * SOFTWARE.
 */

using Loxodon.Framework.Examples.Messages;
using Loxodon.Framework.Net.Connection;
using System;
using System.Text;
using System.Threading;
using UnityEngine;

namespace Loxodon.Framework.Examples
{
    public class ConnectorExample : MonoBehaviour
    {
        Server server;

        IConnector<Request, Response, Notification> connector;
        ISubscription<EventArgs> eventSubscription;
        ISubscription<Notification> messageSubscription;

        int port = 8000;
        void Start()
        {
            //初始化服务器
            server = new Server(port);

            //----------------------

            //创建TcpChannel，如果游戏协议没有定义握手消息，那么HandshakeHandler可以为null
            var channel = new TcpChannel(new DefaultDecoder(), new DefaultEncoder(), new HandshakeHandler());
            channel.NoDelay = true;
            channel.IsBigEndian = true;
            connector = new DefaultConnector<Request, Response, Notification>(channel);
            connector.AutoReconnect = true;

            //订阅事件
            eventSubscription = connector.Events().ObserveOn(SynchronizationContext.Current).Subscribe((e) =>
            {
                Debug.LogFormat("Client Received Event:{0}", e);
            });

            //订阅通知
            //使用ObserveOn(SynchronizationContext.Current)切换消息处理线程为当前的UI线程
            messageSubscription = connector.Received().Filter(notification =>
            {
                //过滤消息，只监听CommandID在0-100之间的消息
                if (notification.CommandID > 0 && notification.CommandID <= 100)
                    return true;
                return false;
            }).ObserveOn(SynchronizationContext.Current).Subscribe(notification =>
            {
                Debug.LogFormat("Client Received Notification:{0}", notification);
            });
        }

        async void Connect()
        {
            try
            {
                await connector.Connect("127.0.0.1", port, 1000);
                Debug.LogFormat("连接成功");
            }
            catch (Exception e)
            {
                Debug.LogFormat("连接异常：{0}", e);
            }
        }

        async void Send(Request request)
        {
            try
            {
                Response response = await connector.Send(request);
                Debug.LogFormat("The client received a response message successfully,Message:{0}", response);

            }
            catch (Exception e)
            {
                Debug.LogFormat("The client failed to send a request,Exception:{0}", e);
            }
        }

        async void Send(Notification notification)
        {
            try
            {
                await connector.Send(notification);
                Debug.LogFormat("The client has successfully sent a notification");
            }
            catch (Exception e)
            {
                Debug.LogFormat("The client failed to send a notification,Exception:{0}", e);
            }
        }

        void OnGUI()
        {
            int x = 50;
            int y = 50;
            int width = 200;
            int height = 100;
            int i = 0;
            int padding = 10;

            GUI.skin.button.fontSize = 25;

            if (GUI.Button(new Rect(x, y + i++ * (height + padding), width, height), server.Started ? "Stop Server" : "Start Server"))
            {
                if (server.Started)
                    server.Stop();
                else
                    server.Start();
            }

            if (GUI.Button(new Rect(x, y + i++ * (height + padding), width, height), connector.Connected ? "Disconnect" : "Connect"))
            {
                if (connector.Connected)
                    connector.Disconnect();
                else
                    Connect();
            }

            if (GUI.Button(new Rect(x, y + i++ * (height + padding), width, height), "Send Request"))
            {
                Request request = new Request();
                request.CommandID = 20;
                request.ContentType = 0;
                request.Content = Encoding.UTF8.GetBytes("this is a request.");
                Send(request);
            }

            if (GUI.Button(new Rect(x, y + i++ * (height + padding), width, height), "Send Notification"))
            {
                Notification notification = new Notification();
                notification.CommandID = 10;
                notification.ContentType = 0;
                notification.Content = Encoding.UTF8.GetBytes("this is a notification.");
                Send(notification);
            }
        }

        private void OnDestroy()
        {
            if (eventSubscription != null)
            {
                eventSubscription.Dispose();
                eventSubscription = null;
            }

            if (messageSubscription != null)
            {
                messageSubscription.Dispose();
                messageSubscription = null;
            }

            if (connector != null)
            {
                connector.Shutdown();
                connector.Dispose();
                connector = null;
            }

            if (server != null)
            {
                server.Stop();
                server = null;
            }
        }

    }
}