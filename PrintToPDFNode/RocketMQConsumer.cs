﻿using NewLife;
using NewLife.Reflection;
using NewLife.RocketMQ;
using Newtonsoft.Json;
using NLog.Fluent;
using System;

namespace PrintToPDFNode
{
    public class RocketMQConsumer
    {
        private NewLife.RocketMQ.Consumer _consumer;
        private readonly Action<List<NewLife.RocketMQ.Protocol.MessageExt>> callback;
        private readonly Func<MyException<List<NewLife.RocketMQ.Protocol.MessageExt>>, Boolean> errorCallback;

        // 静态变量，用于标记当前是否有任务正在执行
        private static bool isTaskRunning = false;
        /**
         * 样例
         * nameServerAddress：192.168.12.12:9876
         */
        public RocketMQConsumer(string topic,string topic_group,string nameServerAddress, Action<List<NewLife.RocketMQ.Protocol.MessageExt>> callback, Func<MyException<List<NewLife.RocketMQ.Protocol.MessageExt>>, Boolean> errorCallback ,int batchSize = 1,string tags="*")
        {
            this.callback = callback ?? throw new ArgumentNullException(nameof(callback));
            this.errorCallback = errorCallback ?? throw new ArgumentNullException(nameof(errorCallback));

            _consumer = new NewLife.RocketMQ.Consumer
            {
                Topic = topic,
                Group = topic_group,
                NameServerAddress = nameServerAddress,
                AclOptions = new AclOptions()
                {
                    AccessKey= Config.Accesskey,
                    SecretKey= Config.Secretkey,
                },
                BatchSize = batchSize,
                //FromLastOffset = true,
                //SkipOverStoredMsgCount = 0,
                //BatchSize = 20,
                Log = NewLife.Log.XTrace.Log,
            };
            // 订阅特定的Tag,只消费tag为tags的消息
            _consumer.Subscription = tags;
            _consumer.OnConsume = (q, ms) =>
            {
                string mInfo = $"BrokerName={q.BrokerName},QueueId={q.QueueId},Length={ms.Length}";
                Log.Info(mInfo);
                if(isTaskRunning)
                {
                    // 不消费该任务，直接返回
                    return false;
                }
                try
                {
                    isTaskRunning = true;
                    callback(ms.ToList());
                    return true;
                }
                catch (Exception ex)
                {
                    try
                    {
                        return errorCallback(new MyException<List<NewLife.RocketMQ.Protocol.MessageExt>>(ex, ms.ToList()));
                    }catch(Exception ex2)
                    {
                        //未知的错误，直接跳过
                        Console.WriteLine(ex2.ToString());
                        return true;
                    }
                   
                    //消费失败就推送一条回执,消费不了就不能占用资源
                }finally { isTaskRunning = false; }

                //   return false;//通知消息队：不消费消息
                //return false;        //通知消息队：消费了消息
                //由回调函数决定是否消费消息
            };
        }
        public void start()
        {
            _consumer.Start();
        }
        public void stop()
        {
            _consumer.Stop();
        }

    }
}
