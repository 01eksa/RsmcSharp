using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace RsmcSharp;

/// <summary>
/// Struct that represents RsmcBitMap from native library.
/// </summary>
/// <remarks>
/// Constructor from ulong.
/// </remarks>
/// <param name="bitmap">ulong that represents bit map.</param>
[StructLayout(LayoutKind.Sequential)]
public struct BitMap(ulong bitmap)
{
    /// <summary>
    /// Bit map stored as ulong.
    /// </summary>
    public ulong Value { get; set; } = bitmap;

    /// <summary>
    /// Implicit conversion to ulong.
    /// </summary>
    /// <param name="bitmap">BitMap to convert.</param>
    public static implicit operator ulong(BitMap bitmap) => bitmap.Value;
    /// <summary>
    /// Implicit conversion from ulong.
    /// </summary>
    /// <param name="bitmap">Bit map stored as ulong.</param>
    public static implicit operator BitMap(ulong bitmap) => new(bitmap);
}

/// <summary>
/// Struct that represents RsmcBitMask from native library.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public readonly struct BitMask
{
    /// <summary>
    /// Bit mask stored as ulong.
    /// </summary>
    public ulong Value { get; init; }

    /// <summary>
    /// Creates BitMask from ulong.
    /// </summary>
    /// <param name="mask">Bit mask stored as ulong.</param>
    public BitMask(ulong mask)
    {
        Value = mask;
    }

    /// <summary>
    /// Creates BitMask from coordinates.
    /// </summary>
    /// <param name="x"> x (column). </param>
    /// <param name="y"> y (row). </param>
    /// <exception cref="InvalidCoordsException"> Coords are out of range. </exception>
    public BitMask(int x, int y)
    {
        if ((uint)y > Board.MaxCoord || (uint)x > Board.MaxCoord)
            throw new InvalidCoordsException($"Coords must be in {Board.MinCoord}..{Board.MaxCoord}");
        var index = y * Board.Size + x;
        Value = 1ul << index;
    }
    /// <summary>
    /// Implicit conversion to ulong.
    /// </summary>
    /// <param name="mask">BitMask to convert.</param>
    public static implicit operator ulong(BitMask mask) => mask.Value;
    /// <summary>
    /// Implicit conversion from ulong.
    /// </summary>
    /// <param name="value">Bit mask stored as ulong.</param>
    public static implicit operator BitMask(ulong value) => new(value);
}

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
public record struct Board
{
    /// <summary>
    /// BitMap that stores black pieces.
    /// </summary>
    public BitMap Black { get; init; }
    /// <summary>
    /// BitMap that stores white pieces.
    /// </summary>
    public BitMap White { get; init; }

    /// <summary>
    /// Creates Board from two BitMap.
    /// </summary>
    /// <param name="black">BitMap that stores black pieces.</param>
    /// <param name="white">BitMap that stores white pieces.</param>
    public Board(BitMap black, BitMap white)
    {
        Black = black;
        White = white;
    }

    /// <summary>
    /// Creates Board from BoardCell 2-dim array.
    /// </summary>
    /// <param name="cells">Cells that represents board.</param>
    /// <exception cref="ArgumentOutOfRangeException">One of dimensions doesn't match size.</exception>
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
    /// One dimesion size.
    /// </summary>
    public const int Size = 8;

    /// <summary>
    /// Total number of cells.
    /// </summary>
    public const int Area = Size * Size;

    /// <summary>
    /// Min coord.
    /// </summary>
    public const int MinCoord = 0;

    /// <summary>
    /// Max coord.
    /// </summary>
    public const int MaxCoord = MinCoord + Size - 1 - MinCoord;

    /// <summary>
    /// Returns <see cref="BoardCell"/> on given coords.
    /// </summary>
    /// <param name="row">Y coord.</param>
    /// <param name="col">X coord.</param>
    /// <exception cref="InvalidCoordsException">Throws if any coord is out of range <see cref="MinCoord"/>..<see cref="MaxCoord"/></exception>
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
    /// Returns <see cref="BoardCell"/> on given coords.
    /// </summary>
    /// <param name="coords"><see cref="Coords"/></param>
    /// <exception cref="InvalidCoordsException">Throws if any coord is out of range <see cref="MinCoord"/>..<see cref="MaxCoord"/></exception>
    public BoardCell this[Coords coords] => this[coords.Y, coords.X];
}

/// <summary>
/// Represents native rsmc library version.
/// </summary>
/// <param name="Encoded">Version packed into uint.</param>
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
    /// Creates <see cref="Version"/> from Major-Minor-Patch values.
    /// </summary>
    /// <param name="Major">Major version.</param>
    /// <param name="Minor">Minor version.</param>
    /// <param name="Patch">Patch version.</param>
    public Version(byte Major, byte Minor, ushort Patch) : this(((uint)Major << 24) | ((uint)Minor << 16) | (uint)Patch) {}

    /// <summary>
    /// Compares two versions of rsmc library.
    /// </summary>
    /// <param name="left">Left operand.</param>
    /// <param name="right">Right operand.</param>
    /// <returns>True if right operand is greater, else false.</returns>
    public static bool operator <(Version left, Version right)
    {
        return left.Encoded < right.Encoded;
    }

    /// <summary>
    /// Compares two versions of rsmc library.
    /// </summary>
    /// <param name="left">Left operand.</param>
    /// <param name="right">Right operand.</param>
    /// <returns>True if right operand is greater or equal, else false.</returns>
    public static bool operator <=(Version left, Version right)
    {
        return left.Encoded <= right.Encoded;
    }

    /// <summary>
    /// Compares two versions of rsmc library.
    /// </summary>
    /// <param name="left">Left operand.</param>
    /// <param name="right">Right operand.</param>
    /// <returns>True if left operand is greater, else false.</returns>
    public static bool operator >(Version left, Version right)
    {
        return left.Encoded > right.Encoded;
    }

    /// <summary>
    /// Compares two versions of rsmc library.
    /// </summary>
    /// <param name="left">Left operand.</param>
    /// <param name="right">Right operand.</param>
    /// <returns>True if left operand is greater or equal, else false.</returns>
    public static bool operator >=(Version left, Version right)
    {
        return left.Encoded >= right.Encoded;
    }

    /// <summary>
    /// Converts version to human-readeble tag in format vMAJOR.MINOR.PATCH
    /// </summary>
    /// <returns>Human-readable version tag.</returns>
    public override readonly string ToString() => $"v{Major}.{Minor}.{Patch}";

}