using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;

namespace CTGP_Installer_.Properties
{
  [GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
  [DebuggerNonUserCode]
  [CompilerGenerated]
  internal class Resources
  {
    private static ResourceManager resourceMan;
    private static CultureInfo resourceCulture;

    internal Resources()
    {
    }

    [EditorBrowsable(EditorBrowsableState.Advanced)]
    internal static ResourceManager ResourceManager
    {
      get
      {
        if (CTGP_Installer_.Properties.Resources.resourceMan == null)
          CTGP_Installer_.Properties.Resources.resourceMan = new ResourceManager("CTGP_Installer_.Properties.Resources", typeof (CTGP_Installer_.Properties.Resources).Assembly);
        return CTGP_Installer_.Properties.Resources.resourceMan;
      }
    }

    [EditorBrowsable(EditorBrowsableState.Advanced)]
    internal static CultureInfo Culture
    {
      get => CTGP_Installer_.Properties.Resources.resourceCulture;
      set => CTGP_Installer_.Properties.Resources.resourceCulture = value;
    }
  }
}
