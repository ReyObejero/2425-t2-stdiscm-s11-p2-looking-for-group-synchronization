namespace LfgDungeonQueue {

    class DungeonManager {
        private const int _partyCompNumTankPlayers = 1;
        private const int _partyCompNumHealerPlayers = 1;
        private const int _partyCompNumDpsPlayers = 3;
        private readonly int _maxDungeons;
        private readonly int _minDungeonDuration;
        private readonly int _maxDungeonDuration;
        private List<Dungeon> _dungeons;
        private List<Thread> _dungeonSimulationWorkers = new();
        private HashSet<int> _activeDungeons = new();
        
        private Queue<Player> _playerQueue = new Queue<Player>();
        private int _totalPartiesServed = 0;
        private int _totalTimeServed = 0;
        private bool _isQueueingActive = true;
        private Mutex _mutex = new();

        public DungeonManager(
            int maxDungeons, int numTankPlayers, int numHealerPlayers,
            int numDpsPlayers, int minDungeonDuration, int maxDungeonDuration) {
            List<string> validationErrors = new List<string>();

            if (maxDungeons < 1)
                validationErrors.Add($"Maximum number of dungeons: {maxDungeons}, must be >= 1.");
            if (numTankPlayers < 1)
                validationErrors.Add($"Number of tank players: {numTankPlayers}, must be >= 1.");
            if (numHealerPlayers < 1)
                validationErrors.Add($"Number of healer players: {numHealerPlayers}, must be >= 1");
            if (numDpsPlayers < 1)
                validationErrors.Add($"Number of DPS players: {numDpsPlayers}, must be >= 1.");
            if (minDungeonDuration < 1 || minDungeonDuration > maxDungeonDuration)
                validationErrors.Add($"Minimum dungeon duration: {minDungeonDuration}, must be >= 1 and <= maximum dungeon duration.");
            if (maxDungeonDuration < 1 || maxDungeonDuration > 15)
                validationErrors.Add($"Maximum dungeon duration: {maxDungeonDuration}, must be >= 1 and <= 15.");

            if (validationErrors.Count > 0) {
                throw new ArgumentException(string.Join("\n", validationErrors));
            }

            _maxDungeons = maxDungeons;
            _minDungeonDuration = minDungeonDuration;
            _maxDungeonDuration = maxDungeonDuration;
            _dungeons = new List<Dungeon>();

            for (int i = 0; i < _maxDungeons; i++) {
                _dungeons.Add(new Dungeon(i));
            }

            for (int i = 0; i < numTankPlayers; i++) {
                _playerQueue.Enqueue(new Player(PlayerRole.Tank));
            }

            for (int i = 0; i < numHealerPlayers; i++) {
                _playerQueue.Enqueue(new Player(PlayerRole.Healer));
            }
            
            for (int i = 0; i < numDpsPlayers; i++) {
                _playerQueue.Enqueue(new Player(PlayerRole.Dps));
            }
        }

        public void StartQueue() {
            IOHandler.Log("Dungeon manager started with the following parameters:");
            IOHandler.Log($" > Maximum number of dungeons: {_maxDungeons}");
            IOHandler.Log($" > Total players: {_playerQueue.Count}");
            IOHandler.Log($" > Minimum dungeon duration: {_minDungeonDuration}");
            IOHandler.Log($" > Maximum dungeon duration: {_maxDungeonDuration}");
            IOHandler.Log("Queueing...");

            Thread queueingWorker = new Thread(ProcessQueueing);
            queueingWorker.Start();

            Thread dungeonsWatcherWorker = new Thread(() => {
                while (_isQueueingActive || _activeDungeons.Count > 0) {
                    Thread.Sleep(500);
                }

                DisplayQueueingStatistics();
            });

            dungeonsWatcherWorker.Start();
            queueingWorker.Join();
            dungeonsWatcherWorker.Join();
        }

        private void ProcessQueueing() {
            while (HasEnoughPlayers()) {
                Thread dungeonSimulationWorker = new Thread(StartDungeonRun);
                _dungeonSimulationWorkers.Add(dungeonSimulationWorker);
                dungeonSimulationWorker.Start();
            }

            _isQueueingActive = false;
        }

        private bool HasEnoughPlayers() {
            return _playerQueue.Count >= (_partyCompNumTankPlayers + _partyCompNumHealerPlayers + _partyCompNumDpsPlayers);
        }

        private void StartDungeonRun() {
            Dungeon? dungeon = null;
            int dungeonDuration = 0;

            _mutex.WaitOne();

            try {
                if (!HasEnoughPlayers()) {
                    return;
                }

                dungeon = _dungeons.FirstOrDefault(d => !_activeDungeons.Contains(d.GetId()));

                if (dungeon != null) {
                    _activeDungeons.Add(dungeon.GetId());
                    dungeonDuration = new Random().Next(_minDungeonDuration, _maxDungeonDuration + 1);

                    List<Player> selectedPlayers = new List<Player>();
                    for (int i = 0; i < _partyCompNumTankPlayers; i++) {
                        selectedPlayers.Add(_playerQueue.Dequeue());
                    }
                    for (int i = 0; i < _partyCompNumHealerPlayers; i++) {
                        selectedPlayers.Add(_playerQueue.Dequeue());
                    }
                    for (int i = 0; i < _partyCompNumDpsPlayers; i++) {
                        selectedPlayers.Add(_playerQueue.Dequeue());
                    }

                    DisplayStatus();
                } else {
                    return;
                }
            } finally {
                _mutex.ReleaseMutex();
            }

            dungeon.Start(dungeonDuration);

            _mutex.WaitOne();

            try {
                _activeDungeons.Remove(dungeon.GetId());
                _totalPartiesServed++;
                _totalTimeServed += dungeonDuration;

                DisplayStatus();
            } finally {
                _mutex.ReleaseMutex();
            }
        }

        private void DisplayStatus() {
            IOHandler.Log("================================= STATUS =========================================");
            IOHandler.Log("|   Active Dungeons    |   Empty Dungeons    |  Tanks  |  Healers  |  DPS  |");
            IOHandler.Log("----------------------------------------------------------------------------------");
            IOHandler.Log($"| {string.Join(", ", _activeDungeons).PadRight(20)} | " +
                        $"{string.Join(", ", _dungeons.Select(d => d.GetId()).Except(_activeDungeons)).PadRight(19)} | " +
                        $"{_playerQueue.Count(p => p.Role == PlayerRole.Tank),7} | " +
                        $"{_playerQueue.Count(p => p.Role == PlayerRole.Healer),9} | " +
                        $"{_playerQueue.Count(p => p.Role == PlayerRole.Dps),5} |");
            IOHandler.Log("**********************************************************************************");
        }

        private void DisplayQueueingStatistics() {
            IOHandler.Log("============================== RESULTS ===========================================");
            IOHandler.Log("|  Parties Served  | Total Time (s) | Unused Tanks | Unused Healers | Unused DPS |");
            IOHandler.Log("----------------------------------------------------------------------------------");
            IOHandler.Log($"| {_totalPartiesServed,16} | {_totalTimeServed,14} | " +
                        $"{_playerQueue.Count(p => p.Role == PlayerRole.Tank),12} | " +
                        $"{_playerQueue.Count(p => p.Role == PlayerRole.Healer),14} | " +
                        $"{_playerQueue.Count(p => p.Role == PlayerRole.Dps),10} |");
            IOHandler.Log("**********************************************************************************");

            IOHandler.Log("============================== PARTIES SERVED ====================================");
            IOHandler.Log("|  Dungeon ID  |  Frequency  | Time Served (s) |");
            IOHandler.Log("----------------------------------------------------------------------------------");
            foreach (var dungeon in _dungeons) {
                IOHandler.Log($"| {dungeon.GetId(),12} | {dungeon.GetPartiesServed(),11} | {dungeon.GetTimeServed(),15} |");
            }
            IOHandler.Log("**********************************************************************************");
        }
    }
}
