using System;
using Godot;

namespace FluidSim
{
    public class LiquidSimulator : Reference
    {
        private float _fluidFlowRate = 1f;
        private float _maxFluidAmount = 2.0f;
        private float _minFluidAmount = 0.005f;
        private float _minFluidTransfer = 0.005f;
        private float _maxFluidCompression = 0.25f;
        private float _maxFluidTransfer = 9f;
        private float[,] _prevSimState;

        public void Initialize(Cell[,] cells)
        {
            _prevSimState = new float[cells.GetLength(0), cells.GetLength(1)];
        }

        float CalculateVerticalFlowValue(float remainingLiquid, Cell destination)
        {
            float sum = remainingLiquid + destination.LiquidAmount;
            float value = 0;

            if (sum <= _maxFluidAmount)
            {
                value = _maxFluidAmount;
            }
            else if (sum < 2 * _maxFluidAmount + _maxFluidCompression)
            {
                value = (_maxFluidAmount * _maxFluidAmount + sum * _maxFluidCompression) / (_maxFluidAmount + _maxFluidCompression);
            }
            else
            {
                value = (sum + _maxFluidCompression) / 2f;
            }

            return value;
        }

        public void CellHasLiquid()
        {
            
        }
        
        public void ResetPreviousSimState(int xLength, int yLength)
        {
            for (int x = 0; x < xLength; x++)
            {
                for (int y = 0; y < yLength; y++)
                {
                    _prevSimState[x, y] = 0;
                }
            } 
        }

        public void UpdateSimState(Cell[,] cells)
        {
            for (int x = 0; x < cells.GetLength(0); x++)
            {
                for (int y = 0; y < cells.GetLength(1); y++)
                {
                    cells[x, y].LiquidAmount += _prevSimState[x, y];
                    if (cells[x, y].LiquidAmount < _minFluidAmount)
                    {
                        cells[x, y].LiquidAmount = 0;
                        cells[x, y].Settled = false; //default empty cell to unsettled
                    }
                }
            }
        }

        public float CalculateFlowRate(float flow, float remainingValue, Cell cell)
        {
            flow = (remainingValue - cell.LiquidAmount) / 4f;
            if (flow > _minFluidTransfer)
                flow *= _fluidFlowRate;
            return flow;
        }
        
        // Run one simulation step
        public void Simulate(ref Cell[,] cells)
        {
            ResetPreviousSimState(cells.GetLength(0), cells.GetLength(1));
            
            for (var x = 0; x < cells.GetLength(0); x++)
            {
                for (var y = 0; y < cells.GetLength(1); y++)
                {
                    var cell = cells[x, y];
                    if (cell.Type == Cell.CellType.Solid)
                    {
                        // If a solid cell is converted to a liquid cell it'll
                        // Explode into water so im resetting its fluid amount here ;-; - Ezekiel
                        cell.LiquidAmount = 0;
                        continue;
                    }

                    if (cell.LiquidAmount == 0 || cell.Settled)
                        continue;
             
                    if (cell.LiquidAmount < _minFluidAmount)
                    {
                        cell.LiquidAmount = 0;
                        continue;
                    }

                    // Keep track of how much liquid this cell started off with
                    var startValue = cell.LiquidAmount;
                    var remainingValue = cell.LiquidAmount;
                    float flow = 0;

                    // Flow to bottom cell
                    if (cell.Bottom != null && cell.Bottom.Type == Cell.CellType.Liquid)
                    {
                        // Determine rate of flow
                        flow = CalculateVerticalFlowValue(cell.LiquidAmount, cell.Bottom) - cell.Bottom.LiquidAmount;
                        if (cell.Bottom.LiquidAmount > 0 && flow > _minFluidTransfer)
                            flow *= _fluidFlowRate;

                        // Constrain flow
                        flow = Mathf.Clamp(flow, 0f, Mathf.Min(_maxFluidTransfer, remainingValue));

                        // Update temp values
                        if (flow != 0)
                        {
                            remainingValue -= flow;
                            _prevSimState[x, y] -= flow;
                            _prevSimState[x, y + 1] += flow;
                            cell.Bottom.Settled = false;
                        }
                    }

                    // Check to ensure we still have liquid in this cell
                    if (remainingValue < _minFluidAmount)
                    {
                        _prevSimState[x, y] -= remainingValue;
                        continue;
                    }

                    // Flow to left cell
                    if (cell.Left != null && cell.Left.Type == Cell.CellType.Liquid)
                    {
                        // Calculate flow rate
                        flow = CalculateFlowRate(flow, remainingValue, cell.Left);

                        // constrain flow
                        flow = Mathf.Clamp(flow, 0f, Mathf.Min(_maxFluidTransfer, remainingValue));
                        

                        // Adjust temp values
                        if (flow != 0)
                        {
                            remainingValue -= flow;
                            _prevSimState[x, y] -= flow;
                            _prevSimState[x - 1, y] += flow;
                            cell.Left.Settled = false;
                        }
                    }

                    // Check to ensure we still have liquid in this cell
                    if (remainingValue < _minFluidAmount)
                    {
                        _prevSimState[x, y] -= remainingValue;
                        continue;
                    }

                    // Flow to right cell
                    if (cell.Right != null && cell.Right.Type == Cell.CellType.Liquid)
                    {
                        // calc flow rate
                        flow = CalculateFlowRate(flow, remainingValue, cell.Right);
                        
                        // constrain flow
                        flow = Mathf.Clamp(flow, 0f, Mathf.Min(_maxFluidTransfer, remainingValue));

                        // Adjust temp values
                        if (flow != 0)
                        {
                            remainingValue -= flow;
                            _prevSimState[x, y] -= flow;
                            _prevSimState[x + 1, y] += flow;
                            cell.Right.Settled = false;
                        }
                    }

                    // Check to ensure we still have liquid in this cell
                    if (remainingValue < _minFluidAmount)
                    {
                        _prevSimState[x, y] -= remainingValue;
                        continue;
                    }

                    // Flow to Top cell
                    if (cell.Top != null && cell.Top.Type == Cell.CellType.Liquid)
                    {
                        flow = remainingValue - CalculateVerticalFlowValue(remainingValue, cell.Top);
                        if (flow > _minFluidTransfer)
                            flow *= _fluidFlowRate;

                        // constrain flow
                        flow = Mathf.Clamp(flow, 0f, Mathf.Min(_maxFluidTransfer, remainingValue));

                        // Adjust values
                        if (flow != 0)
                        {
                            remainingValue -= flow;
                            _prevSimState[x, y] -= flow;
                            _prevSimState[x, y - 1] += flow;
                            cell.Top.Settled = false;
                        }
                    }

                    // Check to ensure we still have liquid in this cell
                    if (remainingValue < _minFluidAmount)
                    {
                        _prevSimState[x, y] -= remainingValue;
                        continue;
                    }

                    // Check if cell is settled
                    if (Math.Abs(startValue - remainingValue) < .1)
                    {
                        cell.SettleCount++;
                        if (cell.SettleCount >= 10)
                        {
                            cell.Settled = true;
                        }
                    }
                    else
                    {
                        cell.UnsettleNeighbors();
                    }
                }
            }
            
            UpdateSimState(cells);
        }
    }
}