using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Peace.AOP;

namespace Peace.Test
{
    public class AOPTest
    {
        public class User
        {
            public string Name { get; set; }
            public DateTime BirthDate { get; set; }
        }

        public interface IUserService
        {
            bool AddUser(User user);
            IList<User> GetUsers();
        }


        //public class UserService : IUserService
        //{
        //    public bool AddUser(User user)
        //    {
        //        Console.WriteLine("添加人员：{0}", user.Name);
        //        return true;
        //    }

        //    public IList<User> GetUsers()
        //    {
        //        return new List<User>
        //        {
        //            new User{Name = "张三",BirthDate = DateTime.Now.AddYears(-1)},
        //            new User{Name = "李四",BirthDate = DateTime.Now.AddYears(-2)},
        //            new User{Name = "王五",BirthDate = DateTime.Now.AddYears(-3)},
        //        };
        //    }
        //}


        public class UserService : IUserService
        {
            public bool AddUser(User user)
            {
                Console.WriteLine("添加人员：{0}", user.Name);
                Console.WriteLine("Log:AddUser");
                return true;
            }

            public IList<User> GetUsers()
            {
                Console.WriteLine("Log:GetUsers");
                return new List<User>
                {
                    new User{Name = "张三",BirthDate = DateTime.Now.AddYears(-1)},
                    new User{Name = "李四",BirthDate = DateTime.Now.AddYears(-2)},
                    new User{Name = "王五",BirthDate = DateTime.Now.AddYears(-3)},
                };
            }
        }

        public class MyUserService : UserService
        {
            public new bool AddUser(User user)
            {
                base.AddUser(user);
                Console.WriteLine("Log:AddUser");
                return true;
            }

            public new IList<User> GetUsers()
            {
                Console.WriteLine("Log:GetUsers");
                return base.GetUsers();
            }

        }

        public interface IInterceptor
        {
            object Invoke(object obj, string methodName, object[] paramters);
        }

        public class MyLogAspect : AspectAttribute, IInterceptor
        {
            public object Invoke(object obj, string methodName, object[] paramters)
            {
                Console.WriteLine("Log:{0}", methodName);

                object result = obj.GetType().GetMethod(methodName).Invoke(obj, paramters);

                return result;
            }
        }

        public class MyUserServiceProxy
        {
            private readonly IInterceptor _interceptor;
            private readonly User _user;
            public MyUserServiceProxy(User user, IInterceptor interceptor)
            {
                _user = user;
                _interceptor = interceptor;
            }

            public bool AddUser(User user)
            {
                return (bool)_interceptor.Invoke(_user, "AddUser", new object[] { user });
            }

            public IList<User> GetUsers()
            {
                return (IList<User>)_interceptor.Invoke(_user, "GetUsers", null);

            }


        }

    }
}
