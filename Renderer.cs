using Godot;

namespace FluidSim
{
    public class Renderer : Node2D
    {
        [Export] private NodePath _labelPath;
        
        private LiquidSimulator _liquidSimulator;
        private Cell[,] _cells;
        private float _cellSize = 4;
        private Label _label;
        private bool _isPlacingLiquid = true;
        private float _fluidAmount = 2f;

        public override void _Ready()
        {
            base._Ready();
            _label = GetNode<Label>(_labelPath);
            _cells = PopulateCells(128);
            _liquidSimulator = new LiquidSimulator();
            _liquidSimulator.Initialize(_cells);
            UpdateNeighbors();
        }

        public override void _UnhandledInput(InputEvent @event)
        {
            if (@event is InputEventMouseButton)
            {
                InputEventMouseButton emb = (InputEventMouseButton)@event;
                if (emb.IsPressed())
                {
                    if (emb.ButtonIndex == (int)ButtonList.WheelUp)
                    {
                        _fluidAmount += .1f;
                    }

                    if (emb.ButtonIndex == (int)ButtonList.WheelDown)
                    {
                        _fluidAmount -= .1f;
                    }
                }
            }
        }

        public override void _Process(float delta)
        {
            base._Process(delta);

            var xLength = _cells.GetLength(0);
            var yLength = _cells.GetLength(1);

            if (Input.IsActionJustPressed("ui_select"))
            {
                _isPlacingLiquid = !_isPlacingLiquid;
            }

            if (Input.IsKeyPressed((int)KeyList.Up))
            {
                _fluidAmount += .1f;
            }
            else if (Input.IsKeyPressed((int)KeyList.Down))
            {
                _fluidAmount -= .1f;
            }

            _fluidAmount = Mathf.Clamp(_fluidAmount, .1f, 10f);
            _label.Text = _isPlacingLiquid ? "(Press Spacebar) Draw Mode: Liquid" : "Draw Mode: Solid";
            _label.Text += $"\n(Press Arrow Keys) Fluid Placement Amount: {_fluidAmount:0.#}";


            if (Input.IsMouseButtonPressed((int)ButtonList.Left))
            {
                for (var x = 0; x < xLength; x++)
                {
                    for (var y = 0; y < yLength - 1; y++)
                    {
                        var rect = new Rect2(GlobalPosition.x + x * _cellSize, GlobalPosition.y + y * _cellSize, _cellSize, _cellSize);
                        if (rect.HasPoint(GetGlobalMousePosition()))
                        {
                            if (_isPlacingLiquid)
                            {
                                _cells[x, y].AddLiquid(_fluidAmount);
                            }
                            else
                            {
                                _cells[x, y].SetType(Cell.CellType.Solid);
                            }
                        }
                    }
                }
            }

            if (Input.IsMouseButtonPressed((int)ButtonList.Right))
            {
                for (var x = 0; x < xLength; x++)
                {
                    for (var y = 0; y < yLength - 1; y++)
                    {
                        if (y == 0 || y == yLength - 1 || x == 0 || x == xLength - 1)
                            continue;
                        
                        var rect = new Rect2(GlobalPosition.x + x * _cellSize, GlobalPosition.y + y * _cellSize, _cellSize, _cellSize);
                        if (rect.HasPoint(GetGlobalMousePosition()))
                        {
                            _cells[x, y].SetType(Cell.CellType.Liquid);
                            _cells[x, y].LiquidAmount = 0;
                        }
                    }
                }
            }

            _liquidSimulator.Simulate(ref _cells);
            Update();
        }

