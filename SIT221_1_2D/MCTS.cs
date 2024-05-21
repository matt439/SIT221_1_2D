using System.CodeDom.Compiler;
using System.Runtime.InteropServices;
using System.Text;

namespace SIT221_1_2D;

public class MonteCarloTreeSearch
{
    private readonly int _maxIterations;
    private readonly double _explorationParameter;
    private readonly Random _random;

    public MonteCarloTreeSearch(int maxIterations, double explorationParameter)
    {
        _maxIterations = maxIterations;
        _explorationParameter = explorationParameter;
        _random = new Random();
    }

    public struct MoveResult
    {
        public int OptimalMove = -1;
        public int OptimalMoveNodeIndex = -1;
        public string Children = string.Empty;

        public MoveResult(int optimalMove, int optimalMoveNodeIndex, string children)
        {
            OptimalMove = optimalMove;
            OptimalMoveNodeIndex = optimalMoveNodeIndex;
            Children = children;
        }

        public override string ToString()
        {
            string result = string.Empty;
            result += Children;
            result += $"Optimal Move: {OptimalMove}, Optimal Move Node Index: {OptimalMoveNodeIndex}";
            return result;
        }
    }

    public MoveResult FindBestMove(Connect4 connect4)
    {
        Node rootNode = new Node(connect4, null, -1, _explorationParameter);
        GenerateChildren(rootNode);

        for (int i = 0; i < _maxIterations; i++)
        {
            Node node = rootNode;

            // Selection
            node = Selection(rootNode);
            if (node == null)
            {
                throw new Exception("No node selected");
            }

            // Expansion
            Node? expandedNode = Expansion(node);
            if (expandedNode == null)
            {
                BackPropagate(node, node.Connect4.CurrentGameState);
                continue;
            }

            // Simulation
            Connect4 simulationConnect4 = new Connect4(expandedNode.Connect4);
            Simulation(simulationConnect4);

            // Backpropagation
            BackPropagate(expandedNode, simulationConnect4.CurrentGameState);
        }

        MoveResult result = new MoveResult();

        int optimalMoveIndex = rootNode.SelectMostVisitedChild();
        result.OptimalMoveNodeIndex = optimalMoveIndex;
        if (optimalMoveIndex == -1)
        {
            return result;
        }
        result.OptimalMove = rootNode.ChildNodes[optimalMoveIndex].Move;
        result.Children = rootNode.ToStringWithChildren();
        return result;
    }

    private void GenerateChildren(Node parent)
    {
        List<int> validMoves = new List<int>();
        for (int i = 0; i < parent.UntriedMoves.Count; i++)
        {
            validMoves.Add(parent.UntriedMoves[i]);
        }
        if (validMoves.Count < 1)
        {
            throw new Exception("No valid moves");
        }
        foreach (int move in validMoves)
        {
            Node childNode = new Node(parent.Connect4, parent, move, _explorationParameter);
            try
            {
                parent.ChildNodes.Add(childNode);
            }
            catch (Exception e)
            {
                throw new Exception("Invalid move");
            }
            parent.UntriedMoves.Remove(move);
        }
    }

    private Node Selection(Node root)
    {
        while (!root.IsLeaf)
        {
            Node? child = root.SelectHighestValueChild();
            if (child == null)
            {
                throw new Exception("No child nodes");
            }
            root = child;
        }
        return root;
    }

    private Node? Expansion(Node node)
    {
        if (node.Visits < 1 || node.IsTerminal)
        {
            return null;
        }

        GenerateChildren(node);

        // select a random child node
        Random random = new Random();
        int randomChildIndex = random.Next(node.ChildNodes.Count);
        return node.ChildNodes[randomChildIndex];
    }

    private Connect4.GameState Simulation(Connect4 connect4)
    {
        Connect4.GameState gameState = connect4.CurrentGameState;
        if (gameState != Connect4.GameState.InProgress)
        {
            return gameState;
        }
        while (gameState == Connect4.GameState.InProgress)
        {
            List<int> validMoves = connect4.GetValidMoves();

            int randomMoveIndex = _random.Next(validMoves.Count);
            int randomMove = validMoves[randomMoveIndex];
            try
            {
                gameState = connect4.MakeMove(randomMove);
            }
            catch (Exception e)
            {
                throw new Exception("Invalid move");
            }
        }
        return gameState;
    }

