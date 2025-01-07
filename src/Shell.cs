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

            string[] splitInput = userInput.Split(" ");
            string[] arguments = GetArguments(splitInput);
            string command = splitInput[0];

            switch (command) {
                case ShellConstants.CMD_ECHO: return CmdEcho(userInput);
                case ShellConstants.CMD_EXIT: isRunning = false; return null;
                case ShellConstants.CMD_TYPE: return CmdType(arguments);
                case ShellConstants.CMD_PWD: return CmdPwd();
                case ShellConstants.CMD_CD: return CmdCd(arguments);
                case ShellConstants.CMD_CLEAR: CmdClear(); return null;
                default:
                   if (CmdTryRun(command, userInput)) {
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

        string[] GetArguments(string[] parsedInput) {

            string[] arguments = new string[parsedInput.Length - 1];

            for (int i = 1; i < parsedInput.Length; ++i) {
                arguments[i - 1] = parsedInput[i];
            }

            return arguments;
        }

        string? CmdEcho(string userInput) {

            if (userInput.Length < ShellConstants.CMD_ECHO.Length + 1) {
                return null;
            }

            string parsedInput = ParseInput(userInput, ShellConstants.CMD_ECHO.Length);

            return parsedInput;
        }

        string? CmdType(string[] arguments) {

            if (arguments.Length == 0) {
                return null;
            }

            string command = arguments[0];

            if (ShellConstants.BUILTINS.Contains(command)) {
                return $"{command} {ShellConstants.RESP_VALID_TYPE}";
            }

            string? executablePath = pathManager.GetExecutablePath(command);

            if (executablePath != null) {
                return $"{command} {ShellConstants.RESP_VALID_PATH} {executablePath}";
            }

            return $"{command}: {ShellConstants.RESP_INVALID_TYPE}";
        }

        string ParseInput(string userInput, int commandLength) {

            string parsedInput = "";

            if (userInput.Length < commandLength + 1) {
                return parsedInput;
            }

            userInput = userInput.Substring(commandLength + 1).Trim();

            for (int i = 0; i < userInput.Length; ++i) {
                if (userInput[i] == ShellConstants.SYMB_QUOTE_SINGLE) {
                    int startQuote = i;
                    int endQuote = userInput.IndexOf(ShellConstants.SYMB_QUOTE_SINGLE, startQuote + 1);

                    if (endQuote > -1) {
                        if (endQuote > startQuote + 1) {
                            parsedInput += userInput.Substring(startQuote + 1, endQuote - (startQuote + 1));
                            i = endQuote + 1;
                            continue;
                        }
                    }
                }

                if (Char.IsWhiteSpace(userInput[i])) {
                    if (Char.IsWhiteSpace(userInput[i - 1])) {
                        continue;
                    }
                }

                parsedInput += userInput[i];
            }

            return parsedInput;
        }

        bool CmdTryRun(string command, string userInput) {

            string? executablePath = pathManager.GetExecutablePath(command);

            if (executablePath == null) {
                return false;
            }

            string externalArguments = ParseInput(userInput, command.Length);
            Process.Start(executablePath, externalArguments);

            return true;
        }
        
        string CmdPwd() {

            return pathManager.GetCurrentDir();
        }

        string? CmdCd(string[] arguments) {

            if (arguments.Length == 0) {
                return null;
            }

            string userDir = arguments[0];

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
