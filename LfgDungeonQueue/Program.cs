using LfgDungeonQueue;

int maxDungeons, numTankPlayers, numHealerPlayers,
    numDpsPlayers, minDungeonDuration, maxDungeonDuration;
DungeonManager? dungeonManager = null;

while (true) {
    try {
        maxDungeons = IOHandler.PromptInput<int>("Maximum number of dungeons");
        numTankPlayers = IOHandler.PromptInput<int>("Number of tank players");
        numHealerPlayers = IOHandler.PromptInput<int>("Number of healer players");
        numDpsPlayers = IOHandler.PromptInput<int>("Number of DPS players");
        minDungeonDuration = IOHandler.PromptInput<int>("Minimum dungeon duration");
        maxDungeonDuration = IOHandler.PromptInput<int>("Maximum dungeon duration");

        dungeonManager = new DungeonManager(maxDungeons, numTankPlayers, numHealerPlayers,
                                            numDpsPlayers, minDungeonDuration, maxDungeonDuration);
        dungeonManager.StartQueue();

        break;
    } catch (ArgumentException ex) {
        IOHandler.Log("The following parameter(s) are invalid:");

        foreach (string error in ex.Message.Split("\n")) {
            IOHandler.Log(" > " + error);
        }

        IOHandler.Log("Enter valid parameters.");
    }
}