using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenMailAddresses
{
    /// <summary>
    /// Parsed program command line options
    /// </summary>
    public class Options
    {
        private bool _opt_rm = false;
        public bool opt_rm
        {
            get { return _opt_rm; }
            set { _opt_rm = value; }
        }
        private bool _opt_recursive = false;
        public bool opt_recursive
        {
            get { return _opt_recursive; }
            set { _opt_recursive = value; }
        }
        private bool _opt_verbose = false;
        public bool opt_verbose
        {
            get { return _opt_verbose; }
            set { _opt_verbose = value; }
        }
        private bool _opt_all_areas = false;
        public bool opt_all_areas
        {
            get { return _opt_all_areas; }
            set { _opt_all_areas = value; }
        }
        public string jazzRepository { get; set; }
        public string user { get; set; }
        public string passwd { get; set; }
        public string area { get; set; }
        public List<string> roles { get; set; }
        public string outfile { get; set; }

        public bool AllMandatoryOptionsPresent()
        {
            if (jazzRepository == null)
                return false;
            //if (user == null)
            //    return false;
            //if (passwd == null)
            //    return false;
            if ((area == null) && (_opt_all_areas == false))
                return false;
            return true;
        }
    }
}
