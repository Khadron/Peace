using System;

namespace Peace.IoC.Setting
{
    public class InjectSetting
    {
        public Type Bind { get; set; }
        public Type To { get; set; }
        public Lifetime Scope { get; set; }
    }
}
