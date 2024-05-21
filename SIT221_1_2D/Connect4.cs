using System.Text;

namespace SIT221_1_2D;

public class Connect4
{
    public enum Player
    {
        None = 0,
        Player1 = 1,
        Player2 = 2
    };

    public enum GameState
    {
        Player1Win,
        Player2Win,
        Draw,
        InProgress
    };

    private readonly int _rows;
    private readonly int _columns;
    private readonly int _winningLength;

    private readonly string _empty;
    private readonly string _player1;
    private readonly string _player2;

    public int RemainingMoves { get; private set; }
    private readonly int _totalMoves;
    private bool _player1Turn = true;
    public Player CurrentPlayerTurn => _player1Turn ? Player.Player1 : Player.Player2;
    public GameState CurrentGameState { get; private set; } = GameState.InProgress;

    private readonly Player[,] _board;

    public Connect4(int rows, int columns, int winningLength,
        string empty, string player1, string player2)
    {
        if (winningLength < 2)
        {
            throw new ArgumentException("Winning length must be at least 2", "winningLength");
        }
        if (rows < winningLength || columns < winningLength)
        {
            throw new ArgumentException("Rows and columns must be at least as large as winningLength", "rows, columns, winningLength");
        }

        _rows = rows;
        _columns = columns;
        _winningLength = winningLength;

        _empty = empty;
        _player1 = player1;
        _player2 = player2;

        RemainingMoves = _rows * _columns;
        _totalMoves = RemainingMoves;
        _board = new Player[_rows, _columns];
    }

    public Connect4(Connect4 connect4, bool swapPlayerTurns = false)
    {
        _rows = connect4._rows;
        _columns = connect4._columns;
        _winningLength = connect4._winningLength;

        _empty = connect4._empty;
        _player1 = connect4._player1;
        _player2 = connect4._player2;

        RemainingMoves = connect4.RemainingMoves;
        if (swapPlayerTurns)
        {
            _player1Turn = !connect4._player1Turn;
        }
        else
        {
            _player1Turn = connect4._player1Turn;
        }
        CurrentGameState = connect4.CurrentGameState;

        _board = new Player[_rows, _columns];
        for (int i = 0; i < _rows; i++)
        {
            for (int j = 0; j < _columns; j++)
            {
                _board[i, j] = connect4._board[i, j];
            }
        }
    }

    private string PlayerToString(Player player)
    {
        switch (player)
        {
            case Player.Player1:
                return _player1;
            case Player.Player2:
                return _player2;
            default:
                return _empty;
        }
    }

    public void PrintBoard()
    {
        string board = ToString();
        Console.WriteLine();
        Console.WriteLine(board);
    }

    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i <= _rows; i++)
        {
            sb.Append(i + " ");
        }
        sb.AppendLine();
        for (int i = 0; i < _rows; i++)
        {
            for (int j = 0; j < _columns; j++)
            {
                sb.Append(PlayerToString(_board[i, j]) + " ");
            }
            sb.AppendLine();
        }

        return sb.ToString();
    }

    public List<int> GetValidMoves()
    {
        List<int> validMoves = new List<int>();
        for (int j = 0; j < _columns; j++)
        {
            if (_board[0, j] == Player.None)
            {
                validMoves.Add(j);
            }
        }
        return validMoves;
    }

    public bool IsValidMove(int column)
    {
        return column >= 0 &&
            column < _columns &&
                _board[0, column] == Player.None;
    }

    //public GameState GetGameState()
    //{
    //    for (int i = 0; i < _rows; i++)
    //    {
    //        for (int j = 0; j < _columns; j++)
    //        {
    //            if (_board[i, j] != Player.None)
    //            {
    //                if (CheckWin(_board[i, j]))
    //                {
    //                    return _board[i, j] == Player.Player1 ? GameState.Player1Win : GameState.Player2Win;
    //                }
    //            }
    //        }
    //    }
    //    return _remainingMoves == 0 ? GameState.Draw : GameState.InProgress;
    //}

    public GameState MakeMove(int column)//, Player player)
    {
        //if (player == Player.None)
        //{
        //    throw new ArgumentException("Player must be either Player1 or Player2", "player");
        //}

        if (CurrentGameState != GameState.InProgress || RemainingMoves < 1)
        {
            throw new InvalidOperationException("Game is already over");
        }
        if (!IsValidMove(column))
        {
            throw new ArgumentException("Invalid move", nameof(column));
        }

        for (int i = _rows - 1; i >= 0; i--)
        {
            if (_board[i, column] == Player.None)
            {
                _board[i, column] = CurrentPlayerTurn;
                break;
            }
        }

        RemainingMoves--;

        if (CheckWin(CurrentPlayerTurn))
        {
            CurrentGameState = _player1Turn ? GameState.Player1Win : GameState.Player2Win;
        }
        else if (RemainingMoves < 1)
        {
            CurrentGameState = GameState.Draw;
        }
        else
        {
            CurrentGameState = GameState.InProgress;
            _player1Turn = !_player1Turn;
        }

        return CurrentGameState;
    }

    private bool CheckWin(Player player)
    {
        for (int i = 0; i < _rows; i++)
        {
            for (int j = 0; j < _columns; j++)
            {
                if (_board[i, j] == player)
                {
                    if (CheckWinDirection(player, i, j, 1, 0) ||
                        CheckWinDirection(player, i, j, 0, 1) ||
                            CheckWinDirection(player, i, j, 1, 1) ||
                                CheckWinDirection(player, i, j, 1, -1))
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    private bool CheckWinDirection(Player player, int row, int column, int rowDir, int colDir)
    {
        int count = 1;
        for (int i = 1; i < _winningLength; i++)
        {
            int newRow = row + i * rowDir;
            int newCol = column + i * colDir;
            if (newRow < 0 || newRow >= _rows || newCol < 0 || newCol >= _columns || _board[newRow, newCol] != player)
            {
                break;
            }
            count++;
        }
        for (int i = 1; i < _winningLength; i++)
        {
            int newRow = row - i * rowDir;
            int newCol = column - i * colDir;
            if (newRow < 0 || newRow >= _rows || newCol < 0 || newCol >= _columns || _board[newRow, newCol] != player)
            {
                break;
            }
            count++;
        }
        return count >= _winningLength;
    }

    private bool IsValidUndoMove(int column)
    {
        return column >= 0 &&
            column < _columns &&
                _board[_rows - 1, column] != Player.None;
    }

    public void UndoMove(int column)
    {
        if (RemainingMoves >= _totalMoves)
        {
            throw new InvalidOperationException("No moves to undo");
        }
        if (!IsValidUndoMove(column))
        {
            throw new ArgumentException("Invalid undo move", nameof(column));
        }

        for (int i = 0; i < _rows; i++)
        {
            if (_board[i, column] != Player.None)
            {
                _board[i, column] = Player.None;
                break;
            }
        }

        RemainingMoves++;
        CurrentGameState = GameState.InProgress;
        _player1Turn = !_player1Turn;
    }
}
