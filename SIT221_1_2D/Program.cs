﻿namespace SIT221_1_2D;

public class Program
{
    static void Main(string[] args)
    {
        //Connect4Driver connect4Driver = new Connect4Driver();
        //connect4Driver.PlayConnect4();

        Connect4MCTSDriver connect4MCTSDriver = new Connect4MCTSDriver();
        double explorationFactor = Math.Sqrt(2.0);
        //double explorationFactor = 1.7;
        connect4MCTSDriver.PlayConnect4MCTS(true, 100, explorationFactor);

        Console.ReadLine();
    }
}