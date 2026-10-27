using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Game.Config;
using Game.Models;
using Game.Services;
using System.Collections.Generic;

namespace Game
{
    public partial class MainWindow : Window
    {
        private readonly LevelMap _currentMap = LevelMap.GetSchoolLevel();
        private readonly CharacterSpriteConfig _playerSprites = CharacterSpriteConfig.LoadBlondeMan();
        private readonly CollisionSystem _collisionSystem = new();

        private TileMapRenderer _mapRenderer = null!;
        private QuizManager _quizManager = null!;

        private double _playerX;
        private double _playerY;
        private Direction _currentDirection = Direction.Front;

        private bool _isMoving;
        private bool _moveUp, _moveDown, _moveLeft, _moveRight;
        private bool _isGameStarted = false; // Toegevoegd om te controleren of we in het menu zijn

        private int _animationFrame;
        private DateTime _lastAnimTime = DateTime.Now;
        private readonly Image _playerImage = new();

        // ==========================================
        // BOEKENKASTEN
        // ==========================================

        private readonly Dictionary<int, BookcaseData> _bookcases = new()
        {
            [20] = new BookcaseData
            {
                TileId = 20,
                Titel = "De Wiskundekast",
                Beschrijving = "Je vindt een oud rekenboek.",
                Vraag = "Wat is 7 × 8?",
                Antwoord = "56",
                Aanwijzing = "De eerste aanwijzing is gevonden!"
            },

            [21] = new BookcaseData
            {
                TileId = 21,
                Titel = "De Geheime Kast",
                Beschrijving = "Je ontdekt een geheimzinnig briefje.",
                Vraag = "Wat is de hoofdstad van Frankrijk?",
                Antwoord = "Parijs",
                Aanwijzing = "Je vindt een tweede aanwijzing!"
            },

            [22] = new BookcaseData
            {
                TileId = 22,
                Titel = "De Geschiedeniskast",
                Beschrijving = "Er ligt een stoffig geschiedenisboek.",
                Vraag = "In welk jaar begon de Tweede Wereldoorlog?",
                Antwoord = "1939",
                Aanwijzing = "Je hebt de derde aanwijzing gevonden!"
            }
        };

        private bool _bookcaseOpen = false;

        public MainWindow()
        {
            InitializeComponent();

            _mapRenderer = new TileMapRenderer(GameCanvas);
            _quizManager = new QuizManager(QuizOverlay, QuizQuestionText, QuizAnswerInput, QuizFeedbackText);

            _playerX = 7 * GameConfig.TileSize;
            _playerY = 6 * GameConfig.TileSize;

            InitializePlayer();
            _mapRenderer.Render(_currentMap, _playerImage);
            GameCanvas.Width = 2500;
            GameCanvas.Height = 2500;

            CompositionTarget.Rendering += OnRenderFrame;
        }

        // ==========================================
        // START MENU FUNCTIES
        // ==========================================

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // Verberg het start menu
            StartMenuOverlay.Visibility = Visibility.Collapsed;

            // Laat de game canvas (in de ScrollViewer) zien
            GameScrollViewer.Visibility = Visibility.Visible;

            // Zorg dat de canvas direct focus krijgt zodat KeyDown events werken
            GameCanvas.Focus();

            _isGameStarted = true;
        }

        private void Afsluiten_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        // ==========================================
        // GAME FUNCTIES
        // ==========================================

        private void InitializePlayer()
        {
            _playerImage.Width = GameConfig.TileSize;
            _playerImage.Height = GameConfig.TileSize;
            UpdatePlayerSprite();

            Canvas.SetLeft(_playerImage, _playerX);
            Canvas.SetTop(_playerImage, _playerY);
            Canvas.SetZIndex(_playerImage, 10);
        }

