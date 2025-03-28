namespace LfgDungeonQueue {

    public class Dungeon {
        private readonly int _id;
        private int _partiesServed;
        private int _timeServed;

        public Dungeon(int id) {
            _id = id;
        }

        public int GetId() {
            return _id;
        }

        public void Start(int duration) {
            _partiesServed++;
            _timeServed += duration;

            for (int i = 1; i <= duration; i++) {
                Thread.Sleep(1000);
            }
        }

        public int GetPartiesServed() {
            return _partiesServed;
        }

        public int GetTimeServed() {
            return _timeServed;
        }
    }
}