﻿using Innouvous.Utils;
using Innouvous.Utils.MVVM;
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
            SelectedDifficulty = MainWindowViewModel.Mode.Medium.ToString();
        }

        private string selectedDifficulty;
        public string SelectedDifficulty
        {
            get { return selectedDifficulty; }
            set
            {
                selectedDifficulty = value;
                RaisePropertyChanged("SelectedDifficulty");
            }
        }

        public string[] Difficulties
        {
            get { return Enum.GetNames(typeof(MainWindowViewModel.Mode)); }
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
            get { return new CommandHelper(Start); }
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
