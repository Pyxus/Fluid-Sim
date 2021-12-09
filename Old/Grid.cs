using Godot;

namespace FluidSim.Old
{
    public class Grid : Node2D
    {
        int Width = 80;
        int Height = 40;
        [Export(PropertyHint.Range, ".1,1")] float CellSize = 1;
        float PreviousCellSize = 1;

        [Export(PropertyHint.Range, ".1,1")] float LineWidth = 0;
        float PreviousLineWidth = 0;

        [Export()] Color LineColor = Colors.Black;
        Color PreviousLineColor = Colors.Black;

        [Export()] bool ShowFlow = true;

        [Export] bool RenderDownFlowingLiquid = false;

        [Export] bool RenderFloatingLiquid = false;

        Cell[,] Cells;
        //GridLine[] HorizontalLines;
        //GridLine[] VerticalLines;

        LiquidSimulator LiquidSimulator;
        Sprite[] LiquidFlowSprites;


        bool Fill;

        public override void _Ready()
        {
            base._Ready();
            CreateGrid();

            // Initialize the liquid simulator
            LiquidSimulator = new LiquidSimulator();
            //LiquidSimulator.Initialize(Cells);
            Cells[0, 0].SetType(CellType.Blank);
            Cells[0, 0].AddLiquid(5);
        }

        void CreateGrid()
        {
            Cells = new Cell[Width, Height];
            Vector2 offset = Position;

            // Cells
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    Cell cell = new Cell();
                    float xpos = offset.x + (x * CellSize) + (LineWidth * x) + LineWidth;
                    float ypos = offset.y - (y * CellSize) - (LineWidth * y) - LineWidth;
                    cell.Set(x, y, new Vector2(xpos, ypos), CellSize, LiquidFlowSprites, ShowFlow, RenderDownFlowingLiquid, RenderFloatingLiquid);

                    // add a border
                    if (x == 0 || y == 0 || x == Width - 1 || y == Height - 1)
                    {
                        cell.SetType(CellType.Solid);
                    }

                    AddChild(cell);
                    Cells[x, y] = cell;
                }
            }

            UpdateNeighbors();
        }

        // Live update the grid properties
        void RefreshGrid()
        {
            /*
            Vector2 offset = this.transform.position;

            // vertical grid lines
            for (int x = 0; x < Width + 1; x++)
            {
                float xpos = offset.x + (CellSize * x) + (LineWidth * x);
                VerticalLines[x].Set(LineColor, new Vector2(xpos, offset.y), new Vector2(LineWidth, (Height * CellSize) + LineWidth * Height + LineWidth));
            }

            // horizontal grid lines
            for (int y = 0; y < Height + 1; y++)
            {
                float ypos = offset.y - (CellSize * y) - (LineWidth * y);
                HorizontalLines[y].Set(LineColor, new Vector2(offset.x, ypos), new Vector2((Width * CellSize) + LineWidth * Width + LineWidth, LineWidth));
            }

            // Cells
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    float xpos = offset.x + (x * CellSize) + (LineWidth * x) + LineWidth;
                    float ypos = offset.y - (y * CellSize) - (LineWidth * y) - LineWidth;
                    Cells[x, y].Set(x, y, new Vector2(xpos, ypos), CellSize, LiquidFlowSprites, ShowFlow, RenderDownFlowingLiquid, RenderFloatingLiquid);
                }
            }

            // Fit camera to grid
            View.transform.position = this.transform.position + new Vector3(HorizontalLines[0].transform.localScale.x / 2f, -VerticalLines[0].transform.localScale.y / 2f);
            View.transform.localScale = new Vector2(HorizontalLines[0].transform.localScale.x, VerticalLines[0].transform.localScale.y);
            Camera.main.GetComponent<Camera2D>().Set();
            */
        }

        // Sets neighboring cell references
        void UpdateNeighbors()
        {
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    if (x > 0)
                    {
                        Cells[x, y].Left = Cells[x - 1, y];
                    }

                    if (x < Width - 1)
                    {
                        Cells[x, y].Right = Cells[x + 1, y];
                    }

                    if (y > 0)
                    {
                        Cells[x, y].Top = Cells[x, y - 1];
                    }

                    if (y < Height - 1)
                    {
                        Cells[x, y].Bottom = Cells[x, y + 1];
                    }
                }
            }
        }

        public override void _Process(float delta)
        {
            base._Process(delta);
            //LiquidSimulator.Simulate(ref Cells);
        }

        void Update()
        {
            /*
             CODE TO PLACE FLUID AND STUFF
            // Update grid lines and cell size
            if (PreviousCellSize != CellSize || PreviousLineColor != LineColor || PreviousLineWidth != LineWidth)
            {
                RefreshGrid();
            }

            // Convert mouse position to Grid Coordinates
            //Vector2 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            int x = (int)((pos.x - this.transform.position.x) / (CellSize + LineWidth));
            int y = -(int)((pos.y - this.transform.position.y) / (CellSize + LineWidth));

            // Check if we are filling or erasing walls
            if (Input.IsMouseButtonPressed((int) ButtonList.Left))
            {
                if ((x > 0 && x < Cells.GetLength(0)) && (y > 0 && y < Cells.GetLength(1)))
                {
                    if (Cells[x, y].Type == CellType.Blank)
                    {
                        Fill = true;
                    }
                    else
                    {
                        Fill = false;
                    }
                }
            }

            // Left click draws/erases walls
            if (Input.IsMouseButtonPressed((int) ButtonList.Left))
            {
                if (x != 0 && y != 0 && x != Width - 1 && y != Height - 1)
                {
                    if ((x > 0 && x < Cells.GetLength(0)) && (y > 0 && y < Cells.GetLength(1)))
                    {
                        if (Fill)
                        {
                            Cells[x, y].SetType(CellType.Solid);
                        }
                        else
                        {
                            Cells[x, y].SetType(CellType.Blank);
                        }
                    }
                }
            }

            // Right click places liquid
            
            if (Input.IsMouseButtonPressed((int) ButtonList.Right))
            {
                if ((x > 0 && x < Cells.GetLength(0)) && (y > 0 && y < Cells.GetLength(1)))
                {
                    Cells[x, y].AddLiquid(5);
                }
            }
            */

            // Run our liquid simulation 
            //LiquidSimulator.Simulate(ref Cells);
        }
    }
}