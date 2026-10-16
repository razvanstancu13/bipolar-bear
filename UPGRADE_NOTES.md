# Bipolar Bear – upgraded to Unity 6 (6000.5.10f1), 2026-10-16

Source: `C:\Users\razva\projects\BipolarBear` (Unity 4.3.4, 2014; product name "BiPolar Bear", company "Team Team").
That folder was not touched; this folder is a copy without its old `Library`, the `*.bak` scripts and the old
Visual Studio / MonoDevelop files (`*.sln`, `*.csproj`, `*.userprefs`). `README.md` is the one of the
CharacterController2D library (prime31) the game is built on, kept as it was.

## What was changed
- **Scripts** (Unity 4 APIs that Unity 6 removed):
  - `rigidbody2D.` → the `rigidBody2D` field / `GetComponent<Rigidbody2D>()`; `.velocity` → `.linearVelocity`
    (CharacterController2D, EnemyLogic).
  - `BoxCollider2D.center` → `.offset` (CharacterController2D).
  - `Application.LoadLevel` → `SceneManager.LoadScene` (GameLogic, PlayerController, MenuLogic).
  - `GetComponent<GUITexture>()` / `GetComponent<GUIText>()` → `LegacyGUITexture` / `LegacyGUIText`
    (PlayerController: the mood icons, the ability texts, the pill counter and the mood timer).
- **GUITexture / GUIText** no longer exist. Their settings were read out of the original binary scenes and put back on
  the same objects as `LegacyGUITexture` / `LegacyGUIText` (same as in Cat on a Wire), which draw the same way:
  object position = point on screen (0..1), pixel values from there, y from the bottom, 0.5 grey = unchanged colour,
  higher z on top. This game was laid out for **1920 x 1080**; those pixels are scaled to the window.
  MenuLogic uses `LegacyGUITexture.HitTest` like before.
- **Sprite frames**: the Unity 4.3 metas of the 8 sliced sprite sheets (bear idle / walk / jump / punch / roar, eskimo,
  stunned eskimo, pill) had no table of the old frame IDs, so Unity 6 gave the frames new random IDs and every
  animation lost its frames (bear, enemies and pills were invisible). The exact old ID of every frame
  (21300000, 21300002, ...) was read from the original project's `Library\metadata` and written back into the metas
  (`internalID`, `internalIDToNameTable`, `nameFileIdTable`); all 8 sheets matched frame by frame by name.
  A stale `Library` still kept the frames out of the build, so `Library` was deleted once and rebuilt.
- `Prefabs/Player.prefab`: a script that was already gone in 2014 (most likely `PhysicsPlayerTester`, which exists only
  as a `.bak`) was removed; Unity 6 refuses to save a prefab with a missing script, so it had stayed binary.
- **FrameRate.cs**: held at 60 fps (clouds and the growl move a fixed step per frame).
- All assets are saved as text (Force Text).
- **Textures compressed**: 23 to BC7 (high quality, 4x smaller). 25 stay uncompressed because BC7 needs sizes divisible
  by 4 (for example the avatars 305 x 305, the props 273 x 172, the platforms 376 x 194); they are small.
- **Unused assets removed** (nothing in the two scenes used them, directly or through a prefab, animation, animator
  controller or material): Animations/Pill/Pill.controller, Animations/Player/Change.anim + "sprite change.png",
  GUITextures/avatar1.png and icon.png, the prefabs cloud1, cloud2, crystal1, crystal2, crystal3, Enemy, Pill, tepi,
  terrain33, volcano (the scene's objects are not linked to them), the sounds "Give'em the bird.wav", music1.wav,
  music3.mp3, pill2.mp3, punch2.mp3, punch3.mp3, walk1.mp3, walk2.mp3, and Sprites/dcrL5kzc9.png, ground1.png.
  Kept although no scene object carries them: CC2DTriggerHelper, GrowlBehavior (used from code).
- Removed the default package "multiplayer center".
- `Assets/Editor/GameBuild.cs`: menu **Bipolar Bear > Build Windows** → `Builds/Windows/BipolarBear.exe`
  (sets the scene order: MainMenu = 0, GameScene = 1).
- `Assets/Scripts/GameTestRun.cs`: only active with `-bear-test <folder> [walk]`; does nothing in normal play.
  Screenshots, logs the bear's position, mood, pills, current sprite, level reloads and errors, then quits.
  walk = the bear holds "right" the whole time.

## Test results (1600 x 900 window), 0 errors
- normal: menu (title, Play button, the four moods), then the level: story screen, igloo, the bear idling
  (frames idle_0..2), the HUD with mood icon, ability text, timer and pill counter; the mood changes by itself.
- walk: the bear walks (walk frames), a pill floats ahead; it dies on the first obstacle after about 6.5 s and the
  level restarts; walks again afterwards.
- Build: 113 MB.

## Controls (from PlayerController)
A / D walk, W jump, Space = the current mood's ability (punch / roar / super jump), Enter = take a pill (a second
ability for a while), Left Ctrl = that second ability, Esc back to the menu (Esc on the menu quits).