        private void OnRenderFrame(object? sender, EventArgs e)
        {
            // Stop rendering als het spel nog niet is gestart of als er een quiz bezig is
            if (!_isGameStarted || _quizManager.IsQuizActive) return;

            UpdateMovement();
            UpdateAnimation();

            // 1. Bepaal de marges (bijv. 250 pixels vanaf de randen)
            double marginX = 250;
            double marginY = 200;

            // 2. Haal de huidige scroll-posities op
            double currentScrollX = GameScrollViewer.HorizontalOffset;
            double currentScrollY = GameScrollViewer.VerticalOffset;

            // 3. Bereken de positie van de speler op het scherm (relatief tot de scrollviewer)
            double playerScreenX = _playerX - currentScrollX;
            double playerScreenY = _playerY - currentScrollY;

            double newScrollX = currentScrollX;
            double newScrollY = currentScrollY;

            // 4. Controleer of de speler de marges raakt en pas de scroll aan
            if (playerScreenX > GameScrollViewer.ActualWidth - marginX)
            {
                newScrollX = _playerX - (GameScrollViewer.ActualWidth - marginX);
            }
            else if (playerScreenX < marginX)
            {
                newScrollX = _playerX - marginX;
            }

            if (playerScreenY > GameScrollViewer.ActualHeight - marginY)
            {
                newScrollY = _playerY - (GameScrollViewer.ActualHeight - marginY);
            }
            else if (playerScreenY < marginY)
            {
                newScrollY = _playerY - marginY;
            }

            // 5. Pas de nieuwe offset toe
            GameScrollViewer.ScrollToHorizontalOffset(newScrollX);
            GameScrollViewer.ScrollToVerticalOffset(newScrollY);
        }

        private void UpdateMovement()
        {
            double nextX = _playerX;
            double nextY = _playerY;

            _isMoving = _moveUp || _moveDown || _moveLeft || _moveRight;

            if (_moveUp) { nextY -= GameConfig.DefaultPlayerSpeed; _currentDirection = Direction.Back; }
            if (_moveDown) { nextY += GameConfig.DefaultPlayerSpeed; _currentDirection = Direction.Front; }
            if (_moveLeft) { nextX -= GameConfig.DefaultPlayerSpeed; _currentDirection = Direction.Left; }
            if (_moveRight) { nextX += GameConfig.DefaultPlayerSpeed; _currentDirection = Direction.Right; }

            bool collided = _collisionSystem.IsColliding(nextX, nextY, _currentMap, doorTileId =>
            {
                _moveUp = _moveDown = _moveLeft = _moveRight = false;
                _quizManager.TriggerQuiz(doorTileId, _currentMap);
            });

            if (!collided)
            {
                _playerX = nextX;
                _playerY = nextY;
            }

            Canvas.SetLeft(_playerImage, _playerX);
            Canvas.SetTop(_playerImage, _playerY);
        }

        private void SubmitAnswer_Click(object sender, RoutedEventArgs e)
        {
            _quizManager.SubmitAnswer(_currentMap, () => _mapRenderer.Render(_currentMap, _playerImage));
            GameCanvas.Focus(); // Herstel focus na sluiten quiz
        }

