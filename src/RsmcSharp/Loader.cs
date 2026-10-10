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
/// Configures how the native <c>rsmc</c> library is located and loaded.
/// </summary>
/// <remarks>
/// Settings must be applied before the first access to <see cref="Rsmc"/>, because the library is resolved once, on first use.
/// <para>
/// The library is searched in the following order:
/// the path set by <see cref="UseLibraryAt"/>; the path from the <c>RSMC_NATIVE_LIB_PATH</c> environment variable;
/// the optimized binaries in <c>runtimes/{rid}/native</c> (only if <see cref="UseNativeOptimization"/> is enabled);
/// the default system probing; and finally the regular binaries in <c>runtimes/{rid}/native</c>.
/// </para>
/// </remarks>
public static class RsmcNative
{
    /// <summary>
    /// Gets or sets whether optimized native binaries should be preferred.
    /// Disabled by default.
    /// </summary>
    /// <remarks>
    /// For v1.0.0 the optimized binaries target x86-64-v3 and are used only on x64 processors that support it;
    /// otherwise the regular binaries are used.
    /// </remarks>
    public static bool UseNativeOptimization
    {
        get => DetectOptimized;
        set => DetectOptimized = value;
    }

    /// <summary>
    /// Sets a custom path to the native rsmc library. This path has the highest priority.
    /// </summary>
    /// <param name="path">
    /// Path to the library file, or to a directory that contains it
    /// (the platform-specific file name is appended automatically).
    /// </param>
    public static void UseLibraryAt(string path)
    {
        SetLibraryPath(path);
    }
}