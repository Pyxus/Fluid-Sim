using Godot;

namespace FluidSim
{
    public class Renderer : Node2D
    {
        [Export] private NodePath _labelPath;
        [Export] private NodePath _quantityLabelPath;
        [Export] private NodePath _baseColorButtonPath;
        [Export] private NodePath _darkColorButtonPath;
        [Export] private NodePath _highlightColorButtonPath;
        [Export] private NodePath _clumpingSliderPath;

        private LiquidSimulator _liquidSimulator;
        private float _cellSize = 4;
        private Label _label;
        private Label _quantityLabel;
        private bool _isPlacingLiquid = true;
        private float _fluidAmount = 10f;
        private ColorPickerButton _baseColorButton;
        private ColorPickerButton _darkColorButton;
        private ColorPickerButton _highlightColorButton;
        private HSlider _clumpingSlider;

        public override void _Ready()
        {
            base._Ready();
            _clumpingSlider = GetNode<HSlider>(_clumpingSliderPath);
            _quantityLabel = GetNode<Label>(_quantityLabelPath);
            _label = GetNode<Label>(_labelPath);
            _baseColorButton = GetNode<ColorPickerButton>(_baseColorButtonPath);
            _darkColorButton = GetNode<ColorPickerButton>(_darkColorButtonPath);
            _highlightColorButton = GetNode<ColorPickerButton>(_highlightColorButtonPath);
            _baseColorButton.Color = Colors.Goldenrod;
            _darkColorButton.Color = Colors.DarkGoldenrod;
            _highlightColorButton.Color = Colors.White;
            
            
            _liquidSimulator = new LiquidSimulator();
            _liquidSimulator.Initialize(128);
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

            var xLength = _liquidSimulator.Cells.GetLength(0);
            var yLength = _liquidSimulator.Cells.GetLength(1);

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
            _label.Text = _isPlacingLiquid ? "(Press Spacebar) Draw Mode: Sand" : "(Press Spacebar) Draw Mode: Solid";
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
                                _liquidSimulator.Cells[x, y].AddLiquid(_fluidAmount);
                            }
                            else
                            {
                                _liquidSimulator.Cells[x, y].SetType(Cell.CellType.Solid);
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
                            _liquidSimulator.Cells[x, y].SetType(Cell.CellType.Liquid);
                            _liquidSimulator.Cells[x, y].LiquidAmount = 0;
                        }
                    }
                }
            }
            Update();
        }

        public override void _PhysicsProcess(float delta)
        {
            base._PhysicsProcess(delta);
            _liquidSimulator.Clumping = (float) _clumpingSlider.Value;
            _liquidSimulator.Process();
            _quantityLabel.Text = $"Fluid Quantity: {_liquidSimulator.FluidQuantity}";
        }

        public override void _Draw()
        {
            base._Draw();
            var xLength = _liquidSimulator.Cells.GetLength(0);
            var yLength = _liquidSimulator.Cells.GetLength(1);

            //DrawGrid(xLength, yLength);
            for (var x = 0; x < xLength; x++)
            {
                for (var y = 0; y < yLength; y++)
                {
                    var fluidCell = _liquidSimulator.Cells[x, y];


                    if (fluidCell.Type == Cell.CellType.Solid)
                    {
                        DrawRect(new Rect2(x * _cellSize, (y - 1) * _cellSize, _cellSize, _cellSize), Colors.Black);
                    }
                    else if (fluidCell.Type == Cell.CellType.Liquid && fluidCell.LiquidAmount > 0)
                    {
                        var size = new Vector2(_cellSize, Mathf.Min(_cellSize, fluidCell.LiquidAmount * _cellSize));
                        var position = new Vector2(x * _cellSize, y * _cellSize - size.y);
                        var DarkColor = _darkColorButton.Color;
                        var LiquidColor = _baseColorButton.Color;
                        LiquidColor.a = .7f;

                        if (fluidCell.LiquidAmount < 1)
                        {
                            LiquidColor = LerpColor(_highlightColorButton.Color, _baseColorButton.Color, fluidCell.LiquidAmount);
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
                            /*
                            var isNotNearSolid = (fluidCell.Top != null && fluidCell.Top.Type != Cell.CellType.Solid)
                                                 && (fluidCell.Bottom != null && fluidCell.Bottom.Type != Cell.CellType.Solid)
                                                 && (fluidCell.Left != null && fluidCell.Left.Type != Cell.CellType.Solid)
                                                 && (fluidCell.Right != null && fluidCell.Right.Type != Cell.CellType.Solid);
                            var hyp = Mathf.Sqrt((_cellSize * _cellSize) + (_cellSize * _cellSize));
                            if (isNotNearSolid && fluidCell.Top != null && fluidCell.Top.LiquidAmount < .2f)
                            {
                                var circColor = color;
                                circColor.a = .4f;
                                DrawCircle(position, hyp / 4f, circColor);
                            }
                            */
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
        
        public void PrintCells()
        {
            for (var i = 0; i < _liquidSimulator.Cells.GetLength(0); i++)
            {
                var str = "";
                for (var j = 0; j < _liquidSimulator.Cells.GetLength(1); j++)
                {
                    var cell = _liquidSimulator.Cells[i, j];
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
    }
}