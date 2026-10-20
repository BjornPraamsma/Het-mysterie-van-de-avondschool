using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Game
{
    public partial class MainWindow : Window
    {
        // 1. CLASS VARIABLES: These sit at the top so the whole window can use them.
        private DispatcherTimer gameTimer = new DispatcherTimer();
        private bool moveLeft, moveRight, moveUp, moveDown;
        private double playerSpeed = 5.0;
        private const int TileSize = 64;
        private const int DoorTile = 2;
        private readonly int[,] levelMap = new int[,]
        {
            { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
            { 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 1 },
            { 1, 0, 0, 0, 0, 2, 0, 0, 0, 0, 0, 1 },
            { 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 1 },
            { 1, 0, 1, 1, 1, 1, 1, 1, 0, 1, 1, 1 },
            { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 },
            { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 },
            { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 }
        };
        private readonly int doorRow = 2;
        private readonly int doorColumn = 5;
        private bool doorOpen;
        private Rectangle doorVisual;
        private TextBlock interactionText;

        // Spritesheet specific variables
        private BitmapImage characterSheet;
        private int frameWidth = 24;  // CHANGE THIS: The width of one single character frame in pixels
        private int frameHeight = 24; // CHANGE THIS: The height of one single character frame in pixels

        // 2. THE CONSTRUCTOR: This runs the moment the window opens.
        public MainWindow()
        {
            InitializeComponent();

            LoadAssets();

            // Start the 60 FPS game loop
            gameTimer.Tick += GameLoop;
            gameTimer.Interval = TimeSpan.FromMilliseconds(16);
            gameTimer.Start();
        }

        // 3. SPRITE SLICER: A helper method to cut a specific square out of the Character.png
        private void UpdateSpriteFrame(int column, int row)
        {
            int startX = column * frameWidth;
            int startY = row * frameHeight;

            // Crop out just the frame we need
            CroppedBitmap frame = new CroppedBitmap(
                characterSheet,
                new Int32Rect(startX, startY, frameWidth, frameHeight)
            );

            Player.Source = frame;
        }

        // 4. LEVEL BUILDER: Loads the graphics and draws the grid map.
        private void LoadAssets()
        {
            // Load the full character sheet into memory
            characterSheet = new BitmapImage(new Uri("pack://application:,,,/Assets/Character.png"));

            // Show the character facing forward immediately (Column 0, Row 0)
            UpdateSpriteFrame(0, 0);

            // Load the environmental tiles
            BitmapImage floorTile = new BitmapImage(new Uri("pack://application:,,,/Assets/tile_0000.png"));
            BitmapImage wallTile = new BitmapImage(new Uri("pack://application:,,,/Assets/tile_0001.png"));

            // Loop through the array and draw the correct image for each number
            for (int row = 0; row < levelMap.GetLength(0); row++)
            {
                for (int col = 0; col < levelMap.GetLength(1); col++)
                {
                    Image tile = new Image
                    {
                        Width = TileSize,
                        Height = TileSize
                    };

                    if (levelMap[row, col] == 0)
                        tile.Source = floorTile;
                    else if (levelMap[row, col] == 1)
                        tile.Source = wallTile;
                    else if (levelMap[row, col] == DoorTile)
                    {
                        tile.Source = floorTile;
                        doorVisual = new Rectangle
                        {
                            Width = TileSize,
                            Height = TileSize,
                            Fill = Brushes.SaddleBrown,
                            Stroke = Brushes.Black,
                            StrokeThickness = 3
                        };
                        Panel.SetZIndex(doorVisual, -1);
                        Canvas.SetLeft(doorVisual, col * TileSize);
                        Canvas.SetTop(doorVisual, row * TileSize);
                    }

                    // Push tiles behind the player
                    Panel.SetZIndex(tile, -1);
                    Canvas.SetLeft(tile, col * TileSize);
                    Canvas.SetTop(tile, row * TileSize);
                    GameCanvas.Children.Add(tile);

                    if (levelMap[row, col] == DoorTile)
                        GameCanvas.Children.Add(doorVisual);
                }
            }

            interactionText = new TextBlock
            {
                Foreground = Brushes.White,
                FontSize = 16,
                Background = new SolidColorBrush(Color.FromArgb(180, 0, 0, 0)),
                Padding = new Thickness(6)
            };
            Canvas.SetLeft(interactionText, 8);
            Canvas.SetTop(interactionText, 8);
            Panel.SetZIndex(interactionText, 1);
            GameCanvas.Children.Add(interactionText);
        }

        // 5. THE GAME LOOP: Runs 60 times a second to update positions and animations.
        private void GameLoop(object sender, EventArgs e)
        {
            double left = Canvas.GetLeft(Player);
            double top = Canvas.GetTop(Player);

            if (moveLeft)
            {
                left -= playerSpeed;
                UpdateSpriteFrame(0, 1); // CHANGE THIS: Column 0, Row 1 for walking left
            }
            if (moveRight)
            {
                left += playerSpeed;
                UpdateSpriteFrame(0, 2); // CHANGE THIS: Column 0, Row 2 for walking right
            }
            if (moveUp)
            {
                top -= playerSpeed;
                UpdateSpriteFrame(0, 3); // CHANGE THIS: Column 0, Row 3 for walking up
            }
            if (moveDown)
            {
                top += playerSpeed;
                UpdateSpriteFrame(0, 0); // CHANGE THIS: Column 0, Row 0 for walking down
            }

            double currentLeft = Canvas.GetLeft(Player);
            double currentTop = Canvas.GetTop(Player);

            if (!IsBlocked(left, currentTop))
                currentLeft = left;
            if (!IsBlocked(currentLeft, top))
                currentTop = top;

            Canvas.SetLeft(Player, currentLeft);
            Canvas.SetTop(Player, currentTop);

            UpdateInteractionPrompt();
        }

        private bool IsBlocked(double left, double top)
        {
            if (left < 0 || top < 0 || left + Player.Width > levelMap.GetLength(1) * TileSize ||
                top + Player.Height > levelMap.GetLength(0) * TileSize)
                return true;

            int firstColumn = (int)(left / TileSize);
            int lastColumn = (int)((left + Player.Width - 1) / TileSize);
            int firstRow = (int)(top / TileSize);
            int lastRow = (int)((top + Player.Height - 1) / TileSize);

            for (int row = firstRow; row <= lastRow; row++)
            {
                for (int column = firstColumn; column <= lastColumn; column++)
                {
                    int tile = levelMap[row, column];
                    if (tile != 0 && !(tile == DoorTile && doorOpen))
                        return true;
                }
            }

            return false;
        }

        private bool IsNearDoor()
        {
            double playerCenterX = Canvas.GetLeft(Player) + Player.Width / 2;
            double playerCenterY = Canvas.GetTop(Player) + Player.Height / 2;
            double doorCenterX = doorColumn * TileSize + TileSize / 2;
            double doorCenterY = doorRow * TileSize + TileSize / 2;
            return Math.Abs(playerCenterX - doorCenterX) <= TileSize * 1.5 &&
                   Math.Abs(playerCenterY - doorCenterY) <= TileSize * 1.5;
        }

        private void UpdateInteractionPrompt()
        {
            interactionText.Text = doorOpen
                ? "Door is open"
                : IsNearDoor() ? "Press E to open the door" : string.Empty;
        }

        // 6. INPUT HANDLERS: These catch the keyboard presses.
        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.E && !doorOpen && IsNearDoor())
            {
                doorOpen = true;
                doorVisual.Fill = Brushes.Transparent;
                doorVisual.Stroke = Brushes.Transparent;
                UpdateInteractionPrompt();
            }

            if (e.Key == Key.A || e.Key == Key.Left) moveLeft = true;
            if (e.Key == Key.D || e.Key == Key.Right) moveRight = true;
            if (e.Key == Key.W || e.Key == Key.Up) moveUp = true;
            if (e.Key == Key.S || e.Key == Key.Down) moveDown = true;
        }

        private void OnKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.A || e.Key == Key.Left) moveLeft = false;
            if (e.Key == Key.D || e.Key == Key.Right) moveRight = false;
            if (e.Key == Key.W || e.Key == Key.Up) moveUp = false;
            if (e.Key == Key.S || e.Key == Key.Down) moveDown = false;
        }
    }
}