using System;
using System.Collections.Generic;

namespace TicTacToeRL
{
    class Program
    {
        static Random rand = new Random();
        static int[] gameBoard = new int[9] { 0, 0, 0, 0, 0, 0, 0, 0, 0 };

        /* State: Board position & which player's turn, move -> value for move*/
        static float[,,] qlTable = new float[19682, 2, 9];
        static void Main()
        {
            /* Initialize QTable with random values from [-1,1]*/
            for (int i = 0; i < 19682; ++i)
            {
                /* board occupation */
                for (int j = 0; j < 2; ++j)
                {
                    /* which players turn */
                    for (int k = 0; k < 9; ++k)
                    {
                        /* which move */
                        qlTable[i, j, k] = rand.Next(0, 1000) / 500.0f - 1;
                    }
                }
            }

            int statePrime = 0;
            float alpha = 0.9f; /* Learning rate */
            float gamma = 1.0f; /* Discount rate */
            float epsilon = 0.8f; /* Probability for random move -> "exploration rate" */

            for (int i = 0; true; ++i)
            {
                if (i % 5000 == 0 && i != 0)
                {
                    int startState = GetState(new int[9] { 0, 0, 0, 0, 0, 0, 0, 0, 0 });
                    Console.Clear();
                    Console.WriteLine("After " + i + " training games the best starting moves are (1: winning; -1: losing; 0: tied):\n");
                    Console.Write(qlTable[startState, 0, 0].ToString("0.000") + " ");
                    Console.Write(qlTable[startState, 0, 1].ToString("0.000") + " ");
                    Console.Write(qlTable[startState, 0, 2].ToString("0.000") + "\n");
                    Console.Write(qlTable[startState, 0, 3].ToString("0.000") + " ");
                    Console.Write(qlTable[startState, 0, 4].ToString("0.000") + " ");
                    Console.Write(qlTable[startState, 0, 5].ToString("0.000") + "\n");
                    Console.Write(qlTable[startState, 0, 6].ToString("0.000") + " ");
                    Console.Write(qlTable[startState, 0, 7].ToString("0.000") + " ");
                    Console.Write(qlTable[startState, 0, 8].ToString("0.000") + "\n\n");

                    PlayVsRandomPlayer(10000);
                    while (true)
                    {
                        Console.WriteLine("Press ENTER for another 5000 training games or t for a test game!\n");
                        string response = Console.ReadLine();
                        if (response.Equals("t"))
                            PlayVsHuman();
                        else break;
                    }

                }
                int currPlayer = 1;
                gameBoard = new int[9] { 0, 0, 0, 0, 0, 0, 0, 0, 0 };

                while (true)
                {
                    int state = GetState(gameBoard);

                    /*if game is done, immediately update qvalue.. */
                    if (CheckWinner(gameBoard) == 1)
                    {
                        for (int k = 0; k < 9; ++k)
                            qlTable[state, currPlayer - 1, k] = 1.0f;
                        break;
                    }
                    else if (CheckWinner(gameBoard) == 2)
                    {
                        for (int k = 0; k < 9; ++k)
                            qlTable[state, currPlayer - 1, k] = -1.0f;
                        break;
                    }
                    else if (CheckDraw(gameBoard))
                    {
                        for (int k = 0; k < 9; ++k)
                            qlTable[state, currPlayer - 1, k] = 0.0f;
                        break;
                    }

                    /*Find best action for current player.. */
                    int action = -1;
                    float randomFloat = (float)rand.NextDouble();
                    if (randomFloat > epsilon)
                    {
                        if (currPlayer == 1)
                        {
                            action = FindHighMove(gameBoard, state, currPlayer);
                        }
                        else
                        {
                            action = FindLowMove(gameBoard, state, currPlayer);
                        }
                    }
                    else
                    {
                        action = GetRandomMove(gameBoard);
                    }

                    /* ..execute it */
                    gameBoard[action] = currPlayer;

                    int nextPlayer = currPlayer;
                    if (nextPlayer == 1) nextPlayer++;
                    else nextPlayer--;

                    statePrime = GetState(gameBoard);

                    /*Propagate qvalue from next state*/
                    int nextAction = -1;
                    if (nextPlayer == 1)
                        nextAction = FindHighMove(gameBoard, statePrime, nextPlayer);
                    if (nextPlayer == 2)
                        nextAction = FindLowMove(gameBoard, statePrime, nextPlayer);

                    /* Update Q value:
                    * Q[s, a] = Q[s, a] + α(r + γmaxa' Q[s', a'] - Q[s,a]) */
                    qlTable[state, currPlayer - 1, action] = qlTable[state, currPlayer - 1, action] +
                        alpha * (gamma * qlTable[statePrime, nextPlayer - 1, nextAction] - qlTable[state, currPlayer - 1, action]);

                    currPlayer = nextPlayer;
                }
            }
        }
        static int FindHighMove(int[] GameBoard, int state, int player)
        {
            /* Choose best move*/
            int maxAction = 0;
            float maxActionVal = -1;

            /* Find maximum action */
            for (int j = 0; j < 9; ++j)
            {
                if (qlTable[state, player - 1, j] >= maxActionVal && GameBoard[j] == 0)
                {
                    maxAction = j;
                    maxActionVal = qlTable[state, player - 1, j];
                }
            }
            return maxAction;
        }
        static int FindLowMove(int[] GameBoard, int state, int player)
        {
            /* Choose worst move*/
            int minAction = 0;
            float minActionVal = 1;

            /* Find maximum action */
            for (int j = 0; j < 9; ++j)
            {
                if (qlTable[state, player - 1, j] <= minActionVal && GameBoard[j] == 0)
                {
                    minAction = j;
                    minActionVal = qlTable[state, player - 1, j];
                }
            }
            return minAction;
        }
        /* player: 1, 2*/
        static int GetState(int[] gameBoard)
        {
            return (gameBoard[0] * 1 + gameBoard[1] * 3 +
            gameBoard[2] * 9 + gameBoard[3] * 27 +
            gameBoard[4] * 81 + gameBoard[5] * 243 +
            gameBoard[6] * 729 + gameBoard[7] * 2187 +
            gameBoard[8] * 6561);
        }
        /* 0 none, 1 player 1, 2 player 2*/
        static int CheckWinner(int[] gameBoard)
        {
            if (gameBoard[0] == 1 && gameBoard[1] == 1 && gameBoard[2] == 1 ||
               gameBoard[3] == 1 && gameBoard[4] == 1 && gameBoard[5] == 1 ||
               gameBoard[6] == 1 && gameBoard[7] == 1 && gameBoard[8] == 1 ||

               gameBoard[0] == 1 && gameBoard[3] == 1 && gameBoard[6] == 1 ||
               gameBoard[1] == 1 && gameBoard[4] == 1 && gameBoard[7] == 1 ||
               gameBoard[2] == 1 && gameBoard[5] == 1 && gameBoard[8] == 1)
                return 1;
            if (gameBoard[0] == 2 && gameBoard[1] == 2 && gameBoard[2] == 2 ||
               gameBoard[3] == 2 && gameBoard[4] == 2 && gameBoard[5] == 2 ||
               gameBoard[6] == 2 && gameBoard[7] == 2 && gameBoard[8] == 2 ||

               gameBoard[0] == 2 && gameBoard[3] == 2 && gameBoard[6] == 2 ||
               gameBoard[1] == 2 && gameBoard[4] == 2 && gameBoard[7] == 2 ||
               gameBoard[2] == 2 && gameBoard[5] == 2 && gameBoard[8] == 2)
                return 2;
            else return 0;
        }
        /* returns: 0 no winner, 1-> player 1, 2 -> player 2*/
        static bool CheckDraw(int[] gameBoard)
        {
            /*Assume no winner*/
            for (int j = 0; j < 9; ++j)
            {
                if (gameBoard[j] == 0) return false;
            }
            return true;
        }
        static int GetRandomMove(int[] gameBoard)
        {
            List<int> moves = new List<int>();
            for (int i = 0; i < gameBoard.Length; ++i)
            {
                if (gameBoard[i] == 0)
                    moves.Add(i);
            }
            return moves[rand.Next(0, moves.Count)];
        }
        static void PlayVsRandomPlayer(int nofGames)
        {
            float winsAI = 0;
            float winsRandomPlayer = 0;
            float ties = 0;
            Random rand = new Random();

            for (int j = 0; j < nofGames; ++j)
            {
                gameBoard = new int[9] { 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                int AIPlayer = rand.Next(0, 2) + 1;
                int randomPlayer = (AIPlayer == 1) ? 2: 1;

                while (true)
                {
                    if (AIPlayer == 1)
                    {
                        int state = GetState(gameBoard);

                        /*Find best action for current player.. */
                        int action = FindHighMove(gameBoard, state, AIPlayer);

                        /* ..execute it */
                        gameBoard[action] = AIPlayer;

                        if (CheckWinner(gameBoard) == AIPlayer)
                        {
                            winsAI++;
                            break;
                        }
                        else if (CheckWinner(gameBoard) == randomPlayer)
                        {
                            winsRandomPlayer++;
                            break;
                        }

                        else if (CheckDraw(gameBoard))
                        {
                            ties++;
                            break;
                        }
                        action = GetRandomMove(gameBoard);
                        gameBoard[action] = randomPlayer;

                        if (CheckWinner(gameBoard) == AIPlayer)
                        {
                            winsAI++;
                            break;
                        }
                        else if (CheckWinner(gameBoard) == randomPlayer)
                        {
                            winsRandomPlayer++;
                            break;
                        }

                        else if (CheckDraw(gameBoard))
                        {
                            ties++;
                            break;
                        }
                    }
                    else {
                        int state = GetState(gameBoard);
                        int action = GetRandomMove(gameBoard);
                        gameBoard[action] = randomPlayer;

                        if (CheckWinner(gameBoard) == AIPlayer)
                        {
                            winsAI++;
                            break;
                        }
                        else if (CheckWinner(gameBoard) == randomPlayer)
                        {
                            winsRandomPlayer++;
                            break;
                        }

                        else if (CheckDraw(gameBoard))
                        {
                            ties++;
                            break;
                        }

                        int statePrime = GetState(gameBoard);

                        /*Find best action for AI player.. */
                        action = FindLowMove(gameBoard, statePrime, AIPlayer);

                        /* ..execute it */
                        gameBoard[action] = AIPlayer;

                        if (CheckWinner(gameBoard) == AIPlayer)
                        {
                            winsAI++;
                            break;
                        }
                        else if (CheckWinner(gameBoard) == randomPlayer)
                        {
                            winsRandomPlayer++;
                            break;
                        }

                        else if (CheckDraw(gameBoard))
                        {
                            ties++;
                            break;
                        }
                    }
                }
            }
            Console.WriteLine("Evaluation vs. random player (" + nofGames + " games): "  + winsAI + " wins, " + winsRandomPlayer + " losses, " + ties +
                " ties, " + (winsAI / nofGames).ToString("0.00") + " winrate\n");
        }
        static void PlayVsHuman()
        {
            gameBoard = new int[9] { 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            int AIPlayer = rand.Next(0, 2) + 1;
            int humanPlayer = (AIPlayer == 1) ? 2 : 1;

            Console.WriteLine("You are player " + humanPlayer +" human!\n");

            while (true)
            {
                if (AIPlayer == 1)
                {
                    int state = GetState(gameBoard);

                    /*Find best action for current player.. */
                    int action = -1;
                    if (AIPlayer == 1)
                        action = FindHighMove(gameBoard, state, AIPlayer);
                    else
                        action = FindLowMove(gameBoard, state, AIPlayer);

                    /* ..execute it */
                    gameBoard[action] = AIPlayer;

                    Console.Clear();
                    Console.Write(gameBoard[0].ToString("0") + " ");
                    Console.Write(gameBoard[1].ToString("0") + " ");
                    Console.Write(gameBoard[2].ToString("0") + "\t");
                    Console.Write("0 1 2\n");
                    Console.Write(gameBoard[3].ToString("0") + " ");
                    Console.Write(gameBoard[4].ToString("0") + " ");
                    Console.Write(gameBoard[5].ToString("0") + "\t");
                    Console.Write("3 4 5\n");
                    Console.Write(gameBoard[6].ToString("0") + " ");
                    Console.Write(gameBoard[7].ToString("0") + " ");
                    Console.Write(gameBoard[8].ToString("0") + "\t");
                    Console.Write("6 7 8\n\n");

                    if (CheckWinnerAIvsHuman(gameBoard, AIPlayer, humanPlayer)) break;

                    Console.WriteLine("Enter your move (choose a field from 0 to 8): ");
                    int responseField = Convert.ToInt32(Console.ReadLine());
                    gameBoard[responseField] = humanPlayer;

                    if (CheckWinnerAIvsHuman(gameBoard, AIPlayer, humanPlayer)) break;
                }
                else
                {
                    Console.Clear();
                    Console.Write(gameBoard[0].ToString("0") + " ");
                    Console.Write(gameBoard[1].ToString("0") + " ");
                    Console.Write(gameBoard[2].ToString("0") + "\n");
                    Console.Write(gameBoard[3].ToString("0") + " ");
                    Console.Write(gameBoard[4].ToString("0") + " ");
                    Console.Write(gameBoard[5].ToString("0") + "\n");
                    Console.Write(gameBoard[6].ToString("0") + " ");
                    Console.Write(gameBoard[7].ToString("0") + " ");
                    Console.Write(gameBoard[8].ToString("0") + "\n\n");

                    Console.WriteLine("Enter your move (choose a field from 0 to 8): ");
                    int responseField = Convert.ToInt32(Console.ReadLine());
                    gameBoard[responseField] = humanPlayer;

                    if (CheckWinnerAIvsHuman(gameBoard, AIPlayer, humanPlayer)) break;

                    int state = GetState(gameBoard);

                    /*Find best action for current player.. */
                    int action = -1;
                    if (AIPlayer == 1)
                        action = FindHighMove(gameBoard, state, AIPlayer);
                    else
                        action = FindLowMove(gameBoard, state, AIPlayer);

                    /* ..execute it */
                    gameBoard[action] = AIPlayer;

                    Console.Write(gameBoard[0].ToString("0") + " ");
                    Console.Write(gameBoard[1].ToString("0") + " ");
                    Console.Write(gameBoard[2].ToString("0") + "\n");
                    Console.Write(gameBoard[3].ToString("0") + " ");
                    Console.Write(gameBoard[4].ToString("0") + " ");
                    Console.Write(gameBoard[5].ToString("0") + "\n");
                    Console.Write(gameBoard[6].ToString("0") + " ");
                    Console.Write(gameBoard[7].ToString("0") + " ");
                    Console.Write(gameBoard[8].ToString("0") + "\n\n");

                    if (CheckWinnerAIvsHuman(gameBoard, AIPlayer, humanPlayer)) break;
                }
            }
        }
        static bool CheckWinnerAIvsHuman(int[] gameBoard, int AIPlayer, int humanPlayer)
        {
            if (CheckWinner(gameBoard) == AIPlayer)
            {
                Console.WriteLine("You have lost! =(\n");
                return true;
            }
            else if (CheckWinner(gameBoard) == humanPlayer)
            {
                Console.WriteLine("You have won! =)\n");
                return true;
            }

            else if (CheckDraw(gameBoard))
            {
                Console.WriteLine("The game is drawn! :/\n");
                return true;
            }
            return false;
        }
        static bool HasMoves(int[] gameBoard)
        {
            for (int j = 0; j < 9; ++j)
            {
                if (gameBoard[j] == 0) return true;
            }
            return false;
        }
    }
}