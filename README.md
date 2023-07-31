# Entity Framework Designer - VS2022 Edition

## Entity Framework visual design surface and code-first code generation for EF6, EFCore and beyond.

### For Visual Studio 2022

Model and generate code for both Entity Framework v6.x and Entity Framework Core 2 through 7.

**[Install with NuGet](https://docs.microsoft.com/en-us/visualstudio/ide/finding-and-using-visual-studio-extensions) from the [Visual Studio Marketplace](https://marketplace.visualstudio.com/items?itemName=michaelsawczyn.EFDesigner2022)** 

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

**Note:** This tool does not reverse engineer from the database (i.e., "database-first"). There are other, quite well-maintained 
open-sourced projects that provide that functionality. Once you've created an assembly from a reverse-engineered database,
you can drop it onto an empty design surface and create a model from that, then evolve your code using that model.

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

[4.2.5](https://github.com/msawczyn/EFDesigner2022/releases/download/v4.2.5/Sawczyn.EFDesigner.EFModel.DslPackage.vsix)
   - Adds missing XML documentation for public and protected members
   - Fixes "No 'Is Identity' setting when adding a new entity property (version 4.2.44)" (see https://github.com/msawczyn/EFDesigner2022/issues/63)
   - Fixes "Bad intersection table generation when M:M tables have the same primary key name" (see https://github.com/msawczyn/EFDesigner2022/issues/64)
   - Fixes "Table names are pluralized even when flag set to false" (see https://github.com/msawczyn/EFDesigner2022/issues/65)

[4.2.4](https://github.com/msawczyn/EFDesigner2022/releases/download/v4.2.4/Sawczyn.EFDesigner.EFModel.DslPackage.vsix)
   - Reduced model load time by 98%
   - Exposed "Collapse Selected Elements" context menu choice for diagrams
   - Fixed colors in saved diagram images (see https://github.com/msawczyn/EFDesigner2022/issues/62)

[4.2.3](https://github.com/msawczyn/EFDesigner2022/releases/download/v4.2.3/Sawczyn.EFDesigner.EFModel.DslPackage.vsix)
   - Added HierarchyId as a property type
   - Added optional index name for indexed properties
   - Added properties to bidirectional associations allowing custom naming of FK columns in join table
   - New example code for EF6 and EFCore. Thanks to [David V](https://github.com/Opzet) for the pull request.
   - Class, property, enum and enum value names are now escaped if they match a reserved C# keyword
   - Ensured that a foreign key property's IsForeignKeyFor value is reset when the association is removed and that property is an Id property. (see https://github.com/msawczyn/EFDesigner2022/issues/47)
   - Replaced legacy EF6Designer.ttinclude and EFCoreDesigner.ttinclude files for legacy model file backward compatability. (see https://github.com/msawczyn/EFDesigner2022/issues/45)
   - Assembly import is a bit smarter now in detecting and using indexes and views
   - New glyphs in diagram and explorer to show transient and view entities
   - Fix bad constructor generation when multiple associations exist between the same classes (see https://github.com/msawczyn/EFDesigner2022/issues/50)
   - Removed modeling restriction on unidirectional many-to-many properties in EFCore7+ projects (see https://github.com/msawczyn/EFDesigner2022/issues/54)
   - Updated file sync logic for generated files. Thanks to [Sancho-Lee](https://github.com/Sancho-Lee) for the pull request. (see https://github.com/msawczyn/EFDesigner2022/issues/57)
   - Fix bad code re: key fields in derived types. (see https://github.com/msawczyn/EFDesigner2022/issues/55)

[4.2.1](https://github.com/msawczyn/EFDesigner2022/releases/download/v4.2.1/Sawczyn.EFDesigner.EFModel.DslPackage.vsix)
   - Added support for EFCore7
   - Added ability to import EFCore7 assemblies
   - Added ability to use property types not available in the select list, along with a global option enabling this feature in Tools/Options/Entity Framework Visual Editor (see https://github.com/msawczyn/EFDesigner2022/issues/28)
   - Added support for DateOnly and TimeOnly types in EFCore 6+
   - Added ability to specify decimal precision in EFCore5+ projects
   - Fix for View->Other Windows->Entity Model Explorer not working (see https://github.com/msawczyn/EFDesigner2022/issues/29)
   - Fix error in code generation template for EFCore5+ projects

[4.2.0](https://github.com/msawczyn/EFDesigner2022/releases/download/4.2.0/Sawczyn.EFDesigner.EFModel.DslPackage.vsix)
   - Added ability to import EFCore6 assemblies
   - Added database default values to properties (see https://github.com/msawczyn/EFDesigner2022/issues/15)
   - Extension now obeys Visual Studio theme colors (see https://github.com/msawczyn/EFDesigner2022/issues/9)
   - Added Persistent property to ModelClass to generate [NotMapped] when false.  (see https://github.com/msawczyn/EFDesigner2022/issues/17)
   - Entity constructors with required navigation properties are now generated with either the required navigation target or its foreign key property, if available (see https://github.com/msawczyn/EFDesigner2022/issues/21)
   - Fixed other Entity Framework assembly imports that broke with VS2022 moving to 64-bit (see https://github.com/msawczyn/EFDesigner2022/issues/14)
   - Fixed problem with generated Timestamp concurrency check (see https://github.com/msawczyn/EFDesigner2022/issues/20)

[4.1.2](https://github.com/msawczyn/EFDesigner2022/releases/download/v4.1.2/Sawczyn.EFDesigner.EFModel.DslPackage.vsix)
   - Added ability to create association classes via drag and drop of an entity onto a bidirectional many-to-many association
   - Restored ability to open secondary diagrams
   - Compiler update to Visual Studio v17.1.0 fixes missing designer menu items

4.0.1
   - Added validations preventing use of temporal tables in unsupported scenarios

[4.0.0](https://github.com/msawczyn/EFDesigner2022/releases/download/v4.0.0/Sawczyn.EFDesigner.EFModel.DslPackage.vsix)
   - VS2022 extension
   - Added support for EFCore6
   - Added support for temporal tables

Earlier changes at [the VS2019 version](https://github.com/msawczyn/EFDesigner).
