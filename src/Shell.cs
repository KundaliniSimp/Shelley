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
                case ShellConstants.CMD_ECHO: response.Message = CmdEcho(arguments); break;
                case ShellConstants.CMD_EXIT: isRunning = false; break;
                case ShellConstants.CMD_TYPE: response.Message = CmdType(arguments); break;
                case ShellConstants.CMD_PWD: response.Message = CmdPwd(); break;
                case ShellConstants.CMD_CD: response.Message = CmdCd(arguments); break;
                case ShellConstants.CMD_CLEAR: CmdClear(); break;

                default:
                   string? runOutput = CmdTryRun(arguments, response.RedirectionType);
                    
                   if (runOutput != null) {
                        response.Message = runOutput;
                   }
                   else { 
                        response.Message = $"{command}: {ShellConstants.RESP_INVALID_CMD}";
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

            if (String.IsNullOrEmpty(response.Message)) {
                return;
            }

            if (response.RedirectionType != RedirectionType.NO_REDIRECT) {
                if (response.RedirectionPrintMode == RedirectionPrintMode.NEW) {
                    File.WriteAllText(response.RedirectDirectory, response.Message);
                }
                else if (response.RedirectionPrintMode == RedirectionPrintMode.APPEND) {
                    File.AppendAllText(response.RedirectDirectory, response.Message);
                }
            }
            else {
                Console.WriteLine(response.Message);
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

        string? CmdTryRun(string[] arguments, RedirectionType redirectionType) {

            string command = arguments[0];
            string? executablePath = pathManager.GetExecutablePath(command);
            string processOutput = "";

            if (executablePath == null) {
                return null;
            }

            ProcessStartInfo processConfig = new(command);

            if (redirectionType == RedirectionType.STD_OUTPUT) {
                processConfig.RedirectStandardOutput = true;
            }
            else if (redirectionType == RedirectionType.STD_ERROR) {
                processConfig.RedirectStandardError = true;
            }

            if (arguments.Length > 1) {
                for (int i = 1; i < arguments.Length; ++i) {
                    processConfig.ArgumentList.Add(arguments[i]);
                }
            }

            Process? currentProcess = Process.Start(processConfig);

            if (currentProcess == null) {
                return null;
            }

            if (redirectionType == RedirectionType.STD_OUTPUT) {
                processOutput += currentProcess.StandardOutput.ReadToEnd().TrimEnd();
            }
            else if (redirectionType == RedirectionType.STD_ERROR) {
                processOutput += currentProcess.StandardError.ReadToEnd();
            }

            while (!currentProcess.HasExited) {

                Thread.Sleep(ShellConstants.SLEEP_INTERVAL);
            }

            return processOutput;
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