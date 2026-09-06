using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace RsmcSharp;

/// <summary>
/// Represents players color.
/// </summary>
public enum Player : byte
{
    /// <summary>
    /// Represents black player.
    /// </summary>
    Black = 0,

    /// <summary>
    /// Represents white player.
    /// </summary>
    White = 1,
}

/// <summary>
/// Extensions for <see cref="Player"/>
/// </summary>
public static class PlayerExtensions
{
    extension(Player player)
    {
        /// <summary>
        /// Returns opposite color.
        /// </summary>
        /// <returns>Opposite color.</returns>
        public Player Opposite()
        {
            return (Player)(((byte)player + 1) % 2);
        }

        /// <summary>
        /// Returns BoardCell with player's color.
        /// </summary>
        /// <returns><see cref="BoardCell"/> with player's color.</returns>
        public BoardCell ToCell()
        {
            return (BoardCell)((byte)player + 1);
        }
    }
}

/// <summary>
/// Represents game status.
/// </summary>
public enum GameStatus : byte
{
    /// <summary>
    /// Game is not finished.
    /// </summary>
    Continue = 0,

    /// <summary>
    /// Game is finished by draw.
    /// </summary>
    Draw = 1,

    /// <summary>
    /// Black player wins.
    /// </summary>
    BlackWin = 2,

    /// <summary>
    /// White player wins.
    /// </summary>
    WhiteWin = 3,
}

/// <summary>
/// Represents cell on the board.
/// </summary>
public enum BoardCell : byte
{
    /// <summary>
    /// Cell is empty.
    /// </summary>
    Empty = 0,

    /// <summary>
    /// Cell contains black disk.
    /// </summary>
    Black = 1,

    /// <summary>
    /// Cell contains white disk.
    /// </summary>
    White = 2,
}

/// <summary>
/// Contains players score.
/// </summary>
/// <param name="BlackScore">Black player score.</param>
/// <param name="WhiteScore">White player score.</param>
[StructLayout(LayoutKind.Sequential)]
public record struct PlayersScore(byte BlackScore, byte WhiteScore);

/// <summary>
/// Represents game state (score + status).
/// </summary>
/// <param name="Score"><see cref="PlayersScore"/></param>
/// <param name="GameStatus"><see cref="GameStatus"/></param>
[StructLayout(LayoutKind.Sequential)]
public record struct GameState(PlayersScore Score, GameStatus GameStatus);

/// <summary>
/// Represents coords on the board.
/// Can be negative. + and - operators accepted.
/// </summary>
/// <param name="X">X coord.</param>
/// <param name="Y">Y coord.</param>
[StructLayout(LayoutKind.Sequential)]
public record struct Coords(sbyte X, sbyte Y)
{
    /// <summary>
    /// Adds two <see cref="Coords"/> instances together component-wise.
    /// </summary>
    /// <param name="left">The first coordinate or base position.</param>
    /// <param name="right">The second coordinate or offset to add.</param>
    /// <returns>A new <see cref="Coords"/> representing the sum of the two coordinates.</returns>
    public static Coords operator +(Coords left, Coords right)
    {
        return new Coords((sbyte)(left.X + right.X), (sbyte)(left.Y + right.Y));
    }

    /// <summary>
    /// Subtracts one <see cref="Coords"/> instance from another component-wise.
    /// </summary>
    /// <param name="left">The base coordinate or target position.</param>
    /// <param name="right">The coordinate or offset to subtract.</param>
    /// <returns>A new <see cref="Coords"/> representing the difference between the two coordinates.</returns>
    public static Coords operator -(Coords left, Coords right)
    {
        return new Coords((sbyte)(left.X - right.X), (sbyte)(left.Y - right.Y));
    }
}

/// <summary>
/// Represents game board.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct Board
{
    /// <summary>
    /// One dimesion size.
    /// </summary>
    public const int Size = 8;

    /// <summary>
    /// Min coord.
    /// </summary>
    public const int MinCoord = 0;

    /// <summary>
    /// Max coord.
    /// </summary>
    public const int MaxCoord = Size - 1 - MinCoord;

    [StructLayout(LayoutKind.Sequential)]
    [InlineArray(Size * Size)]
    private struct Cells
    {
        private BoardCell _;
    }

    private Cells _cells;

    /// <summary>
    /// Returns <see cref="BoardCell"/> on given coords.
    /// </summary>
    /// <param name="row">Y coord.</param>
    /// <param name="col">X coord.</param>
    /// <exception cref="InvalidCoordsException">Throws if any coord is out of range <see cref="MinCoord"/>..<see cref="MaxCoord"/></exception>
    public BoardCell this[int row, int col]
    {
        get => _cells[Index(row, col)];
        set => _cells[Index(row, col)] = value;
    }

    /// <summary>
    /// Returns <see cref="BoardCell"/> on given coords.
    /// </summary>
    /// <param name="coords"><see cref="Coords"/></param>
    /// <exception cref="InvalidCoordsException">Throws if any coord is out of range <see cref="MinCoord"/>..<see cref="MaxCoord"/></exception>
    public BoardCell this[Coords coords]
    {
        get => this[coords.Y, coords.X];
        set => this[coords.Y, coords.X] = value;
    }

    /// <summary>
    /// Validates coords and converts them to one-dimension index.
    /// </summary>
    /// <param name="row">Y coord.</param>
    /// <param name="col">X coord.</param>
    /// <returns>One-dimension index.</returns>
    /// <exception cref="InvalidCoordsException">Throws if any coord is out of range <see cref="MinCoord"/>..<see cref="MaxCoord"/></exception>
    private static int Index(int row, int col)
    {
        if ((uint)row > MaxCoord || (uint)col > MaxCoord)
            throw new InvalidCoordsException($"Coords must be in {MinCoord}..{MaxCoord}");
        return row * Size + col;
    }
}

/// <summary>
/// Represents possible moves.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct Moves
{
    /// <summary>
    /// The maximum number of valid moves.
    /// Proven by Takizawa, Hiroki in 2023.
    /// </summary>
    public const int MaxValidMoves = 34;

    /// <summary>
    /// The number of moves.
    /// </summary>
    public byte Count;

    /// <summary>
    /// A buffer of possible move coordinates. Only the first <see cref="Count"/> elements are valid.
    /// </summary>
    public CoordsBuffer Coords;

    /// <summary>
    /// Represents a fixed-size inline buffer of <see cref="Coords"/> used to store valid moves without heap allocations.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    [InlineArray(MaxValidMoves)]
    public struct CoordsBuffer
    {
        private Coords _;
    }

    /// <summary>
    /// Returns an enumerator that iterates through the valid moves.
    /// </summary>
    /// <returns>
    /// An <see cref="IEnumerator{T}"/> that can be used to iterate through the collection of valid moves.
    /// </returns>
    public IEnumerator<Coords> GetEnumerator()
    {
        for (var i = 0; i < Count; i++)
        {
            yield return Coords[i];
        }
    }

    /// <summary>
    /// Determines whether the collection contains the specified coordinates.
    /// </summary>
    /// <param name="coords">The <see cref="Coords"/> to locate in the collection.</param>
    /// <returns><c>true</c> if the specified <see cref="Coords"/> is found; otherwise, <c>false</c>.</returns>
    public bool Contains(Coords coords)
    {
        for (var i = 0; i < Count; i++)
        {
            if (Coords[i] == coords)
                return true;
        }

        return false;
    }
}