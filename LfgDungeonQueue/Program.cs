using LfgDungeonQueue;

int maxDungeons = IOHandler.PromptInput<int>("Maximum number of dungeons");
int numTankPlayers = IOHandler.PromptInput<int>("Number of tank players");
int numHealerPlayers = IOHandler.PromptInput<int>("Number of healer players");
int numDpsPlayers = IOHandler.PromptInput<int>("Number of DPS players");
int minDungeonDuration = IOHandler.PromptInput<int>("Minimum dungeon duration");
int maxDungeonDuration = IOHandler.PromptInput<int>("Maximum dungeon duration");

var dungeonManager = new DungeonManager(maxDungeons, numTankPlayers, numHealerPlayers,
                                        numDpsPlayers, minDungeonDuration, maxDungeonDuration);
dungeonManager.StartQueue();