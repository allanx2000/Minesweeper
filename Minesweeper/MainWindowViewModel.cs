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

        public enum Mode
        {
            Easy,
            Medium,
            Hard,
            Expert,
            Impossible
        }

        public MainWindowViewModel(Window window, System.Windows.Controls.Grid grid)
        {
            this.window = window;
            this.grid = grid;
        }

        private double ConvertDifficulty(Mode mode)
        {
            switch (mode)
            {
                default:
                case Mode.Easy:
                    return .1;
                case Mode.Medium:
                    return .2;
                case Mode.Hard:
                    return .30;
                case Mode.Expert:
                    return .40;
                case Mode.Impossible:
                    return .90;
            }
        }

        #region Game Creation

        internal void CreateGame(int rows, int columns, Mode mode)
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

            //Set Handlers
            GridButton.SetHandlers(
                OnClick,
                Flag);
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

        private void Flag(int r, int c, bool flagged)
        {
            var isMine = board.IsMine(r, c);

            if (flagged)
            {
                MinesLeft--;

                if (isMine)
                    actualMinesLeft--;
            }
            else
            {
                MinesLeft++;

                if (isMine)
                    actualMinesLeft++;
            }

            if (actualMinesLeft == 0)
            {
                MessageBoxFactory.ShowInfo("Won", "You Win!");
            }
        }

        //TODO: Clean, figure out whether should do in GameBoard or here
        private void OnClick(int r, int c)
        {
            if (board.IsMine(r, c))
            {
                MessageBoxFactory.ShowError("Lose", "You Lose");

                //TODO: Create new game
            }
            else
            {
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
