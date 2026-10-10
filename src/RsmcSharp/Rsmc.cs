using System.Runtime.InteropServices;

namespace RsmcSharp;

/// <summary>
/// Provides a managed API wrapper and P/Invoke bindings for interacting with the native <c>rsmc</c> library.
/// </summary>
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
    /// Cached start position for Reversi.
    /// </summary>
    public static readonly Board StartPosition;
    /// <summary>
    /// Minimal compabile version of native rsmc library.
    /// </summary>
    public static readonly Version MinimalVersion = new(1, 0, 0);

    /// <summary>
    /// Returns version of native rsmc library.
    /// </summary>
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
    /// Returns the initial position of Reversi.
    /// </summary>
    /// <returns>Returns the initial position of Reversi.</returns>
    public static Board GetStartPosition()
    {
        return StartPosition;
    }

    /// <summary>
    /// Applies a move to the board for the specified player.
    /// </summary>
    /// <param name="board">Current <see cref="Board"/></param>
    /// <param name="move">The target <see cref="BitMask"/> for the move.</param>
    /// <param name="player">The <see cref="Player"/> making the move.</param>
    /// <returns>New <see cref="Board"/>.</returns>
    public static Board ApplyMove(Board board, BitMask move, Player player)
    {
        return rsmc_apply_move(board, move, player);
    }

    /// <summary>
    /// Calculates and retrieves the current score for both players.
    /// </summary>
    /// <param name="board">The <see cref="Board"/> to evaluate.</param>
    /// <returns>A <see cref="PlayersScore"/> object containing the current scores.</returns>
    public static PlayersScore GetPlayersScore(Board board)
    {
        return rsmc_get_players_score(board);
    }

    /// <summary>
    /// Determines whether a proposed move is valid for the specified player.
    /// </summary>
    /// <param name="board">The <see cref="Board"/> to check.</param>
    /// <param name="move">The target <see cref="BitMask"/> of the proposed move.</param>
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
    /// <returns>A <see cref="BitMap"/> containing all legal moves.</returns>
    public static BitMap GetValidMoves(Board board, Player player)
    {
        return rsmc_get_valid_moves(board, player);
    }

    /// <summary>
    /// Gets the current state of the game.
    /// </summary>
    /// <param name="board">The <see cref="Board"/> to inspect.</param>
    /// <returns>The current <see cref="GameState"/> of the game.</returns>
    public static GameState GetGameState(Board board)
    {
        return rsmc_get_game_state(board);
    }
}