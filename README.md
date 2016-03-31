# BasketballBot

## Pull the repo
1. Create a folder called Basketball and cd into that folder.
2. Run the following git commands:
  1. `git init`
  2. `git remote add origin https://github.com/megaspazz/BasketballBot`
  3. `gite pull origin master`

## Install and Configure Bluestacks 2
1. Download and install [Bluestacks 2](http://www.bluestacks.com).
2. Change Bluestacks 2 on-screen resolution to 1280×720:
  1. In the registry, navigate to key: HKEY_LOCAL_MACHINE/SOFTWARE/BlueStacks/Guests/Android/FrameBuffer/0
  2. Change WindowWidth value to 1280 (decimal base).
  3. Change WindowHeight value to 720 (decimal base).
3. Install Messenger on Bluestacks 2.

## Running the bot (subject to change!)
1. Open Visual Studio as Administrator.
2. Build the solution.  It might download some Nuget packages.
3. Make sure there is only one Bluestacks window already open when you run the program.
  * NOTE: Make sure the Bluestacks sidebar window that sometimes pops up is closed!
3. In the console that comes up, you can type a series of commands, notably:
  * `test` runs the bot.  Make sure that the basket will not change direction during the shot, i.e. make sure that it's not too close to the edges.  Note that it takes around 200 ms to take a shot.
  * `quit` exits the program.

## Acknowledgements
* [swishx](https://github.com/swishx) a.k.a. "Hu Boy"
  * Assisting in brainstorm ideas.
  * Determining the MouseClickDrag speed to be 1 (no longer used).
  * Lots of help testing the project.
  * Finding a plethora bugs that were exterminated mercilessly.
  * Top-tier proofreading and editing the documentation.
  * Moral support.
