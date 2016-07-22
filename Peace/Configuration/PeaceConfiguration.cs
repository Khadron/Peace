using System;
using System.Configuration;
using Peace.IoC;

namespace Peace.Configuration
{
    public class PeaceConfigurationSectionGroup : ConfigurationSectionGroup
    {
        public IoC IoC
        {
            get { return (IoC)base.Sections["IoC"]; }
        }
    }

    public class IoC : ConfigurationSection
    {
        [ConfigurationProperty("Binds", IsDefaultCollection = false)]
        [ConfigurationCollection(typeof(Bind), CollectionType = ConfigurationElementCollectionType.AddRemoveClearMap, AddItemName = "Bind", RemoveItemName = "remove")]
        public Binds Binds
        {
            get { return (Binds)base["Binds"]; }
        }
    }

    public class Binds : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new Bind();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((Bind)element).Name;
        }

        public Bind this[int i]
        {
            get
            {
                return (Bind)base.BaseGet(i);
            }
        }

    }

    public class Bind : ConfigurationElement
    {
        [ConfigurationProperty("name", IsRequired = true, IsKey = true)]
        public string Name
        {
            get { return base["name"].ToString(); }
        }

        [ConfigurationProperty("Ikey", IsRequired = true)]
        public string Source
        {
            get { return base["Ikey"].ToString(); }
        }

        [ConfigurationProperty("value", IsRequired = true)]
        public string Target
        {
            get { return base["value"].ToString(); }
        }

        [ConfigurationProperty("lifetime")]
        public Lifetime Lifetime
        {
            get
            {
                Lifetime lifetime;
                if (TryParseifetime(base["lifetime"].ToString(), out lifetime)) return lifetime;

                throw new Exception();
            }
            set
            {
                Lifetime lifetime;
                if (!TryParseifetime(value.ToString(), out lifetime)) throw new Exception();

                base["lifetime"] = lifetime.ToString();
            }
        }
        private bool TryParseifetime(string value, out Lifetime lifetime)
        {
            return Enum.TryParse(value, true, out lifetime);
        }
    }
}

#region Solution 1
//public class PeaceConfigurationSectionGroup : ConfigurationSectionGroup
//{
//    [ConfigurationProperty("IocContainers", IsRequired = true, IsDefaultCollection = false)]
//    [ConfigurationCollection(typeof(Container), CollectionType = ConfigurationElementCollectionType.AddRemoveClearMap, RemoveItemName = "remove")]
//    public IocContainers IocContainers
//    {
//        get { return (IocContainers)base.Sections["IocContainers"]; }

//    }
//}

//public class IocContainers : ConfigurationSection
//{
//    public Container Container
//    {
//        get
//        {
//            return ??
//        }
//    }
//}

//public class Container : ConfigurationElementCollection
//{
//    [ConfigurationProperty("name", IsRequired = true)]
//    public string Name
//    {
//        get { return base["name"].ToString(); }
//        set { base["name"] = value; }
//    }

//    protected override ConfigurationElement CreateNewElement()
//    {
//        return new Register();
//    }

//    protected override object GetElementKey(ConfigurationElement element)
//    {
//        return ((Register)element).Name;
//    }

//    public Register this[int i]
//    {
//        get { return (Register)base.BaseGet(i); }
//    }

//    public Register this[string key]
//    {
//        get { return (Register)base.BaseGet(key); }
//    }
//}

//public class Register : ConfigurationElement
//{
//    [ConfigurationProperty("name", IsRequired = true)]
//    public string Name
//    {
//        get { return base["name"].ToString(); }
//        set { base["name"] = value; }
//    }

//    [ConfigurationProperty("source", IsRequired = true)]
//    public string Source
//    {
//        get { return base["source"].ToString(); }
//        set { base["source"] = value; }
//    }

//    [ConfigurationProperty("target", IsRequired = true)]
//    public string Target
//    {
//        get { return base["target"].ToString(); }
//        set { base["target"] = value; }
//    }

//    public Lifetime Lifetime
//    {
//        get
//        {
//            Lifetime lifetime;
//            if (TryParseifetime(base["lifetime"].ToString(), out lifetime)) return lifetime;

//            throw new Exception();
//        }
//        set
//        {
//            Lifetime lifetime;
//            if (!TryParseifetime(value.ToString(), out lifetime)) throw new Exception();

//            base["lifetime"] = lifetime;
//        }
//    }


#endregion

