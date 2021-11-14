# Simple Weapon Base (S&Box)
![GitHub release (latest SemVer)](https://img.shields.io/github/v/release/timmybo5/simple-weapon-base)
![GitHub repo size](https://img.shields.io/github/repo-size/timmybo5/simple-weapon-base)

A community managed simple to use weapon base for S&amp;Box

## Demonstration
[![Watch the video](https://img.youtube.com/vi/dXaeXi6mX04/maxresdefault.jpg)](https://www.youtube.com/watch?v=dXaeXi6mX04)

## Goal
During the lifetime of garrysmod many weapon bases consisting of poor quality were created, and it would be a shame to see the same in S&box. The goal is to offer an easy to use yet very configurable weapon base maintained by the community for the community.

## Features

### Various
* No programming skills needed to use
* Highly configurable
* Support for custom animations and actions

### Built-in Animations
* Walking
* Crouching
* Running
* Jumping
* Idle
* Sway
* Zooming

### Built-in HUD
* Modular Health Display
* Modular Ammo Display
* Dynamic Crosshair
* Dynamic Hitmarkers

### Bases
* Magazine (default)
* Shotgun (shell based reloading)
* Sniper (scoped zooming)
* Entity (grenades, arrows, props, ...)
* Melee

### Weapon Models
* Properly licensed weapon models are included in the base
* They can be used for any purpose as long as the license is respected (credits.txt)

## Contributing
Anyone can contribute by creating a pull request to their branch. Contributors can help out with the planned updates, outstanding bug/issues, or maybe you just have a cool idea that would improve the base, any help is welcome!

## Planned Updates
* More base-included entities (e.g. bomb)
* Physical bullets
* Attachment base

## Deatchmatch Elements
For now some deathmatch dependencies are included as the base uses the inventory and hud elements from the deathmatch gamemode.

## Installing
If you want to test out the base just drag and drop simple-weapon-base-master into your addons folder.

## Usage
Using the base into your own gamemode can be done by:
1. Implementing the PlayerBase class into your player class `partial class MyPlayer : PlayerBase` 
   and calling the PlayerBase constructor on your player constructor `public MyPlayer() : base() {}` 
3. If you want the HUD you'll need to initialize it in your game constructor 
```
public Game()
{
  if ( IsServer )
    {
      new DeathmatchHud();
    }
  }
}
```
