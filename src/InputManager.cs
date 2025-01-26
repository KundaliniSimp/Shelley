using System.Text;

namespace CodeCraftersShell
{
    class InputManager
    {
        
        enum CursorDirection {
            LEFT = -1,
            RIGHT = 1,
        }

        HashSet<string> autocompletable = new();

        public InputManager(string[]? executables) {
            
            foreach (string builtin in ShellConstants.BUILTINS) {
                autocompletable.Add(builtin);
            }

            if (executables != null) {
                foreach (string exe in executables) {
                    autocompletable.Add(exe);
                }
            }
        }

        public string? GetUserInput() {

            StringBuilder inputBuffer = new();
            bool isReading = true;
            string[] autocompletionCache = Array.Empty<string>();

            while (isReading) {

                string writeBuffer = "";

                ConsoleKeyInfo currentKey = Console.ReadKey(true);

                switch (currentKey.Key) {
                    case ConsoleKey.Enter: isReading = false; break;
                    case ConsoleKey.LeftArrow: MoveCursor(CursorDirection.LEFT, inputBuffer.Length); break;
                    case ConsoleKey.RightArrow: MoveCursor(CursorDirection.RIGHT, inputBuffer.Length); break;
                    case ConsoleKey.Tab: 

                        if (autocompletionCache.Length > 0) {
                            PrintAutocompletionCache(autocompletionCache);
                            RedrawInput(inputBuffer);

                            autocompletionCache = Array.Empty<string>();
                            break;
                        }

                        string[] matches = GetAutocompleteMatches(inputBuffer);
                        
                        if (matches.Length == 0) {
                            ShellUtilities.PlayAlertBell();
                        }
                        else if (matches.Length == 1) {
                            writeBuffer += AutocompleteInput(matches[0], inputBuffer.Length);
                        }
                        else if (matches.Length > 1) {
                            string commonPrefix = GetLongestCommonPrefix(matches);

                            if (commonPrefix.Length > 0) {
                                writeBuffer += AutocompleteInput(commonPrefix, inputBuffer.Length, false);
                            }
                            else {
                                autocompletionCache = matches;
                                ShellUtilities.PlayAlertBell();
                            }
                        }
                        
                        break;

                    default:
                        if (IsLegalInputChar(currentKey.KeyChar)) {
                            writeBuffer += currentKey.KeyChar; 
                        }
                        break;
                }

                foreach (char c in writeBuffer) {
                    inputBuffer.Append(c);
                    Console.Write(c);
                }
            }

            return inputBuffer.ToString();
        }

        string[] GetAutocompleteMatches(StringBuilder inputBuffer) {

            List<string> matches = new();
            string input = inputBuffer.ToString();

            foreach (string completion in autocompletable) {
                if (completion.StartsWith(input)) {
                    matches.Add(completion);
                }
            }

            matches.Sort();

            return matches.ToArray();
        }

        string AutocompleteInput(string completionMatch, int prefixLength, bool endCompletion = true) {

            string completion = "";

            for (int i = prefixLength; i < completionMatch.Length; ++i) {
                completion += completionMatch[i];
            }

            if (endCompletion) {
                completion += ShellConstants.SYMB_WHITESPACE;
            }
                 
            return completion;
        }

        void PrintAutocompletionCache(string[] autocompletionCache) {

            string cache = String.Join(ShellConstants.AUTOCOMPLETION_SEPARATOR, autocompletionCache);

            Console.Write(ShellConstants.SYMB_NEWLINE);
            Console.WriteLine(cache);
        }

        void RedrawInput(StringBuilder inputBuffer) {

            Console.Write(ShellConstants.NEW_PROMPT + inputBuffer.ToString());
        }

        void MoveCursor(CursorDirection direction, int inputLength) {

            (int Left, int Top) cursorPosition = Console.GetCursorPosition();
            int newPosition = cursorPosition.Left + (int)direction;

            if (newPosition > inputLength + ShellConstants.INPUT_BUFFER_START) {
                return;
            }

            if (newPosition < ShellConstants.INPUT_BUFFER_START) {
                return;
            }

            Console.SetCursorPosition(newPosition, cursorPosition.Top);
        }

        bool IsLegalInputChar(char character) {
            return Char.IsWhiteSpace(character) || Char.IsLetterOrDigit(character) || Char.IsSymbol(character) || Char.IsPunctuation(character);
        }

        static string GetLongestCommonPrefix(string[] matches) {

            string shortest = "";
            string commonPrefix = "";
            int shortestLen = Int32.MaxValue;

            foreach (string match in matches) {
                if (match.Length < shortestLen) {
                    shortest = match;
                    shortestLen = match.Length;
                }
            }

            for (int i = 0; i < shortest.Length; ++i) {
                for (int j = 0; j < matches.Length; ++j) {
                    if (shortest[i] != matches[j][i]) {
                        return commonPrefix;
                    }
                }
                commonPrefix += shortest[i];
            }

            return commonPrefix;
        }
    }
}