using Innouvous.Utils;
using Innouvous.Utils.MVVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Minesweeper
{
    public class MainWindowViewModel : ViewModel
    {
        private Window window;
        private System.Windows.Controls.Grid grid;


        public MainWindowViewModel(Window window, System.Windows.Controls.Grid grid)
        {
            this.window = window;
            this.grid = grid;
        }

        private double ConvertDifficulty(GameDifficulty mode)
        {
            switch (mode)
            {
                default:
                case GameDifficulty.Easy:
                    return .1;
                case GameDifficulty.Medium:
                    return .2;
                case GameDifficulty.Hard:
                    return .30;
                case GameDifficulty.Expert:
                    return .40;
                case GameDifficulty.Impossible:
                    return .90;
            }
        }

        #region Game Creation

        internal void CreateGame(int rows, int columns, GameDifficulty mode)
        {
            int mines = (int)((double)(rows * columns) * ConvertDifficulty(mode));

            CreateGame(rows, columns, mines);
        }

        internal void CreateGame(int rows, int columns, int mines)
        {

            GameBoard board = GameBoard.CreateBoard(rows, columns, mines);
            CreateGameGrid(board);
        }

        private void CreateGameGrid(GameBoard board)
        {
            grid.ColumnDefinitions.Clear();
            grid.Children.Clear();
            grid.RowDefinitions.Clear();


            this.board = board;

            this.buttonGrid = new GridButton[board.Height, board.Width];

            //Create rows, columns
            for (int i = 0; i < board.Width; i++)
            {
                grid.ColumnDefinitions.Add(new ColumnDefinition());
            }

            for (int i = 0; i < board.Height; i++)
            {
                grid.RowDefinitions.Add(new RowDefinition());
            }

            //Create buttons
            for (int r = 0; r < board.Height; r++)
            {
                for (int c = 0; c < board.Width; c++)
                {
                    var info = new ButtonInfo()
                    {
                        Column = c,
                        Row = r
                    };

                    if (board.IsMine(r, c))
                    {
                        info.IsMine = true;
                    }
                    else
                    {
                        int adjacentMines = board.GetNumMines(r, c);
                        info.Value = adjacentMines;
                        info.IsMine = false;
                    }

                    GridButton button = new GridButton();
                    button.SetValue(Grid.RowProperty, r);
                    button.SetValue(Grid.ColumnProperty, c);
                    button.InitializeButton(info);
                    grid.Children.Add(button);

                    buttonGrid[r, c] = button;
                }
            }

            //Initialize Game
            MinesLeft = board.TotalMines;
            actualMinesLeft = board.TotalMines;

            CellsLeft = board.Width * board.Height;

            //Set Handlers
            GridButton.SetHandlers(
                OnClick,
                Flag,
                AutoClick);

            InGame = true;
        }

        #endregion

        #region Commands
        public ICommand SolveCommand
        {
            get { return new CommandHelper(Solve); }
        }

        internal void Solve()
        {
            foreach (var c in grid.Children)
            {
                var gb = c as GridButton;

                if (gb.Flagged)
                    gb.Flag();

                if (!gb.IsMine)
                    gb.Click();
                else
                    gb.Flag();
            }
        }

        public ICommand NewGameCommand
        {
            get { return new CommandHelper(NewGame); }
        }

        internal void NewGame()
        {

            var dlg = new NewGameWindow();
            dlg.Owner = window;
            dlg.ShowDialog();

            //int r = 40, c = 40;
            CreateGame(dlg.Rows, dlg.Columns, dlg.Difficulty);
        }

        #endregion

        #region Active Game State

        private GameBoard board;
        private int actualMinesLeft;
        private GridButton[,] buttonGrid;

        private void AutoClick(int r, int c)
        {
            int mines;

            var locations = board.GetAdjacentsMap(r, c, out mines);

            List<GridButton> adjacents = new List<GridButton>();

            int flagged = 0;
            foreach (var l in locations)
            {
                var button = buttonGrid[l.Row, l.Column];
                adjacents.Add(button);

                if (button.Flagged)
                    flagged++;
            }

            if (flagged == mines)
            {
                foreach (var b in adjacents)
                {
                    if (!b.Flagged)
                        b.Click();
                }
            }
            
        }

        private void Flag(int r, int c, bool flagged)
        {
            var isMine = board.IsMine(r, c);

            if (flagged)
            {
                MinesLeft--;
                CellsLeft--;

                if (isMine)
                    actualMinesLeft--;
            }
            else
            {
                MinesLeft++;
                CellsLeft++;

                if (isMine)
                    actualMinesLeft++;
            }

            CheckWin();
        }

        public bool InGame { get; private set; }

        private void CheckWin()
        {
            if (!InGame)
                return;

            if (actualMinesLeft == 0 && minesLeft == 0 && CellsLeft == 0)
            {
                InGame = false;
                MessageBoxFactory.ShowInfo("Won", "You Win!");
            }
        }

        //TODO: Clean, figure out whether should do in GameBoard or here
        private void OnClick(int r, int c)
        {
            if (board.IsMine(r, c))
            {
                MessageBoxFactory.ShowError("Lose", "You Lose");

                InGame = false;
            }
            else
            {
                CellsLeft--;

                var button = buttonGrid[r, c];

                if (button.IsEmpty)
                {
                    for (int row = r - 1; row <= r + 1; row++)
                    {
                        for (int col = c - 1; col <= c + 1; col++)
                        {
                            GridButton gb = GetButton(row, col);
                            if (gb != null && gb != button && !gb.Clicked && !gb.IsMine)
                                gb.Click(); //This is recursive
                        }
                    }
                }

                CheckWin();
            }
        }

        private GridButton GetButton(int r, int c)
        {
            if (r < 0 || r >= board.Height)
                return null;
            else if (c < 0 || c >= board.Width)
                return null;
            else
                return buttonGrid[r, c];
        }

        public int CellsLeft { get; private set; }

        private int minesLeft;
        public int MinesLeft
        {
            get { return minesLeft; }
            set
            {
                minesLeft = value;
                RaisePropertyChanged("MinesLeft");
            }
        }

        #endregion
    }
}
