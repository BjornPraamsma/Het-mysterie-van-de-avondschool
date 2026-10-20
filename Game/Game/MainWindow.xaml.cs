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
                new BitmapImage(new Uri("pack://application:,,,/Assets/blonde_man/IdleFrontTop2BlondeMan.png"))
            };

            _animations["Idle_Back"] = new BitmapImage[]
            {
                new BitmapImage(new Uri("pack://application:,,,/Assets/blonde_man/IdleBackBotBlondMan.png")),
                new BitmapImage(new Uri("pack://application:,,,/Assets/blonde_man/IdleBackMidBlondMan.png")),
                new BitmapImage(new Uri("pack://application:,,,/Assets/blonde_man/IdleBackTopBlondMan.png")),
                new BitmapImage(new Uri("pack://application:,,,/Assets/blonde_man/IdleBackTop1BlondMan.png"))
            };

            _animations["Idle_Left"] = new BitmapImage[]
            {
                new BitmapImage(new Uri("pack://application:,,,/Assets/blonde_man/IdleLeftBot1BlondMan.png")),
                new BitmapImage(new Uri("pack://application:,,,/Assets/blonde_man/IdleLeftBot2BlondMan.png")),
                new BitmapImage(new Uri("pack://application:,,,/Assets/blonde_man/IdleLeftTop1BlondMan.png")),
                new BitmapImage(new Uri("pack://application:,,,/Assets/blonde_man/IdleLeftTop2BlondMan.png"))
            };

            _animations["Idle_Right"] = new BitmapImage[]
            {
                new BitmapImage(new Uri("pack://application:,,,/Assets/blonde_man/IdleRightBot1BlondMan.png")),
                new BitmapImage(new Uri("pack://application:,,,/Assets/blonde_man/IdleRightBot2BlondMan.png")),
                new BitmapImage(new Uri("pack://application:,,,/Assets/blonde_man/IdleRightTop1BlondMan.png")),
                new BitmapImage(new Uri("pack://application:,,,/Assets/blonde_man/IdleRightTop2BlondMan.png"))
            };

            // --- WALK ANIMATIONS ---
            _animations["Walk_Front"] = new BitmapImage[]
            {
                new BitmapImage(new Uri("pack://application:,,,/Assets/blonde_man/WalkFrontStand1BlondMan.png")),
                new BitmapImage(new Uri("pack://application:,,,/Assets/blonde_man/WalkFrontStrideLeftBlondMan.png")),
                new BitmapImage(new Uri("pack://application:,,,/Assets/blonde_man/WalkFrontStand2BlondMan.png")),
                new BitmapImage(new Uri("pack://application:,,,/Assets/blonde_man/WalkFrontStrideRightBlondMan.png"))
            };

            _animations["Walk_Back"] = new BitmapImage[]
            {
                new BitmapImage(new Uri("pack://application:,,,/Assets/blonde_man/WalkBackStand1BlondMan.png")),
                new BitmapImage(new Uri("pack://application:,,,/Assets/blonde_man/WalkBackStrideLeftBlondMan.png")),
                new BitmapImage(new Uri("pack://application:,,,/Assets/blonde_man/WalkBackStand2BlondMan.png")),
                new BitmapImage(new Uri("pack://application:,,,/Assets/blonde_man/WalkBackStrideRightBlondMan.png"))
            };

            _animations["Walk_Left"] = new BitmapImage[]
            {
                new BitmapImage(new Uri("pack://application:,,,/Assets/blonde_man/WalkLeftStand1BlondMan.png")),
                new BitmapImage(new Uri("pack://application:,,,/Assets/blonde_man/WalkLeftStrideLeftBlondMan.png")),
                new BitmapImage(new Uri("pack://application:,,,/Assets/blonde_man/WalkLeftStandBlondMan.png")),
                new BitmapImage(new Uri("pack://application:,,,/Assets/blonde_man/WalkLeftStrideRightBlondMan.png"))
            };

            _animations["Walk_Right"] = new BitmapImage[]
            {
                new BitmapImage(new Uri("pack://application:,,,/Assets/blonde_man/WalkRightStand1BlondMan.png")),
                new BitmapImage(new Uri("pack://application:,,,/Assets/blonde_man/WalkRightStrideLeftBlondMan.png")),
                new BitmapImage(new Uri("pack://application:,,,/Assets/blonde_man/WalkRightStand2BlondMan.png")),
                new BitmapImage(new Uri("pack://application:,,,/Assets/blonde_man/WalkRightStrideRightBlondMan.png"))
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

    // 3. DOOR CLASS: Stores state and visual reference for individual doors
    public class Door
    {
        public int Row { get; set; }
        public int Column { get; set; }
        public bool IsOpen { get; set; }
        public Rectangle Visual { get; set; }
    }

    public partial class MainWindow : Window
    {
        // 4. CLASS VARIABLES
        private DispatcherTimer gameTimer = new DispatcherTimer();
        private DispatcherTimer animationTimer = new DispatcherTimer();

        private bool moveLeft, moveRight, moveUp, moveDown;
        private double playerSpeed = 4.0;
        private const int TileSize = 64;
        private const int DoorTile = 2;

        private readonly int[,] levelMap = new int[,]
        {
            { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
            { 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 1 },
            { 1, 0, 0, 0, 0, 2, 0, 0, 0, 0, 0, 1 },
            { 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 1 },
            { 1, 2, 1, 1, 1, 1, 1, 1, 2, 1, 1, 1 },
            { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 },
            { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 },
            { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 }
        };

        // Collection to track all dynamic doors present on the map
        private List<Door> doors = new List<Door>();
        private TextBlock interactionText;

        // Sprite and Animation tracking variables
        private SpriteManager spriteManager;
        private CharacterState currentState = CharacterState.Idle;
        private Direction currentDirection = Direction.Front;
        private int currentFrameIndex = 0;

        // 5. THE CONSTRUCTOR
        public MainWindow()
        {
            InitializeComponent();

            spriteManager = new SpriteManager();

            LoadAssets();

            // 60 FPS Game Loop (Movement & Collisions)
            gameTimer.Tick += GameLoop;
            gameTimer.Interval = TimeSpan.FromMilliseconds(16);
            gameTimer.Start();

            // Animation Loop (~150ms per frame)
            animationTimer.Tick += AnimationLoop;
            animationTimer.Interval = TimeSpan.FromMilliseconds(150);
            animationTimer.Start();
        }

        // 6. ANIMATION LOOP
        private void AnimationLoop(object sender, EventArgs e)
        {
            BitmapImage[] currentFrames = spriteManager.GetAnimationFrames(currentState, currentDirection);

            if (currentFrames != null && currentFrames.Length > 0)
            {
                currentFrameIndex = (currentFrameIndex + 1) % currentFrames.Length;
                Player.Source = currentFrames[currentFrameIndex];
            }
        }

        private void SetCharacterState(CharacterState newState, Direction newDirection)
        {
            if (currentState != newState || currentDirection != newDirection)
            {
                currentState = newState;
                currentDirection = newDirection;
                currentFrameIndex = 0;

                BitmapImage[] frames = spriteManager.GetAnimationFrames(currentState, currentDirection);
                if (frames != null && frames.Length > 0)
                {
                    Player.Source = frames[0];
                }
            }
        }

        // 7. LEVEL BUILDER: Dynamically spawns tiles and door objects
        private void LoadAssets()
        {
            SetCharacterState(CharacterState.Idle, Direction.Front);

            BitmapImage floorTile = new BitmapImage(new Uri("pack://application:,,,/Assets/tile_0000.png"));
            BitmapImage wallTile = new BitmapImage(new Uri("pack://application:,,,/Assets/tile_0001.png"));

            doors.Clear();

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
                    {
                        tile.Source = floorTile;
                    }
                    else if (levelMap[row, col] == 1)
                    {
                        tile.Source = wallTile;
                    }
                    else if (levelMap[row, col] == DoorTile)
                    {
                        tile.Source = floorTile;

                        Rectangle doorVisual = new Rectangle
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

                        // Instantiate independent Door object and add to collection
                        Door door = new Door
                        {
                            Row = row,
                            Column = col,
                            IsOpen = false,
                            Visual = doorVisual
                        };
                        doors.Add(door);

                        GameCanvas.Children.Add(doorVisual);
                    }

                    Panel.SetZIndex(tile, -1);
                    Canvas.SetLeft(tile, col * TileSize);
                    Canvas.SetTop(tile, row * TileSize);
                    GameCanvas.Children.Add(tile);
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

        // 8. GAME LOOP
        private void GameLoop(object sender, EventArgs e)
        {
            double left = Canvas.GetLeft(Player);
            double top = Canvas.GetTop(Player);

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

            CharacterState newState = isMoving ? CharacterState.Walk : CharacterState.Idle;
            SetCharacterState(newState, newDirection);

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

        // Dynamic Collision Handling
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

                    if (tile == 1) // Wall
                    {
                        return true;
                    }
                    else if (tile == DoorTile)
                    {
                        // Look up the door state for this specific grid cell
                        Door door = GetDoorAt(row, column);
                        if (door == null || !door.IsOpen)
                        {
                            return true; // Block player if door is closed
                        }
                    }
                }
            }

            return false;
        }

        // Returns the Door object located at a specific grid row and column
        private Door GetDoorAt(int row, int col)
        {
            foreach (var door in doors)
            {
                if (door.Row == row && door.Column == col)
                    return door;
            }
            return null;
        }

        // Returns the closest Door object within interaction distance
        private Door GetNearestDoor()
        {
            double playerCenterX = Canvas.GetLeft(Player) + Player.Width / 2;
            double playerCenterY = Canvas.GetTop(Player) + Player.Height / 2;

            Door nearestDoor = null;
            double shortestDistance = double.MaxValue;

            foreach (var door in doors)
            {
                double doorCenterX = door.Column * TileSize + TileSize / 2;
                double doorCenterY = door.Row * TileSize + TileSize / 2;

                double deltaX = playerCenterX - doorCenterX;
                double deltaY = playerCenterY - doorCenterY;

                if (Math.Abs(deltaX) <= TileSize * 1.5 && Math.Abs(deltaY) <= TileSize * 1.5)
                {
                    double distance = Math.Sqrt(deltaX * deltaX + deltaY * deltaY);
                    if (distance < shortestDistance)
                    {
                        shortestDistance = distance;
                        nearestDoor = door;
                    }
                }
            }

            return nearestDoor;
        }

        private void UpdateInteractionPrompt()
        {
            Door nearbyDoor = GetNearestDoor();

            if (nearbyDoor != null)
            {
                interactionText.Text = nearbyDoor.IsOpen
                    ? "Door is open"
                    : "Press E to open the door";
            }
            else
            {
                interactionText.Text = string.Empty;
            }
        }

        // 9. INPUT HANDLERS
        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.E)
            {
                Door doorToOpen = GetNearestDoor();
                if (doorToOpen != null && !doorToOpen.IsOpen)
                {
                    doorToOpen.IsOpen = true;
                    doorToOpen.Visual.Fill = Brushes.Transparent;
                    doorToOpen.Visual.Stroke = Brushes.Transparent;
                    UpdateInteractionPrompt();
                }
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