namespace LfgDungeonQueue {

    public class IOHandler {
        public static T PromptInput<T>(string message) {
            while (true) {
                Console.Write($"{message}: ");
                string? input = Console.ReadLine()?.Trim();
                
                if (string.IsNullOrWhiteSpace(input)) {
                    Console.WriteLine("Input cannot be empty.");
                    continue;
                }
                
                try {
                    return (T)Convert.ChangeType(input, typeof(T));
                } catch {
                    Console.WriteLine("Input is invalid.");
                }
            }
        }
    }
}