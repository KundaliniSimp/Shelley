using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace CodeCraftersShell
{
    class Shell
    {
        bool isRunning;
        DirectoryManager directoryManager;
        InputManager inputManager;

        public Shell() {

            Console.Title = ShellConstants.APP_TITLE;
            isRunning = false;
            directoryManager = new();
            inputManager = new(directoryManager.GetAllPathExecutables());
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

        string Read() {

            Console.Write(ShellConstants.NEW_PROMPT);
            string? userInput = inputManager.GetUserInput();

            Console.Write(ShellConstants.SYMB_NEWLINE);

            if (userInput != null) {
                return userInput;
            }

            return "";
        }

        CommandResponse Eval(string userInput) {

            string[] arguments = ShellUtilities.ParseInput(userInput);

            if (arguments.Length < 1) {
                return new CommandResponse();
            }

            string command = arguments[0];
            CommandResponse response = new();

            Redirection.GetRedirectionData(ref arguments, response);

            switch (command) {
                case ShellConstants.CMD_ECHO: CmdEcho(arguments, response); break;
                case ShellConstants.CMD_TYPE: CmdType(arguments, response); break;
                case ShellConstants.CMD_PWD: CmdPwd(response); break;
                case ShellConstants.CMD_CD: CmdCd(arguments, response); break;
                case ShellConstants.CMD_CAT: CmdCat(arguments, response); break;
                //case ShellConstants.CMD_LS: CmdLs(arguments, response); break;
                case ShellConstants.CMD_CLEAR: CmdClear(); break;
                case ShellConstants.CMD_EXIT: CmdExit(); break;
                default: CmdTryRun(arguments, response); break;
            }

            return response;
        }

        static void Print(CommandResponse response) {

            ProcessPrintAction(
                response.OutputMessage, response.RedirectionType == RedirectionType.STD_OUTPUT, response.RedirectDirectory, response.RedirectionPrintMode
            );
            ProcessPrintAction(
                response.ErrorMessage, response.RedirectionType == RedirectionType.STD_ERROR, response.RedirectDirectory, response.RedirectionPrintMode
            );
        }

        static void ProcessPrintAction(string message, bool isRedirected, string redirectionDirectory = "", RedirectionPrintMode printMode = RedirectionPrintMode.NULL) {

            if (!isRedirected) {
                if (!String.IsNullOrEmpty(message)) {
                    Console.WriteLine(message);
                }
                return;
            }

            if (String.IsNullOrEmpty(redirectionDirectory)) {
                return;
            }

            if (printMode == RedirectionPrintMode.NEW) {
                File.WriteAllText(redirectionDirectory, message);
            }
            else if (printMode == RedirectionPrintMode.APPEND) {
                if (!File.Exists(redirectionDirectory)) {
                    File.WriteAllText(redirectionDirectory, message);
                }
                else {
                    int firstCharacterIndex;
                    using (StreamReader reader = new(redirectionDirectory)) {
                        firstCharacterIndex = reader.Peek();
                    }

                    File.AppendAllText(redirectionDirectory, $"{(firstCharacterIndex > -1 ? ShellConstants.SYMB_NEWLINE : "")}{message}");
                }
            }
        }

        static void CmdEcho(string[] arguments, CommandResponse response) {

            string output = "";

            if (arguments.Length == 1) {
                return;
            }

            for (int i = 1; i < arguments.Length; ++i) {
                output += arguments[i];

                if (i < arguments.Length - 1) {
                    output += ShellConstants.SYMB_WHITESPACE;
                }
            }

            response.OutputMessage = output;
        }

        void CmdType(string[] arguments, CommandResponse response) {

            if (arguments.Length < 2) {
                return;
            }

            string command = arguments[1];

            if (ShellConstants.BUILTINS.Contains(command)) {
                response.OutputMessage = $"{command} {ShellConstants.RESP_VALID_TYPE}";
                return;
            }

            string? executablePath = directoryManager.GetExecutablePath(command);

            if (executablePath != null) {
                response.OutputMessage = $"{command} {ShellConstants.RESP_VALID_PATH} {executablePath}";
                return;
            }

            response.OutputMessage = $"{command}: {ShellConstants.RESP_INVALID_TYPE}";
        }

        void CmdTryRun(string[] arguments, CommandResponse response) {

            string command = arguments[0];
            string? executablePath = directoryManager.GetExecutablePath(command);

            if (executablePath == null) {
                response.OutputMessage = $"{command}: {ShellConstants.RESP_INVALID_CMD}";
                return;
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
                return;
            }

            response.OutputMessage = currentProcess.StandardOutput.ReadToEnd().TrimEnd();
            response.ErrorMessage = currentProcess.StandardError.ReadToEnd().TrimEnd();

            while (!currentProcess.HasExited) {
                Thread.Sleep(ShellConstants.SLEEP_INTERVAL);
            }
        }
        
        void CmdPwd(CommandResponse response) {

            response.OutputMessage = directoryManager.GetCurrentDir();
        }

        void CmdCd(string[] arguments, CommandResponse response) {

            if (arguments.Length < 2) {
                return;
            }

            string userDir = arguments[1];

            if (directoryManager.TrySetDir(userDir)) {
                return;
            }

            response.OutputMessage = $"{ShellConstants.CMD_CD}: {userDir}: {ShellConstants.RESP_INVALID_DIR}";
        }

        void CmdCat(string[] arguments, CommandResponse response) {

            if (arguments.Length < 2) {
                return;
            }

            string userFile = arguments[1];

            if (!directoryManager.FileExists(userFile)) {
                response.OutputMessage = $"{ShellConstants.CMD_CAT}: {userFile}: {ShellConstants.RESP_INVALID_DIR}";
                return;
            }

            using (StreamReader reader = new(userFile)) {
                response.OutputMessage = reader.ReadToEnd();
            }

            response.OutputMessage = response.OutputMessage.TrimEnd();
        }

        // TODO: implement
        void CmdLs(string[] arguments, CommandResponse response) {
            return;

        }

        static void CmdClear() {

            Console.Clear();
        }
        
        void CmdExit() {
            isRunning = false;
        }
    }
}