# 🏰 Tower Battle Real-Time Strategy Game (OCR A Level Computer Science NEA)

A real-time 2D strategy game built in C# using MonoGame, where the player defends their tower against an AI opponent while simultaneously managing resources, spawning units, and pushing attacks across the battlefield.

The game is designed around real-time decision making, requiring the player to balance offence, defence, and economy under constant pressure.

---

## 🎮 Core Concept

The game is a head-to-head tower battle between a player and an AI system.

Both sides:
- Spawn units onto the battlefield
- Automatically send units towards the enemy tower
- Engage in combat along the way
- Attempt to destroy the opposing tower

The match ends when one tower reaches zero health.

---

## ⚔️ Gameplay Systems

### Unit Spawning & Combat
- Player spawns units using on-screen controls
- AI spawns units automatically at timed intervals and randomised positions
- Units automatically move towards the enemy tower after spawning
- Units attack enemy units they encounter along the way

The battlefield is constantly active, requiring the player to manage multiple threats at once.

---

### Unit Interaction
- Player can click on enemy units to deal damage
- Stronger units require multiple clicks to destroy
- Units vary in health, speed, and damage

This creates a layered combat system where prioritisation matters during fights.

---

### Unit Types
The game includes multiple unit classes such as:
- Basic soldiers (fast, low health)
- Machine gunners (moderate durability, higher damage)
- Tanks (slow, high health, high damage)

Stronger units are intentionally harder to remove, forcing the player to adjust strategy during gameplay.

---

### Economy System
- Players earn money by defeating enemy units
- Different unit types reward different amounts of money
- Money is used to spawn units and purchase upgrades
- Stronger units cost more and are harder to spam

This introduces a resource management layer on top of real-time combat.

---

### Cooldowns & Balance
- Powerful units have cooldown timers
- Cooldowns are displayed to the player
- Prevents repetitive or overpowered unit spam

This ensures the game stays balanced and encourages varied strategies.

---

### Towers & Win Condition
- Each side has a tower with fixed position
- Towers have health values that decrease when attacked
- The game ends when a tower reaches zero health
- A clear win/loss screen is shown at the end of the match

---

## 🧠 AI Design (Work in Progress)

The AI system is being developed to behave strategically rather than randomly.

Planned approaches include:
- Finite State Machine (FSM) for structured behaviour (attack / defend / recover states)
- Utility-based decision making for unit spawning choices
- Adaptive response based on battlefield conditions

These systems are being iterated as development continues, and may be adjusted depending on performance and complexity.

---

## 🖥️ UI & Feedback Systems

- Health bars for all units and towers
- Real-time health updates during combat
- Clear display of money and cooldown timers
- Menu screen for starting the game
- Visual feedback when units are hit or destroyed

These elements ensure the player always understands the current game state.

---

## 🎨 Visual & Audio Design

- Animated sprites for movement and combat
- Hit and explosion effects for feedback
- Units visibly disappear when destroyed
- Parallax-style background for depth
- Background music and sound effects for actions

---

## 🧱 Technical Implementation

This project demonstrates several core programming concepts:

- Object-oriented design for unit types and behaviours
- List-based data structures for active entity management
- Collision detection for combat interactions
- Real-time game loop updates using MonoGame
- State management (menu, gameplay, game over)
- Event-driven input handling

---

## 🛠️ Technologies Used

- C#
- MonoGame Framework
- Visual Studio 2022

---

## 📌 Project Purpose

This project is my OCR A Level Computer Science NEA.

It was created to demonstrate my ability to design and implement a full software system involving real-time mechanics, AI behaviour, resource management, and structured gameplay systems.

The project also explores more advanced AI concepts such as FSM and utility-based decision making, which are being integrated progressively during development.

---

## 🚧 Future Improvements

If development continues beyond the NEA, planned improvements include:

- Smarter adaptive AI behaviour
- Additional unit classes and abilities
- More maps and environments
- Improved animations and visual effects
- Better balancing system for units
- Save/load progression system

---

## 👨‍💻 Development Status

This project is actively in development as part of my A Level coursework. Systems are being built incrementally, tested individually, and refined as gameplay complexity increases.
