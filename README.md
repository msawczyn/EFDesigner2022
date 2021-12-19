# Entity Framework Designer - VS2022 Edition

## Entity Framework visual design surface and code-first code generation for EF6, EFCore and beyond.

### For Visual Studio 2022

Model and generate code for both Entity Framework v6.x and Entity Framework Core 2 through 6.

**[Install with NuGet](https://docs.microsoft.com/en-us/visualstudio/ide/finding-and-using-visual-studio-extensions) from the [Visual Studio Marketplace](https://marketplace.visualstudio.com/items?itemName=michaelsawczyn.EFDesigner2022)** (currently available as a pre-release extension)

**Complete documentation in the [project's documentation site](https://msawczyn.github.io/EFDesigner/)**

<table><tbody><tr><td>
<img src="https://msawczyn.github.io/EFDesigner/images/Designer.jpg">
</td></tr></tbody></table>

This Visual Studio 2022 extension is the easiest way to add a consistently correct Entity Framework (EF6 or EFCore) model to your project. 

It's an opinionated code generator, adding a new file type (.efmodel) that allows for fast, easy and, most importantly, **visual** design 
of persistent classes. Inheritance, unidirectional and bidirectional associations are all supported. Enumerations are also included in 
the visual model, as is the ability to add text blocks to explain potentially arcane parts of your design.

While giving you complete control over how the code is generated you'll be able to create, out of the box, sophisticated, 
consistent and **correct** Entity Framework code that can be regenerated when your model changes. And, since the code is written using 
partial classes, any additions you make to your generated code are retained across subsequent generations.
The designer doesn't need to be present to use the code that's generated - it generates standard C#, using the code-first, fluent API - so the tool doesn't
become a dependency to your project.

If you are used to the EF visual modeling that comes with Visual Studio, you'll be pretty much at home.
The goal was to duplicate at least those features and, in addition, 
add all the little things that *should* have been there. Things like: 
*   importing entities from C# source, or existing DbContext definitions (including their entities) from compiled EF6 or EFCore assemblies
*   multiple views of your model to highlight important aspects of your design
*   the ability to show and hide parts of the model
*   easy customization of generated output by editing or even replacing the T4 templates
*   entities by default generated as partial classes so the generated code can be easily extended
*   class and enumeration nodes that can be colored to visually group the model
*   different concerns being generated into different subdirectories (entities, enums, dbcontext)
*   string length, index flags, required attributes and other properties being available in the designer

and many other nice-to-have bits.

Code generation is completely customizable via T4 templates. The tool installs templates that 
target both EF6 and EFCore, and generate both a code-first DbContext class and 
POCO entity classes. The DbContext code is written to allow consumption in 
ASP.Net Core in addition to any other project type, so you'll have flexibility in your development.

**Note:** This tool does not reverse engineer from the database (i.e., "database-first"). Microsoft has provided
tools for that, and there are other, well-maintained opensourced projects that provide that functionality as
well. 

You can read more about how to use the designer in the [Documentation site](https://msawczyn.github.io/EFDesigner/).

**Migrating from VS2019**
You'll have to change a line in your model's <modelname>.tt file to accommodate how Visual Studio 2022 handles library imports in text templates.

Change line 5, which reads
```
#><#@ assembly name="EnvDTE"
```
to be
```
#><#@ assembly name="Microsoft.VisualStudio.Interop"

```
That's it. Of course, if you customized any of the standard templates, you'll have to go through the customizations the same way you've always done and 
bump your changes up against the new templates. Not much has changed that didn't absolutely _need_ to be changed due to the VS2022 differences, but there
may be a bugfix or two your customized templates don't have. But, hey ... if you've been customizing the templates, you know the drill, right?

### Shout out and a big thanks
<!-- <table border="0" cellspacing="0" cellpadding="0" style="border:none">
<tr vstyle="center" style="border:none"><td>to</td><td><a href="https://www.jetbrains.com/?from=EFDesigner"><img src="https://msawczyn.github.io/EFDesigner/images/jetbrains-variant-2a.png"></a></td><td>for providing free development tools to support this project.</td></tr>
</table> -->
to <a href="https://www.jetbrains.com/?from=EFDesigner"><img src="https://msawczyn.github.io/EFDesigner/images/jetbrains-variant-2a.png"></a> for providing free development tools to support this project.

### Change Log

4.0.0
   - VS2022 extension
   - Added support for EFCore6
   - Added support for temporal tables

Earlier changes at [the VS2019 version](https://github.com/msawczyn/EFDesigner).
