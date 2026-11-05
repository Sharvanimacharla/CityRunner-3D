CITY RUNNER 3D  -  Unity 6 endless runner (city theme)

HOW TO RUN
1. Unzip this folder.
2. Unity Hub > Add > Add project from disk > select the CityRunner3D folder, open with Unity 6.
3. Wait for the import, then press the Play button. (Works from any scene.)

CONTROLS
A / D or Left / Right : change lane
W / Up / Space        : jump (over low red-white barriers and taxis)
S / Down              : slide (under orange gantries)
Swipe on touch screens.

GAMEPLAY
- Hit something once = STUMBLE (the guard starts chasing you, slower speed).
  Hit something again within 8 seconds = BUSTED (game over).
- Oncoming yellow taxis drive toward you. Green vans block the lane.
- Power-ups: MAGNET (pulls coins), JETPACK (fly over everything),
  SHIELD (absorbs one hit), x2 SCORE.
- The city skyline backdrop changes: day -> sunset -> night as your score grows (windows light up at night).

SCRIPTS (Assets/Scripts)
GameManager, PlayerMovement, SegmentGenerator, CameraFollow, SaveLoad,
InputHelper, ShapeFactory, Obstacle, Coin, PowerUp, Chaser, SoundFx, GameBootstrap

TROUBLESHOOTING
If keys do nothing: Edit > Project Settings > Player > Other Settings >
Active Input Handling = Both.
