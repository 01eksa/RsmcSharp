using System.Reflection;
using System.Runtime.InteropServices;

namespace RsmcSharp;

internal static class NativeLoader
{
    public const string LibraryName = "rsmc";
    private static string? _customPath;
    private static bool _registered;

    public static void SetLibraryPath(string path)
    {
        _customPath = Directory.Exists(path)
            ? Path.Combine(path, GetPlatformFileName())
            : path;
    }

    internal static void EnsureRegistered()
    {
        if (_registered) return;
        NativeLibrary.SetDllImportResolver(typeof(NativeLoader).Assembly, Resolve);
        _registered = true;
    }

    private static IntPtr Resolve(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
    {
        if (libraryName != LibraryName)
            return IntPtr.Zero;

        if (!string.IsNullOrEmpty(_customPath) && NativeLibrary.TryLoad(_customPath, out var handle))
            return handle;

        var envPath = Environment.GetEnvironmentVariable("RSMC_NATIVE_LIB_PATH");
        if (!string.IsNullOrEmpty(envPath) && NativeLibrary.TryLoad(envPath, out handle))
            return handle;

        if (NativeLibrary.TryLoad(LibraryName, assembly, searchPath, out handle))
            return handle;

        foreach (var candidate in GetFallbackCandidates())
        {
            if (File.Exists(candidate) && NativeLibrary.TryLoad(candidate, out handle))
                return handle;
        }

        return IntPtr.Zero;
    }

    private static IEnumerable<string> GetFallbackCandidates()
    {
        var rid = GetRid();
        var fileName = GetPlatformFileName();

        yield return Path.Combine(AppContext.BaseDirectory, "runtimes", rid, "native", fileName);

        var asmDir = Path.GetDirectoryName(typeof(NativeLoader).Assembly.Location);
        if (asmDir is not null)
            yield return Path.Combine(asmDir, "runtimes", rid, "native", fileName);
    }

    private static string GetPlatformFileName() =>
        RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? $"{LibraryName}.dll" :
        RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? $"lib{LibraryName}.dylib" :
        $"lib{LibraryName}.so";

    private static string GetRid()
    {
        var arch = RuntimeInformation.ProcessArchitecture switch
        {
            Architecture.X64 => "x64",
            Architecture.Arm64 => "arm64",
            var other => throw new PlatformNotSupportedException($"Unsupported architecture: {other}")
        };

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return $"win-{arch}";
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) return $"osx-{arch}";
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) return $"linux-{arch}";

        throw new PlatformNotSupportedException("Unsupported OS platform.");
    }
}

/// <summary>
/// Manage rsmc native library.
/// </summary>
public static class RsmcNative
{
    
    /// <summary>
    /// Sets custom path to the rsmc native library.
    /// </summary>
    /// <param name="path">Path to the library file.</param>
    public static void UseLibraryAt(string path)
    {
        NativeLoader.SetLibraryPath(path);
    }
}