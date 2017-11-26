namespace Minesweeper
{
    internal struct Location
    {
        public int Row { get; private set; }
        public int Column { get; private set; }

        public Location(int r, int c)
        {
            this.Row = r;
            this.Column = c;
        }
    }
}