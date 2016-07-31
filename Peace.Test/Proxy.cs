using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Peace.AOP;

namespace Peace.Test
{

    public sealed class BirdProxy : UnitTest.Bird
    {
        private IInterceptor[] _interceptors = null;
        private UnitTest.Bird _realObj = null;

        public BirdProxy()
        {
            this._realObj = new UnitTest.Bird();
            object[] customAttributes = typeof(UnitTest.Bird).GetCustomAttributes(typeof(IInterceptor), false);
            this._interceptors = customAttributes.Cast<IInterceptor>().ToArray<IInterceptor>();
        }

        public override void Fly(string text1)
        {
            InvokeContext context2 = new InvokeContext("Fly")
            {
                RealType = typeof(UnitTest.Bird),
                Tag = this._realObj,
                Parameters = new object[] { text1 }
            };
            try
            {
                foreach (IInterceptor interceptor in this._interceptors)
                {
                    interceptor.BeginInvoke(context2);
                }
                this._realObj.Fly(text1);
            }
            catch (Exception exception)
            {
                context2.Exception = exception;
                foreach (IInterceptor interceptor2 in this._interceptors)
                {
                    interceptor2.OnException(context2);
                }
            }
        }

        public override int GetWingCount()
        {
            int num = new int();
            InvokeContext context2 = new InvokeContext("Sum")
            {
                RealType = typeof(UnitTest.Bird),
                Tag = this._realObj
            };
            try
            {
                foreach (IInterceptor interceptor in this._interceptors)
                {
                    interceptor.BeginInvoke(context2);
                }
                num = this._realObj.GetWingCount();
                context2.ResultValue = num;
            }
            catch (Exception exception)
            {
                context2.Exception = exception;
                foreach (IInterceptor interceptor2 in this._interceptors)
                {
                    interceptor2.OnException(context2);
                }
            }
            return num;
        }
    }



}
