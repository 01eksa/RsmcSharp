using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.X86;
using static RsmcSharp.NativeLoader;

namespace RsmcSharp;

internal static class NativeLoader
{
    public const string LibraryName = "rsmc";
    private static string? _customPath;
    private static bool _registered;
    public static bool DetectOptimized { get; set; } = false;

    private static readonly Lock NativeLoaderLock = new();

    public static void SetLibraryPath(string path)
    {
        _customPath = Directory.Exists(path)
            ? Path.Combine(path, GetPlatformFileName())
            : path;
    }

    internal static void EnsureRegistered()
    {
        lock(NativeLoaderLock)
        {
            if (_registered) return;
            NativeLibrary.SetDllImportResolver(typeof(NativeLoader).Assembly, Resolve);
            _registered = true;
        }
    }

    private static IntPtr Resolve(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
    {
        if (libraryName != LibraryName)
            return IntPtr.Zero;

        IntPtr handle;

        if (!string.IsNullOrEmpty(_customPath) && NativeLibrary.TryLoad(_customPath, out handle))
            return handle;

        var envPath = Environment.GetEnvironmentVariable("RSMC_NATIVE_LIB_PATH");
        if (!string.IsNullOrEmpty(envPath) && NativeLibrary.TryLoad(envPath, out handle))
            return handle;

        if (DetectOptimized)
        {
            foreach (var candidate in GetFallbackCandidates(DetectOptimized))
            {
                if (File.Exists(candidate) && NativeLibrary.TryLoad(candidate, out handle))
                    return handle;
            }
        }

        if (NativeLibrary.TryLoad(LibraryName, assembly, searchPath, out handle))
            return handle;

        foreach (var candidate in GetFallbackCandidates(false))
        {
            if (File.Exists(candidate) && NativeLibrary.TryLoad(candidate, out handle))
                return handle;
        }

        return IntPtr.Zero;
    }

    private static IEnumerable<string> GetFallbackCandidates(bool detectOptimized)
    {
        var rid = GetRid(detectOptimized);
        var fileName = GetPlatformFileName();

        yield return Path.Combine(AppContext.BaseDirectory, "runtimes", rid, "native", fileName);

        var asmDir = Path.GetDirectoryName(typeof(NativeLoader).Assembly.Location);
        if (asmDir is not null) yield return Path.Combine(asmDir, "runtimes", rid, "native", fileName);
    }

    private static string GetPlatformFileName() =>
        RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? $"{LibraryName}.dll" :
        RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? $"lib{LibraryName}.dylib" :
        $"lib{LibraryName}.so";

    private static string GetRid(bool detectOptimized)
    {
        var arch = RuntimeInformation.ProcessArchitecture switch
        {
            Architecture.X64 => "x64",
            Architecture.Arm64 => "arm64",
            var other => throw new PlatformNotSupportedException($"Unsupported architecture: {other}")
        };

        if (detectOptimized && arch == "x64" && CheckX64V3Support())
            arch = "x64-v3";

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return $"win-{arch}";
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) return $"osx-{arch}";
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) return $"linux-{arch}";

        throw new PlatformNotSupportedException("Unsupported OS platform.");
    }

    private static bool CheckX64V3Support()
    {
        return Avx2.IsSupported && Avx.IsSupported && Bmi2.IsSupported && Bmi1.IsSupported && Lzcnt.IsSupported
               && Fma.IsSupported && Popcnt.IsSupported && Sse42.IsSupported && Sse41.IsSupported && Sse3.IsSupported &&
               Ssse3.IsSupported;
    }
}

/// <summary>
/// Manage rsmc native library.
/// </summary>
public static class RsmcNative
{
    /// <summary>
    /// Decide if you want to use native optimization.
    /// For v1.0.0 it uses x86-64-v3 binaries if your hardware supports it.
    /// </summary>
    public static bool UseNativeOptimization
    {
        get => DetectOptimized;
        set => DetectOptimized = value;
    }

    /// <summary>
    /// Sets custom path to the rsmc native library.
    /// </summary>
    /// <param name="path">Path to the library file.</param>
    public static void UseLibraryAt(string path)
    {
        SetLibraryPath(path);
    }
}