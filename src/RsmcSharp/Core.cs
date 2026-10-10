using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace RsmcSharp;

/// <summary>
/// Set of board cells stored as a 64-bit integer. Mirrors <c>RsmcBitMap</c> from the native library.
/// </summary>
/// <remarks>
/// Bit <c>i</c> corresponds to the cell with <c>x = i % <see cref="Board.Size"/></c> and
/// <c>y = i / <see cref="Board.Size"/></c>, so cell (0, 0) is the least significant bit.
/// Unlike <see cref="BitMask"/>, which normally marks a single cell, a <see cref="BitMap"/> may contain any number of cells.
/// </remarks>
/// <param name="bitmap">Bit map stored as <see cref="ulong"/>.</param>
[StructLayout(LayoutKind.Sequential)]
public struct BitMap(ulong bitmap)
{
    /// <summary>
    /// Bit map stored as <see cref="ulong"/>.
    /// </summary>
    public ulong Value { get; set; } = bitmap;

    /// <summary>
    /// Implicit conversion to <see cref="ulong"/>.
    /// </summary>
    /// <param name="bitmap"><see cref="BitMap"/> to convert.</param>
    public static implicit operator ulong(BitMap bitmap) => bitmap.Value;
    /// <summary>
    /// Implicit conversion from <see cref="ulong"/>.
    /// </summary>
    /// <param name="bitmap">Bit map stored as <see cref="ulong"/>.</param>
    public static implicit operator BitMap(ulong bitmap) => new(bitmap);
}

/// <summary>
/// Mask that identifies a board cell (typically exactly one bit is set). Mirrors <c>RsmcBitMask</c> from the native library.
/// </summary>
/// <remarks>
/// The bit layout is the same as in <see cref="BitMap"/>: the bit index of a cell is <c>y * <see cref="Board.Size"/> + x</c>.
/// </remarks>
[StructLayout(LayoutKind.Sequential)]
public readonly struct BitMask
{
    /// <summary>
    /// Bit mask stored as <see cref="ulong"/>.
    /// </summary>
    public ulong Value { get; init; }

    /// <summary>
    /// Creates a <see cref="BitMask"/> from a raw <see cref="ulong"/> value. The value is not validated.
    /// </summary>
    /// <param name="mask">Bit mask stored as <see cref="ulong"/>.</param>
    public BitMask(ulong mask)
    {
        Value = mask;
    }

    /// <summary>
    /// Creates a <see cref="BitMask"/> with a single bit set for the cell at the given coordinates.
    /// </summary>
    /// <param name="x">Column (X coordinate).</param>
    /// <param name="y">Row (Y coordinate).</param>
    /// <exception cref="InvalidCoordsException">
    /// <paramref name="x"/> or <paramref name="y"/> is outside <see cref="Board.MinCoord"/>..<see cref="Board.MaxCoord"/>.
    /// </exception>
    public BitMask(int x, int y)
    {
        if ((uint)y > Board.MaxCoord || (uint)x > Board.MaxCoord)
            throw new InvalidCoordsException($"Coords must be in {Board.MinCoord}..{Board.MaxCoord}");
        var index = y * Board.Size + x;
        Value = 1ul << index;
    }
    /// <summary>
    /// Implicit conversion to <see cref="ulong"/>.
    /// </summary>
    /// <param name="mask"><see cref="BitMask"/> to convert.</param>
    public static implicit operator ulong(BitMask mask) => mask.Value;
    /// <summary>
    /// Implicit conversion from <see cref="ulong"/>.
    /// </summary>
    /// <param name="value">Bit mask stored as <see cref="ulong"/>.</param>
    public static implicit operator BitMask(ulong value) => new(value);
}

/// <summary>
/// Represents a player's color.
/// </summary>
public enum Player : byte
{
    /// <summary>
    /// Black player.
    /// </summary>
    Black = 0,

    /// <summary>
    /// White player.
    /// </summary>
    White = 1,
}

/// <summary>
/// Extensions for <see cref="Player"/>.
/// </summary>
public static class PlayerExtensions
{
    extension(Player player)
    {
        /// <summary>
        /// Returns the opposite player.
        /// </summary>
        /// <returns><see cref="Player.White"/> for <see cref="Player.Black"/> and vice versa.</returns>
        public Player Opposite()
        {
            return (Player)(((byte)player + 1) % 2);
        }

        /// <summary>
        /// Returns the <see cref="BoardCell"/> that holds this player's disc.
        /// </summary>
        /// <returns><see cref="BoardCell.Black"/> for <see cref="Player.Black"/>, <see cref="BoardCell.White"/> for <see cref="Player.White"/>.</returns>
        public BoardCell ToCell()
        {
            return (BoardCell)((byte)player + 1);
        }
    }
}

/// <summary>
/// Represents the game status.
/// </summary>
public enum GameStatus : byte
{
    /// <summary>
    /// Game is not finished.
    /// </summary>
    Continue = 0,

    /// <summary>
    /// Game ended in a draw.
    /// </summary>
    Draw = 1,

    /// <summary>
    /// Game ended with a win for the black player.
    /// </summary>
    BlackWin = 2,

