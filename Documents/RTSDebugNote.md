How to Run RTS Game

In current version, the single player mode is built on a multiplayer scene without other connected player, so there is not a "singleplayer scene".
Load the multiplayer scene, check the "Host in Editor" checkbox in GameManager (which will host game in editor), then the you can start the game.
By default the host player will get player index 0, so if you want to connect other player in, simply build the game and run it after the game start in editor.
You will see a small UI at top left to connect to localhost with specific player index.

Edit the "Init Data Asset" in GameManager allow you to modify the start config of game, there are some to choose and they are loacted in Assets\Resources\RTSLibrary\InitData