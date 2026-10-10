namespace RsmcSharp;

/// <summary>
/// Represents errors in Rsmcsharp library.
/// </summary>
public class RsmcException(string message, Exception? inner = null)
    : Exception(message, inner);

/// <summary>
/// Throws when you try to access invalid coords on the <see cref="Board"/>.
/// </summary>
public class InvalidCoordsException(string message = "", Exception? inner = null)
    : RsmcException(message, inner);

/// <summary>
/// Throws when you try to make an impossible move.
/// </summary>
public class InvalidMoveException(string message = "", Exception? inner = null)
    : RsmcException(message, inner);
    
/// <summary>
/// Throws when native library version is not supported
/// </summary>
public class UnsupportedVersionException(string message = "", Exception? inner = null)
    : RsmcException(message, inner);