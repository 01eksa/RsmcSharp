using System.Runtime.InteropServices;

namespace RsmcSharp;

/// <summary>
/// Provides a managed API wrapper and P/Invoke bindings for interacting with the native <c>rsmc</c> library.
/// </summary>
/// <remarks>
/// The native library is loaded and its version is checked on the first access to this class.
/// If the check fails, an <see cref="UnsupportedVersionException"/> is thrown from the static constructor,
/// so callers observe it as the <see cref="Exception.InnerException"/> of a <see cref="TypeInitializationException"/>.
/// Configure the library location with <see cref="RsmcNative"/> before the first access.
/// </remarks>
public static partial class Rsmc
{
    static Rsmc()
    {
        Version version;
        NativeLoader.EnsureRegistered();
        try
        {
            version = RsmcVersion;
        }
        catch (EntryPointNotFoundException)
        {
            throw new UnsupportedVersionException();
        }

        if (version < MinimalVersion)
        {
            throw new UnsupportedVersionException($"Version {version} is too low. At least {MinimalVersion} expected.");
        }

        if (version.Major > MinimalVersion.Major)
        {
            throw new UnsupportedVersionException($"Major version {version} is too high. v{version.Major}.X.X expected.");
        }

        StartPosition = rsmc_get_start_position();
    }

    /// <summary>
    /// Cached initial position of Reversi.
    /// </summary>
    public static readonly Board StartPosition;
    /// <summary>
    /// Minimal compatible version of the native rsmc library.
    /// </summary>
    /// <remarks>
    /// The native library is accepted if its version is not lower than this value
    /// and its major version is equal to the major version of this value.
    /// </remarks>
    public static readonly Version MinimalVersion = new(1, 0, 0);

    /// <summary>
    /// Gets the version of the native rsmc library.
    /// </summary>
    /// <remarks>
    /// The version is decoded from the <see cref="uint"/> returned by the native <c>rsmc_version</c> function.
    /// </remarks>
    public static Version RsmcVersion
    {
        get
        {
            return new Version(rsmc_version());
        }
    }

    private const string LibName = NativeLoader.LibraryName;

    [LibraryImport(LibName)]
    private static partial uint rsmc_version();


    [LibraryImport(LibName)]
    private static partial Board rsmc_get_start_position();

    [LibraryImport(LibName)]
    private static partial Board rsmc_apply_move(Board board, BitMask move, Player player);

    [LibraryImport(LibName)]
    private static partial PlayersScore rsmc_get_players_score(Board board);

    [LibraryImport(LibName)]
    [return: MarshalAs(UnmanagedType.U1)]
    private static partial bool rsmc_is_move_valid(Board board, BitMask move, Player player);

    [LibraryImport(LibName)]
    private static partial BitMap rsmc_get_valid_moves(Board board, Player player);

    [LibraryImport(LibName)]
    private static partial GameState rsmc_get_game_state(Board board);

    /// <summary>
    /// Gets the initial position of Reversi.
    /// </summary>
    /// <remarks>
    /// Returns the cached <see cref="StartPosition"/> value.
    /// </remarks>
    /// <returns>A <see cref="Board"/> with the standard starting position.</returns>
    public static Board GetStartPosition()
    {
        return StartPosition;
    }

    /// <summary>
    /// Applies a move to the board for the specified player.
    /// </summary>
    /// <remarks>
    /// This wrapper does not validate the move. Call <see cref="IsMoveValid"/> first;
    /// the result for an invalid move is determined by the native library.
    /// </remarks>
    /// <param name="board">The <see cref="Board"/> on which to apply the move.</param>
    /// <param name="move">The <see cref="BitMask"/> of the cell to place a disc on.</param>
    /// <param name="player">The <see cref="Player"/> making the move.</param>
    /// <returns>A new <see cref="Board"/> with the move applied. The original board is not modified.</returns>
    public static Board ApplyMove(Board board, BitMask move, Player player)
    {
        return rsmc_apply_move(board, move, player);
    }

    /// <summary>
    /// Calculates the current score of both players.
    /// </summary>
    /// <param name="board">The <see cref="Board"/> to evaluate.</param>
    /// <returns>A <see cref="PlayersScore"/> containing the score of each player.</returns>
    public static PlayersScore GetPlayersScore(Board board)
    {
        return rsmc_get_players_score(board);
    }

    /// <summary>
    /// Determines whether a proposed move is valid for the specified player.
    /// </summary>
    /// <param name="board">The <see cref="Board"/> to check.</param>
    /// <param name="move">The <see cref="BitMask"/> of the cell of the proposed move.</param>
    /// <param name="player">The <see cref="Player"/> attempting the move.</param>
    /// <returns><c>true</c> if the move is valid; otherwise, <c>false</c>.</returns>
    public static bool IsMoveValid(Board board, BitMask move, Player player)
    {
        return rsmc_is_move_valid(board, move, player);
    }

    /// <summary>
    /// Gets all valid moves available for the specified player.
    /// </summary>
    /// <param name="board">The <see cref="Board"/> to analyze.</param>
    /// <param name="player">The <see cref="Player"/> whose valid moves to retrieve.</param>
    /// <returns>A <see cref="BitMap"/> with a bit set for every cell where the player can make a legal move.</returns>
    public static BitMap GetValidMoves(Board board, Player player)
    {
        return rsmc_get_valid_moves(board, player);
    }

    /// <summary>
    /// Gets the current state of the game.
    /// </summary>
    /// <param name="board">The <see cref="Board"/> to inspect.</param>
    /// <returns>The <see cref="GameState"/> (score and status) for the given board.</returns>
    public static GameState GetGameState(Board board)
    {
        return rsmc_get_game_state(board);
    }
}