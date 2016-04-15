# BasketballBot

## Pull the repo
1. Create a folder called Basketball and cd into that folder.
2. Run the following git commands:
  1. `git init`
  2. `git remote add origin https://github.com/megaspazz/BasketballBot`
  3. `git pull origin master`

## Install and Configure Bluestacks 2
1. Download and install [Bluestacks 2](http://www.bluestacks.com).
  * NOTE: As of 04/14/2016, if you want to stream with [Open Broadcaster Software (OBS)](https://obsproject.com/), the newest version of the Bluestacks App Player (version 2.2.19.6015) will not work since it has issues with click-and-drag when OBS is running, which is necessary to shoot the basketball.  You can [download version 2.1.8.5663 here](http://www.mediafire.com/download/q6ct1dzuzttpqaf/BlueStacks2_native.exe).
2. Change Bluestacks 2 internal and on-screen resolutions to 1280×720:
  1. In the registry, navigate to key: HKEY_LOCAL_MACHINE/SOFTWARE/BlueStacks/Guests/Android/FrameBuffer/0
  2. Change GuestWidth value to 1280 (decimal base).
  3. Change GuestHeight value to 720 (decimal base).
  4. Change WindowWidth value to 1280 (decimal base).
  5. Change WindowHeight value to 720 (decimal base).
3. Install Messenger on Bluestacks 2.

## Running the bot (subject to change!)
1. Open Visual Studio.
2. Build the solution.  It might download some Nuget packages.
3. Make sure there is only one Bluestacks window with Messenger already open when you run the program.
  * NOTE: Make sure the Bluestacks sidebar window that sometimes pops up is closed!
3. In the console that comes up, you can type commands, notably:
  * Game commands:
    * `handle` attempts to acquire a new handle for the game window.  It should get one automatically when you run the program, but if you restarted Bluestacks, you can use this command.  A non-zero handle means that you probably acquired the correct handle.  Like before, make sure that there's only one Bluestacks window with Messenger already open when running this command.
    * `window` sets the handle to the window handle that your cursor is currently on when you enter this command.  You can use it if you can't automatically acquire the correct handle by mousing over the game in Bluestacks and entering this command.
    * `quit` exits the program.
  * Level commands:
    * `setlevel ###` sets the internal level counter to the number you specify.  Levels are important because they determine the parameters of the game that allow the bot to predict the position of the basket, such as the velocity of the basket and the boundaries of movement.
    * `reset` sets the level counter to zero.
    * `derank` decrements the level counter by one.
    * `uprank` increases the level counter by one.
  * Shooting commands:
    * `auto` takes a well-aimed shot that only fires when the basket is in a good position.  This is almost guaranteed to make the shot.
      * Make sure that the level is correctly set before using this command.
      * Make sure that the console and other windows are not obstructing the game area in Bluestacks.
    * `freelo ###` automatically takes the number of shots you specified in the command.  Leaving it blank will let it run forever.
      * Make sure that the level is correctly set before using this command.
      * Make sure that the console and other windows are not obstructing the game area in Bluestacks.
      * You can terminate this command at any time by pressing a key in the console while this command is running.  Make sure the console has the focus; the console should have focus most of the time, except when it's taking the shot.

## Acknowledgements
* [swishx](https://github.com/swishx) a.k.a. "Hu Boy"
  * Assisting in brainstorm ideas.
  * Determining the MouseClickDrag speed to be 1 (no longer used).
  * Lots of help testing the project.
  * Finding a plethora bugs that were exterminated mercilessly.
  * Top-tier proofreading and editing the documentation.
  * Moral support.
