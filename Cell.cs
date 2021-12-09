using Godot;

namespace FluidSim
{
    public class Cell : Reference
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

        private bool _settled;
        public int SettleCount { get; set; }

        public bool Settled
        {
            get { return _settled; }
            set
            {
                _settled = value;
                if (!_settled)
                {
                    SettleCount = 0;
                }
            }
        }

        public void AddLiquid(float amount)
        {
            LiquidAmount += amount;
            Settled = false;
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
                Top.Settled = false;
            if (Bottom != null)
                Bottom.Settled = false;
            if (Left != null)
                Left.Settled = false;
            if (Right != null)
                Right.Settled = false;
        }
    }
}