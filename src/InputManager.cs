using System.Text;

namespace CodeCraftersShell
{
    class InputManager
    {

        public InputManager() {}

        public string? GetUserInput() {

            StringBuilder inputBuffer = new();
            bool isReading = true;

            while (isReading) {

                string writeBuffer = "";

                ConsoleKeyInfo currentKey = Console.ReadKey(true);

                switch (currentKey.Key) {
                    case ConsoleKey.Enter: isReading = false; break;
                    case ConsoleKey.Tab: 
                        List<string> matches = GetAutocompleteMatches(inputBuffer); 
                        
                        if (matches.Count == 0) {
                            ShellUtilities.PlayAlertBell();
                        }
                        else if (matches.Count == 1) {
                            writeBuffer += CompleteInput(matches[0], inputBuffer.Length);
                        }
                        else { }
                        
                        break;

                    default: writeBuffer += currentKey.KeyChar; break;
                }

                foreach (char c in writeBuffer) {
                    inputBuffer.Append(c);
                    Console.Write(c);
                }
            }

            return inputBuffer.ToString();
        }

        List<string> GetAutocompleteMatches(StringBuilder inputBuffer) {

            List<string> matches = new();
            string input = inputBuffer.ToString();

            foreach (string builtin in ShellConstants.BUILTINS) {
                if (builtin.StartsWith(input)) {
                    matches.Add(builtin);
                }
            }

            return matches;
        }

        string CompleteInput(string completionMatch, int prefixLength) {

            string completion = "";

            for (int i = prefixLength; i < completionMatch.Length; ++i) {
                completion += completionMatch[i];
            }

            return completion + ShellConstants.SYMB_WHITESPACE;
        }
    }
}