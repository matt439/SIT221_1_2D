namespace SIT221_1_2D;

public class Program
{
    static void Main(string[] args)
    {
        // Player vs Player
        //Connect4Driver connect4Driver = new Connect4Driver();
        //connect4Driver.PlayConnect4();

        // Player vs AI
        Connect4MCTSDriver connect4MCTSDriver = new Connect4MCTSDriver();
        double explorationFactor = Math.Sqrt(2.0);
        connect4MCTSDriver.PlayConnect4MCTS(false, 10000, explorationFactor, 6, 7, 4);

        Console.ReadLine();
    }
}