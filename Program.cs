using System;
using System.Collections.Generic;

namespace FinalProject
{
    class GameState
	{
        private GameConsole console;
        public bool ended = false;
        /**
          * Game state X/O grid
          * [
          *  [ "", "X", "O", "", "", "", "O" ],
          *  [ "", "X", "O", "", "", "", "O" ],
          * ]
          */
        public List<String[]> grid { get; private set; } = new List<String[]>();
        public Player winner { get; set; }

        public GameState( GameConsole console )
		{
            this.console = console;
            this.Reset();
        }

        // returns 2 dim. array of unreserved grid coordinates
        // [ [0,1], [2,3], [3,5] ]
        public List<int[]> GetAvailableGridItems()
		{
            List<int[]> available = new List<int[]>();

            for (int col = 0; col < console.cols; col++)
            {
                available.Add(new int[2] { -1, -1 });
            }

            for (int row = 0; row < console.rows; row++)
            {
                for (int col = 0; col < console.cols; col++)
                {
                    if ( null == grid[row][col] )
					{
                        available[col] = new int[2] { row, col };
                    }
                }
            }

            foreach ( int[] entry in available.FindAll(e => -1 == e[0] && -1 == e[0]))
			{
                available.Remove(entry);
            }

            return available;
        }

        // mark grid for user
        public void AcquireGridItem( Player player, int[] spot )
		{
            grid[spot[0]][spot[1]] = player.GetAlias();
            ended = GetAvailableGridItems().Count == 0;
        }

        // reset game state
        public void Reset()
		{
            ended = false;

            grid = new List<String[]>();

            for (int row = 0; row < console.rows; row++)
            {
                grid.Add(new String[console.cols]);
            }

            winner = null;
		}
    }

    class GameConsole
	{
        public int rows { get; }
        public int cols { get; }
        private int originCursorLeft, originCursorTop;
        private bool cursorSetup = true;

        public GameConsole( int rows, int cols )
		{
            this.rows = rows;
            this.cols = cols;
        }

        // draw grid and controls
        public void Draw( GameState state, Player currentPlayer )
		{
            if ( cursorSetup )
			{
                // record original cursor position for the console
                originCursorLeft = Console.CursorLeft;
                originCursorTop = Console.CursorTop;
                cursorSetup = false;
            }

            // adjust to cursor position to update grid/controls
            Console.SetCursorPosition(originCursorLeft, originCursorTop);

            for ( int row=0; row<rows; row++ )
			{
                Console.Write("\n| ");

                for (int col=0; col<cols; col++)
                {
                    Console.Write("{0} ", null == state.grid[row][col] ? "#" : state.grid[row][col]);
                }

                Console.Write("|");
            }

            Console.Write("\n  ");

            for (int col=0; col<cols; col++)
            {
                Console.Write("{0} ", col+1);
            }

            Console.Write("\n");

            if ( ! state.ended )
			{
                String message = String.Format("Now Playing: {0} ({1})", currentPlayer.GetName(), currentPlayer.GetAlias());
                // add extra space to clear remainder of previous line if any
                Console.WriteLine("{0}{1}", message, new string(' ', Console.WindowWidth - message.Length));
                // clear restart line below
                Console.Write(new string(' ', Console.WindowWidth));
            }
            else
			{
                if ( null != state.winner )
				{
                    Console.WriteLine("It is a Connect 4. {0} wins!", state.winner.GetName());
                } else
				{
                    Console.WriteLine("It is a draw, nobody won.");
                }
                Console.Write("Restart? Yes(1) No(0):");
            }
        }
    }

    // game registry class
    class Game
	{
        public Player player1 { get; }
        public Player player2 { get; }
        private GameConsole console;
        private GameState state;
        private Player currentPlayer;

        public Game( Player player1, Player player2, GameConsole console, GameState state )
		{
            this.player1 = player1;
            this.player2 = player2;
            this.console = console;
            this.state = state;
            this.currentPlayer = player1;
        }

