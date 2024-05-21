using System;

namespace SIT221_1_2D;

public class Connect4MCTSDriver
{
    public void PlayConnect4MCTS(bool goFirst = true, int iterations = 1000,
        double explorationFactor = 1.4142, int rows = 6, int columns = 7,
        int winningLength = 4)
    {
        Connect4 connect4 = new Connect4(rows, columns, winningLength, ".", "X", "O");
        MonteCarloTreeSearch mcts = new MonteCarloTreeSearch(iterations, explorationFactor);

        while(connect4.CurrentGameState == Connect4.GameState.InProgress)
        {
            connect4.PrintBoard();
            if (connect4.CurrentPlayerTurn == Connect4.Player.Player1)
            {
                Console.WriteLine("Player 1's turn");
                if (goFirst)
                {
                    int column = GetPlayerMove(connect4);
                    connect4.MakeMove(column);
                }
                else
                {
                    MonteCarloTreeSearch.MoveResult moveResult = mcts.FindBestMove(connect4);
                    Console.WriteLine(moveResult);
                    connect4.MakeMove(moveResult.OptimalMove);
                }
            }
            else
            {
                Console.WriteLine("Player 2's turn");

                if (goFirst)
                {
                    MonteCarloTreeSearch.MoveResult moveResult = mcts.FindBestMove(connect4);
                    Console.WriteLine(moveResult);
                    connect4.MakeMove(moveResult.OptimalMove);
                }
                else
                {
                    int column = GetPlayerMove(connect4);
                    connect4.MakeMove(column);
                }
            }

        }

        connect4.PrintBoard();
        Connect4.GameState gameState = connect4.CurrentGameState;
        if (gameState == Connect4.GameState.Player1Win)
        {
            Console.WriteLine("Player 1 wins!");
        }
        else if (gameState == Connect4.GameState.Player2Win)
        {
            Console.WriteLine("Player 2 wins!");
        }
        else
        {
            Console.WriteLine("It's a draw!");
        }
    }

    private int GetPlayerMove(Connect4 connect4)
    {
        string input;
        int column;
        do
        {
            Console.Write("Enter column: ");
            input = Console.ReadLine();
        } while (!int.TryParse(input, out column) || !connect4.IsValidMove(column));
        return column;
    }
}
