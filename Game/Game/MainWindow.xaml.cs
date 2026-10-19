using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Game
{
    public partial class MainWindow : Window
    {
        // 1. CLASS VARIABLES: These sit at the top so the whole window can use them.
        private DispatcherTimer gameTimer = new DispatcherTimer();
        private bool moveLeft, moveRight, moveUp, moveDown;
        private double playerSpeed = 5.0;

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

            // 0 = Floor, 1 = Wall. You can expand this grid to build out rooms.
            int[,] levelMap = new int[,]
            {
                { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
                { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 },
                { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 },
                { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 },
                { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 }
            };

            int tileSize = 64;

            // Loop through the array and draw the correct image for each number
            for (int row = 0; row < levelMap.GetLength(0); row++)
            {
                for (int col = 0; col < levelMap.GetLength(1); col++)
                {
                    Image tile = new Image
                    {
                        Width = tileSize,
                        Height = tileSize
                    };

                    if (levelMap[row, col] == 0)
                        tile.Source = floorTile;
                    else if (levelMap[row, col] == 1)
                        tile.Source = wallTile;

                    // Push tiles behind the player
                    Panel.SetZIndex(tile, -1);
                    Canvas.SetLeft(tile, col * tileSize);
                    Canvas.SetTop(tile, row * tileSize);
                    GameCanvas.Children.Add(tile);
                }
            }
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

            Canvas.SetLeft(Player, left);
            Canvas.SetTop(Player, top);
        }

        // 6. INPUT HANDLERS: These catch the keyboard presses.
        private void OnKeyDown(object sender, KeyEventArgs e)
        {
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