        public override void _Draw()
        {
            base._Draw();
            var xLength = _cells.GetLength(0);
            var yLength = _cells.GetLength(1);

            //DrawGrid(xLength, yLength);
            for (var x = 0; x < xLength; x++)
            {
                for (var y = 0; y < yLength; y++)
                {
                    var fluidCell = _cells[x, y];


                    if (fluidCell.Type == Cell.CellType.Solid)
                    {
                        DrawRect(new Rect2(x * _cellSize, (y - 1) * _cellSize, _cellSize, _cellSize), Colors.Black);
                    }
                    else if (fluidCell.Type == Cell.CellType.Liquid && fluidCell.LiquidAmount > 0)
                    {
                        var size = new Vector2(_cellSize, Mathf.Min(_cellSize, fluidCell.LiquidAmount * _cellSize));
                        var position = new Vector2(x * _cellSize, y * _cellSize - size.y);
                        var DarkColor = Colors.DarkCyan;
                        var LiquidColor = Colors.Cyan;
                        LiquidColor.a = .7f;
                        
                        if (fluidCell.LiquidAmount < 1)
                        {
                            LiquidColor = LerpColor(Colors.White, Colors.Cyan, fluidCell.LiquidAmount);
                        }

                        var color = LerpColor(LiquidColor, DarkColor, fluidCell.LiquidAmount / 4f);
                        
                        if (fluidCell.LiquidAmount > .4f)
                            color.a = Mathf.Min(1, fluidCell.LiquidAmount / 2f);

                        if (fluidCell.Bottom != null && fluidCell.Bottom.LiquidAmount > .4f)
                            color.a = Mathf.Min(.95f, color.a + fluidCell.LiquidAmount / 2f);
                        
                        if (fluidCell.Bottom != null && fluidCell.Bottom.Type != Cell.CellType.Solid && fluidCell.Bottom.LiquidAmount <= .99f)
                        {
                            size = new Vector2(0, 0);
                        }

                        if (fluidCell.Type == Cell.CellType.Liquid && fluidCell.Top != null && (fluidCell.Top.LiquidAmount > 0.05f))
                        {
                            size = new Vector2(_cellSize, _cellSize);
                            color.a = Mathf.Min(1, fluidCell.LiquidAmount / 2f);
                            
                            var isNotNearSolid = (fluidCell.Top != null && fluidCell.Top.Type != Cell.CellType.Solid)
                                                 && (fluidCell.Bottom != null && fluidCell.Bottom.Type != Cell.CellType.Solid)
                                                 && (fluidCell.Left != null && fluidCell.Left.Type != Cell.CellType.Solid)
                                                 && (fluidCell.Right != null && fluidCell.Right.Type != Cell.CellType.Solid);
                            var hyp = Mathf.Sqrt((_cellSize * _cellSize) + (_cellSize * _cellSize));
                            if (isNotNearSolid && fluidCell.Top != null && fluidCell.LiquidAmount > .2) 
                            {
                                //DrawCircle(position, hyp / 4f, color);
                            }
                        }
                        var rect = new Rect2(position, size);
                        DrawRect(rect, color);
                    }
                }
            }
        }

        private Color LerpColor(Color c1, Color c2, float t)
        {
            t = Mathf.Clamp(t, 0, 1.5f);
            var r = Mathf.Lerp(c1.r, c2.r, t);
            var g = Mathf.Lerp(c1.g, c2.g, t);
            var b = Mathf.Lerp(c1.b, c2.b, t);
            var a = Mathf.Lerp(c1.a, c2.a, t);
            return new Color(r, g, b, a);
        }

        private void DrawGrid(int xLength, int yLength)
        {
            var color = Colors.DarkGray;

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

        void UpdateNeighbors()
        {
            var width = _cells.GetLength(0);
            var height = _cells.GetLength(1);

            for (var x = 0; x < width; x++)
            {
                for (var y = 0; y < height; y++)
                {
                    if (x > 0)
                    {
                        _cells[x, y].Left = _cells[x - 1, y];
                    }

                    if (x < width - 1)
                    {
                        _cells[x, y].Right = _cells[x + 1, y];
                    }

                    if (y > 0)
                    {
                        _cells[x, y].Top = _cells[x, y - 1];
                    }

                    if (y < height - 1)
                    {
                        _cells[x, y].Bottom = _cells[x, y + 1];
                    }
                }
            }
        }


        public void PrintCells()
        {
            for (var i = 0; i < _cells.GetLength(0); i++)
            {
                var str = "";
                for (var j = 0; j < _cells.GetLength(1); j++)
                {
                    var cell = _cells[i, j];
                    if (cell.Type == Cell.CellType.Liquid)
                    {
                        if (cell.LiquidAmount > 0)
                        {
                            str += "L ";
                        }
                        else
                        {
                            str += "# ";
                        }
                    }
                    else if (cell.Type == Cell.CellType.Solid)
                    {
                        str += "X ";
                    }
                }

                GD.Print(str);
            }

            GD.Print();
        }

        public Cell[,] PopulateCells(int size)
        {
            var cells = new Cell[size, size];

            for (var i = 0; i < size; i++)
            {
                for (var j = 0; j < size; j++)
                {
                    var cell = new Cell();
                    if (j == 0 || j == size - 1 || i == 0 || i == size - 1)
                    {
                        cell.SetType(Cell.CellType.Solid);
                    }

                    cells[i, j] = cell;
                }
            }

            return cells;
        }
    }
}