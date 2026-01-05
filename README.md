# 🐦 Flappy Bird - WPF Game

A modern recreation of the classic Flappy Bird game built with C# and WPF, featuring smooth 60 FPS gameplay, dynamic pipe generation, and professional UI/UX design.

![Game Screenshot](/images/01.png)
![Game Screenshot](/images/02.png)

## ✨ Features

- **Smooth 60 FPS gameplay** with optimized rendering
- **Random pipe heights** - Each playthrough is unique
- **Physics-based movement** with realistic gravity and jump mechanics
- **Professional UI** with animated Game Over screen
- **Pause/Resume functionality** (ESC key)
- **Score tracking** with persistent high scores
- **Responsive controls** with smooth bird rotation

## 🎮 How to Play

- **SPACE** - Make the bird jump
- **R** - Restart game after Game Over
- **ESC** - Pause/Resume game

Avoid hitting pipes and the ground to achieve the highest score!

## 🛠️ Technologies Used

- **C# .NET Framework 4.7.2+**
- **WPF (Windows Presentation Foundation)**
- **XAML** for UI design
- **DispatcherTimer** for game loop (60 FPS)
- **LINQ** for object management

## 📦 Installation

### Option 1: Run from Release
1. Download the latest release from [Releases](https://github.com/YOUR_USERNAME/flappy-bird-wpf/releases)
2. Extract the ZIP file
3. Run `Flappy Bird Game.exe`

### Option 2: Build from Source
1. Clone the repository:
```bash
   git clone https://github.com/YOUR_USERNAME/flappy-bird-wpf.git
```
2. Open `Flappy Bird Game.sln` in Visual Studio
3. Build and run (F5)

## 📂 Project Structure

```
Flappy-Bird-Game/
├── MainWindow.xaml       # UI Layout
├── MainWindow.xaml.cs    # Game Logic
├── images/               # Game Assets
│   ├── clouds.png
│   ├── flappyBird.png
│   ├── pipeTop.png
│   └── pipeBottom.png
└── README.mdb
```

## 🎯 Key Implementation Features

### Performance Optimization
- Hardware acceleration enabled
- Object caching to avoid repeated lookups
- Efficient collision detection
- 60 FPS target frame rate

### Game Mechanics
- Gravity-based physics with velocity clamping
- Dynamic bird rotation based on vertical velocity
- Random pipe generation for replayability
- Precise hitbox collision detection

### UI/UX Design
- Gradient backgrounds
- Drop shadow effects for depth
- Smooth animations (BackEase, DoubleAnimation)
- Professional Game Over screen with score display

## 📸 Screenshots

*Add screenshots here*


## 📝 License

This project is open source and available under the [MIT License](LICENSE).

## 🙏 Acknowledgments

- Original Flappy Bird game by Dong Nguyen
- WPF tutorials and documentation from Microsoft