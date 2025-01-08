using System;
using System.Diagnostics;

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

        string Read() {

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

        void Print(string response) {
            
            Console.WriteLine(response);
        }

        string? CmdEcho(string[] arguments) {

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
            userInput = userInput.Trim();

            for (int i = 0; i < userInput.Length; ++i) {

                if (Char.IsWhiteSpace(userInput[i])) {
                    if (currentArg.Length > 0) {
                        arguments.Add(currentArg);
                        currentArg = "";
                    }

                    while (Char.IsWhiteSpace(userInput[i])) {
                        ++i;
                    }

                    i -= 1;
                    continue;
                }

                if (userInput[i] != ShellConstants.SYMB_QUOTE_SINGLE) {
                    currentArg += userInput[i];
                    continue;
                }

                int openQuote = i;
                int closeQuote = userInput.IndexOf(ShellConstants.SYMB_QUOTE_SINGLE, openQuote + 1);

                if (closeQuote == -1) {
                    currentArg += userInput[i];
                    continue;
                }

                bool concatPrevious = false;

                if (i > 0 && userInput[i - 1] == ShellConstants.SYMB_QUOTE_SINGLE) {
                    concatPrevious = true;
                }

                if (currentArg.Length > 0) {
                    arguments.Add(currentArg);
                    currentArg = "";
                }

                if (closeQuote - openQuote > 1) {
                    string literal = userInput.Substring(openQuote + 1, closeQuote - (openQuote + 1));

                    if (concatPrevious) {
                        arguments[arguments.Count - 1] = arguments[arguments.Count - 1] + literal;
                    }
                    else {
                        arguments.Add(literal);
                    }
                }

                i = closeQuote;
            }

            if (currentArg.Length > 0) {
                arguments.Add(currentArg);
            }

            return arguments.ToArray();
            
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

            Process.Start(processConfig);

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

        void CmdClear() {

            Console.Clear();
        }
    }
}
