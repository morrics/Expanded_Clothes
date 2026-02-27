using System.Linq;
using System.Reflection;

public static class BuildInfo
{
    public static string Commit
    {
        get
        {
            var asm = typeof(BuildInfo).Assembly;

            var a = asm.GetCustomAttributes(typeof(AssemblyInformationalVersionAttribute), false)
                       .OfType<AssemblyInformationalVersionAttribute>()
                       .FirstOrDefault();

            if (a == null) return "UNKNOWN";

            var v = a.InformationalVersion;

            if (v == null || v.Trim() == "")
                return "UNKNOWN";

            return v.Trim();
        }
    }
}