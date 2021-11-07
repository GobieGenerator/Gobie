using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoNotify;

namespace GenGen.Client
{
    public partial class Person
    {
        [AutoNotify]
        private string name = string.Empty;
    }
}
