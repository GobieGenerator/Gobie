namespace ConsoleClient
{
    using Gobie;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class CustomAssemblyGeneratorAttribute : GobieAssemblyGeneratorAttribute
    {
        public string Name { get; }
        public string Template { get; }
        public CustomAssemblyGeneratorAttribute(string name, string template)
            => (Name, Template) = (name, template);
    }
}
