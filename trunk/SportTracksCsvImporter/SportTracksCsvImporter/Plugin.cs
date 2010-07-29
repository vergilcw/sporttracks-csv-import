using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.IO;

using ZoneFiveSoftware.Common.Visuals.Fitness;


namespace WbSportTracksCsvImporter
{
    class Plugin : IPlugin
    {
        #region IPlugin Members

        public Plugin()
        {
            try
            {
                string oldPluginFullPath = Directory.GetCurrentDirectory() + "\\Plugins\\WbSportTracksCsvImporter.dll";
                if (File.Exists(oldPluginFullPath))
                {
                    System.Reflection.Module mod = System.Reflection.Assembly.GetExecutingAssembly().GetModules()[0];
                    if (mod.FullyQualifiedName != oldPluginFullPath)
                    {
                        try
                        {
                            File.Delete(oldPluginFullPath);
                        }
                        catch (Exception)
                        {
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        public IApplication Application
        {
            set
            {
                application = value;
            }
        }

        public Guid Id
        {
            get
            {
                return new Guid("{360BC71A-F3B5-4252-8A95-574E5E649532}");
            }
        }

        public string Name
        {
            get
            {
                return "WB CSV Importer";
            }
        }

        public void ReadOptions(XmlDocument xmlDoc, XmlNamespaceManager nsmgr, XmlElement pluginNode)
        {
        }

        public string Version
        {
            get
            {
                return GetType().Assembly.GetName().Version.ToString(3);
            }
        }

        public void WriteOptions(XmlDocument xmlDoc, XmlElement pluginNode)
        {
        }

        #endregion

        public static IApplication GetApplication()
        {
            return application;
        }

        public static string GetName()
        {
            return "WB CSV Importer";
        }

        public static Guid GetPluginId()
        {
            return new Guid("{360BC71A-F3B5-4252-8A95-574E5E649532}");
        }

        #region Private members
        private static IApplication application;
        #endregion
    }
}
