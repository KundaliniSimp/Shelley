using System;
using System.Collections.Generic;

namespace CodeCraftersShell 
{
    class PathManager {

        string[] directories;

        public PathManager(string envPath = "") {

            directories = envPath.Split(ShellConstants.ENV_VAR_PATH_SEPARATOR);

        }

        public string? GetExecutablePath(string exeName) {

            foreach (string dir in directories) {
                string fullPath = dir + ShellConstants.ENV_DIR_SEPARATOR + exeName;

                if (File.Exists(fullPath + ShellConstants.EXT_EXE)) {
                    return fullPath;
                }
            }

            return null;
        }
    } 
}