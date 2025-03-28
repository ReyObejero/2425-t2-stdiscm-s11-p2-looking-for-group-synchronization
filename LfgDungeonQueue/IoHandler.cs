namespace LfgDungeonQueue {

    public class IOHandler {
        private static readonly string logsDirectory = "Logs";
        private static readonly string LogFilePath;

        static IOHandler() {
            if (!Directory.Exists(logsDirectory)) {
                Directory.CreateDirectory(logsDirectory);
            }

            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            LogFilePath = Path.Combine(logsDirectory, $"{timestamp}.txt");
        }

        public static T PromptInput<T>(string message) {
            while (true) {
                Console.Write($"{message}: ");
                string? input = Console.ReadLine()?.Trim();

                if (string.IsNullOrWhiteSpace(input)) {
                    Log("Input cannot be empty.");
                    continue;
                }

                try {
                    return (T)Convert.ChangeType(input, typeof(T));
                } catch {
                    Log("Input is invalid.");
                }
            }
        }

        public static void Log(string message) {
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string logMessage = $"[{timestamp}] {message}";

            Console.WriteLine(logMessage);

            try {
                File.AppendAllText(LogFilePath, logMessage + Environment.NewLine);
            } catch {
                Console.WriteLine($"[{timestamp}] Failed to log.");
            }
        }
    }
}
