namespace FluidSim
{
    public class Cell
    {
        public enum CellType
        {
            Liquid,
            Solid
        }

 
        public CellType Type { get; private set; }


        public Cell Top;
        public Cell Bottom { get; set; }
        public Cell Left { get; set; }
        public Cell Right { get; set; }
        public float LiquidAmount { get; set; }

        private bool _isSettled;
        public int SettleCount { get; set; }

        public bool IsSettled
        {
            get { return _isSettled; }
            set
            {
                _isSettled = value;
                if (!_isSettled)
                {
                    SettleCount = 0;
                }
            }
        }

        public void AddLiquid(float amount)
        {
            LiquidAmount += amount;
            IsSettled = false;
        }

        public void SetType(CellType type)
        {
            Type = type;
            if (Type == CellType.Solid)
            {
                LiquidAmount = 0;
            }

            UnsettleNeighbors();
        }


        public void UnsettleNeighbors()
        {
            if (Top != null)
                Top.IsSettled = false;
            if (Bottom != null)
                Bottom.IsSettled = false;
            if (Left != null)
                Left.IsSettled = false;
            if (Right != null)
                Right.IsSettled = false;
        }
    }
}