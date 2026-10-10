namespace RsmcSharp.Tests;

public class RsmcIntegrationTest
{
    [Fact]
    public void LoadLibrary_VersionIsCorrect()
    {
        Assert.True(Rsmc.MinimalVersion <= Rsmc.RsmcVersion);
    }

    [Fact]
    public void GetStartPosition_PositionIsCorrect()
    {
        var expected = new Board(new BoardCell[,]
        {
            { 0, 0, 0, 0, 0, 0, 0, 0 },
            { 0, 0, 0, 0, 0, 0, 0, 0 },
            { 0, 0, 0, 0, 0, 0, 0, 0 },
            { 0, 0, 0, BoardCell.White, BoardCell.Black, 0, 0, 0 },
            { 0, 0, 0, BoardCell.Black, BoardCell.White, 0, 0, 0 },
            { 0, 0, 0, 0, 0, 0, 0, 0 },
            { 0, 0, 0, 0, 0, 0, 0, 0 },
            { 0, 0, 0, 0, 0, 0, 0, 0 },
        });
        var actual = Rsmc.StartPosition;

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void GetValidMoves_StartPositionBlackPlayer_MovesAreValid()
    {
        Coords[] expected = [new(2, 3), new(3, 2), new(4, 5), new(5, 4)];
        var actual = Rsmc.GetValidMoves(Rsmc.StartPosition, Player.Black);

        foreach (var move in expected)
        {
            Assert.NotEqual(0ul, actual & new BitMask(move.X, move.Y));
        }
    }

    [Fact]
    public void GetValidMoves_BlackPlayerHasNoMoves_Return0()
    {
        var board = new Board(new BoardCell[,]{
            { 0, 0, 0, 0, BoardCell.White, BoardCell.White, BoardCell.White, BoardCell.White },
            { 0, 0, 0, 0, 0, 0, 0, BoardCell.White },
            { 0, 0, 0, 0, 0, 0, 0, BoardCell.White },
            { BoardCell.Black, BoardCell.Black, BoardCell.Black, BoardCell.Black, BoardCell.Black, BoardCell.Black, BoardCell.White, BoardCell.White },
            { 0, 0, 0, BoardCell.Black, BoardCell.Black, 0, 0, 0 },
            { 0, 0, 0, 0, 0, 0, 0, 0 },
            { 0, 0, 0, 0, 0, 0, 0, 0 },
            { 0, 0, 0, 0, 0, 0, 0, 0 },
        });
        var expected = new BitMap(0);
        var actual = Rsmc.GetValidMoves(board, Player.Black);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void GetValidMoves_InvalidPlayer_Return0()
    {
        var expected = new BitMap(0);
        var actual = Rsmc.GetValidMoves(Rsmc.StartPosition, (Player)100);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void ApplyMove_ValidMoveStartPositionBlackPlayer_MoveAppliedCorrectly()
    {
        var before = Rsmc.StartPosition;
        var move = new BitMask(2, 3);
        var after = Rsmc.ApplyMove(before, move, Player.Black);
        var expected = new Board(new BoardCell[,]
        {
            { 0, 0, 0, 0, 0, 0, 0, 0 },
            { 0, 0, 0, 0, 0, 0, 0, 0 },
            { 0, 0, 0, 0, 0, 0, 0, 0 },
            { 0, 0, BoardCell.Black, BoardCell.Black, BoardCell.Black, 0, 0, 0 },
            { 0, 0, 0, BoardCell.Black, BoardCell.White, 0, 0, 0 },
            { 0, 0, 0, 0, 0, 0, 0, 0 },
            { 0, 0, 0, 0, 0, 0, 0, 0 },
            { 0, 0, 0, 0, 0, 0, 0, 0 },
        });

        Assert.NotEqual(before, after);
        Assert.Equal(expected, after);
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(0, 7)]
    [InlineData(7, 7)]
    [InlineData(7, 0)]
    [InlineData(3, 3)]
    [InlineData(4, 4)]
    [InlineData(3, 4)]
    [InlineData(4, 3)]
    [InlineData(2, 3)]
    [InlineData(3, 2)]
    [InlineData(4, 5)]
    [InlineData(5, 4)]
    public void ApplyMove_InvalidMove_NoChanges(int x, int y)
    {
        var mask = new BitMask(x, y);
        var expected = Rsmc.StartPosition;
        var actual = Rsmc.ApplyMove(Rsmc.StartPosition, mask, (Player)4);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void ApplyMove_InvalidPlayer_NoChanges()
    {
        var before = Rsmc.StartPosition;
        var move = new BitMask(2, 3);
        var after = Rsmc.ApplyMove(before, move, (Player)100);

        Assert.Equal(before, after);
    }

    [Theory]
    [InlineData(0b0101ul)]
    [InlineData(0b10000000001ul)]
    [InlineData(0xF3FFFFFFul)]
    public void ApplyMove_InvalidMoveMask_NoChanges(ulong moveMask)
    {
        var before = Rsmc.StartPosition;
        var after = Rsmc.ApplyMove(before, new BitMask(moveMask), Player.Black);

        Assert.Equal(before, after);
    }

    [Fact]
    public void GetPlayersScore_ReturnsRightScore()
    {
        var board = new Board(new BoardCell[,]
        {
            { 0, 0, 0, 0, 0, 0, 0, 0 },
            { 0, 0, 0, 0, 0, 0, 0, 0 },
            { 0, 0, 0, 0, 0, 0, 0, 0 },
            { 0, 0, BoardCell.Black, BoardCell.Black, BoardCell.Black, 0, 0, 0 },
            { 0, 0, 0, BoardCell.Black, BoardCell.White, 0, 0, 0 },
            { 0, 0, 0, 0, 0, 0, 0, 0 },
            { 0, 0, 0, 0, 0, 0, 0, 0 },
            { 0, 0, 0, 0, 0, 0, 0, 0 },
        });
        var expected = new PlayersScore(BlackScore: 4, WhiteScore: 1);
        var actual = Rsmc.GetPlayersScore(board);

        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData(2, 3)]
    [InlineData(3, 2)]
    [InlineData(4, 5)]
    [InlineData(5, 4)]
    public void IsMoveValid_StartPositionBlackPlayer_ReturnTrue(sbyte x, sbyte y)
    {
        var move = new BitMask(x, y);
        Assert.True(Rsmc.IsMoveValid(Rsmc.StartPosition, move, Player.Black));
    }

    [Theory]
    [InlineData(2, 3)]
    [InlineData(3, 2)]
    [InlineData(4, 5)]
    [InlineData(5, 4)]
    public void IsMoveValid_StartPositionWhitePlayer_ReturnFalse(sbyte x, sbyte y)
    {
        var move = new BitMask(x, y);
        Assert.False(Rsmc.IsMoveValid(Rsmc.StartPosition, move, Player.White));
    }

    [Theory]
    [InlineData(2, 3)]
    [InlineData(3, 2)]
    [InlineData(4, 5)]
    [InlineData(5, 4)]
    public void IsMoveValid_StartPositionInvalidPlayer_ReturnFalse(sbyte x, sbyte y)
    {
        var move = new BitMask(x, y);
        Assert.False(Rsmc.IsMoveValid(Rsmc.StartPosition, move, (Player)100));
    }

    [Theory]
    [InlineData(0b0101ul)]
    [InlineData(0b10000000001ul)]
    [InlineData(0xF3FFFFFFul)]
    public void IsMoveValid_StartPositionBlackPlayerInvalidMoveMask_ReturnFalse(ulong moveMask)
    {
        var move = new BitMask(moveMask);
        Assert.False(Rsmc.IsMoveValid(Rsmc.StartPosition, move, Player.Black));
    }

    [Fact]
    public void GetGameState_StartPosition_ReturnContinueAndValidScore()
    {
        var expected = new GameState(new PlayersScore(BlackScore: 2, WhiteScore: 2), GameStatus.Continue);
        var actual = Rsmc.GetGameState(Rsmc.StartPosition);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void GetGameState_PlayersDontHaveMovesAndDraw_ReturnDrawAndValidScore()
    {
        var board = new Board(new BoardCell[,]{
            { 0, 0, 0, 0, BoardCell.White, BoardCell.White, BoardCell.White, BoardCell.White },
            { 0, 0, 0, 0, 0, 0, 0, BoardCell.White },
            { 0, 0, 0, 0, 0, 0, 0, BoardCell.White },
            { BoardCell.Black, BoardCell.Black, BoardCell.Black, BoardCell.Black,
            BoardCell.Black, BoardCell.Black, BoardCell.White, BoardCell.White },
            { 0, 0, 0, BoardCell.Black, BoardCell.Black, 0, 0, 0 },
            { 0, 0, 0, 0, 0, 0, 0, 0 },
            { 0, 0, 0, 0, 0, 0, 0, 0 },
            { 0, 0, 0, 0, 0, 0, 0, 0 },
        });
        var expected = new GameState(new PlayersScore(BlackScore: 8, WhiteScore: 8), GameStatus.Draw);
        var actual = Rsmc.GetGameState(board);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void GetGameState_PlayerDontHaveMovesAndWhiteWin_ReturnWhiteWinAndValidScore()
    {
        var board = new Board(new BoardCell[,]{
            { 0, 0, 0, 0, 0, 0, 0, 0 },
            { 0, 0, 0, 0, 0, 0, 0, 0 },
            { 0, 0, 0, 0, 0, 0, 0, 0 },
            { 0, 0, 0, BoardCell.White, BoardCell.White, 0, 0, 0 },
            { 0, 0, 0, BoardCell.White, BoardCell.White, 0, 0, 0 },
            { 0, 0, 0, 0, 0, 0, 0, 0 },
            { 0, 0, 0, 0, 0, 0, 0, 0 },
            { 0, 0, 0, 0, 0, 0, 0, 0 },
        });
        var expected = new GameState(new PlayersScore(BlackScore: 0, WhiteScore: 4), GameStatus.WhiteWin);
        var actual = Rsmc.GetGameState(board);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void GetGameState_FullBoardBlackWin_ReturnBlackWinAndValidScore()
    {
        var board = new Board(new BoardCell[,]{
            {
                BoardCell.Black,
                BoardCell.Black,
                BoardCell.Black,
                BoardCell.White,
                BoardCell.White,
                BoardCell.White,
                BoardCell.White,
                BoardCell.White,
            },
            {
                BoardCell.Black,
                BoardCell.Black,
                BoardCell.Black,
                BoardCell.Black,
                BoardCell.Black,
                BoardCell.Black,
                BoardCell.Black,
                BoardCell.Black,
            },
            {
                BoardCell.Black,
                BoardCell.Black,
                BoardCell.Black,
                BoardCell.Black,
                BoardCell.Black,
                BoardCell.Black,
                BoardCell.Black,
                BoardCell.Black,
            },
            {
                BoardCell.Black,
                BoardCell.Black,
                BoardCell.Black,
                BoardCell.Black,
                BoardCell.Black,
                BoardCell.Black,
                BoardCell.Black,
                BoardCell.Black,
            },
            {
                BoardCell.Black,
                BoardCell.Black,
                BoardCell.Black,
                BoardCell.Black,
                BoardCell.Black,
                BoardCell.Black,
                BoardCell.Black,
                BoardCell.Black,
            },
            {
                BoardCell.Black,
                BoardCell.White,
                BoardCell.White,
                BoardCell.White,
                BoardCell.White,
                BoardCell.White,
                BoardCell.White,
                BoardCell.White,
            },
            {
                BoardCell.Black,
                BoardCell.White,
                BoardCell.White,
                BoardCell.White,
                BoardCell.White,
                BoardCell.White,
                BoardCell.White,
                BoardCell.White,
            },
            {
                BoardCell.Black,
                BoardCell.White,
                BoardCell.White,
                BoardCell.White,
                BoardCell.White,
                BoardCell.White,
                BoardCell.White,
                BoardCell.White,
            },
        });
        var expected = new GameState(new PlayersScore(BlackScore: 38, WhiteScore: 26), GameStatus.BlackWin);
        var actual = Rsmc.GetGameState(board);

        Assert.Equal(expected, actual);
    }
}
