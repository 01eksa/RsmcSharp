namespace RsmcSharp;

/// <summary>
/// Base class for errors in the RsmcSharp library.
/// </summary>
public class RsmcException(string message, Exception? inner = null)
    : Exception(message, inner);

/// <summary>
/// Thrown when accessing coordinates that are outside the <see cref="Board"/>.
/// </summary>
public class InvalidCoordsException(string message = "", Exception? inner = null)
    : RsmcException(message, inner);

/// <summary>
/// Thrown when an impossible move is attempted.
/// </summary>
public class InvalidMoveException(string message = "", Exception? inner = null)
    : RsmcException(message, inner);

/// <summary>
/// Thrown when the version of the native library is not supported.
/// </summary>
public class UnsupportedVersionException(string message = "", Exception? inner = null)
    : RsmcException(message, inner);