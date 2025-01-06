using System;
using System.Collections.Generic;

namespace CodeCraftersShell 
{
    class PathManager {

        string[] directories;

        public PathManager(string envPath = "") {

            directories = envPath.Split(";");

            foreach (string dir in directories) {
                Console.WriteLine(dir);
            }
        }

        public string? GetExecutablePath(string exeName) {

            foreach (string dir in directories) {
                string fullPath = dir + exeName;

                if (File.Exists(fullPath + ShellConstants.EXT_EXE)) {
                    return fullPath;
                }
            }

            return null;
        }
    } 
}