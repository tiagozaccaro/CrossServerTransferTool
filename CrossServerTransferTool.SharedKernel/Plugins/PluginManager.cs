using CrossServerTransferTool.SharedKernel.Parsers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;

namespace CrossServerTransferTool.SharedKernel.Plugins
{
    public class PluginManager
    {
        private List<Type> _plugins;

        public PluginManager()
        {
            AppDomain.CurrentDomain.AssemblyResolve += (sender, e) => {
                var pluginName = e.RequestingAssembly.GetName().Name;

                // Extract dependency name from the full assembly name:
                // PluginTest.HalloWorldHelper, Version = 1.0.0.0, Culture = neutral, PublicKeyToken = null
                var pluginDependencyName = e.Name.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).First();

                var pluginDependencyFullName = Directory.EnumerateFiles(Directory.GetCurrentDirectory(), $"{pluginDependencyName}.dll", SearchOption.AllDirectories).FirstOrDefault();

                return
                    File.Exists(pluginDependencyFullName)
                        ? Assembly.LoadFile(pluginDependencyFullName)
                        : null;
            };
        }
        
        public void LoadPlugins()
        {
            var availableTypes = new List<Type>();
            GetPluginAssemblies().ForEach(p => availableTypes.AddRange(p.GetTypes()));              
            
            _plugins = availableTypes.FindAll(delegate(Type t) {
                List<Type> interfaceTypes = new List<Type>(t.GetInterfaces());
                object[] arr = t.GetCustomAttributes(typeof(PluginNameAttribute), true);

                return !(arr == null || arr.Length == 0) && interfaceTypes.Contains(typeof(IScriptParser));
            });
        }

        public List<String> ListPlugins()
        {
            return _plugins.Select(p => p.GetCustomAttribute<PluginNameAttribute>().Name).ToList();
        }

        public Type GetPluginType(string name)
        {
            return _plugins.Where(p => p.GetCustomAttribute<PluginNameAttribute>().Name.Equals(name)).FirstOrDefault();         
        }

        private List<Assembly> GetPluginAssemblies()
        {
            return Directory.EnumerateFiles(Directory.GetCurrentDirectory(), "*Converter.dll", SearchOption.AllDirectories)                
                .Select(AssemblyLoadContext.Default.LoadFromAssemblyPath)
                .ToList();
        }
    }
}
