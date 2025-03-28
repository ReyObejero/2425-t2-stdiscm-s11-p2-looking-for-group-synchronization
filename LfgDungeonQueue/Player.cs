namespace LfgDungeonQueue {

    public enum PlayerRole {
        Tank,
        Healer,
        Dps,
    }

    public class Player {
        public PlayerRole Role { get; private set; }

        public Player(PlayerRole role) {
            Role = role;
        }
    }
}