        private void QuizAnswerInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                _quizManager.SubmitAnswer(_currentMap, () => _mapRenderer.Render(_currentMap, _playerImage));
                GameCanvas.Focus(); // Herstel focus na sluiten quiz
            }
        }

        private void CancelQuiz_Click(object sender, RoutedEventArgs e)
        {
            _quizManager.CloseQuiz();
            GameCanvas.Focus(); // Herstel focus na sluiten quiz
        }

        private void UpdateAnimation()
        {
            if ((DateTime.Now - _lastAnimTime).TotalMilliseconds >= GameConfig.AnimationFrameRateMs)
            {
                _animationFrame++;
                _lastAnimTime = DateTime.Now;
                UpdatePlayerSprite();
            }
        }

        private void UpdatePlayerSprite()
        {
            var frameList = _isMoving
                ? _playerSprites.WalkFrames[_currentDirection]
                : _playerSprites.IdleFrames[_currentDirection];

            if (frameList.Count == 0) return;

            string relativePath = frameList[_animationFrame % frameList.Count];
            _playerImage.Source = AssetCache.GetImage(relativePath);
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            // Geen interactie tijdens menu of quiz
            if (!_isGameStarted ||
                _quizManager.IsQuizActive ||
                _bookcaseOpen)
                return;

            // E-toets voor boekenkasten
            if (e.Key == Key.E)
            {
                InteractWithBookcase();
                e.Handled = true;
                return;
            }

            // Normale beweging
            SetMovementState(e.Key, true);
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            SetMovementState(e.Key, false);
        }

        private void SetMovementState(Key key, bool state)
        {
            if (key == Key.W || key == Key.Up) _moveUp = state;
            if (key == Key.S || key == Key.Down) _moveDown = state;
            if (key == Key.A || key == Key.Left) _moveLeft = state;
            if (key == Key.D || key == Key.Right) _moveRight = state;
        }

        private void InteractWithBookcase()

        {
            double tileSize = GameConfig.TileSize;

            // Bepaal op welke tegel de speler staat
            int playerCol = (int)((_playerX + tileSize / 2) / tileSize);
            int playerRow = (int)((_playerY + tileSize / 2) / tileSize);

            // Controleer de tegels rondom de speler
            (int row, int col)[] positions =
            {
        (playerRow, playerCol),
        (playerRow - 1, playerCol),
        (playerRow + 1, playerCol),
        (playerRow, playerCol - 1),
        (playerRow, playerCol + 1)

    };


            foreach (var position in positions)
            {
                int row = position.row;
                int col = position.col;

                // Controleer of de positie binnen de map valt
                if (row < 0 || col < 0 ||
                    row >= _currentMap.Height ||
                    col >= _currentMap.Width)
                    continue;

                int tileId = _currentMap.Layout[row, col];

                // Kijk of dit een boekenkast is
                if (_bookcases.TryGetValue(tileId, out var bookcase))
                {
                    OpenBookcase(bookcase);
                    return;
                }
            }
        }

        private void OpenBookcase(BookcaseData bookcase)
        {
            _bookcaseOpen = true;

            // Stop de speler
            _moveUp = false;
            _moveDown = false;
            _moveLeft = false;
            _moveRight = false;

            if (bookcase.IsOpgelost)
            {
                MessageBox.Show(
                    bookcase.Aanwijzing,
                    bookcase.Titel);

                _bookcaseOpen = false;
                GameCanvas.Focus();
                return;
            }

            // Maak het puzzelvenster
            Window popup = new Window
            {
                Title = bookcase.Titel,
                Width = 400,
                Height = 310,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = this,
                ResizeMode = ResizeMode.NoResize,
                Background = new SolidColorBrush(
                    Color.FromRgb(43, 43, 59))
            };

            StackPanel panel = new StackPanel
            {
                Margin = new Thickness(25)
            };

            TextBlock title = new TextBlock
            {
                Text = bookcase.Titel,
                FontSize = 22,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.Gold,
                Margin = new Thickness(0, 0, 0, 15)
            };

            TextBlock description = new TextBlock
            {
                Text = bookcase.Beschrijving,
                Foreground = Brushes.White,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 0, 0, 12)
            };

            TextBlock question = new TextBlock
            {
                Text = bookcase.Vraag,
                Foreground = Brushes.White,
                FontSize = 16,
                Margin = new Thickness(0, 0, 0, 12)
            };

            TextBox answer = new TextBox
            {
                Height = 32,
                FontSize = 16
            };

            Button submit = new Button
            {
                Content = "Controleren",
                Height = 35,
                Margin = new Thickness(0, 15, 0, 0),
                Background = Brushes.ForestGreen,
                Foreground = Brushes.White
            };

            submit.Click += (sender, e) =>
            {
                if (answer.Text.Trim().Equals(
                    bookcase.Antwoord,
                    StringComparison.OrdinalIgnoreCase))
                {
                    bookcase.IsOpgelost = true;

                    MessageBox.Show(
                        bookcase.Aanwijzing,
                        "Goed antwoord!");

                    popup.Close();
                }
                else
                {
                    MessageBox.Show(
                        "Helaas, probeer het opnieuw!",
                        "Fout antwoord");
                }
            };

            panel.Children.Add(title);
            panel.Children.Add(description);
            panel.Children.Add(question);
            panel.Children.Add(answer);
            panel.Children.Add(submit);

            popup.Content = panel;

            try
            {
                popup.ShowDialog();
            }
            finally
            {
                _bookcaseOpen = false;
                GameCanvas.Focus();
            }
        }


    }

}