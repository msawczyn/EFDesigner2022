//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace EFCore5Parser {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "17.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("EFCore5Parser.Resources", typeof(Resources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;?xml version=&quot;1.0&quot; encoding=&quot;utf-8&quot; ?&gt;
        ///&lt;log4net&gt;
        ///   &lt;root&gt;
        ///      &lt;level value=&quot;ALL&quot; /&gt;
        ///      &lt;!--&lt;appender-ref ref=&quot;console&quot; /&gt;--&gt;
        ///      &lt;appender-ref ref=&quot;file&quot; /&gt;
        ///      &lt;appender-ref ref=&quot;OutputDebugStringAppender&quot; /&gt;
        ///   &lt;/root&gt;
        ///
        ///   &lt;appender name=&quot;console&quot; type=&quot;log4net.Appender.ManagedColoredConsoleAppender&quot;&gt;
        ///      &lt;mapping&gt;
        ///         &lt;level value=&quot;ERROR&quot; /&gt;
        ///         &lt;foreColor value=&quot;DarkRed&quot; /&gt;
        ///      &lt;/mapping&gt;
        ///      &lt;mapping&gt;
        ///         &lt;level value=&quot;WARN&quot; /&gt;
        ///         &lt;foreColor value=&quot; [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string Log4netConfig {
            get {
                return ResourceManager.GetString("Log4netConfig", resourceCulture);
            }
        }
    }
}
