using System.Runtime.InteropServices;

namespace RsmcSharp;

/// <summary>
/// Provides a managed API wrapper and P/Invoke bindings for interacting with the native <c>rsmc</c> library.
/// </summary>
public static partial class Rsmc
{
    static Rsmc() => NativeLoader.EnsureRegistered();

    private const string LibName = NativeLoader.LibraryName;

    [LibraryImport(LibName)]
    private static partial void rsmc_set_start_position(ref Board board);

    [LibraryImport(LibName)]
    [return: MarshalAs(UnmanagedType.U1)]
    private static partial bool rsmc_apply_move(ref Board board, Coords coords, Player player);

    [LibraryImport(LibName)]
    private static partial PlayersScore rsmc_get_players_score(ref Board board);

    [LibraryImport(LibName)]
    [return: MarshalAs(UnmanagedType.U1)]
    private static partial bool rsmc_is_move_valid(ref Board board, Coords coords, Player player);

    [LibraryImport(LibName)]
    private static partial Moves rsmc_get_valid_moves(ref Board board, Player player);

    [LibraryImport(LibName)]
    private static partial GameState rsmc_get_game_state(ref Board board);

    /// <summary>
    /// Sets the initial starting position on the board.
    /// </summary>
    /// <param name="board">The target <see cref="Board"/> to initialize.</param>
    public static void SetStartPosition(ref Board board)
    {
        rsmc_set_start_position(ref board);
    }

    /// <summary>
    /// Applies a move to the board for the specified player.
    /// </summary>
    /// <param name="board">The <see cref="Board"/> on which to apply the move.</param>
    /// <param name="coords">The target <see cref="Coords"/> for the move.</param>
    /// <param name="player">The <see cref="Player"/> making the move.</param>
    /// <exception cref="InvalidMoveException">Thrown when the requested move is invalid.</exception>
    public static void ApplyMove(ref Board board, Coords coords, Player player)
    {
        var success = rsmc_apply_move(ref board, coords, player);
        if (!success)
            throw new InvalidMoveException();
    }

    /// <summary>
    /// Calculates and retrieves the current score for both players.
    /// </summary>
    /// <param name="board">The <see cref="Board"/> to evaluate.</param>
    /// <returns>A <see cref="PlayersScore"/> object containing the current scores.</returns>
    public static PlayersScore GetPlayersScore(ref Board board)
    {
        return rsmc_get_players_score(ref board);
    }

    /// <summary>
    /// Determines whether a proposed move is valid for the specified player.
    /// </summary>
    /// <param name="board">The <see cref="Board"/> to check.</param>
    /// <param name="coords">The target <see cref="Coords"/> of the proposed move.</param>
    /// <param name="player">The <see cref="Player"/> attempting the move.</param>
    /// <returns><c>true</c> if the move is valid; otherwise, <c>false</c>.</returns>
    public static bool IsMoveValid(ref Board board, Coords coords, Player player)
    {
        return rsmc_is_move_valid(ref board, coords, player);
    }

    /// <summary>
    /// Gets all valid moves available for the specified player.
    /// </summary>
    /// <param name="board">The <see cref="Board"/> to analyze.</param>
    /// <param name="player">The <see cref="Player"/> whose valid moves to retrieve.</param>
    /// <returns>A <see cref="Moves"/> collection containing all legal moves.</returns>
    public static Moves GetValidMoves(ref Board board, Player player)
    {
        return rsmc_get_valid_moves(ref board, player);
    }

    /// <summary>
    /// Gets the current state of the game.
    /// </summary>
    /// <param name="board">The <see cref="Board"/> to inspect.</param>
    /// <returns>The current <see cref="GameState"/> of the game.</returns>
    public static GameState GetGameState(ref Board board)
    {
        return rsmc_get_game_state(ref board);
    }
}