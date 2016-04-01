# BasketballBot

## Pull the repo
1. Create a folder called Basketball and cd into that folder.
2. Run the following git commands:
  1. `git init`
  2. `git remote add origin https://github.com/megaspazz/BasketballBot`
  3. `git pull origin master`

## Install and Configure Bluestacks 2
1. Download and install [Bluestacks 2](http://www.bluestacks.com).
2. Change Bluestacks 2 on-screen resolution to 1280×720:
  1. In the registry, navigate to key: HKEY_LOCAL_MACHINE/SOFTWARE/BlueStacks/Guests/Android/FrameBuffer/0
  2. Change WindowWidth value to 1280 (decimal base).
  3. Change WindowHeight value to 720 (decimal base).
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
    * `test` runs a very primitive version of the bot.  It might miss, especially for longer shots.
    * `auto` takes a well-aimed shot that only fires when the basket is in a good position.  This is almost guaranteed to make the shot.
    * `freelo ###` automatically takes the number of shots you specified in the command.  Leaving it blank will let it run forever.  You can terminate this command at any time by pressing a key in the console while this command is running.  Make sure the console has the focus; the console should have focus most of the time, except when it's taking the shot.

## Acknowledgements
* [swishx](https://github.com/swishx) a.k.a. "Hu Boy"
  * Assisting in brainstorm ideas.
  * Determining the MouseClickDrag speed to be 1 (no longer used).
  * Lots of help testing the project.
  * Finding a plethora bugs that were exterminated mercilessly.
  * Top-tier proofreading and editing the documentation.
  * Moral support.
