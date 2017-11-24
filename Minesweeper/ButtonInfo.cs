using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Minesweeper
{
    public class ButtonInfo
    {
        public bool IsMine { get; set; }
        public int Row { get; set; }
        public int Column { get; set; }
        public int Value { get; set; }

        public bool IsEmpty { get { return Value == null || Value == 0; } }
    }
}
