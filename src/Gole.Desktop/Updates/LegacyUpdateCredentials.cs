using System.IO;

namespace Gole.Desktop.Updates;

internal static class LegacyUpdateCredentials
{
    // Older installations requested a token. Public updates no longer read or retain it.
    internal static void Remove(string dataFolder)
    {
        foreach (string name in new[] { "updates-auth.dat", "updates-auth.dat.tmp" })
        {
            try
            {
                File.Delete(Path.Combine(dataFolder, name));
            }
            catch (IOException) { }
            catch (System.UnauthorizedAccessException) { }
        }
    }
}
