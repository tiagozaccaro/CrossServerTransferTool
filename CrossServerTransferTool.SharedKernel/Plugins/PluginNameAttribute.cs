using System;

namespace CrossServerTransferTool.SharedKernel.Plugins
{
    [AttributeUsage(AttributeTargets.Class)]
    public class PluginNameAttribute : Attribute
    {
        public string Name { get; }

        public PluginNameAttribute(string name)
        {
            Name = name;
        }
    }
}
