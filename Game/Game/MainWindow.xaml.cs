using System;
using System.Collections.Generic;
using System.Security.Policy;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Game
{
    // 1. CHARACTER STATES: Enums to track what the player is doing and facing
    public enum CharacterState { Idle, Walk }
    public enum Direction { Front, Back, Left, Right }

    // 2. SPRITE MANAGER: Pre-loads all individual PNG frames into memory to prevent lag
    public class SpriteManager
    {
        private Dictionary<string, BitmapImage[]> _animations = new Dictionary<string, BitmapImage[]>();

        public SpriteManager()
        {
            LoadSprites();
        }

        private void LoadSprites()
        {
            // --- IDLE ANIMATIONS ---
            _animations["Idle_Front"] = new BitmapImage[]
            {
                new BitmapImage(new Uri("pack://application:,,,/Assets/blonde_man/IdleFrontBotBlondeMan.png")),
                new BitmapImage(new Uri("pack://application:,,,/Assets/blonde_man/IdleFrontMidBlondeMan.png")),
                new BitmapImage(new Uri("pack://application:,,,/Assets/blonde_man/IdleFrontTop1BlondeMan.png")),
                new BitmapImage(new Uri("pack://application:,,,/Assets/blonde_man/IdleFrontTop2BlondeMan.png")),
            };

            _animations["Idle_Back"] = new BitmapImage[]
            {
                new BitmapImage(new Uri("pack://application:,,,/Assets/blonde_man/IdleBackBotBlondMan.png")),
                new BitmapImage(new Uri("pack://application:,,,/Assets/blonde_man/IdleBackMidBlondMan.png")),
                new BitmapImage(new Uri("pack://application:,,,/Assets/blonde_man/IdleBackTopBlondMan.png")),
                new BitmapImage(new Uri("pack://application:,,,/Assets/blonde_man/IdleBackTop1BlondMan.png")),
            };

            _animations["Idle_Left"] = new BitmapImage[]
            {
                new BitmapImage(new Uri("pack://application:,,,/Assets/blonde_man/IdleLeftBot1BlondMan.png")),
                new BitmapImage(new Uri("pack://application:,,,/Assets/blonde_man/IdleLeftBot2BlondMan.png")),
                new BitmapImage(new Uri("pack://application:,,,/Assets/blonde_man/IdleLeftTop1BlondMan.png")),
                new BitmapImage(new Uri("pack://application:,,,/Assets/blonde_man/IdleLeftTop2BlondMan.png")),
            };

            _animations["Idle_Right"] = new BitmapImage[]
            {
                new BitmapImage(new Uri("pack://application:,,,/Assets/blonde_man/IdleRightBot1BlondMan.png")),
                new BitmapImage(new Uri("pack://application:,,,/Assets/blonde_man/IdleRightBot2BlondMan.png")),
                new BitmapImage(new Uri("pack://application:,,,/Assets/blonde_man/IdleRightTop1BlondMan.png")),
                new BitmapImage(new Uri("pack://application:,,,/Assets/blonde_man/IdleRightTop2BlondMan.png")),
            };

            // --- WALK ANIMATIONS ---
            _animations["Walk_Front"] = new BitmapImage[]
            {
                new BitmapImage(new Uri("pack://application:,,,/Assets/blonde_man/WalkFrontStand1BlondMan.png")),
                new BitmapImage(new Uri("pack://application:,,,/Assets/blonde_man/WalkFrontStrideLeftBlondMan.png")),
                new BitmapImage(new Uri("pack://application:,,,/Assets/blonde_man/WalkFrontStand2BlondMan.png")),
                new BitmapImage(new Uri("pack://application:,,,/Assets/blonde_man/WalkFrontStrideRightBlondMan.png")),
            };

            _animations["Walk_Back"] = new BitmapImage[]
            {
                new BitmapImage(new Uri("pack://application:,,,/Assets/blonde_man/WalkBackStand1BlondMan.png")),
                new BitmapImage(new Uri("pack://application:,,,/Assets/blonde_man/WalkBackStrideLeftBlondMan.png")),
                new BitmapImage(new Uri("pack://application:,,,/Assets/blonde_man/WalkBackStand2BlondMan.png")),
                new BitmapImage(new Uri("pack://application:,,,/Assets/blonde_man/WalkBackStrideRightBlondMan.png")),
            };

            _animations["Walk_Left"] = new BitmapImage[]
            {
                new BitmapImage(new Uri("pack://application:,,,/Assets/blonde_man/WalkLeftStand1BlondMan.png")),
                new BitmapImage(new Uri("pack://application:,,,/Assets/blonde_man/WalkLeftStrideLeftBlondMan.png")),
                new BitmapImage(new Uri("pack://application:,,,/Assets/blonde_man/WalkLeftStandBlondMan.png")),
                new BitmapImage(new Uri("pack://application:,,,/Assets/blonde_man/WalkLeftStrideRightBlondMan.png")),
            };

            _animations["Walk_Right"] = new BitmapImage[]
            {
                new BitmapImage(new Uri("pack://application:,,,/Assets/blonde_man/WalkRightStand1BlondMan.png")),
                new BitmapImage(new Uri("pack://application:,,,/Assets/blonde_man/WalkRightStrideLeftBlondMan.png")),
                new BitmapImage(new Uri("pack://application:,,,/Assets/blonde_man/WalkRightStand2BlondMan.png")),
                new BitmapImage(new Uri("pack://application:,,,/Assets/blonde_man/WalkRightStrideRightBlondMan.png")),
            };
        }

        public BitmapImage[] GetAnimationFrames(CharacterState state, Direction direction)
        {
            string key = $"{state}_{direction}";
            if (_animations.ContainsKey(key))
                return _animations[key];

            return null;
        }
    }

    public partial class MainWindow : Window
    {
        // 3. CLASS VARIABLES
        private DispatcherTimer gameTimer = new DispatcherTimer();
        private DispatcherTimer animationTimer = new DispatcherTimer(); // NEW: Separate timer just for animation frames

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

        // NEW: Replaced the slicer variables with SpriteManager and State tracking variables
        private SpriteManager spriteManager;
        private CharacterState currentState = CharacterState.Idle;
        private Direction currentDirection = Direction.Front;
        private int currentFrameIndex = 0;

        // 4. THE CONSTRUCTOR
        public MainWindow()
        {
            InitializeComponent();

            // Initialize our new dictionary-based sprite loader
            spriteManager = new SpriteManager();

            LoadAssets();

            // Start the 60 FPS game loop (Handles movement and collisions)
            gameTimer.Tick += GameLoop;
            gameTimer.Interval = TimeSpan.FromMilliseconds(16);
            gameTimer.Start();

            // Start the Animation loop (~150ms per frame to animate the character)
            animationTimer.Tick += AnimationLoop;
            animationTimer.Interval = TimeSpan.FromMilliseconds(150);
            animationTimer.Start();
        }

        // 5. ANIMATION LOOP: Only handles advancing the image index, not the physics
        private void AnimationLoop(object sender, EventArgs e)
        {
            BitmapImage[] currentFrames = spriteManager.GetAnimationFrames(currentState, currentDirection);

            if (currentFrames != null && currentFrames.Length > 0)
            {
                // Move to the next frame, loop back to 0 if at the end of the array
                currentFrameIndex = (currentFrameIndex + 1) % currentFrames.Length;
                Player.Source = currentFrames[currentFrameIndex];
            }
        }

        // Helper method to smoothly transition states instantly (prevents input delay visually)
        private void SetCharacterState(CharacterState newState, Direction newDirection)
        {
            // Only update if the player actually changed what they are doing
            if (currentState != newState || currentDirection != newDirection)
            {
                currentState = newState;
                currentDirection = newDirection;
                currentFrameIndex = 0; // Reset animation to the beginning

                // Instantly show the first frame of the new state
                BitmapImage[] frames = spriteManager.GetAnimationFrames(currentState, currentDirection);
                if (frames != null && frames.Length > 0)
                {
                    Player.Source = frames[0];
                }
            }
        }

        // 6. LEVEL BUILDER: Loads the graphics and draws the grid map.
        private void LoadAssets()
        {
            // Set initial character state to idle facing forward
            SetCharacterState(CharacterState.Idle, Direction.Front);

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
                        Canvas.SetLeft(doorVisual, col * TileSize); // Fixed formatting
                        Canvas.SetTop(doorVisual, row * TileSize); // Fixed formatting
                    }

                    // Push tiles behind the player
                    Panel.SetZIndex(tile, -1);
                    Canvas.SetLeft(tile, col * TileSize); // Fixed formatting
                    Canvas.SetTop(tile, row * TileSize); // Fixed formatting
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

        // 7. THE GAME LOOP: Runs 60 times a second to update positions
        private void GameLoop(object sender, EventArgs e)
        {
            double left = Canvas.GetLeft(Player);
            double top = Canvas.GetTop(Player);

            // Track what the player is doing this specific frame
            bool isMoving = false;
            Direction newDirection = currentDirection;

            if (moveLeft)
            {
                left -= playerSpeed;
                newDirection = Direction.Left;
                isMoving = true;
            }
            else if (moveRight)
            {
                left += playerSpeed;
                newDirection = Direction.Right;
                isMoving = true;
            }

            if (moveUp)
            {
                top -= playerSpeed;
                newDirection = Direction.Back;
                isMoving = true;
            }
            else if (moveDown)
            {
                top += playerSpeed;
                newDirection = Direction.Front;
                isMoving = true;
            }

            // Decide if we should be playing the Walk or Idle animation
            CharacterState newState = isMoving ? CharacterState.Walk : CharacterState.Idle;

            // Send the states to the animation helper
            SetCharacterState(newState, newDirection);

            // Collision Logic
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

            // Fixed formatting: re-added minus signs that were stripped in paste
            return Math.Abs(playerCenterX - doorCenterX) <= TileSize * 1.5 &&
                   Math.Abs(playerCenterY - doorCenterY) <= TileSize * 1.5;
        }

        private void UpdateInteractionPrompt()
        {
            interactionText.Text = doorOpen
                ? "Door is open"
                : IsNearDoor() ? "Press E to open the door" : string.Empty;
        }

        // 8. INPUT HANDLERS: These catch the keyboard presses.
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