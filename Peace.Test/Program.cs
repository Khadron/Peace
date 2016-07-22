using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Peace.IoC.Attribute;
using Peace.IoC.Kernel;

namespace Peace.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            IKernel kernel = new StandardKernel();
            var module = new XmlInjectModule();
            module.Load(kernel);

            //kernel.Bind<IPerson>().To<Chinese>();
            ////todo: 一对多 映射关系 ？？？
            ////kernel.Bind<IPerson>().To<Chinese>();
            ////kernel.Bind<IPerson>().To<British>().InTransientScope();
            var print = kernel.Get<Printer>();
            var person = kernel.Resolve<IPerson>();

            print.Print();
            Console.WriteLine(print.Person.ToString());
            Console.WriteLine("==============================");
            person.SayHello();



            Console.ReadKey();
        }

        public interface IPerson
        {
            void SayHello();
        }

        public class Chinese : IPerson
        {
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
    }
}
