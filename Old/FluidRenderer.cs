using System;
using Godot;
using Color = Godot.Color;

namespace FluidSim.Old
{
    public class FluidRenderer : Node2D
    {
        public struct FluidCell
        {
            public CellType Type;
            public float Amount;
            public bool IsUpdated;

            public FluidCell(CellType type = CellType.Empty, float amount = 0, bool isUpdated = false)
            {
                Type = type;
                Amount = amount;
                IsUpdated = isUpdated;
            }
        }

        public enum CellType
        {
            Empty,
            Water,
        }

        private FluidCell[,] _matrix = new FluidCell[50, 50];
        private float _cellSize = 10;
        private float _fluidLevels = 5;

        public override void _Ready()
        {
            _matrix[25, 49] = new FluidCell(CellType.Water, 3f);
        }

        public override void _Input(InputEvent @event)
        {
            if (@event is InputEventMouseButton mouseButton)
            {
                if (mouseButton.ButtonIndex == (int)ButtonList.Left)
                {
                }
            }
        }

        public override void _Process(float delta)
        {
            var xLength = _matrix.GetLength(0);
            var yLength = _matrix.GetLength(1);

            if (Input.IsMouseButtonPressed((int)ButtonList.Left))
            {
                for (var x = 0; x < xLength; x++)
                {
                    for (var y = 0; y < yLength - 1; y++)
                    {
                        var rect = new Rect2(GlobalPosition.x + x * _cellSize, GlobalPosition.y + y * _cellSize, _cellSize, _cellSize);
                        if (rect.HasPoint(GetGlobalMousePosition()))
                        {
                            _matrix[x, y] = new FluidCell(CellType.Water, 1);
                        }
                    }
                }
            }

            for (var x = 0; x < xLength; x++)
            {
                for (var y = 0; y < yLength; y++)
                {
                    var sourceCell = _matrix[x, y];
                    if (!sourceCell.IsUpdated)
                    {
                        // Flow into bottom cell
                        if (y + 1 < yLength)
                        {
                            Flow((x, y), (x, y + 1));
                        }
                        
                        // Flow into left cell
                        if (x - 1 > 0)
                        {
                            Flow((x, y), (x - 1, y));
                        }
                        
                        // Flow into right cell
                        if (x + 1 < xLength)
                        {
                            //Flow((x, y), (x + 1, y));
                        }
                    }
                }
            }

            for (var x = 0; x < xLength; x++)
            {
                for (var y = 0; y < yLength; y++)
                {
                    _matrix[x, y].IsUpdated = false;
                }
            }

            Update();
        }

        public override void _Draw()
        {
            var xLength = _matrix.GetLength(0);
            var yLength = _matrix.GetLength(1);

            //DrawGrid();

            for (var x = 0; x < xLength; x++)
            {
                for (var y = 0; y < yLength; y++)
                {
                    var fluidCell = _matrix[x, y];
                    var fluidRatio = (fluidCell.Amount / _fluidLevels);
                    var rect = new Rect2(x * _cellSize, y * _cellSize - _cellSize * fluidRatio, _cellSize, _cellSize * fluidRatio);
                    DrawRect(rect, GetCellColor(fluidCell));
                }
            }
        }

        private void Flow((int X, int Y) fromPos, (int X, int Y) toPos)
        {
            var fromCell = _matrix[fromPos.X, fromPos.Y];
            var destCell = _matrix[toPos.X, toPos.Y];
            
            if (destCell.Type == CellType.Empty)
            {
                fromCell.IsUpdated = true;
                _matrix[toPos.X, toPos.Y] = fromCell;
                _matrix[fromPos.X, fromPos.Y] = new FluidCell { IsUpdated = true };
            }
            else if (destCell.Type == CellType.Water)
            {
                fromCell.IsUpdated = true;
                if (destCell.Amount < fromCell.Amount)
                {
                    var flowAmount = (fromCell.Amount - destCell.Amount) / 2f;
                    fromCell.Amount -= flowAmount;
                    destCell.Amount += flowAmount;
                }
            }
        }
        
        private void DrawGrid()
        {
            var xLength = _matrix.GetLength(0) - 1;
            var yLength = _matrix.GetLength(1) - 1;
            var color = Colors.Gray;

            DrawRect(new Rect2(0, -1 * _cellSize, xLength * _cellSize, (yLength + 1) * _cellSize), color, false);

            for (var x = 0; x < xLength; x++)
            {
                DrawLine(new Vector2(x * _cellSize, -1 * _cellSize), new Vector2(x * _cellSize, yLength * _cellSize), color, .5f);
                for (var y = -1; y < yLength; y++)
                {
                    DrawLine(new Vector2(0, y * _cellSize), new Vector2(xLength * _cellSize, y * _cellSize), color, .5f);
                }
            }
        }

        private Color GetCellColor(FluidCell cell)
        {
            switch (cell.Type)
            {
                case CellType.Empty:
                    return Colors.Transparent;
                case CellType.Water:
                    return Colors.Aqua;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}