        public void Play()
		{
            // draw grid and controls
            console.Draw(state, currentPlayer);

            while ( true )
			{
                // retrieve available grid items
                List<int[]> availableSpots = this.state.GetAvailableGridItems();

                if ( state.ended )
				{
                    // listen for restart
                    char input = Console.ReadKey(true).KeyChar;
                    Console.Write(input);

                    if ( '1' == input )
					{
                        // reset game state
                        state.Reset();

                        // set player to first
                        currentPlayer = player1;

                        // update game grid/controls
                        console.Draw(state, currentPlayer);
                    } else if ( '0' == input )
					{
                        // no restart requested, exit loop and thus program
                        break;
					}
				} else if ( availableSpots.Count > 0 )
				{
                    // read 1 key from user
                    char input = Console.ReadKey(true).KeyChar;

                    try
                    {
                        // attempt to parse integer
                        int num = int.Parse(input.ToString());

                        // attempt to locate specified spot
                        int[] spot = availableSpots.Find(e => e[1] == num-1);

                        if ( null == spot )
                            continue;

                        // mark spot for current player
						state.AcquireGridItem(currentPlayer, spot);

						// swap players
						currentPlayer = player1.GetAlias() == currentPlayer.GetAlias() ? player2 : player1;

                        Player winner;

                        // check if any player won
                        if (CheckWinner(player1))
                            winner = player1;
                        else if (CheckWinner(player2))
                            winner = player2;
                        else winner = null;

                        if ( null != winner ) // we have a winner, update game state
						{
                            state.ended = true;
                            state.winner = winner;
						}

                        // re-draw game grid
                        console.Draw(state, currentPlayer);
                    } catch ( Exception e ) {}
                }
            }
        }

        // safely retrieve a grid element by x,y coords
        private String getGridKey(int row, int col)
        {
            if ( row >= 0 && col >= 0 && row < console.rows && col < console.cols )
                return state.grid[row][col];

            return "";
        }

        public bool CheckWinner( Player player )
		{
            // horizontal check
            for (int row = 0; row < console.rows; row++)
            {
                for (int col = 0; col < console.cols; col++)
				{
                    if (player.GetAlias() == getGridKey(row, col) && player.GetAlias() == getGridKey(row, col + 1) && player.GetAlias() == getGridKey(row, col + 2) && player.GetAlias() == getGridKey(row, col + 3))
                    {
                        return true;
                    }
				}
            }

            // vertical check
            for (int col = 0; col < console.cols; col++)
            {
                for (int row = 0; row < console.rows; row++)
                {
                    if (player.GetAlias() == getGridKey(row, col) && player.GetAlias() == getGridKey(row + 1, col) && player.GetAlias() == getGridKey(row + 2, col) && player.GetAlias() == getGridKey(row + 3, col))
                    {
                        return true;
                    }
                }
            }

            // NW-SE diagonal check
            for (int row = 0; row < console.rows; row++)
            {
                for (int col = 0; col < console.cols; col++)
                {
                    if (player.GetAlias() == getGridKey(row, col) && player.GetAlias() == getGridKey(row + 1, col + 1) && player.GetAlias() == getGridKey(row + 2, col + 2) && player.GetAlias() == getGridKey(row + 3, col + 3))
                    {
                        return true;
                    }
                }
            }

            // SW-NE diagonal check
            for (int row = 0; row < console.rows; row++)
            {
                for (int col = console.cols; col >= 0; col--)
                {
                    if (player.GetAlias() == getGridKey(row, col) && player.GetAlias() == getGridKey(row + 1, col - 1) && player.GetAlias() == getGridKey(row + 2, col - 2) && player.GetAlias() == getGridKey(row + 3, col - 3))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }

    interface Player
	{
        public void SetName(string Name);
        public String GetName();
        public void SetAlias(String Alias);
        public String GetAlias();
    }

    class InteractivePlayer : Player
	{
        private String Name;
        private String Alias;

        public void SetName(string Name)
        {
            this.Name = Name;
        }

        public String GetName()
        {
            return Name;
        }

        public void SetAlias(String Alias)
        {
            this.Alias = Alias;
        }

        public String GetAlias()
        {
            return Alias;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            InteractivePlayer player1 = new InteractivePlayer();
            player1.SetName("David");
            player1.SetAlias("X");

            InteractivePlayer player2 = new InteractivePlayer();
            player2.SetName("Sam");
            player2.SetAlias("O");

            GameConsole console = new GameConsole(6, 7); // 6 rows and 7 cols
            GameState state = new GameState(console);

            Game game = new Game(player1, player2, console, state);

            Console.WriteLine("Connect 4 Game Development Project:");
            game.Play();
        }
    }
}
