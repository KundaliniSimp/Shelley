namespace CodeCraftersShell
{
    static class ShellConstants
    {
        public const char SYMB_PROMPT = '$';
        public const char SYMB_HOME = '~';
        public const char SYMB_QUOTE_SINGLE = '\'';
        public const char SYMB_QUOTE_DOUBLE = '"';
        public const char SYMB_BACKSLASH = '\\';
        public const string SYMB_WHITESPACE = " ";
        public const string APP_TITLE = "LiteShell";
        public const string CMD_ECHO = "echo";
        public const string CMD_EXIT = "exit";
        public const string CMD_TYPE = "type";
        public const string CMD_PWD = "pwd";
        public const string CMD_CD = "cd";
        public const string CMD_CLEAR = "clear";
        public const string RESP_INVALID_CMD = "command not found";
        public const string RESP_VALID_TYPE = "is a shell builtin";
        public const string RESP_INVALID_TYPE = "not found";
        public const string RESP_VALID_PATH = "is";
        public const string RESP_INVALID_DIR = "No such file or directory";
        public const string ENV_VAR_PATH = "PATH";
        public const string ENV_VAR_PATH_SEPARATOR = ":";
        public const string ENV_VAR_HOME = "HOME";
        public const string ENV_DIR_SEPARATOR = "/";
        public const int SLEEP_INTERVAL = 100;
        public static readonly HashSet<string> BUILTINS = new([CMD_ECHO, CMD_EXIT, CMD_TYPE, CMD_PWD, CMD_CD, CMD_CLEAR]);
        public static readonly HashSet<char> SYMB_QUOTES = new([SYMB_QUOTE_SINGLE, SYMB_QUOTE_DOUBLE]);
    }
}