    private void BackPropagate(Node node, Connect4.GameState gameState)
    {
        while (node != null)
        {
            node.Update(gameState);
            node = node.Parent;
        }
    }

    private class Node
    {
        public Node Parent { get; }
        public int Move { get; }
        public Connect4 Connect4 { get; }
        public Connect4.Player PlayerTurn => Connect4.CurrentPlayerTurn;
        public Connect4.GameState GameState => this.Connect4.CurrentGameState;
        public List<int> UntriedMoves { get; }
        public List<Node> ChildNodes { get; }
        public int Wins { get; private set; }
        public int Loses { get; private set; }
        public int Draws { get; private set; }
        public int Visits => Wins + Loses + Draws;
        public bool IsLeaf => ChildNodes.Count < 1;
        public bool IsRoot => Parent == null;
        public bool IsTerminal => GameState != Connect4.GameState.InProgress ||
            (UntriedMoves.Count < 1 && ChildNodes.Count < 1);
        public float WinRate => Wins / (float)Visits;

        public double Value
        {
            get
            {
                if (IsRoot)
                {
                    return -1.0;
                }
                if (Visits < 1)
                {
                    return double.MaxValue;
                }
                return Wins / (double)Visits +
                    _explorationParameter * Math.Sqrt(Math.Log(Parent.Visits) / Visits);
            }
        }

        private readonly double _explorationParameter;

        public Node(Connect4 connect4, Node parent, int move,
            double explorationParameter)
        {
            Parent = parent;
            Move = move;
            
            if (!IsRoot)
            {
                Connect4 = new Connect4(connect4);
                try
                {
                    Connect4.MakeMove(move);
                }
                catch (Exception e)
                {
                    throw new Exception("Invalid move");
                }
            }
            else
            {
                Connect4 = new Connect4(connect4, true);
            }
            if (Connect4.CurrentGameState != Connect4.GameState.InProgress)
            {
                UntriedMoves = new List<int>();
            }
            else
            {
                UntriedMoves = Connect4.GetValidMoves();
            }
            ChildNodes = new List<Node>();
            Wins = 0;
            Loses = 0;
            Draws = 0;
            _explorationParameter = explorationParameter;
        }

        public Node AddChild(Connect4 connect4, int move)
        {
            Node childNode = new Node(connect4, this, move, _explorationParameter);
            UntriedMoves.Remove(move);
            ChildNodes.Add(childNode);
            return childNode;
        }

        public Node? SelectHighestValueChild()
        {
            Node? bestChild = null;
            double maxUCB = double.MinValue;
            foreach (Node child in ChildNodes)
            {
                double ucb = child.Value;
                if (ucb > maxUCB)
                {
                    maxUCB = ucb;
                    bestChild = child;
                }
            }
            return bestChild;
        }

        public int SelectMostVisitedChild()
        {
            int mostVisitedIndex = -1;

            int maxVisits = 0;
            for (int i = 0; i < ChildNodes.Count; i++)
            {
                if (ChildNodes[i].Visits > maxVisits)
                {
                    maxVisits = ChildNodes[i].Visits;
                    mostVisitedIndex = i;
                }
            }
            return mostVisitedIndex;
        }

        public void Update(Connect4.GameState gameState)
        {
            if (gameState == Connect4.GameState.Draw)
            {
                Draws++;
            }
            else if (gameState == Connect4.GameState.Player1Win &&
                PlayerTurn == Connect4.Player.Player1)
            {
                Wins++;
            }
            else if (gameState == Connect4.GameState.Player2Win &&
                PlayerTurn == Connect4.Player.Player2)
            {
                Wins++;
            }
            else
            {
                Loses++;
            }
        }

        public override string ToString()
        {
            return $"Move: {Move}, Wins: {Wins}, Visits: {Visits}, WinRate: {WinRate}, ChildNodes: {ChildNodes.Count}, Value: {Value}";
        }

        public string ToStringWithChildren()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Root: " + ToString() + "\nRoot's children:");
            for (int i = 0; i < ChildNodes.Count; i++)
            {
                sb.AppendLine(i.ToString() + ": " + ChildNodes[i].ToString());
            }
            return sb.ToString();
        }
    }
}
