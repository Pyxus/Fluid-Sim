using Godot;

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

        private bool _active;
        private int _itterCount;

        public bool Active
        {
            get => _active;
            set
            {
                _active = value;
                if (!_active)
                {
                    _itterCount = 0;
                }
            }
        }

        public void AddLiquid(float amount)
        {
            LiquidAmount += amount;
            Active = false;
        }

        public void SetType(CellType type)
        {
            Type = type;
            if (Type == CellType.Solid)
            {
                LiquidAmount = 0;
            }

            DeactivateNeighbors();
        }

        public void UpdateActivity()
        {
            _itterCount++;
            if (_itterCount >= 5)
            {
                Active = true;
            }
        }

        public void DeactivateNeighbors()
        {
            if (Top != null)
                Top.Active = false;
            if (Bottom != null)
                Bottom.Active = false;
            if (Left != null)
                Left.Active = false;
            if (Right != null)
                Right.Active = false;
        }
    }
}