    /// <summary>
    /// Game ended with a win for the white player.
    /// </summary>
    WhiteWin = 3,
}

/// <summary>
/// Represents the contents of a board cell.
/// </summary>
public enum BoardCell : byte
{
    /// <summary>
    /// Cell is empty.
    /// </summary>
    Empty = 0,

    /// <summary>
    /// Cell contains a black disc.
    /// </summary>
    Black = 1,

    /// <summary>
    /// Cell contains a white disc.
    /// </summary>
    White = 2,
}

/// <summary>
/// Contains the score of both players.
/// </summary>
/// <param name="BlackScore">Black player's score (number of black discs).</param>
/// <param name="WhiteScore">White player's score (number of white discs).</param>
[StructLayout(LayoutKind.Sequential)]
public record struct PlayersScore(byte BlackScore, byte WhiteScore);

/// <summary>
/// Represents the game state: score and status.
/// </summary>
/// <param name="Score">Current score of both players.</param>
/// <param name="GameStatus">Current status of the game.</param>
[StructLayout(LayoutKind.Sequential)]
public record struct GameState(PlayersScore Score, GameStatus GameStatus);

/// <summary>
/// Represents a position or an offset on the board.
/// </summary>
/// <remarks>
/// <see cref="X"/> is the column and <see cref="Y"/> is the row.
/// Values may be negative or outside the board (e.g. when used as direction offsets).
/// The <c>+</c> and <c>-</c> operators operate component-wise and truncate the result to <see cref="sbyte"/>
/// without overflow checks (unless the project is compiled with <c>checked</c>).
/// </remarks>
/// <param name="X">X coordinate (column).</param>
/// <param name="Y">Y coordinate (row).</param>
[StructLayout(LayoutKind.Sequential)]
public record struct Coords(sbyte X, sbyte Y)
{
    /// <summary>
    /// Adds two <see cref="Coords"/> instances component-wise.
    /// </summary>
    /// <param name="left">The base position.</param>
    /// <param name="right">The offset to add.</param>
    /// <returns>A new <see cref="Coords"/> representing the sum of the two coordinates.</returns>
    public static Coords operator +(Coords left, Coords right)
    {
        return new Coords((sbyte)(left.X + right.X), (sbyte)(left.Y + right.Y));
    }

    /// <summary>
    /// Subtracts one <see cref="Coords"/> instance from another component-wise.
    /// </summary>
    /// <param name="left">The base position.</param>
    /// <param name="right">The offset to subtract.</param>
    /// <returns>A new <see cref="Coords"/> representing the difference between the two coordinates.</returns>
    public static Coords operator -(Coords left, Coords right)
    {
        return new Coords((sbyte)(left.X - right.X), (sbyte)(left.Y - right.Y));
    }
}

/// <summary>
/// Represents the game board.
/// </summary>
/// <remarks>
/// The board is stored as two <see cref="BitMap"/> values (black and white discs).
/// Bit <c>y * <see cref="Size"/> + x</c> corresponds to the cell at column <c>x</c> and row <c>y</c>.
/// </remarks>
[StructLayout(LayoutKind.Sequential)]
public record struct Board
{
    /// <summary>
    /// <see cref="BitMap"/> that stores black discs.
    /// </summary>
    public BitMap Black { get; init; }
    /// <summary>
    /// <see cref="BitMap"/> that stores white discs.
    /// </summary>
    public BitMap White { get; init; }

    /// <summary>
    /// Creates a <see cref="Board"/> from two bit maps.
    /// </summary>
    /// <remarks>
    /// The bit maps are not validated; they are expected not to overlap.
    /// </remarks>
    /// <param name="black"><see cref="BitMap"/> that stores black discs.</param>
    /// <param name="white"><see cref="BitMap"/> that stores white discs.</param>
    public Board(BitMap black, BitMap white)
    {
        Black = black;
        White = white;
    }

    /// <summary>
    /// Creates a <see cref="Board"/> from a two-dimensional array of cells.
    /// </summary>
    /// <param name="cells">
    /// Cells of the board, indexed as <c>[row, column]</c> (that is, <c>[y, x]</c>).
    /// Both dimensions must be equal to <see cref="Size"/>.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">One of the dimensions doesn't match <see cref="Size"/>.</exception>
    public Board(BoardCell[,] cells)
    {
        if (cells.GetLength(0) != Size || cells.GetLength(1) != Size)
        {
            throw new ArgumentOutOfRangeException();
        }

        BitMap black = 0;
        BitMap white = 0;


        for (var y = 0; y < Size; y++)
        {
            for (var x = 0; x < Size; x++)
            {
                switch (cells[y, x])
                {
                    case BoardCell.Black:
                        black |= new BitMask(x, y);
                        break;
                    case BoardCell.White:
                        white |= new BitMask(x, y);
                        break;
                    case BoardCell.Empty:
                    default:
                        continue;
                }
            }
        }

        Black = black;
        White = white;
    }

    /// <summary>
    /// Number of rows and columns of the board.
    /// </summary>
    public const int Size = 8;

    /// <summary>
    /// Total number of cells.
    /// </summary>
    public const int Area = Size * Size;

