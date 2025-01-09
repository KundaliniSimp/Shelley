using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace CodeCraftersShell
{
    class Shell
    {

        bool isRunning;
        PathManager pathManager;

        public Shell() {

            Console.Title = ShellConstants.APP_TITLE;
            isRunning = false;
            pathManager = new();
        }

        public void Run() {

            isRunning = true;

            while (isRunning) {
                REPLoop();
            }
        }

        void REPLoop() {

            string userInput = Read();
            string? response = Eval(userInput);

            if (response == null) {
                return;
            }

            Print(response);
        }

        static string Read() {

            Console.Write($"{ShellConstants.SYMB_PROMPT} ");
            string? userInput = Console.ReadLine();

            if (userInput != null) {
                return userInput.TrimStart();
            }
            return "";
        }

        string? Eval(string userInput) {

            string[] arguments = ParseInput(userInput);
            string command = arguments[0];

            switch (command) {
                case ShellConstants.CMD_ECHO: return CmdEcho(arguments);
                case ShellConstants.CMD_EXIT: isRunning = false; return null;
                case ShellConstants.CMD_TYPE: return CmdType(arguments);
                case ShellConstants.CMD_PWD: return CmdPwd();
                case ShellConstants.CMD_CD: return CmdCd(arguments);
                case ShellConstants.CMD_CLEAR: CmdClear(); return null;

                default:
                   if (CmdTryRun(arguments)) {
                        return null;
                   }
                   else { 
                        return $"{command}: {ShellConstants.RESP_INVALID_CMD}";
                   }
            }
        }

        static void Print(string response) {
            
            Console.WriteLine(response);
        }

        static string? CmdEcho(string[] arguments) {

            if (arguments.Length == 1) {
                return null;
            }

            string output = "";

            for (int i = 1; i < arguments.Length; ++i) {
                output += arguments[i];

                if (i < arguments.Length - 1) {
                    output += ShellConstants.SYMB_WHITESPACE;
                }
            }

            return output;
        }

        string? CmdType(string[] arguments) {

            if (arguments.Length < 2) {
                return null;
            }

            string command = arguments[1];

            if (ShellConstants.BUILTINS.Contains(command)) {
                return $"{command} {ShellConstants.RESP_VALID_TYPE}";
            }

            string? executablePath = pathManager.GetExecutablePath(command);

            if (executablePath != null) {
                return $"{command} {ShellConstants.RESP_VALID_PATH} {executablePath}";
            }

            return $"{command}: {ShellConstants.RESP_INVALID_TYPE}";
        }

        string[] ParseInput(string userInput) {

            List<string> arguments = new();
            string currentArg = "";
            int end = userInput.Length - 1;

            for (int i = 0; i < userInput.Length; ++i) {

                if (userInput[i] == ShellConstants.SYMB_ESCAPE && i < end) {
                    if (ShellConstants.ESCAPABLES.Contains(userInput[i + 1].ToString())) {
                        currentArg += userInput[i + 1];
                        ++i;
                        continue;
                    }
                }

                if (Char.IsWhiteSpace(userInput[i])) {
                    if (currentArg.Length > 0) {
                        arguments.Add(currentArg);
                        currentArg = "";
                    }

                    while (Char.IsWhiteSpace(userInput[i]) && i < end) {
                        ++i;
                    }

                    i -= 1;
                    continue;
                }

                if (ShellConstants.SYMB_QUOTES.Contains(userInput[i])) {
                    string literal = "";
                    bool isExtracted = false;

                    switch (userInput[i]) {

                        case ShellConstants.SYMB_QUOTE_SINGLE:
                            isExtracted = TryExtractSingleQuote(userInput, i, out literal); break;
                        case ShellConstants.SYMB_QUOTE_DOUBLE:
                            isExtracted = TryExtractDoubleQuote(userInput, i, out literal); break;
                    }

                    if (isExtracted && !String.IsNullOrEmpty(literal)) {

                        if (currentArg.Length > 0) {
                            arguments.Add(currentArg);    // clear argument buffer
                        }

                        if (i > 0 && userInput[i - 1] == userInput[i]) {
                            arguments[arguments.Count - 1] = arguments[arguments.Count - 1] + literal;  //  concat with previous argument
                        }
                        else {
                            arguments.Add(literal);
                        }
                        
                    }

                    if (isExtracted) {
                        i += literal.Length + 1;
                        continue;
                    }
                }

                currentArg += userInput[i];
            }

            if (currentArg.Length > 0) {
                arguments.Add(currentArg);    
            }

            return arguments.ToArray();
        }

        static bool TryExtractSingleQuote(string userInput, int startPosition, out string literal) {

            int quoteStart = startPosition;
            int quoteEnd = userInput.IndexOf(ShellConstants.SYMB_QUOTE_SINGLE, quoteStart + 1);
            literal = "";

            if (quoteEnd == -1) {              // no matching quote found
                return false;
            }

            if (quoteEnd - quoteStart < 2) {  // literal is empty but quote skip required
                return true;
            }

            literal = userInput.Substring(quoteStart + 1, quoteEnd - (quoteStart + 1));

            return true;
        }

        static bool TryExtractDoubleQuote(string userInput, int startPosition, out string literal) {

            int quoteStart = startPosition;
            int quoteEnd = quoteStart;
            literal = "";

            while (true) {
                quoteEnd = userInput.IndexOf(ShellConstants.SYMB_QUOTE_DOUBLE, quoteEnd + 1);

                if (quoteEnd == -1) {
                    break;
                }

                if (userInput[quoteEnd - 1] != ShellConstants.SYMB_ESCAPE) {
                    break;
                }
            }

            if (quoteEnd == -1) {
                return false;
            }

            if (quoteEnd - quoteStart < 2) {
                return true;
            }

            literal = userInput.Substring(quoteStart + 1, quoteEnd - (quoteStart + 1));

            return true;
        }

        //TODO: implement
        static string ParseDoubleQuotes(string literal) {

            string parsed = "";

            return parsed;
        }

        bool CmdTryRun(string[] arguments) {

            string command = arguments[0];
            string? executablePath = pathManager.GetExecutablePath(command);

            if (executablePath == null) {
                return false;
            }

            ProcessStartInfo processConfig = new(command);

            if (arguments.Length > 1) {
                for (int i = 1; i < arguments.Length; ++i) {
                    processConfig.ArgumentList.Add(arguments[i]);
                }
            }

            Process? currentProcess = Process.Start(processConfig);

            if (currentProcess == null) {
                return true;
            }

            while (!currentProcess.HasExited) {
                Thread.Sleep(ShellConstants.SLEEP_INTERVAL);
            }

            return true;
        }
        
        string CmdPwd() {

            return pathManager.GetCurrentDir();
        }

        string? CmdCd(string[] arguments) {

            if (arguments.Length < 2) {
                return null;
            }

            string userDir = arguments[1];

            if (pathManager.TrySetDir(userDir)) {
                return null;
            }

            return $"{ShellConstants.CMD_CD}: {userDir}: {ShellConstants.RESP_INVALID_DIR}";
        }

        static void CmdClear() {

            Console.Clear();
        }
    }
}