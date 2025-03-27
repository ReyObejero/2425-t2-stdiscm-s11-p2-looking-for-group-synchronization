namespace LfgDungeonQueue {

    public class Dungeon {
        private readonly int _id;
        private bool _isActive;

        public Dungeon(int id) {
            _id = id;
            _isActive = false;
        }

        public int GetId() {
            return _id;
        }

        public bool IsActive() {
            return _isActive;
        }

        public void Toggle() {
            _isActive = !_isActive;
        }
    }
}