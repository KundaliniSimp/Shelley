using System;
using System.Collections.Generic;

namespace CodeCraftersShell 
{
    class DirectoryManager {

        readonly string[]? directories;
        readonly string? userHomeDir;

        public DirectoryManager() {

            string? envPath = Environment.GetEnvironmentVariable(ShellConstants.ENV_VAR_PATH);

            if (envPath != null) {
                directories = envPath.Split(ShellConstants.ENV_PATH_SEPARATOR);
            }

            userHomeDir = Environment.GetEnvironmentVariable(ShellConstants.ENV_VAR_HOME);
        }

        public string? GetExecutablePath(string exeName) {

            if (directories == null) {
                return null;
            }

            foreach (string dir in directories) {
                string fullPath = $"{dir}{ShellConstants.ENV_DIR_SEPARATOR}{exeName}{ShellConstants.ENV_EXECUTABLE_EXT}";

                if (File.Exists(fullPath)) {
                    return fullPath;
                }
            }

            return null;
        }

        public string GetCurrentDir() => Environment.CurrentDirectory;
        
        public bool FileExists(string userFile) {

            if (userFile.LastIndexOf(ShellConstants.ENV_DIR_SEPARATOR) == -1) {     // if only file name provided, check current directory
                return File.Exists($"{GetCurrentDir()}{ShellConstants.ENV_DIR_SEPARATOR}{userFile}");
            }

            return File.Exists(userFile);
        }

        public bool TrySetDir(string userDir) {

            if (userDir.Length == 1 && userDir[0] == ShellConstants.SYMB_HOME) {
                if (userHomeDir != null) {
                    Environment.CurrentDirectory = userHomeDir;
                    return true;
                }
                else {
                    return false;
                }
            }

            if (Directory.Exists(userDir)) {
                Environment.CurrentDirectory = userDir;
                return true;
            }

            return false;
        }
    } 
}