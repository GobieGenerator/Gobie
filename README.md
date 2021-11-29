# Gobie


[GobieFieldTemplate] should impact fields to which it is specifically applied OR if applied to a class then it will apply to anything where a type filter applies and myabe a type exclude does not match.
[GobieClassTemplate] would apply to the the class only
[GobieAssemblyAttribute] this is probaby where 'global' templates should be defined.


* Implementing partial methods.


* `[Template]` - This would get inserted into the generated partial class.
  * Exposing a field as a property and or adding methods.
* `[ClassTemplate]` - This would let you make a whole new class or really any extra file. OR it would let you make a partial for the current class, but implementing an interface or whatever.
  * Making extra classes.
* `[GlobalTemplate]` - Somehting we always generate for the assembly (going to have to test muti assembly?). Like blazor, we assume there is just one spot we can put stuff within the template.
  * `[GlobalTemplateChild(TemplateName = "Something", TagName = "" defaults to "ChildTemplate")]`. Child templates get written in a deterministic order which cannot be controlled by the user.
  * Boostrapping the EFCore stuff
  * Setting up extension methods
* Method wrapper. 
  * If you wanted to time something, maybe log args, maybe check nulls. We could use paramater attributes to let the user define specific templates per parameter. Or maybe you could apply an attribute to a method and it had templates which each have a method filter? 

Type filtering :https://stackoverflow.com/questions/42430974/typeinfo-isassignablefrom-in-roslyn-analyzer

## Field

Dictionary
* Namespace
* Class
* FullClass = Namespace.Class
* field
* Property (overridable)
* PluralProperty (overridable)
* SingularProperty (overridable)
* All the named parameters




``` csharp

    [Template]
```