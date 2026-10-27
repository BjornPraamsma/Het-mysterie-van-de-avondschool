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


        // DE SCHIM
        private readonly Canvas _schim = new();

        private double _schimX = 400;
        private double _schimY = 400;

        // Willekeurige spawnplekken
        private readonly Random _random = new Random();

        
        // PATROUILLEREN
        private double _schimLinks = 300;
        private double _schimRechts = 600;

        private bool _schimGaatNaarRechts = true;
        private bool _schimAchtervolgt = false;

        // Muziek voor de Schim
        private readonly MediaPlayer _schimMuziek = new MediaPlayer();
        private readonly MediaPlayer _jumpscareGeluid = new MediaPlayer();

        // Tijd bijhouden voor achtervolging
        private DateTime _startAchtervolging;

        // Tot wanneer de Schim moet uitrusten
        private DateTime _schimRustTot = DateTime.MinValue;

        // Schim verschijnt pas na 10 seconden
        private bool _schimActief = false;
        private DateTime _schimStartTijd;
        private bool _wordtGevangen = false;

        private DateTime _laatsteSchimUpdate = DateTime.Now;


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
            InitializeSchimMuziek();
            _mapRenderer.Render(_currentMap, _playerImage);
            InitializeSchim();
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
            _laatsteSchimUpdate = DateTime.Now;

            KiesSchimSpawnplek();
            _schimStartTijd = DateTime.Now;
            _schimActief = false;
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

        private void InitializeSchim()
        {
            _schim.Width = 32;
            _schim.Height = 48;

            // Zwart lichaam
            Border lichaam = new Border
            {
                Width = 32,
                Height = 48,
                Background = Brushes.Black,
                CornerRadius = new CornerRadius(12)
            };

            _schim.Children.Add(lichaam);

            // Twee witte ogen
            for (int i = 0; i < 2; i++)
            {
                Border oog = new Border
                {
                    Width = 7,
                    Height = 7,
                    Background = Brushes.White,
                    CornerRadius = new CornerRadius(4)
                };

                Canvas.SetLeft(oog, 7 + i * 12);
                Canvas.SetTop(oog, 12);

                _schim.Children.Add(oog);
            }

            Canvas.SetLeft(_schim, _schimX);
            Canvas.SetTop(_schim, _schimY);

            Canvas.SetZIndex(_schim, 9);

            _schim.Visibility = Visibility.Hidden;
            GameCanvas.Children.Add(_schim);
        }

        private void KiesSchimSpawnplek()
        {
            double tegelGrootte = GameConfig.TileSize;

            // Kies een willekeurige positie in de grote gang
            double nieuweX;

            do
            {
                // Willekeurige X-positie in de gang
                nieuweX = (2 + _random.NextDouble() * 21)
                           * tegelGrootte;
            }
            while (Math.Abs(nieuweX - _playerX) < 300);

            // Zet de Schim op rij 6 van de gang
            _schimX = nieuweX;
            _schimY = 6 * tegelGrootte;

            // Patrouille tussen de uiteinden van de gang
            _schimLinks = 2 * tegelGrootte;
            _schimRechts = 23 * tegelGrootte;

            // Willekeurige looprichting
            _schimGaatNaarRechts = _random.Next(2) == 0;

            _schimAchtervolgt = false;

            // Verplaats de Schim op het scherm
            Canvas.SetLeft(_schim, _schimX);
            Canvas.SetTop(_schim, _schimY);
        }
        private void UpdateSchim()
        {
            DateTime nu = DateTime.Now;
            double deltaTime = (nu - _laatsteSchimUpdate).TotalSeconds;
            _laatsteSchimUpdate = nu;

            deltaTime = Math.Min(deltaTime, 0.05);

            // Afstand tussen de speler en de Schim
            double verschilX = _playerX - _schimX;
            double verschilY = _playerY - _schimY;

            double afstand = Math.Sqrt(
                verschilX * verschilX + verschilY * verschilY
            );

            // Start achtervolging als de speler dichtbij is
            // en de Schim voldoende heeft gerust
            if (!_schimAchtervolgt &&
                afstand < 150 &&
                nu >= _schimRustTot)
            {
                _schimAchtervolgt = true;
                _startAchtervolging = nu;

                StartSchimMuziek();
            }

            // Stop na 5 seconden of als de speler ontsnapt
            if (_schimAchtervolgt &&
                ((nu - _startAchtervolging).TotalSeconds >= 5 ||
                 afstand > 250))
            {
                _schimAchtervolgt = false;

                StopSchimMuziek();

                // Schim mag 5 seconden niet opnieuw achtervolgen
                _schimRustTot = nu.AddSeconds(5);
            }

            if (_schimAchtervolgt)
            {
                // ACHTERVOLGEN
                double snelheid = 135;

                if (afstand > 0)
                {
                    double stap = Math.Min(snelheid * deltaTime, afstand);

                    _schimX += verschilX / afstand * stap;
                    _schimY += verschilY / afstand * stap;
                }
            }
            else
            {
                // NORMAAL PATROUILLEREN
                double snelheid = 75;

                if (_schimGaatNaarRechts)
                {
                    _schimX += snelheid * deltaTime;

                    if (_schimX >= _schimRechts)
                    {
                        _schimX = _schimRechts;
                        _schimGaatNaarRechts = false;
                    }
                }
                else
                {
                    _schimX -= snelheid * deltaTime;

                    if (_schimX <= _schimLinks)
                    {
                        _schimX = _schimLinks;
                        _schimGaatNaarRechts = true;
                    }
                }
            }

            // Nieuwe positie tekenen
            Canvas.SetLeft(_schim, _schimX);
            Canvas.SetTop(_schim, _schimY);
        }

        private void StartSchimMuziek()
        {
            _schimMuziek.Position = TimeSpan.Zero;
            _schimMuziek.Play();
        }

        private void StopSchimMuziek()
        {
            _schimMuziek.Stop();
        }


        private void InitializeSchimMuziek()
        {
            string pad = System.IO.Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Audio",
                "Chase.mp3"
            );

            if (System.IO.File.Exists(pad))
            {
                _schimMuziek.Open(new Uri(pad));
                _schimMuziek.Volume = 0.5;
            }

            // Herhaal de muziek zolang de Schim achtervolgt
            _schimMuziek.MediaEnded += (sender, e) =>
            {
                if (_schimAchtervolgt)
                {
                    _schimMuziek.Position = TimeSpan.Zero;
                    _schimMuziek.Play();
                }
            };
        }
        private void ControleerGevangen()
        {
            // Voorkom meerdere jumpscares tegelijk
            if (_wordtGevangen)
                return;

            // Bereken afstand tussen speler en Schim
            double verschilX = _playerX - _schimX;
            double verschilY = _playerY - _schimY;

            double afstand = Math.Sqrt(
                verschilX * verschilX + verschilY * verschilY
            );

            // Speler wordt gevangen als Schim dichtbij is
            if (afstand < 25)
            {
                _wordtGevangen = true;
                StopSchimMuziek();

                // Stop de speler
                _moveUp = false;
                _moveDown = false;
                _moveLeft = false;
                _moveRight = false;

               
                // Jumpscare
                ToonJumpscare();

                // Laat de speler kiezen
                bool terugNaarMenu = ToonGameOverMenu();

                // Speler terug naar beginpositie
                _playerX = 7 * GameConfig.TileSize;
                _playerY = 6 * GameConfig.TileSize;

                Canvas.SetLeft(_playerImage, _playerX);
                Canvas.SetTop(_playerImage, _playerY);

                // Schim terug naar beginpositiewd
                KiesSchimSpawnplek();

                Canvas.SetLeft(_schim, _schimX);
                Canvas.SetTop(_schim, _schimY);

                // Schim gaat weer patrouilleren
                _schimAchtervolgt = false;
                _schimGaatNaarRechts = true;

                // Geef speler na Game Over 10 seconden rust
                _schimActief = false;
                _schim.Visibility = Visibility.Hidden;
                _schimStartTijd = DateTime.Now;

                // Tijdmeting opnieuw beginnen
                _laatsteSchimUpdate = DateTime.Now;

                _wordtGevangen = false;

                if (terugNaarMenu)
                {
                    // Stop het spel
                    _isGameStarted = false;

                    // Laat het startmenu opnieuw zien
                    StartMenuOverlay.Visibility = Visibility.Visible;

                    // Verberg het speelveld
                    GameScrollViewer.Visibility = Visibility.Collapsed;
                }
                else
                {
                    // Opnieuw proberen
                    GameCanvas.Focus();
                }
            }
        }

        private void SpeelJumpscareGeluid()
        {
            string pad = System.IO.Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "audio",
                "Jumpscare.mp3"
            );

            if (System.IO.File.Exists(pad))
            {
                _jumpscareGeluid.Open(new Uri(pad));
                _jumpscareGeluid.Volume = 0.7;
                _jumpscareGeluid.Position = TimeSpan.Zero;
                _jumpscareGeluid.Play();
            }
        }
        private void ToonJumpscare()
        {
            // Maak een zwart jumpscarevenster
            Window jumpscare = new Window
            {
                Title = "De Schim",
                WindowState = WindowState.Maximized,
                WindowStyle = WindowStyle.None,
                Background = Brushes.Black,
                Topmost = true,
                ShowInTaskbar = false,
                Owner = this
            };

            // Inhoud van het venster
            StackPanel inhoud = new StackPanel
            {
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            // Grote rode ogen
            TextBlock ogen = new TextBlock
            {
                Text = "●     ●",
                FontSize = 110,
                Foreground = Brushes.Red,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            // Enge tekst
            TextBlock tekst = new TextBlock
            {
                Text = "DE SCHIM HEEFT JE!",
                FontSize = 45,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.Red,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 40, 0, 0)
            };

            inhoud.Children.Add(ogen);
            inhoud.Children.Add(tekst);

            jumpscare.Content = inhoud;

            // Sluit automatisch na 1,5 seconde
            System.Windows.Threading.DispatcherTimer timer =
                new System.Windows.Threading.DispatcherTimer();

            timer.Interval = TimeSpan.FromSeconds(1.5);

            timer.Tick += (sender, e) =>
            {
                timer.Stop();
                jumpscare.Close();
            };

            timer.Start();

            // Speel het jumpscaregeluid af
            SpeelJumpscareGeluid();

            // Toon de jumpscare
            jumpscare.ShowDialog();
        }

        private bool ToonGameOverMenu()
        {
            bool terugNaarStart = false;

            // Maak Game Over-venster
            Window menu = new Window
            {
                Title = "Game Over",
                Width = 600,
                Height = 400,
                WindowStyle = WindowStyle.None,
                ResizeMode = ResizeMode.NoResize,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Background = Brushes.Black,
                Owner = this
            };

            // Alles onder elkaar plaatsen
            StackPanel inhoud = new StackPanel
            {
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(50)
            };

            // Titel
            TextBlock titel = new TextBlock
            {
                Text = "GAME OVER",
                FontSize = 42,
                FontWeight = FontWeights.Bold,
                FontFamily = new FontFamily("Consolas"),
                Foreground = Brushes.Orange,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 40)
            };

            // Knop 1: opnieuw proberen
            Button opnieuw = new Button
            {
                Content = "Probeer opnieuw",
                Height = 55,
                FontSize = 20,
                FontWeight = FontWeights.Bold,
                FontFamily = new FontFamily("Consolas"),
                Foreground = Brushes.White,
                Background = Brushes.DarkGreen,
                BorderBrush = Brushes.Lime,
                BorderThickness = new Thickness(3),
                Margin = new Thickness(0, 0, 0, 20)
            };

            opnieuw.Click += (sender, e) =>
            {
                terugNaarStart = false;
                menu.Close();
            };

            // Knop 2: terug naar startmenu
            Button startmenu = new Button
            {
                Content = "Terug naar startscherm",
                Height = 55,
                FontSize = 20,
                FontWeight = FontWeights.Bold,
                FontFamily = new FontFamily("Consolas"),
                Foreground = Brushes.White,
                Background = Brushes.DarkGreen,
                BorderBrush = Brushes.Lime,
                BorderThickness = new Thickness(3)
            };

            startmenu.Click += (sender, e) =>
            {
                terugNaarStart = true;
                menu.Close();
            };

            inhoud.Children.Add(titel);
            inhoud.Children.Add(opnieuw);
            inhoud.Children.Add(startmenu);

            menu.Content = inhoud;
            menu.ShowDialog();

            return terugNaarStart;
        }
        private void OnRenderFrame(object? sender, EventArgs e)
        {
            // Stop rendering als het spel nog niet is gestart of als er een quiz bezig is
            if (!_isGameStarted || _quizManager.IsQuizActive) return;

            UpdateMovement();
            UpdateAnimation();

            // Controleer of de 10 seconden voorbij zijn
            if (!_schimActief &&
                (DateTime.Now - _schimStartTijd).TotalSeconds >= 10)
            {
                _schimActief = true;
                _schim.Visibility = Visibility.Visible;

                _laatsteSchimUpdate = DateTime.Now;
            }

            // Schim beweegt alleen als hij actief is
            if (_schimActief)
            {
                UpdateSchim();
                ControleerGevangen();
            }

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
            _quizManager.SubmitAnswer(_currentMap,
                () =>
                {
                    _mapRenderer.Render(_currentMap, _playerImage);
                    GameCanvas.Children.Add(_schim);
                });

            GameCanvas.Focus(); // Herstel focus na sluiten quiz
        }

        private void QuizAnswerInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                _quizManager.SubmitAnswer(_currentMap,
                    () =>
                    {
                        _mapRenderer.Render(_currentMap, _playerImage);
                        GameCanvas.Children.Add(_schim);
                    });

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