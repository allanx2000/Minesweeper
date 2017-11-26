using Innouvous.Utils;
using Innouvous.Utils.Merged45.MVVM45;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace Minesweeper
{
    class NewGameWindowViewModel : ViewModel
    {
        private NewGameWindow newGameWindow;

        public bool Cancelled { get; private set; }

        public NewGameWindowViewModel(NewGameWindow newGameWindow)
        {
            this.newGameWindow = newGameWindow;

            Cancelled = true;
            Rows = Columns = 30;
            SelectedDifficulty = GameDifficulty.Medium.ToString();
        }

        public string SelectedDifficulty
        {
            get { return Get<string>(); }
            set
            {
                Set(value);
                RaisePropertyChanged();

                UpdateMines();
            }
        }

        private void UpdateMines()
        {
            if (string.IsNullOrEmpty(SelectedDifficulty) || Rows < 0 || Columns < 0)
                return;

            var difficulty = (GameDifficulty)Enum.Parse(typeof(GameDifficulty), SelectedDifficulty);

            Mines = (int)(ConvertDifficulty(difficulty) * (double)Rows * (double)Columns);
        }

        public string[] Difficulties
        {
            get { return Enum.GetNames(typeof(GameDifficulty)); }
        }

        private int rows, columns;

        private const int MaxRC = 50;

        public int Rows
        {
            get { return rows; }
            set
            {
                if (value > MaxRC)
                    rows = MaxRC;
                else
                    rows = value;
                
                UpdateMines();
                RaisePropertyChanged("Rows");
            }
        }

        public int Columns
        {
            get { return columns; }
            set
            {
                if (value > MaxRC)
                    columns = MaxRC;
                else
                    columns = value;

                UpdateMines();
                RaisePropertyChanged("Columns");
            }
        }

        public ICommand StartCommand
        {
            get { return new Innouvous.Utils.MVVM.CommandHelper(Start); }
        }

        public int Mines {
            get { return Get<int>(); }
            set
            {
                Set(value);
                RaisePropertyChanged();
            }
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
                    return .25;
                case GameDifficulty.Expert:
                    return .30;
                case GameDifficulty.Impossible:
                    return .80;
            }
        }
        
        private void Start()
        {
            try
            {
                if (Rows < 2 || Columns < 2)
                    throw new Exception("Rows or Columns must be greater than 2");

                Cancelled = false;
                newGameWindow.Close();
            }
            catch (Exception e)
            {
                MessageBoxFactory.ShowError(e);
            }
        }

    }
}
