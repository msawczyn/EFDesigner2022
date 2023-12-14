### Please note:

The files in this directory are used **ONLY** for editing, formatting, intellisense niceness, etc. 

Their only reason for existence is that, despite the best intent of Devart et al, there really isn't good 
support for the T4 editing experience in Visual Studio.

*Don't* compile these things, and don't include them in the VSIX.
To re-emphasize, they're only here as a convenience mechanism, and really only because [Resharper](https://www.jetbrains.com/resharper/) makes me look good,
and it needs a real C# file to give me its kind assistance.

Note, too, that the text of these files will be copied into the appropriate `.ttinclude`
during the build process. That means that all edits should be done here, and not in the `.ttinclude`. Changes
there will get overwritten.

