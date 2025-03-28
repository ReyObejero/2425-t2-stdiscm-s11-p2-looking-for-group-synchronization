namespace LfgDungeonQueue {

    public class Dungeon {
        private readonly int _id;

        public Dungeon(int id) {
            _id = id;
        }

        public int GetId() {
            return _id;
        }

        public void Start(int duration) {
            for (int i = 1; i <= duration; i++) {
                Thread.Sleep(1000);
            }
        }
    }
}