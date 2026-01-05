using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace Flappy_Bird_Game
{
    public partial class MainWindow : Window
    {
        // Game constants
        private const int GRAVITY_DOWN = 6;
        private const int JUMP_FORCE = -9;
        private const int PIPE_SPEED = 4;
        private const double CLOUD_SPEED = 0.4;
        private const int TIMER_INTERVAL = 16; // ~60 FPS
        private const double SCORE_INCREMENT = 1;

        // Pipe settings
        private const int PIPE_GAP = 150;

        // Game timer
        private readonly DispatcherTimer gameTimer;

        // Game state
        private double verticalVelocity;
        private double score;
        private bool isGameOver;
        private bool isJumping;
        private HashSet<string> passedPipes;

        // Collision detection
        private Rect birdHitbox;

        // Game objects cache
        private List<Image> allPipes;
        private List<Image> clouds;

        // Random for pipe heights
        private Random random;

        public MainWindow()
        {
            InitializeComponent();

            // Enable hardware acceleration
            RenderOptions.SetBitmapScalingMode(this, BitmapScalingMode.LowQuality);

            // Initialize timer with optimal settings
            gameTimer = new DispatcherTimer(DispatcherPriority.Render)
            {
                Interval = TimeSpan.FromMilliseconds(TIMER_INTERVAL)
            };
            gameTimer.Tick += GameEngine;

            // Initialize collections
            passedPipes = new HashSet<string>();
            random = new Random();
            CacheGameObjects();

            StartGame();
        }

        private void CacheGameObjects()
        {
            // Cache all pipes
            allPipes = MyCanvas.Children.OfType<Image>()
                .Where(x => x.Tag != null && x.Tag.ToString().StartsWith("obs"))
                .ToList();

            // Cache clouds
            clouds = MyCanvas.Children.OfType<Image>()
                .Where(x => x.Tag != null && x.Tag.ToString() == "clouds")
                .ToList();
        }

        private void Canvas_KeyisDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space && !isGameOver && !isJumping)
            {
                Jump();
                e.Handled = true;
            }
            else if (e.Key == Key.R && isGameOver)
            {
                StartGame();
                e.Handled = true;
            }
            else if (e.Key == Key.Escape)
            {
                if (gameTimer.IsEnabled)
                    PauseGame();
                else if (!isGameOver)
                    ResumeGame();
            }
        }

        private void Canvas_KeyisUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                isJumping = false;
                AnimateBirdRotation(15);
            }
        }

        private void Jump()
        {
            isJumping = true;
            verticalVelocity = JUMP_FORCE;
            AnimateBirdRotation(-20);
        }

        private void AnimateBirdRotation(double angle)
        {
            var rotateTransform = new RotateTransform(angle, flappyBird.Width / 2, flappyBird.Height / 2);
            flappyBird.RenderTransform = rotateTransform;
        }

        private void StartGame()
        {
            // Reset game state
            score = 0;
            isGameOver = false;
            isJumping = false;
            verticalVelocity = 0;
            passedPipes.Clear();

            // Hide game over panel
            GameOverPanel.Visibility = Visibility.Collapsed;

            // Reset bird position
            Canvas.SetTop(flappyBird, 250);
            Canvas.SetLeft(flappyBird, 80);
            AnimateBirdRotation(0);

            // Reset pipes with random heights - SIMPLIFIED
            RandomizePipeHeights("obs1", 500);
            RandomizePipeHeights("obs2", 800);
            RandomizePipeHeights("obs3", 1100);

            // Reset clouds
            int cloudOffset = 100;
            foreach (var cloud in clouds)
            {
                Canvas.SetLeft(cloud, cloudOffset);
                cloudOffset += 400;
            }

            // Update UI
            UpdateScoreDisplay();

            // Start game loop
            gameTimer.Start();
        }

        private void RandomizePipeHeights(string tag, double leftPosition)
        {
            // Get pipes with this tag
            var pipes = allPipes.Where(p => p.Tag.ToString() == tag).ToList();

            if (pipes.Count != 2)
            {
                // Fallback: just set left position
                foreach (var pipe in pipes)
                {
                    Canvas.SetLeft(pipe, leftPosition);
                }
                return;
            }

            // Random gap center position (between 150 and 400)
            double gapCenter = random.Next(200, 400);

            // Find which pipe is top and which is bottom based on their name/source
            Image topPipe = null;
            Image bottomPipe = null;

            foreach (var pipe in pipes)
            {
                string source = pipe.Source?.ToString() ?? "";
                if (source.Contains("pipeTop"))
                {
                    topPipe = pipe;
                }
                else if (source.Contains("pipeBottom"))
                {
                    bottomPipe = pipe;
                }
            }

            // If we found both pipes
            if (topPipe != null && bottomPipe != null)
            {
                // Set horizontal positions
                Canvas.SetLeft(topPipe, leftPosition);
                Canvas.SetLeft(bottomPipe, leftPosition);

                // Calculate vertical positions
                double gapTop = gapCenter - (PIPE_GAP / 2);
                double gapBottom = gapCenter + (PIPE_GAP / 2);

                // Top pipe: its bottom edge should be at gapTop
                Canvas.SetTop(topPipe, gapTop - topPipe.Height);

                // Bottom pipe: its top edge should be at gapBottom
                Canvas.SetTop(bottomPipe, gapBottom);
            }
        }

        private void PauseGame()
        {
            gameTimer.Stop();
            scoreText.Content = "PAUSED - Press ESC to Resume";
        }

        private void ResumeGame()
        {
            UpdateScoreDisplay();
            gameTimer.Start();
        }

        private void GameEngine(object sender, EventArgs e)
        {
            // Update bird physics
            UpdateBirdPhysics();

            // Update bird rotation based on velocity
            UpdateBirdRotation();

            // Check boundaries
            if (CheckBoundaryCollision())
            {
                EndGame();
                return;
            }

            // Update game objects
            UpdatePipes();
            UpdateClouds();

            // Update UI
            UpdateScoreDisplay();
        }

        private void UpdateBirdPhysics()
        {
            // Apply gravity
            verticalVelocity += 0.5;

            // Clamp velocity
            verticalVelocity = Math.Min(verticalVelocity, 12);

            // Update position
            double newTop = Canvas.GetTop(flappyBird) + verticalVelocity;
            Canvas.SetTop(flappyBird, newTop);

            // Update hitbox
            birdHitbox = new Rect(
                Canvas.GetLeft(flappyBird) + 3,
                Canvas.GetTop(flappyBird) + 2,
                flappyBird.Width - 8,
                flappyBird.Height - 4
            );
        }

        private void UpdateBirdRotation()
        {
            // Dynamic rotation based on velocity
            double targetRotation = verticalVelocity * 3;
            targetRotation = Math.Max(-30, Math.Min(90, targetRotation));

            if (!isJumping)
            {
                AnimateBirdRotation(targetRotation);
            }
        }

        private bool CheckBoundaryCollision()
        {
            double birdTop = Canvas.GetTop(flappyBird);
            double birdBottom = birdTop + flappyBird.Height;

            return birdBottom > MyCanvas.ActualHeight - 10 || birdTop < 0;
        }

        private void UpdatePipes()
        {
            // Group pipes by tag
            var pipeGroups = allPipes.GroupBy(p => p.Tag.ToString());

            foreach (var group in pipeGroups)
            {
                string tag = group.Key;
                var pipes = group.ToList();

                if (pipes.Count == 0) continue;

                // Get current position from first pipe
                var firstPipe = pipes[0];
                double currentLeft = Canvas.GetLeft(firstPipe);
                double newLeft = currentLeft - PIPE_SPEED;

                bool hasCollision = false;

                // Move all pipes in this group and check collision
                foreach (var pipe in pipes)
                {
                    Canvas.SetLeft(pipe, newLeft);

                    // Check collision with each pipe
                    Rect pipeRect = new Rect(
                        newLeft,
                        Canvas.GetTop(pipe),
                        pipe.Width,
                        pipe.Height
                    );

                    if (birdHitbox.IntersectsWith(pipeRect))
                    {
                        hasCollision = true;
                    }
                }

                if (hasCollision)
                {
                    EndGame();
                    return;
                }

                // Check if bird passed the pipe (for scoring)
                double birdLeft = Canvas.GetLeft(flappyBird);
                double pipeRight = newLeft + firstPipe.Width;

                if (pipeRight < birdLeft && !passedPipes.Contains(tag))
                {
                    passedPipes.Add(tag);
                    score += SCORE_INCREMENT;
                }

                // Reset pipe when off screen
                if (newLeft < -firstPipe.Width - 100)
                {
                    RandomizePipeHeights(tag, MyCanvas.ActualWidth + 100);
                    passedPipes.Remove(tag);
                }
            }
        }

        private void UpdateClouds()
        {
            foreach (var cloud in clouds)
            {
                double currentLeft = Canvas.GetLeft(cloud);
                Canvas.SetLeft(cloud, currentLeft - CLOUD_SPEED);

                if (currentLeft < -cloud.Width)
                {
                    Canvas.SetLeft(cloud, MyCanvas.ActualWidth + 100);
                }
            }
        }

        private void UpdateScoreDisplay()
        {
            scoreText.Content = $"Score: {(int)score}";
        }

        private void EndGame()
        {
            gameTimer.Stop();
            isGameOver = true;

            // Update game over panel
            FinalScoreText.Text = $"{(int)score}";
            GameOverPanel.Visibility = Visibility.Visible;

            // Animate game over panel
            AnimateGameOver();
        }

        private void AnimateGameOver()
        {
            // Scale animation for game over panel
            var scaleTransform = new ScaleTransform(0, 0);
            GameOverPanel.RenderTransform = scaleTransform;
            GameOverPanel.RenderTransformOrigin = new Point(0.5, 0.5);

            var scaleXAnim = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromMilliseconds(300),
                EasingFunction = new BackEase { EasingMode = EasingMode.EaseOut }
            };

            var scaleYAnim = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromMilliseconds(300),
                EasingFunction = new BackEase { EasingMode = EasingMode.EaseOut }
            };

            scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, scaleXAnim);
            scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, scaleYAnim);

            // Shake screen
            ShakeScreen();
        }

        private void ShakeScreen()
        {
            var transform = new TranslateTransform();
            MyCanvas.RenderTransform = transform;

            var anim = new DoubleAnimation
            {
                From = 0,
                To = 10,
                Duration = TimeSpan.FromMilliseconds(50),
                AutoReverse = true,
                RepeatBehavior = new RepeatBehavior(3)
            };

            anim.Completed += (s, e) => MyCanvas.RenderTransform = null;
            transform.BeginAnimation(TranslateTransform.XProperty, anim);
        }
    }
}