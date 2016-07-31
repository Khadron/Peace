using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Peace.AOP
{
    public class DynamicProxySample : Sample
    {
        private readonly IInterceptor[] _interceptors;

        public DynamicProxySample()
        {
            foreach (IInterceptor interceptor in _interceptors)
            {
                interceptor.BeginInvoke(new InvokeContext(""));
            }
            //    var interceptors = typeof(Sample).GetCustomAttributes(typeof(IInterceptor), false);
            //    _interceptors = interceptors.Cast<IInterceptor>().ToArray();
        }

        public void ShowMessage(string msg)
        {
            //InvokeContext context = new InvokeContext("ShowMessage")
            //{
            //    //RealType = typeof(Sample),
            //    //Parameters = new object[] { msg },
            //    Tag = _interceptors.Length
            //};

            //try
            //{
            //    foreach (var interceptor in _interceptors)
            //    {
            //        interceptor.BeginInvoke(context);
            //    }

            base.ShowMessage(msg);

            //    //_sample.ShowMessage(msg);

            //    //context.ResultValue = _sample.GetType().GetMethod("ShowMessage").Invoke(_sample, context.Parameters);

            //    foreach (var interceptor in _interceptors)
            //    {
            //        interceptor.EndInvoke(context);
            //    }
            //}
            //catch (Exception ex)
            //{
            //    context.Exception = ex;
            //    foreach (var interceptor in _interceptors)
            //    {
            //        interceptor.OnException(context);
            //    }
            //}


        }

        public int Calculate()
        {
            InvokeContext context = new InvokeContext("Calculate")
            {
                RealType = typeof(Sample),
                Parameters = new object[] { }
            };

            try
            {
                foreach (var interceptor in _interceptors)
                {
                    interceptor.BeginInvoke(context);
                }

                context.ResultValue = base.Calculate();

                //context.ResultValue = _sample.Calculate();
                //context.ResultValue = _sample.GetType().GetMethod("Calculate").Invoke(_sample, context.Parameters);


                foreach (var interceptor in _interceptors)
                {
                    interceptor.EndInvoke(context);
                }
            }
            catch (Exception ex)
            {
                context.Exception = ex;
                foreach (var interceptor in _interceptors)
                {
                    interceptor.OnException(context);
                }
            }

            return (int)context.ResultValue;
        }
    }

    public interface IInterface
    {
        void ShowMessage(string msg);
        int Calculate();
    }

    [UnitTest.LogAspect]
    public class Sample : IInterface
    {
        public virtual void ShowMessage(string msg)
        {
            Console.WriteLine("ehheh");
        }

        public virtual int Calculate()
        {
            return 2;
        }
    }
}
