namespace LfgDungeonQueue {

    class DungeonManager {
        private const int _partyCompNumTankPlayers = 1;
        private const int _partyCompNumHealerPlayers = 1;
        private const int _partyCompNumDpsPlayers = 3;
        private readonly int _maxDungeons;
        private int _numTankPlayers;
        private int _numHealerPlayers;
        private int _numDpsPlayers;
        private readonly int _minDungeonDuration;
        private readonly int _maxDungeonDuration;
        private List<Dungeon> _dungeons;
        private List<Thread> _dungeonSimulationWorkers = new();        
        private HashSet<int> _activeDungeons = new();
        private int _partiesServed = 0;
        private int _totalTimeServed = 0;
        private Mutex _mutex = new();
        private bool _isQueueingActive = true;

        public DungeonManager(
            int maxDungeons, int numTankPlayers, int numHealerPlayers,
            int numDpsPlayers, int minDungeonDuration, int maxDungeonDuration) {
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
        }

        public void StartQueue() {
            IOHandler.Log("Queueing...");

            Thread queueingWorker = new Thread(ProcessQueueing);
            queueingWorker.Start();

            Thread dungeonsWatcherWorker = new Thread(() => {
                while (_isQueueingActive || _activeDungeons.Count > 0) {
                    Thread.Sleep(500);
                }

                DisplaySummary();
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
            return _numTankPlayers >= _partyCompNumTankPlayers &&
                   _numHealerPlayers >= _partyCompNumHealerPlayers &&
                   _numDpsPlayers >= _partyCompNumDpsPlayers;
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
                    _numTankPlayers -= _partyCompNumTankPlayers;
                    _numHealerPlayers -= _partyCompNumHealerPlayers;
                    _numDpsPlayers -= _partyCompNumDpsPlayers;

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
                _partiesServed++;
                _totalTimeServed += dungeonDuration;

                DisplayStatus();
            } finally {
                _mutex.ReleaseMutex();
            }
        }

        private void DisplayStatus() {
            IOHandler.Log("================================= STATUS =================================");
            IOHandler.Log("| Active Dungeons       | Empty Dungeons        | Tanks | Healers | DPS |");
            IOHandler.Log("--------------------------------------------------------------------------");
            IOHandler.Log($"| {string.Join(", ", _activeDungeons).PadRight(21)} | " +
                          $"{string.Join(", ", _dungeons.Select(d => d.GetId()).Except(_activeDungeons)).PadRight(21)} | " +
                          $"{_numTankPlayers,5} | {_numHealerPlayers,7} | {_numDpsPlayers,3} |");
            IOHandler.Log("**************************************************************************");
        }

        private void DisplaySummary() {
            IOHandler.Log("================================ RESULTS ===============================================");
            IOHandler.Log("| Parties Served | Total Time (s) | Unqueued Tanks | Unqueued Healers | Unqueued DPS |");
            IOHandler.Log("----------------------------------------------------------------------------------------");
            IOHandler.Log($"| {_partiesServed,14} | {_totalTimeServed,14} | {_numTankPlayers,14} | " +
                          $"{_numHealerPlayers,16} | {_numDpsPlayers,12} |");
            IOHandler.Log("****************************************************************************************");
        }
    }
}
