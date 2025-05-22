# AI for playing classical strategy games - Quoridor
This project features the implementation of the board game **Quoridor**, developed using the **Godot Engine** sing **C#** and **GDScript**.

## Requirements
- Godot Engine (C# version)
- .NET SDK (matching Godot version)
- Visual Studio Code or another C#-capable IDE

## How to Run
1. Clone the repository:
   ```bash
   git clone https://github.com/RealSGM/Quoridor.git
   cd quoridor-ai
   ```
2. Run the application doing either of the following:
    - Run `Quoridor.exe`
    - Launch Godot Mono and run the main scene (Press `F5`)

## Requirements
- Godot Engine (C# version)
- .NET SDK (matching Godot version)

## Features 
From the main menu you can choose between four selections
- Singleplayer
    - Play against a Bot
- Local Multiplayer
    - Play against another person on the same device
- Bot vs Bot
    - View two bots play against each other
- Training
    - Train the Q-Learning Agent

## AI Overview
- **Random**: Selects 50-50 guided moves
- **Minimax**: Standard minimax agent with Alpha-Beta Prunijng
- **Monte-Carlo Tree Search**: MCTS Agent which runs 1000 random simulations to select its best move
- **Q-Learning**: Reinforcement learning-based agent with state-action value storage.

## Board Customisability
- Coloured Fences [Enabled/Disabled] - Allow placed fences to be the same colour as the pawn that placed it
- Bot Type - Used in Bot Games to determine which algorithm is used
- Player Name - Used to distinguish players visually

## Console
A console is provided to help run some functions, primarily viewing the bitboards in a more visible layout. 
- It is toggled by pressing the TILDE (`~`) key.