    /// <summary>
    /// Minimum valid coordinate (inclusive).
    /// </summary>
    public const int MinCoord = 0;

    /// <summary>
    /// Maximum valid coordinate (inclusive).
    /// </summary>
    public const int MaxCoord = MinCoord + Size - 1 - MinCoord;

    /// <summary>
    /// Gets the cell at the given row and column.
    /// </summary>
    /// <param name="row">Row (Y coordinate).</param>
    /// <param name="col">Column (X coordinate).</param>
    /// <returns>Contents of the cell: <see cref="BoardCell.Black"/>, <see cref="BoardCell.White"/> or <see cref="BoardCell.Empty"/>.</returns>
    /// <exception cref="InvalidCoordsException">Thrown if any coordinate is outside <see cref="MinCoord"/>..<see cref="MaxCoord"/>.</exception>
    public readonly BoardCell this[int row, int col]
    {
        get
        {
            var mask = new BitMask(col, row);

            if ((Black & mask) != 0)
            {
                return BoardCell.Black;
            }

            if ((White & mask) != 0)
            {
                return BoardCell.White;
            }

            return BoardCell.Empty;
        }
    }

    /// <summary>
    /// Gets the cell at the given coordinates.
    /// </summary>
    /// <param name="coords">Position of the cell (<see cref="Coords.X"/> is the column, <see cref="Coords.Y"/> is the row).</param>
    /// <returns>Contents of the cell: <see cref="BoardCell.Black"/>, <see cref="BoardCell.White"/> or <see cref="BoardCell.Empty"/>.</returns>
    /// <exception cref="InvalidCoordsException">Thrown if any coordinate is outside <see cref="MinCoord"/>..<see cref="MaxCoord"/>.</exception>
    public BoardCell this[Coords coords] => this[coords.Y, coords.X];
}

/// <summary>
/// Represents the native rsmc library version.
/// </summary>
/// <remarks>
/// The version is packed into a single <see cref="uint"/> as <c>(Major &lt;&lt; 24) | (Minor &lt;&lt; 16) | Patch</c>,
/// so versions are compared by comparing the encoded values.
/// </remarks>
/// <param name="Encoded">Version packed into <see cref="uint"/>.</param>
public record struct Version(uint Encoded)
{
    /// <summary>
    /// Major version.
    /// </summary>
    public readonly byte Major => (byte)(Encoded >> 24);
    /// <summary>
    /// Minor version.
    /// </summary>
    public readonly byte Minor => (byte)(Encoded >> 16);
    /// <summary>
    /// Patch version.
    /// </summary>
    public readonly ushort Patch => (ushort)Encoded;

    /// <summary>
    /// Creates a <see cref="Version"/> from Major, Minor and Patch values.
    /// </summary>
    /// <param name="Major">Major version.</param>
    /// <param name="Minor">Minor version.</param>
    /// <param name="Patch">Patch version.</param>
    public Version(byte Major, byte Minor, ushort Patch) : this(((uint)Major << 24) | ((uint)Minor << 16) | (uint)Patch) {}

    /// <summary>
    /// Determines whether the left version is lower than the right one.
    /// </summary>
    /// <param name="left">Left operand.</param>
    /// <param name="right">Right operand.</param>
    /// <returns><c>true</c> if <paramref name="left"/> is lower than <paramref name="right"/>; otherwise, <c>false</c>.</returns>
    public static bool operator <(Version left, Version right)
    {
        return left.Encoded < right.Encoded;
    }

    /// <summary>
    /// Determines whether the left version is lower than or equal to the right one.
    /// </summary>
    /// <param name="left">Left operand.</param>
    /// <param name="right">Right operand.</param>
    /// <returns><c>true</c> if <paramref name="left"/> is lower than or equal to <paramref name="right"/>; otherwise, <c>false</c>.</returns>
    public static bool operator <=(Version left, Version right)
    {
        return left.Encoded <= right.Encoded;
    }

    /// <summary>
    /// Determines whether the left version is greater than the right one.
    /// </summary>
    /// <param name="left">Left operand.</param>
    /// <param name="right">Right operand.</param>
    /// <returns><c>true</c> if <paramref name="left"/> is greater than <paramref name="right"/>; otherwise, <c>false</c>.</returns>
    public static bool operator >(Version left, Version right)
    {
        return left.Encoded > right.Encoded;
    }

    /// <summary>
    /// Determines whether the left version is greater than or equal to the right one.
    /// </summary>
    /// <param name="left">Left operand.</param>
    /// <param name="right">Right operand.</param>
    /// <returns><c>true</c> if <paramref name="left"/> is greater than or equal to <paramref name="right"/>; otherwise, <c>false</c>.</returns>
    public static bool operator >=(Version left, Version right)
    {
        return left.Encoded >= right.Encoded;
    }

    /// <summary>
    /// Converts the version to a human-readable tag in the format <c>vMAJOR.MINOR.PATCH</c>.
    /// </summary>
    /// <returns>Human-readable version tag.</returns>
    public override readonly string ToString() => $"v{Major}.{Minor}.{Patch}";

}