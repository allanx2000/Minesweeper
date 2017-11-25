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

        public NewGameWindowViewModel(NewGameWindow newGameWindow)
        {
            this.newGameWindow = newGameWindow;

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
            }
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

                RaisePropertyChanged("Columns");
            }
        }

        public ICommand StartCommand
        {
            get { return new Innouvous.Utils.MVVM.CommandHelper(Start); }
        }

        private void Start()
        {
            try
            {
                if (Rows < 2 || Columns < 2)
                    throw new Exception("Rows or Columns must be greater than 2");

                if (string.IsNullOrEmpty(SelectedDifficulty))
                    throw new Exception("Difficulty Must Be Set");

                newGameWindow.Close();
            }
            catch (Exception e)
            {
                MessageBoxFactory.ShowError(e);
            }
        }

    }
}
