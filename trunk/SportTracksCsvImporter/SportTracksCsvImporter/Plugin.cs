/*
Copyright (c) 2010 Wolfgang Bruessler

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
*/

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
