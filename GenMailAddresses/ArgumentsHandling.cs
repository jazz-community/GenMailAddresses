using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenMailAddresses
{
    public class NoArgumentsException : Exception
    {
        public NoArgumentsException () {}

        public NoArgumentsException (string message)
            : base(message)
        {}

        public NoArgumentsException(string message, Exception inner)
            : base(message, inner)
        {}
    }

    public class ArgumentsParseException : Exception
    {
        public ArgumentsParseException() { }

        public ArgumentsParseException(string message)
            : base(message)
        { }

        public ArgumentsParseException(string message, Exception inner)
            : base(message, inner)
        { }
    }

    public class ArgumentsHandling
    {
        private static Dictionary<string, List<string>> CL_dict;
        private static List<string> ErrorList;
        private static bool noAreaValue = false;

        private static void enqueueStrayParamsToErrList(string optionName, int maxValues)
        {
            if (CL_dict[optionName] != null)
                if (CL_dict[optionName].Count > maxValues)
                    for (int i = maxValues; i < CL_dict[optionName].Count; i++)
                        ErrorList.Add("Stray argument: " + CL_dict[optionName][i]);
        }

        private static bool BoolOption(string optionName, bool isMandatory)
        {
            if (CL_dict.ContainsKey(optionName))
            {
                enqueueStrayParamsToErrList(optionName, 0);
                CL_dict.Remove(optionName);
                return true;
            }
            else if (isMandatory)
            {
                ErrorList.Add("Missing option: " + optionName);
            }
            return false;
        }

        private static string SingleStringParameter(string parName, bool isMandatory)
        {
            string ret = null;
            if (CL_dict.ContainsKey(parName))
            {
                if (CL_dict[parName] == null)
                {
                    ErrorList.Add("Missing value for parameter: " + parName);
                    if (parName == "area")
                        noAreaValue = true;
                }
                else
                {
                    ret = CL_dict[parName][0];
                    enqueueStrayParamsToErrList(parName, 1);
                }
                CL_dict.Remove(parName);
            }
            else if (isMandatory)
                ErrorList.Add("Missing parameter: " + parName);
            return ret;
        }

        private static List<string> ListStringParameter(string parName, bool isMandatory)
        {
            List<string> ret = null;
            if (CL_dict.ContainsKey(parName))
            {
                if (CL_dict[parName] == null)
                    ErrorList.Add("Missing value(s) for parameter: " + parName);
                else
                {
                    ret = new List<string>();
                    ret.AddRange(CL_dict[parName]);
                }
                CL_dict.Remove(parName);
            }
            else if (isMandatory)
                ErrorList.Add("Missing parameter: " + parName);
            return ret;
        }

        private static void checkAreaOptions(string aString, bool aOption)
        {
            if (aOption)
            {
                if (aString != null)
                    ErrorList.Add("Either specify area path OR -a but not both");
            }
            else
            {
                if ((aString == null) && (!noAreaValue))
                    ErrorList.Add("Missing parameter '--area' OR option '-a'");
            }
        }

        public static Options getOptionsFromArguments(string[] args)
        {
            if (args.Length == 0)
                throw new NoArgumentsException();

            Options o = new Options();

            CL_dict = CommandLineParser.ParseCommandLine(args, true, true);
            ErrorList = new List<string>();

            o.jazzRepository = SingleStringParameter("url", true);
            o.area = SingleStringParameter("area", false);
            o.user = SingleStringParameter("usr", false);
            o.passwd = SingleStringParameter("pwd", false);
            o.roles = ListStringParameter("role", false);
            o.opt_recursive = BoolOption("r", false);
            o.opt_verbose = BoolOption("v", false);
            o.opt_rm = BoolOption("rm", false);
            o.opt_all_areas = BoolOption("a", false);
            o.outfile = SingleStringParameter("out", false);
            checkAreaOptions(o.area, o.opt_all_areas);

            if (CL_dict.ContainsKey(""))
            {
                enqueueStrayParamsToErrList("", 0);
                CL_dict.Remove("");
            }

            // Complain about parsing errors
            if ((CL_dict.Count > 0) || (ErrorList.Count > 0))
            {
                string m = string.Empty; int i = 0;
                foreach (string err in ErrorList)
                {
                    if (i > 0) m += "\n";
                    m += err;
                    i++;
                }
                foreach (string opt in CL_dict.Keys)
                {
                    if (i > 0) m += "\n";
                    m += "Unknown option: " + opt;
                    i++;
                }
                throw new ArgumentsParseException(m);
            }

            int limit = -1;
            if (o.jazzRepository.Length > 9)
                limit = o.jazzRepository.IndexOf('/', 10);
            if (limit > 0)
                o.jazzRepository = o.jazzRepository.Substring(0, limit);

            return o;
        }

        public static string getUsageMessage(string applicationName)
        {
            return "Usage: " + applicationName + " --url <jazzserver url> ( --area <area path> | -a ) [--usr <username>] [--pwd <password>]\n" +
                   "".PadLeft(applicationName.Length + 7) + " [--role <role name(s)>] [-r] [-v] [-rm] [--out <output filename>]\n\n" +
                   "".PadLeft(applicationName.Length + 7) + " <area path>   : project area name extended by '\\' and subsequent team area name(s)\n" +
                   "".PadLeft(applicationName.Length + 7) + "        example: GF-D\\GF-D-PDM\\PDM-Team2\n" +
                   "".PadLeft(applicationName.Length + 7) + "           note: if an area path name contains spaces, please delimit by \" (e.g. \"GF-D\\SW Team\")\n" +
                   "".PadLeft(applicationName.Length + 7) + "           -a  : scan all areas (note: only areas visible to the user are scanned)\n" +
                   "".PadLeft(applicationName.Length + 7) + " <role name(s)>: only list members with a certain role, more than one role can be specified\n" +
                   "".PadLeft(applicationName.Length + 7) + "          notes: if a role name contains spaces, please delimit by \" (e.g. \"Team Admin T\")\n" +
                   "".PadLeft(applicationName.Length + 7) + "                 an empty role \"\" addresses members with NO role assignment\n" +
                   "".PadLeft(applicationName.Length + 7) + "           -r  : recursive, walks down the area path and thus includes all (sub) team areas\n" +
                   "".PadLeft(applicationName.Length + 7) + "           -v  : verbose, show some infos while scanning area(s)\n" +
                   "".PadLeft(applicationName.Length + 7) + "           -rm : connect to 'rm' repository (instead of 'ccm')";
        }
    }
}
