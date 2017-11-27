using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Minesweeper
{
    class GameBoard
    {
        private GameBoard()
        {
        }

        public static GameBoard CreateBoard(int h, int w, int mines, int? seed = null)
        {
            int size = w * h;
            if (mines > w * h)
                throw new Exception("Too many mines");

            var mineLocations = new Dictionary<int, SortedSet<int>>(); //Row: List<Column>

            Random rand = seed == null ? new Random() : new Random(seed.Value);

            var ctr = mines;
            while (ctr > 0)
            {
                int r = rand.Next(0, h);
                int c = rand.Next(0, w);

                if (!mineLocations.ContainsKey(r))
                    mineLocations[r] = new SortedSet<int>();

                var columns = mineLocations[r];

                if (!columns.Contains(c))
                {
                    columns.Add(c);
                    ctr--;
                }
            }

            GameBoard board = new GameBoard();

            board.Width = w;
            board.Height = h;
            board.TotalMines = mines;
            board.MinesMap = mineLocations;

            return board;
        }

        public int Width { get; private set; }

        public int Height { get; private set; }

        public int TotalMines { get; private set; }

        public Dictionary<int, SortedSet<int>> MinesMap { get; private set; }

        //TODO: Just generate the whole board here and return the Grid, not a mine map.
        //This is essentially the game; the board is an input to the GUI.
        //No need for these functions after board is created
        //Similar to model-viewmodel

        //TODO: But then does GUI also need the Grid, meta info? MAinWindowVM can just change the state color of each cell when it receives a press.
        //No need to copy the values to GridButton in ButtonInfo
        //MainWindowVM manages the whole game and it's state

        internal bool IsMine(int r, int c)
        {
            //Invalid indexes
            if (r < 0 || r >= Height)
                return false;
            else if (c < 0 || c >= Width)
                return false;

            return MinesMap.ContainsKey(r) && MinesMap[r].Contains(c);
        }

        internal int GetNumMines(int r, int c)
        {
            int num = 0;

            for (int row = r - 1; row <= r + 1; row++)
            {
                for (int col = c - 1; col <= c + 1; col++)
                {
                    if (IsMine(row, col))
                        num++;
                }
            }

            return num;

        }
        
        internal List<Location> GetAdjacentsMap(int r, int c, out int mines)
        {
            mines = 0;

            List<Location> locs = new List<Location>();
            for (int row = r - 1; row <= r + 1; row++)
            {
                for (int col = c - 1; col <= c + 1; col++)
                {
                    if (!(row == r && col == c) 
                        && (row >= 0 && row < Height)
                        && (col >= 0 && col < Width))
                    {
                        if (IsMine(row, col))
                            mines++;

                        locs.Add(new Location(row, col));
                    }
                }
            }

            return locs;

        }
    }
}
