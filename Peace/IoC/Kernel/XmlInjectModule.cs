using System;
using System.Configuration;
using System.IO;
using Peace.Configuration;

namespace Peace.IoC.Kernel
{
    public class XmlInjectModule
    {
        private const string ConfigFileName = "Peace.config";
        private const string PeaceSectionGroupName = "Peace";

        private string _configFilePath;

        public XmlInjectModule()
            : this("")
        {
        }

        public XmlInjectModule(string configFilePath)
        {
            _configFilePath = configFilePath;
        }

        public void Load(IKernel kernel)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(_configFilePath))
                {
                    if (ExistConfigFile())
                    {
                        _configFilePath = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;
                    }
                    else
                    {
                        _configFilePath = String.Format("{0}\\{1}", AppDomain.CurrentDomain.BaseDirectory.TrimEnd('/', '\\'),
                          ConfigFileName);
                        if (!File.Exists(_configFilePath))
                        {
                            throw new Exception("为找到配置文件");
                        }

                        _configFilePath = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;
                    }
                }

                ExeConfigurationFileMap exeConfigMap = new ExeConfigurationFileMap { ExeConfigFilename = _configFilePath };
                System.Configuration.Configuration config = ConfigurationManager.OpenMappedExeConfiguration(exeConfigMap, ConfigurationUserLevel.None);

                PeaceConfigurationSectionGroup group = config.GetSectionGroup(PeaceSectionGroupName) as PeaceConfigurationSectionGroup;

                if (group != null)
                {
                    var binds = group.IoC.Binds;

                    foreach (Bind bind in binds)
                    {
                        Type sourceType = Type.GetType(bind.Source);
                        Type targetType = Type.GetType(bind.Target);
                        Lifetime lifetime = Lifetime.Singleton; //bind.Lifetime;

                        var syntax = kernel.Bind(sourceType).To(targetType);
                        switch (lifetime)
                        {
                            case Lifetime.Singleton:
                                syntax.InSingletonScope();
                                break;
                            case Lifetime.Transient:
                                syntax.InTransientScope();
                                break;
                            default:
                                syntax.InSingletonScope();
                                break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("加载Peace配置文件失败:" + ex.Message);
            }
        }

        public static bool ExistConfigFile()
        {
            return File.Exists(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile);
        }
    }
}
