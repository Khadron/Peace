using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Peace.AOP;
using Peace.IoC.Attribute;
using Peace.IoC.Kernel;
using Peace.LoadBalance;
using Peace.LRU;
using Peace.Message_Queue;

namespace Peace.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            #region IoC

            //IKernel kernel = new StandardKernel();
            //var module = new XmlInjectModule();
            //module.Load(kernel);

            //kernel.Bind<IPerson>().To<Chinese>();
            ////todo: 一对多 映射关系 ？？？
            ////kernel.Bind<IPerson>().To<Chinese>();
            ////kernel.Bind<IPerson>().To<British>().InTransientScope();
            //var print = kernel.Get<Printer>();
            //var person = kernel.Resolve<IPerson>();

            //print.Print();
            //Console.WriteLine(print.Person.ToString());
            //Console.WriteLine("==============================");
            //person.SayHello();


            //var chinese = FastObjectCreater.CreateInstance<Child>("小明");
            //chinese.SayHello();

            #endregion

            Console.WriteLine("======================华丽的分割线============================");

            #region AOP

            //var proxy = new AOPTest.MyUserServiceProxy(new AOPTest.User
            //{
            //    Name = "Khadron",
            //    BirthDate = DateTime.Now
            //},
            //    new AOPTest.MyLogAspect());

            //IList<AOPTest.User> users = proxy.GetUsers();

            //if (users.Count > 0)
            //{
            //    proxy.AddUser(users[0]);
            //}

            //var test = ProxyFactory.CreateProxy<Test>();

            //test.Sum(12, 2);
            //Console.WriteLine("-------------------");
            //test.Show();
            //Console.WriteLine("-------------------");
            //test.Exception();

            #endregion

            //var a = typeof(Test).GetCustomAttributes(typeof(ParentAttribute), false);

            #region LRU
            //LRUCache<long, Data> cache = new LRUCache<long, Data>();

            //for (int i = 0; i < 1000000; i++)
            //{
            //    cache.Put(i, new Data(i));
            //}


            //for (int i = 0; i < 1000000; i++)
            //{
            //    var data = cache.Get(i);
            //    Console.WriteLine(data.Index);
            //}

            #endregion

            #region LoadBalance

            //WeightedRoundRobinLoadBalancer.Instance.Init(new List<Server>()
            //{
            //    new Server{ServerName = "192.168.131.12",Version = "v1",Weight = 2,Isolated = false},
            //    new Server{ServerName = "192.168.131.6",Version = "v1",Weight = 6,Isolated = false},
            //    new Server{ServerName = "192.168.131.76",Version = "v1",Weight = 10,Isolated = false},
            //    new Server{ServerName = "192.168.131.88",Version = "v1",Weight = 8,Isolated = false},

            //});

            //List<Thread> threads = new List<Thread>();
            //for (int i = 0; i < 100; i++)
            //{
            //    Thread thread = new Thread(() =>
            //    {

            //        var s = WeightedRoundRobinLoadBalancer.Instance.ChooseServer();
            //        Console.WriteLine("服务器：{0}", s.ServerName);
            //    });
            //    threads.Add(thread);
            //}

            //foreach (var thread in threads)
            //{
            //    thread.Start();
            //    Thread.Sleep(50);
            //}

            #endregion

            #region MessageQueue



            ReceiveServer server = new ReceiveServer();
            MessageQueueManager.Manager.MessageNotifyEvent += server.Process;
            MessageQueueManager.Manager.Load(server.Publish());
            MessageQueueManager.Manager.Start();
            Console.WriteLine(MessageQueueManager.Manager.Count);


            #endregion


            Console.ReadKey();
        }

        #region IoC

        public interface IPerson
        {
            void SayHello();
        }

        public class Chinese : IPerson
        {
            public Chinese()
            {
            }

            public Chinese(string name)
            {
            }

            public void SayHello()
            {
                Console.WriteLine("你好！！！");
            }

            public override string ToString()
            {
                return "中国人";
            }
        }

        public class British : IPerson
        {
            public void SayHello()
            {
                Console.WriteLine("Hello!!!");
            }
        }

        public class Child
        {
            public Child()
            {
            }

            private string _name;
            public Child(string name)
            {
                _name = name;
            }

            public void SayHello()
            {
                Console.WriteLine("nihao");
            }
        }

        public class Printer
        {
            private readonly IPerson _person;
            public Printer(IPerson person)
            {
                _person = person;
            }

            [Inject]
            public IPerson Person { get; set; }

            public void Print()
            {
                Console.WriteLine("Print:");
                _person.SayHello();
            }
        }

        #endregion

        #region AOP

        public class TesttAspect : AspectAttribute, IInterceptor
        {
            public void BeginInvoke(InvokeContext context)
            {
                Console.WriteLine("开始");
            }

            public void EndInvoke(InvokeContext context)
            {
                Console.WriteLine("结束");
            }

            public void OnException(InvokeContext context)
            {
                Console.WriteLine("异常");
            }
        }


        [TesttAspect]
        public class Test
        {
            public virtual int Sum(int i, int j)
            {
                Console.WriteLine("Sum");
                return i + j;
            }

            public virtual void Show()
            {
                Console.WriteLine("Show");
            }

            public virtual object Exception()
            {
                int i = 0;
                int r = 1 / i;
                return null;
            }
        }

        public class Data
        {
            public int Index { get; set; }

            public Data(int index)
            {
                Index = index;
            }
        }

        #endregion

        #region Messsage Queue


        public class Receiver
        {
            private string _name;
            public Receiver(string name)
            {
                _name = name;
            }

            public void Do(string id)
            {

                Console.WriteLine("{0} :{1}正在执行", _name, id);
            }
        }

        public class ReceiveServer
        {
            public List<QueueMessage> Publish()
            {
                var messages = new List<QueueMessage>();
                for (int i = 0; i < 10; i++)
                {
                    messages.Add(new QueueMessage
                    {
                        Body = new Receiver("呵呵" + i),
                        Id = Guid.NewGuid(),
                        State = State.None,
                        Type = typeof(Receiver).FullName
                    });
                }

                return messages;
            }


            public void Process(QueueMessage message)
            {
                var receiver = message.Body as Receiver;
                if (receiver != null)
                {
                    try
                    {
                        receiver.Do(message.Id.ToString("N"));
                        message.Completed();
                    }
                    catch (Exception ex)
                    {
                        message.State = State.Error;
                    }

                }
            }

        }

        #endregion

    }




}
