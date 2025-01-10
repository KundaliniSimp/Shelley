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

                if (userInput[i] == ShellConstants.SYMB_ESCAPE) {
                    if (i == end || !ShellConstants.ESCAPABLES.Contains(userInput[i + 1])) {  // a non-escaping backslash
                        goto pushChar;
                    }

                    ++i;                     
                    goto pushChar;                                               // backslash precedes an escapable char
                }

                if (Char.IsWhiteSpace(userInput[i])) {
                    if (currentArg.Length > 0) {                                // argument break
                        arguments.Add(currentArg);
                        currentArg = "";
                    }
                    continue;                                                 // skip whitespace
                }

                if (ShellConstants.SYMB_QUOTES.Contains(userInput[i])) {
                    string literal = "";
                    int jumpPosition = 0;

                    switch (userInput[i]) {
                        case ShellConstants.SYMB_QUOTE_SINGLE:
                            jumpPosition = TryExtractSingleQuote(userInput, i, out literal); break;
                        case ShellConstants.SYMB_QUOTE_DOUBLE:
                            jumpPosition = TryExtractDoubleQuote(userInput, i, out literal); break;
                    }
                    
                    currentArg += literal;

                    if (jumpPosition >= 0) {
                        i = jumpPosition;
                        continue;
                    }
                }

            pushChar:
                currentArg += userInput[i];
            }

            if (currentArg.Length > 0) {         // clear argument buffer
                arguments.Add(currentArg);    
            }

            return arguments.ToArray();
        }

        static int TryExtractSingleQuote(string userInput, int startPosition, out string literal) {

            int quoteStart = startPosition;
            int quoteEnd = userInput.IndexOf(ShellConstants.SYMB_QUOTE_SINGLE, quoteStart + 1);
            literal = "";

            if (quoteEnd == -1) {              // no matching quote found
                return -1;
            }

            if (quoteEnd - quoteStart < 2) {  // literal is empty but quote skip is required
                return quoteEnd;
            }

            literal = userInput.Substring(quoteStart + 1, quoteEnd - (quoteStart + 1));

            return quoteEnd;
        }

        static int TryExtractDoubleQuote(string userInput, int startPosition, out string literal) {

            int quoteStart = startPosition;
            int quoteEnd = quoteStart;
            literal = "";

            while (true) {
                quoteEnd = userInput.IndexOf(ShellConstants.SYMB_QUOTE_DOUBLE, quoteEnd + 1);

                if (quoteEnd == -1) {
                    return -1;
                }

                if (userInput[quoteEnd - 1] != ShellConstants.SYMB_ESCAPE) {
                    break;
                }

                if (userInput[quoteEnd - 1] == ShellConstants.SYMB_ESCAPE && userInput[quoteEnd - 2] == ShellConstants.SYMB_ESCAPE) {
                    break;
                }
            }

            if (quoteEnd - quoteStart < 2) {
                return quoteEnd;
            }

            literal = userInput.Substring(quoteStart + 1, quoteEnd - (quoteStart + 1));
            literal = ParseDoubleQuotes(literal);

            return quoteEnd;
        }

        static string ParseDoubleQuotes(string literal) {

            string parsedLiteral = "";
            int end = literal.Length - 1;

            for (int i = 0; i < literal.Length; ++i) {
                if (literal[i] == ShellConstants.SYMB_ESCAPE && i < end) {
                    if (ShellConstants.DOUBLE_QUOTE_ESCAPABLES.Contains(literal[i + 1])) {
                        ++i;
                    }
                }
                parsedLiteral += literal[i];
            }

            return parsedLiteral;
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