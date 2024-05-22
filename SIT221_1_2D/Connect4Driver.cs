namespace SIT221_1_2D;

public class Connect4Driver
{
    public void PlayConnect4()
    {
        Connect4 connect4 = new Connect4(6, 7, 4, ".", "X", "O");

        while(connect4.CurrentGameState == Connect4.GameState.InProgress)
        {
            connect4.PrintBoard();
            if (connect4.CurrentPlayer == Connect4.Player.Player1)
            {
                Console.WriteLine("Player 1's turn");
            }
            else
            {
                Console.WriteLine("Player 2's turn");
            }

            string input;
            int column;
            do
            {
                Console.Write("Enter column: ");
                input = Console.ReadLine();
            } while (!int.TryParse(input, out column) || !connect4.IsValidMove(column));

            connect4.MakeMove(column);
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
}
