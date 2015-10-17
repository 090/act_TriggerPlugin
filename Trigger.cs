using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace TriggerTrigger
{
    public class Trigger
    {
        Regex _regex=null;
        public string Name { get; set; }
        public string Match { get; set; }
        private string _regex_str;
        public string Key
        {
            get
            {
                return RegEx + "/" + Name+"/" + Match;
            }
        }
        public string RegEx {
            get
            {
                return _regex_str;
            }
            set
            {
                _regex_str = value;
                try
                {
                    if (_regex_str != string.Empty)
                        _regex = new Regex(_regex_str, RegexOptions.Compiled);
                    else
                        _regex = null;
                }
                catch
                {
                    _regex = null;
                }
            }
        }
        public List<string> EnableCustomTriggers = new List<string>();
        public List<string> DisableCustomTriggers = new List<string>();
        public List<string> EnableSpellTimers = new List<string>();
        public List<string> DisableSpellTimers = new List<string>();

        public bool EnableRegex()
        {
            return _regex != null;
        }
        public Regex GetRegex()
        {
            return _regex;
        }
        public bool Check(string str)
        {
            if (_regex == null)
                return false;
            return _regex.IsMatch(str);
        }
    }
}
