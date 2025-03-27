namespace LfgDungeonQueue {

    class DungeonManager {
        private const int _partyCompMinTankPlayers = 1;
        private const int _partyCompMinHealerPlayers = 1;
        private const int _partyCompMinDpsPlayers = 3;
        private readonly int _maxDungeons;
        private int _numTankPlayers;
        private int _numHealerPlayers;
        private int _numDpsPlayers;
        private readonly int _minDungeonDuration;
        private readonly int _maxDungeonDuration;
        private List<Dungeon> _dungeons;
        private readonly SemaphoreSlim _dungeonSemaphore;
        private readonly object _lockObj = new();
        private int _partiesServed = 0;
        private int _totalTimeServed = 0;

        public DungeonManager(
            int maxDungeons, int numTankPlayers, int numHealerPlayers,
            int numDpsPlayers, int minDungeonDuration, int maxDungeonDuration) {
            List<string> validationErrors = new List<string>();

            if (maxDungeons < 1)
                validationErrors.Add("Maximum number of dungeons must be >= 1.");
            if (numTankPlayers < 1)
                validationErrors.Add("Number of tank players must be >= 1.");
            if (numHealerPlayers < 1)
                validationErrors.Add("Number of healer players must be >= 1");
            if (numDpsPlayers < 1)
                validationErrors.Add("Number of DPS players must be >= 1.");
            if (minDungeonDuration < 1)
                validationErrors.Add("Minimum dungeon duration must be >= 1.");
            if (maxDungeonDuration < 1 || maxDungeonDuration > 15)
                validationErrors.Add("Maximum dungeon duration must be >= 1 and <= 15.");
            if (minDungeonDuration > maxDungeonDuration)
                validationErrors.Add("Minimum dungeon duration must be <= maximum dungeon duration.");

            if (validationErrors.Count > 0) {
                Console.WriteLine("The following input(s) are invalid:\n" + " > " + string.Join("\n > ", validationErrors));
            }

            _maxDungeons = maxDungeons;
            _numTankPlayers = numTankPlayers;
            _numHealerPlayers = numHealerPlayers;
            _numDpsPlayers = numDpsPlayers;
            _minDungeonDuration = minDungeonDuration;
            _maxDungeonDuration = maxDungeonDuration;
            _dungeons = new List<Dungeon>();

            for (int i = 0; i < _maxDungeons; i++) {
                _dungeons.Add(new Dungeon(i));
            }

            _dungeonSemaphore = new SemaphoreSlim(_maxDungeons);
        }   

        public void StartQueue() {
            List<Task> dungeonRuns = new List<Task>();

            while (HasEnoughPlayers()) {
                Task dungeonRun = RunDungeonInstance();
                dungeonRuns.Add(dungeonRun);
            }


            Task.WhenAll(dungeonRuns).Wait();

            Console.WriteLine("\n-------------------------------------------------------------");
            Console.WriteLine(" Parties Served  | Total Time (s)   | Unqueued Tanks | Unqueued Healers | Unqueued DPS ");
            Console.WriteLine("-------------------------------------------------------------");
            Console.WriteLine($" {_partiesServed,14} | {_totalTimeServed,15} | {_numTankPlayers,14} | {_numHealerPlayers,16} | {_numDpsPlayers,10} ");
            Console.WriteLine("-------------------------------------------------------------");
        }

        private bool HasEnoughPlayers() {
            return
                _numTankPlayers >= _partyCompMinTankPlayers &&
                _numHealerPlayers >= _partyCompMinHealerPlayers &&
                _numDpsPlayers >= _partyCompMinDpsPlayers;
        }

        private async Task RunDungeonInstance() {
            await _dungeonSemaphore.WaitAsync();
            Dungeon? dungeonInstance = null;
            int dungeonDuration = 0;

            lock (_lockObj) {
                if (!HasEnoughPlayers()) {
                    _dungeonSemaphore.Release();
                    return;
                }

                dungeonInstance = _dungeons.FirstOrDefault(instance => !instance.IsActive());

                if (dungeonInstance != null) {
                    dungeonInstance.Toggle();

                    _numTankPlayers -= _partyCompMinTankPlayers;
                    _numHealerPlayers -= _partyCompMinHealerPlayers;
                    _numDpsPlayers -= _partyCompMinDpsPlayers;
                    
                    dungeonDuration = new Random().Next(_minDungeonDuration, _maxDungeonDuration + 1);

                    Console.WriteLine("\n-------------------------------------------------------------");
                    Console.WriteLine(" Active Instances | Inactive Instances | Tanks | Healers | DPS ");
                    Console.WriteLine("-------------------------------------------------------------");
                    Console.WriteLine($" {string.Join(", ", _dungeons.Where(i => i.IsActive()).Select(i => i.GetId())).PadRight(17)}" +
                                    $" | {string.Join(", ", _dungeons.Where(i => !i.IsActive()).Select(i => i.GetId())).PadRight(18)}" +
                                    $" | {_numTankPlayers,5} | {_numHealerPlayers,7} | {_numDpsPlayers,3} ");
                    Console.WriteLine("-------------------------------------------------------------");

                    Console.WriteLine($" > Dungeon instance {dungeonInstance.GetId()} will serve a party run for {dungeonDuration} seconds.");
                } else {
                    _dungeonSemaphore.Release();
                    return;
                }
            }

            await Task.Delay(dungeonDuration * 1000);

            lock (_lockObj) {
                dungeonInstance.Toggle();
                _partiesServed++;
                _totalTimeServed += dungeonDuration;

                Console.WriteLine("\n-------------------------------------------------------------");
                Console.WriteLine(" Active Instances | Inactive Instances | Tanks | Healers | DPS ");
                Console.WriteLine("-------------------------------------------------------------");
                Console.WriteLine($" {string.Join(", ", _dungeons.Where(i => i.IsActive()).Select(i => i.GetId())).PadRight(17)}" +
                                $" | {string.Join(", ", _dungeons.Where(i => !i.IsActive()).Select(i => i.GetId())).PadRight(18)}" +
                                $" | {_numTankPlayers,5} | {_numHealerPlayers,7} | {_numDpsPlayers,3} ");
                Console.WriteLine("-------------------------------------------------------------");
                Console.WriteLine($" > Dungeon instance {dungeonInstance.GetId()} party run finished.");
            }

            _dungeonSemaphore.Release();
        }
    }
}