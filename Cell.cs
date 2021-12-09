using Godot;

namespace FluidSim
{
    public class Cell : Reference
    {
        public enum CellType
        {
            Blank,
            Solid
        }

        public enum FlowDirection
        {
            Top = 0,
            Right = 1,
            Bottom = 2,
            Left = 3
        }

        public CellType Type { get; private set; }

        public bool[] FlowDirections = new bool[4];

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

        public void ResetFlowDirections()
        {
            FlowDirections[0] = false;
            FlowDirections[1] = false;
            FlowDirections[2] = false;
            FlowDirections[3] = false;
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