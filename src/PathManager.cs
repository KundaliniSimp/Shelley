using System;
using System.Collections.Generic;

namespace CodeCraftersShell 
{
    class PathManager {

        string[] directories;
        string currentDir;

        public PathManager(string envPath = "") {

            directories = envPath.Split(ShellConstants.ENV_VAR_PATH_SEPARATOR);
            currentDir = Environment.CurrentDirectory;
        }

        public string? GetExecutablePath(string exeName) {

            foreach (string dir in directories) {
                string fullPath = $"{dir}{ShellConstants.ENV_DIR_SEPARATOR}{exeName}";

                if (File.Exists(fullPath)) {
                    return fullPath;
                }
            }

            return null;
        }

        public string GetCurrentDir() {
            return currentDir;
        }
    } 
}