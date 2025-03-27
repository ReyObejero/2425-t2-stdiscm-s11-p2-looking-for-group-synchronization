using LfgDungeonQueue;

int maxDungeonInstances = ConsoleIOHandler.PromptInput<int>("Maximum number of dungeons");
int numTankPlayers = ConsoleIOHandler.PromptInput<int>("Number of tank players");
int numHealerPlayers = ConsoleIOHandler.PromptInput<int>("Number of healer players");
int numDpsPlayers = ConsoleIOHandler.PromptInput<int>("Number of DPS players");
int minDungeonInstanceDuration = ConsoleIOHandler.PromptInput<int>("Minimum dungeon duration");
int maxDungeonInstanceDuration = ConsoleIOHandler.PromptInput<int>("Maximum dungeon duration");