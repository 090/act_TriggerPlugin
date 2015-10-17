using System;
using System.Collections.Generic;
using System.Linq;

namespace TriggerTrigger
{
    public class Triggers
    {
        //XML保存ファイル
        string _filepath;
        //Trigger一覧
        SortedList<string, Trigger> _sortedlist;
        //ループ時 使用配列
        public List<List<Trigger>> TList = new List<List<Trigger>>();

        public Triggers(string filepath)
        {
            _filepath = filepath;
            _sortedlist = new SortedList<string, Trigger>();

        }

        public Trigger Get(string key)
        {
            if (_sortedlist.ContainsKey(key))
                return _sortedlist[key];
            return null;
        }
        public bool Add(Trigger trigger,bool update=true)
        {
            if (_sortedlist.ContainsKey(trigger.Key))
                return false;
            _sortedlist.Add(trigger.Key, trigger);
            if(update)
                updateTList();
            return true;
        }
        public bool Edit(string key,Trigger trigger)
        {
            if (key != trigger.Key && _sortedlist.ContainsKey(trigger.Key))
                return false;
            if (!Remove(key))
                return false;
            return Add(trigger);
        }
        public bool Remove(string key)
        {
            if (!_sortedlist.ContainsKey(key))
                return false;
            _sortedlist.Remove(key);
            updateTList();
            return true;
        }
        public string[] Keys()
        {
            return _sortedlist.Keys.ToArray();
        }


        void updateTList()
        {
            lock (TList)
            {
                TList.Clear();
                List<Trigger> tmpList = new List<Trigger>();
                string lastRegex = string.Empty;
                foreach (var trigger in _sortedlist.Values)
                {
                    if (!trigger.EnableRegex())
                        continue;
                    if (lastRegex != trigger.RegEx)
                    {
                        tmpList = new List<Trigger>();
                        TList.Add(tmpList);
                        lastRegex = trigger.RegEx;
                    }
                    tmpList.Add(trigger);
                }
            }
        }

        public void Load()
        {
            if (!System.IO.File.Exists(_filepath))
                return;
            try {
                using (System.IO.TextReader reader = new System.IO.StreamReader(_filepath))
                {
                    System.Xml.Serialization.XmlSerializer serializer
                        = new System.Xml.Serialization.XmlSerializer(typeof(List<Trigger>));
                    List<Trigger> list = serializer.Deserialize(reader) as List<Trigger>;
                    foreach (var item in list)
                    {
                        this.Add(item,false);
                    }

                }
            }
            catch(Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.ToString());
            }
            updateTList();
        }

        public void Save()
        {
            List<Trigger> list = _sortedlist.Values.ToList();
            try {

                System.Xml.Serialization.XmlSerializerNamespaces ns = new System.Xml.Serialization.XmlSerializerNamespaces();
                ns.Add(String.Empty, String.Empty);

                using (System.IO.TextWriter writer = new System.IO.StreamWriter(_filepath))
                {
                    System.Xml.Serialization.XmlSerializer serializer
                        = new System.Xml.Serialization.XmlSerializer(typeof(List<Trigger>));
                    serializer.Serialize(writer, list,ns);
                }
            }catch(Exception e)
            {

                System.Diagnostics.Debug.WriteLine(e.ToString());
            }
        }

      

    }
}
