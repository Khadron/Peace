using System;
using System.Collections.Generic;
using System.Linq;

namespace Peace.LoadBalance
{
    /// <summary>
    /// 服务器
    /// </summary>
    public class Server
    {
        public int Weight { get; set; }
        public string ServerName { get; set; }
        public string Version { get; set; }
        public bool Isolated { get; set; }

        public override string ToString()
        {
            return string.Format("{0}#{1}", ServerName, Version);
        }
    }
    /// <summary>
    /// 负载均衡接口
    /// </summary>
    public interface ILoadBalance
    {
        /// <summary>
        /// 根据请求选择一个服务器
        /// </summary>
        /// <returns></returns>
        Server ChooseServer();

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="servers"></param>
        void Init(IList<Server> servers);

        /// <summary>
        /// 返回所有服务器列表
        /// </summary>
        /// <returns></returns>
        IList<Server> GetServers();

        /// <summary>
        /// 更新服务器列表
        /// </summary>
        /// <param name="servers"></param>
        void UpdateServers(IList<Server> servers);

        /// <summary>
        /// 隔离一个服务器
        /// </summary>
        /// <param name="serverName"></param>
        void IsolateServer(string serverName);

        /// <summary>
        /// 恢复一个服务器
        /// </summary>
        /// <param name="serverName"></param>
        void ResumeServer(string serverName);

    }
    /// <summary>
    /// 负载均衡异常
    /// </summary>
    public class LoadBalanceException : Exception
    {
        public LoadBalanceException(string message)
            : base(message)
        {
        }
    }
    /// <summary>
    /// 负载均衡抽象类
    /// </summary>
    public abstract class LoadBalancer : ILoadBalance
    {
        protected List<Server> Servers;

        public Server this[string name]
        {
            get { return Servers.FirstOrDefault(r => r.ServerName == name); }
        }

        protected LoadBalancer()
        {
            Servers = new List<Server>();
        }

        public abstract Server ChooseServer();

        public virtual void Init(IList<Server> servers)
        {
            RegisterServers(servers);
        }

        public virtual IList<Server> GetServers()
        {
            return Servers;
        }

        public virtual void UpdateServers(IList<Server> servers)
        {
            Servers = (List<Server>)servers;
        }

        public virtual void IsolateServer(string serverName)
        {
            var targetServer = Servers.FirstOrDefault(r => r.ServerName == serverName);
            if (targetServer != null)
            {
                targetServer.Isolated = true;
            }
            throw new LoadBalanceException("在服务器列表中为找到" + serverName);
        }

        public virtual void ResumeServer(string serverName)
        {
            var targetServer = Servers.FirstOrDefault(r => r.ServerName == serverName);
            if (targetServer != null)
            {
                targetServer.Isolated = false;
            }
            throw new LoadBalanceException("在服务器列表中为找到" + serverName);

        }

        public virtual void RegisterServers(IList<Server> servies)
        {
            Servers.AddRange(servies);
        }

    }
    /// <summary>
    /// Weighted Round Robin实现
    /// </summary>
    public class WeightedRoundRobinLoadBalancer : LoadBalancer
    {
        private static readonly object Locker = new object();
        private static readonly object CreateLocker = new object();
        private static LoadBalancer _loadBalance;

        private int _cycle;
        private int _currentIndex = -1;
        private readonly IList<string> _rrList;

        private WeightedRoundRobinLoadBalancer()
        {
            _rrList = new List<string>();
        }

        public static LoadBalancer Instance
        {
            get
            {
                if (_loadBalance == null)
                {
                    lock (CreateLocker)
                    {
                        if (_loadBalance == null)
                        {
                            _loadBalance = new WeightedRoundRobinLoadBalancer();
                        }
                    }
                }

                return _loadBalance;
            }
        }

        public override Server ChooseServer()
        {
            Server result = null;
            if (Servers.Count == 0)
            {
                //log
                return result;
            }

            lock (Locker)
            {
                int index = 0;
                while (index < _cycle && result == null)
                {
                    _currentIndex = (_currentIndex + 1) % _rrList.Count;
                    result = base[_rrList[_currentIndex]];

                    if (!result.Isolated)
                        index++;
                }

            }
            return result;
        }

        public override void Init(IList<Server> servers)
        {
            Servers = (List<Server>)servers;
            BuildRrList();
        }

        private void BuildRrList()
        {
            var facotrs = Servers.Select(r => r.Weight).ToArray();
            int gcd = CalcMaxGcd(facotrs);//服务器权重的最大公约数
            int maxWeight = GetMaxWeight(facotrs);//获取最高权重
            _cycle = CalcCycle(facotrs, gcd);//轮询周期

            int index = -1;
            int weight = 0;
            for (int i = 0; i < _cycle; i++)
            {
                while (true)
                {
                    index = (index + 1) % Servers.Count;//服务器下标
                    if (index == 0)
                    {
                        weight = weight - gcd;//获得处理的权重
                        if (0 >= weight)
                        {
                            weight = maxWeight;
                            if (weight == 0) break;
                        }
                    }
                    var server = Servers[index];
                    if (server.Weight >= weight)//当前权重大于上次权重
                    {
                        _rrList.Add(server.ServerName);
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// 计算最大公约数（辗转相相减法）
        /// </summary>
        /// <param name="factors"></param>
        /// <returns></returns>
        private int CalcMaxGcd(int[] factors)
        {
            int max = 0;

            for (int i = 0; i < factors.Length; i++)
            {
                int minus = 0;
                int factor = factors[i];

                do
                {
                    if (factor < max)
                    {
                        factor = factor ^ max;
                        max = max ^ factor;
                        factor = factor ^ max;
                    }

                    if (0 == max) max = factor;

                    factor = factor - max;

                } while (factor != 0);
            }

            return max;
        }

        private int GetMaxWeight(int[] facotrs)
        {
            int maxWeight = 0;
            foreach (var i in facotrs)
            {
                if (maxWeight < i) maxWeight = i;
            }
            return maxWeight;
        }

        private int CalcCycle(int[] factors, int gcd)
        {
            int cycle = 0;
            for (int i = 0; i < factors.Length; i++)
            {
                cycle += factors[i] / gcd;
            }
            return cycle;
        }

    }

}
