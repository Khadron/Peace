using System;
using System.Collections.Generic;
using System.Threading;
using System.Timers;
using Timer = System.Timers.Timer;

namespace Peace.Message_Queue
{
    public interface IMessageQueue
    {
        void Load(IEnumerable<QueueMessage> messages);
        void Enqueue(QueueMessage message);
        QueueMessage Dequeue();
        void Purge(string token);
        void PurgeAll();

    }

    public class MessageQueues : IMessageQueue, IDisposable
    {
        private readonly Dictionary<string, QueueMessage> messageDic;
        private readonly ReaderWriterLockSlim locker = new ReaderWriterLockSlim();
        private int current = 1, count = 0;

        public int Count
        {
            get { return count; }
        }

        public MessageQueues()
        {
            messageDic = new Dictionary<string, QueueMessage>();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public void Load(IEnumerable<QueueMessage> messages)
        {
            locker.EnterWriteLock();
            try
            {
                foreach (QueueMessage queueMessage in messages)
                {
                    Put(queueMessage);
                }
            }
            finally
            {
                locker.ExitWriteLock();
            }
        }

        /// <summary>
        /// 入队
        /// </summary>
        /// <param name="message"></param>
        public void Enqueue(QueueMessage message)
        {
            locker.EnterWriteLock();

            try
            {
                Put(message);
            }
            finally
            {

                locker.ExitWriteLock();
            }
        }

        /// <summary>
        /// 出队
        /// </summary>
        public QueueMessage Dequeue()
        {
            QueueMessage result = null;

            if (messageDic.Count > 0)
            {
                locker.EnterReadLock();

                try
                {
                    var enumerator = messageDic.Values.GetEnumerator();
                    int index = 1;
                    while (enumerator.MoveNext())
                    {
                        if (index == current)
                        {
                            result = enumerator.Current;
                            result.State = State.Processing;
                            current++;
                            break;
                        }
                        else
                        {
                            index++;
                        }
                    }
                }
                finally
                {
                    locker.ExitReadLock();
                }
            }



            return result;
        }

        public void Purge(string token)
        {
            locker.EnterWriteLock();

            try
            {
                Console.WriteLine("删除 {0}", token);

                count--;
                var cur = messageDic[token];
                cur.State = State.Completed;

                Console.Write("   剩余{0}\n", count);

                if (current == messageDic.Count)
                {
                    PurgeAll();
                }
            }
            finally
            {
                locker.ExitWriteLock();
            }

        }

        public void PurgeAll()
        {
            messageDic.Clear();
            Console.WriteLine("最后剩余：{0}", messageDic.Count);
        }

        private void Put(QueueMessage message)
        {
            var key = message.Token();

            if (!messageDic.ContainsKey(key))
            {
                message.CompletedEvent += Purge;
                messageDic.Add(key, message);
                count++;
            }
        }
    }

    public delegate void MessageQueueNotifyEventHandler(QueueMessage message);

    public class MessageQueueManager
    {
        private readonly MessageQueues _queue;
        private readonly Timer _timer;
        private static readonly object Locker = new object();
        public static readonly MessageQueueManager Manager = new MessageQueueManager();

        /// <summary>
        /// 处理速度（单位毫秒）
        /// </summary>
        public double ProcessSpeed { get; set; }

        public int Count
        {
            get { return _queue.Count; }
        }

        private MessageQueueManager()
        {
            _queue = new MessageQueues();
            _timer = new Timer();
            ProcessSpeed = 1000;
            this._timer.Interval = 500;
            this._timer.Elapsed += Notify;
        }

        public void Start()
        {
            this._timer.Enabled = true;
        }

        public void Stop()
        {
            this._timer.Enabled = false;
        }

        public void Add(QueueMessage message)
        {
            _queue.Enqueue(message);
        }

        public void Load(IEnumerable<QueueMessage> messages)
        {
            _queue.Load(messages);
        }

        private void Notify(object sender, ElapsedEventArgs e)
        {
            lock (Locker)
            {

                var message = _queue.Dequeue();

                if (message != null)
                {
                    this._messageNotifyEventHandler(message);
                }
            }

        }

        private MessageQueueNotifyEventHandler _messageNotifyEventHandler;
        public event MessageQueueNotifyEventHandler MessageNotifyEvent
        {
            add { this._messageNotifyEventHandler += value; }
            remove
            {
                if (this._messageNotifyEventHandler != null)
                {
                    //
                    this._messageNotifyEventHandler -= value;
                }
            }
        }

    }

    public class QueueMessage
    {
        public delegate void CompletedEventHandler(string token);
        public event CompletedEventHandler CompletedEvent;
        public Guid Id { get; set; }
        public string Type { get; set; }
        public object Body { get; set; }//todo:还可以使用序列化，在先这里简单处理
        public State State { get; set; }

        public void Completed()
        {
            if (CompletedEvent != null)
            {
                CompletedEvent(this.Token());
            }
        }

        public string Token()
        {
            return string.Format("{0}***{1}", Id, Body.GetHashCode());
        }
    }

    public enum State
    {
        None = 0,
        Processing,
        Error,
        Completed
    }


}
