using System;
using System.Diagnostics;

namespace CodeCraftersShell
{
    class Shell
    {

        bool isRunning;
        PathManager pathManager;

        public Shell() {

            isRunning = false;
            string? envPath = Environment.GetEnvironmentVariable(ShellConstants.ENV_VAR_PATH);

            if (envPath != null) {
                pathManager = new(envPath);
            }
            else {
                pathManager = new();
            }

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
                return userInput;
            }
            return "";
        }

        string? Eval(string userInput) {

            string[] parsedInput = userInput.Split(" ");
            string[] arguments = GetArguments(parsedInput);
            string command = parsedInput[0];

            switch (command) {
                case ShellConstants.CMD_ECHO: return CmdEcho(userInput);
                case ShellConstants.CMD_EXIT: isRunning = false; return null;
                case ShellConstants.CMD_TYPE: return CmdType(arguments[0]);
                case ShellConstants.CMD_PWD: return CmdPwd();
                case ShellConstants.CMD_CD: return CmdCd(arguments[0]);
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

        string CmdEcho(string userInput) {

            return userInput.Substring(ShellConstants.CMD_ECHO.Length + 1);
        }

        string CmdType(string command) {

            if (ShellConstants.BUILTINS.Contains(command)) {
                return $"{command} {ShellConstants.RESP_VALID_TYPE}";
            }

            string? executablePath = pathManager.GetExecutablePath(command);

            if (executablePath != null) {
                return $"{command} {ShellConstants.RESP_VALID_PATH} {executablePath}";
            }

            return $"{command}: {ShellConstants.RESP_INVALID_TYPE}";
        }

        bool CmdTryRun(string command, string fullInput) {

            string? executablePath = pathManager.GetExecutablePath(command);

            if (executablePath == null) {
                return false;
            }

            string externalArguments = fullInput.Substring(command.Length + 1);
            Process.Start(executablePath, externalArguments);

            return true;
        }

        string CmdPwd() {

            return pathManager.GetCurrentDir();
        }

        string? CmdCd(string userDir) {

            if (pathManager.TrySetDir(userDir)) {
                return null;
            }

            return $"{ShellConstants.CMD_CD}: {userDir}: {ShellConstants.RESP_INVALID_DIR}";
        }
    }
}
