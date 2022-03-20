namespace Gobie.UnitTests;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class MustacheTests
{
    [Test]
    public void T()
    {
        const string template = @"{{#name}}
  <b>{{name}}</b>
{{/name}}
{{^name}}
  No name. 
{{/name}}";

        Mustache.Tokenize(template);
    }
}
