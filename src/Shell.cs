using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

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
            CommandResponse response = Eval(userInput);
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

        CommandResponse Eval(string userInput) {

            string[] arguments = ParseInput(userInput);

            if (arguments.Length < 1) {
                return new CommandResponse();
            }

            string command = arguments[0];
            CommandResponse response = new();

            response = GetRedirectionData(ref arguments, response);

            switch (command) {
                case ShellConstants.CMD_ECHO: response.OutputMessage = CmdEcho(arguments); break;
                case ShellConstants.CMD_EXIT: isRunning = false; break;
                case ShellConstants.CMD_TYPE: response.OutputMessage = CmdType(arguments); break;
                case ShellConstants.CMD_PWD: response.OutputMessage = CmdPwd(); break;
                case ShellConstants.CMD_CD: response.OutputMessage = CmdCd(arguments); break;
                case ShellConstants.CMD_CLEAR: CmdClear(); break;

                default:
                   bool runOutcome = CmdTryRun(arguments, response);
                    
                   if (!runOutcome) {
                        response.OutputMessage = $"{command}: {ShellConstants.RESP_INVALID_CMD}";
                   }

                   break;
            }

            return response;
        }

        CommandResponse GetRedirectionData(ref string[] arguments, CommandResponse response) {

            string redirectFlag = "";
            string redirectDirectory = "";
            int redirectFlagIndex = -1;
            int end = arguments.Length - 1;

            for (int i = 0; i < arguments.Length; ++i) {
                if (arguments[i][arguments[i].Length - 1] == ShellConstants.FLAG_REDIRECT_DEFAULT[0]) {
                    redirectFlag = arguments[i];
                    redirectFlagIndex = i;

                    if (i < end) {
                        redirectDirectory = arguments[i + 1];
                    }

                    break;
                }
            }

            if (String.IsNullOrEmpty(redirectFlag) || String.IsNullOrEmpty(redirectDirectory)) {      // complete redirection data not found
                goto exit;
            }

            response.RedirectDirectory = redirectDirectory;

            switch (redirectFlag) {
                case ShellConstants.FLAG_REDIRECT_DEFAULT:
                case ShellConstants.FLAG_REDIRECT_OUTPUT_NEW:
                    response.RedirectionType = RedirectionType.STD_OUTPUT;
                    response.RedirectionPrintMode = RedirectionPrintMode.NEW;
                    break;
                case ShellConstants.FLAG_REDIRECT_OUTPUT_APPEND:
                    response.RedirectionType = RedirectionType.STD_OUTPUT;
                    response.RedirectionPrintMode = RedirectionPrintMode.APPEND;
                    break;
                case ShellConstants.FLAG_REDIRECT_ERROR_NEW:
                    response.RedirectionType = RedirectionType.STD_ERROR;
                    response.RedirectionPrintMode = RedirectionPrintMode.NEW;
                    break;
                case ShellConstants.FLAG_REDIRECT_ERROR_APPEND:
                    response.RedirectionType = RedirectionType.STD_ERROR;
                    response.RedirectionPrintMode = RedirectionPrintMode.APPEND;
                    break;
            }

        exit:
            if (redirectFlagIndex > -1) {
                Array.Resize(ref arguments, redirectFlagIndex);             // truncate redirection data from arguments
            }

            return response;
        }

        static void Print(CommandResponse response) {

            ProcessPrintAction(
                response.OutputMessage, response.RedirectionType == RedirectionType.STD_OUTPUT, response.RedirectDirectory, response.RedirectionPrintMode
            );
            ProcessPrintAction(response.ErrorMessage, response.RedirectionType == RedirectionType.STD_ERROR, response.RedirectDirectory, response.RedirectionPrintMode);
        }

        static void ProcessPrintAction(string message, bool isRedirected, string redirectionDirectory = "", RedirectionPrintMode printMode = RedirectionPrintMode.NULL) {

            if (String.IsNullOrEmpty(message)) {
                return;
            }

            if (!isRedirected) {
                Console.WriteLine(message);
            }

            if (String.IsNullOrEmpty(redirectionDirectory)) {
                return;
            }

            if (printMode == RedirectionPrintMode.NEW) {
                File.WriteAllText(redirectionDirectory, message);
            }
            else if (printMode == RedirectionPrintMode.APPEND) {
                File.AppendAllText(redirectionDirectory, message);
            }

        }

        static string CmdEcho(string[] arguments) {

            string output = "";

            if (arguments.Length == 1) {
                return output;
            }

            for (int i = 1; i < arguments.Length; ++i) {
                output += arguments[i];

                if (i < arguments.Length - 1) {
                    output += ShellConstants.SYMB_WHITESPACE;
                }
            }

            return output;
        }

        string CmdType(string[] arguments) {

            if (arguments.Length < 2) {
                return "";
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

            if (quoteEnd - quoteStart < 2) {  // quotes match but literal is empty
                return quoteEnd;
            }

            literal = userInput.Substring(quoteStart + 1, quoteEnd - (quoteStart + 1));

            return quoteEnd;
        }

        static int TryExtractDoubleQuote(string userInput, int startPosition, out string literal) {

            int quoteStart = startPosition;
            int quoteEnd = quoteStart;
            literal = "";

            while (quoteEnd != -1) {
                quoteEnd = userInput.IndexOf(ShellConstants.SYMB_QUOTE_DOUBLE, quoteEnd + 1);

                if (quoteEnd == -1) {
                    return quoteEnd;
                }

                if (userInput[quoteEnd - 1] != ShellConstants.SYMB_ESCAPE) {
                    break;
                }

                int i = quoteEnd - 1;
                int escapeCount = 0;

                while (userInput[i] == ShellConstants.SYMB_ESCAPE) {
                    ++escapeCount;
                    --i;
                }

                if (escapeCount % 2 == 0) {
                    break;
                }
                else {
                    continue;
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

        bool CmdTryRun(string[] arguments, CommandResponse response) {

            string command = arguments[0];
            string? executablePath = pathManager.GetExecutablePath(command);

            if (executablePath == null) {
                return false;
            }

            ProcessStartInfo processConfig = new(command);
            processConfig.RedirectStandardOutput = true;
            processConfig.RedirectStandardError = true;

            if (arguments.Length > 1) {
                for (int i = 1; i < arguments.Length; ++i) {
                    processConfig.ArgumentList.Add(arguments[i]);
                }
            }

            Process? currentProcess = Process.Start(processConfig);

            if (currentProcess == null) {
                return false;
            }

            response.OutputMessage = currentProcess.StandardOutput.ReadToEnd().TrimEnd();
            response.ErrorMessage = currentProcess.StandardError.ReadToEnd().TrimEnd();

            while (!currentProcess.HasExited) {

                Thread.Sleep(ShellConstants.SLEEP_INTERVAL);
            }

            return true;
        }
        
        string CmdPwd() {

            return pathManager.GetCurrentDir();
        }

        string CmdCd(string[] arguments) {

            if (arguments.Length < 2) {
                return "";
            }

            string userDir = arguments[1];

            if (pathManager.TrySetDir(userDir)) {
                return "";
            }

            return $"{ShellConstants.CMD_CD}: {userDir}: {ShellConstants.RESP_INVALID_DIR}";
        }

        static void CmdClear() {

            Console.Clear();
        }

        public static bool IsEnvironmentWindows() {

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
                return true;
            }
            else {
                return false;
            }
        }
    }
}