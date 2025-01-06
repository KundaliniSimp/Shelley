using System;
using System.Collections.Generic;

namespace CodeCraftersShell 
{
    class PathManager {

        string[] directories;

        public PathManager(string envPath = "") {

            directories = envPath.Split(";");
        }

        public string? GetExecutablePath(string exeName) {

            foreach (string directory in directories) {
                string fullPath = directory + exeName;

                if (File.Exists(fullPath + ShellConstants.EXT_EXE)) {
                    return fullPath;
                }
            }

            return null;
        }
    } 
}