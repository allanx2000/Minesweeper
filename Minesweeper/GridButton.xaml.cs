using Innouvous.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Minesweeper
{
    /// <summary>
    /// Interaction logic for GridButton.xaml
    /// </summary>
    public partial class GridButton : UserControl
    {

        private enum ButtonStates
        {
            Unpressed,

            Empty,
            Number,

            Mine,
            Flag,

            Undo
        }

        private static SolidColorBrush GRAY = new SolidColorBrush(Colors.Gray);
        private static SolidColorBrush YELLOW = new SolidColorBrush(Colors.Yellow);
        private static SolidColorBrush ORANGE = new SolidColorBrush(Colors.Orange);
        private static SolidColorBrush RED = new SolidColorBrush(Colors.Red);
        private static SolidColorBrush WHITE = new SolidColorBrush(Colors.White);

        private ButtonInfo info;

        public void InitializeButton(ButtonInfo info)
        {
            this.info = info;
        }

        #region Game Actions Handlers

        private static Action<int, int> onClick;
        private static Action<int, int, bool> onFlag;
        private static Action<int, int> onAutoClick;

        private static bool alwaysContinue { get; set; }

        //onFlag: r,c, isFlagged
        public static void SetHandlers(Action<int, int> onClick, Action<int, int, bool> onFlag, Action<int, int>  onAutoClick, bool alwaysContinue = true)
        {
            GridButton.onClick = onClick;
            GridButton.onFlag = onFlag;
            GridButton.onAutoClick = onAutoClick;

            GridButton.alwaysContinue = alwaysContinue;
        }

        #endregion

        #region DPs

        public static readonly DependencyProperty ButtonColorProperty =
      DependencyProperty.Register("ButtonColor", typeof(SolidColorBrush),
        typeof(GridButton), null);

        public SolidColorBrush ButtonColor
        {
            get
            {
                return (SolidColorBrush)GetValue(ButtonColorProperty);
            }
            set
            {
                SetValue(ButtonColorProperty, value);
            }
        }

        public static readonly DependencyProperty ButtonTextProperty =
        DependencyProperty.Register("ButtonText", typeof(string),
            typeof(GridButton), null);

        public string ButtonText
        {
            get
            {
                return (string)GetValue(ButtonTextProperty);
            }
            set
            {
                SetValue(ButtonTextProperty, value);
            }
        }


        #endregion

        public bool Clicked { get; private set; }

        public GridButton()
        {
            InitializeComponent();

            SetColor(ButtonStates.Unpressed);
            Clicked = false;
        }

        private void SetColor(ButtonStates buttonState)
        {
            SolidColorBrush color;

            switch (buttonState)
            {
                case ButtonStates.Number:
                case ButtonStates.Empty:
                    color = WHITE;
                    break;
                case ButtonStates.Mine:
                    color = RED;
                    break;
                case ButtonStates.Flag:
                    color = YELLOW;
                    break;
                case ButtonStates.Undo:
                    color = ORANGE;
                    break;
                case ButtonStates.Unpressed:
                default:
                    color = GRAY;
                    break;
            }

            SetValue(ButtonColorProperty, color);
        }
        
        public void Flag(bool isUndo = false)
        {
            if (Clicked)
                return;

            if (isUndo)
            {
                Flagged = true;
                Clicked = true;

                SetColor(ButtonStates.Undo);

            }
            else
            {
                Flagged = !Flagged;

                SetColor(Flagged ? ButtonStates.Flag : ButtonStates.Unpressed);

            }

            if (onFlag != null)
                onFlag.Invoke(info.Row, info.Column, Flagged);
            
        }

        public bool Flagged { get; set; }



        private void DoClick()
        {

            if (info.IsMine)
            {
                //TODO: Make Auto-Undo a global setting
                
                bool undo;
                
                if (alwaysContinue)
                    undo = true;
                else
                    undo = MessageBoxFactory.ShowConfirmAsBool("Undo?", "You hit a mine!");


                if (undo)
                {
                    Flag(true);
                    return;
                }

                SetColor(ButtonStates.Mine);
            }
            else
            {
                ButtonText = info.IsEmpty ? null : info.Value.ToString();

                SetColor(ButtonStates.Number);
            }

            Clicked = true;

            if (onClick != null)
                onClick.Invoke(info.Row, info.Column);
        }

        public bool IsMine
        {
            get { return info.IsMine; }
        }

        public void Click()
        {

            if (Clicked || Flagged)
            {
                return;
            }

            DoClick();
        }

        public bool IsEmpty { get { return info.IsEmpty; } }


        private DelayedAction delayedClick = new DelayedAction();

        private void Click(object sender, MouseButtonEventArgs e)
        {
            if (e.RightButton == MouseButtonState.Pressed && e.LeftButton == MouseButtonState.Pressed)
            {
                delayedClick.Cancel();

                onAutoClick.Invoke(info.Row, info.Column);
            }
            else if (e.RightButton == MouseButtonState.Pressed)
            {
                delayedClick.Start(() => Flag());
            }
            else if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (Clicked || Flagged)
                    e.Handled = true;
                else
                    delayedClick.Start(DoClick);
            }
        }
        
    }

    public class DelayedAction
    {
        private Timer clickTimer;
        private int delay = 100; //ms
        
        public void Cancel()
        {
            if (clickTimer != null)
            {
                clickTimer.Change(Timeout.Infinite, Timeout.Infinite);
                clickTimer = null;
            }
        }

        public void Start(Action action)
        {
            Cancel();

            clickTimer = new Timer((x) =>
                {
                    App.Current.Dispatcher.Invoke(action);
                },    
                null, delay, -1);
        }
        
        public void SetDelay(int delay) { this.delay = delay; }
    }
}
