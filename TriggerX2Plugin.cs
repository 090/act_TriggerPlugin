using Advanced_Combat_Tracker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;
using System.Threading;

namespace TriggerTrigger
{
    public class TriggerX2Plugin : IActPluginV1
    {
        Queue<string> _queue;
        ManualResetEvent _signal;
        bool _active;

        Label lblStatus;
        MainTab maintab;
        public Log LogWindow;
        public Triggers Triggers;
        public void InitPlugin(TabPage pluginScreenSpace, Label pluginStatusText)
        {
            _signal = new ManualResetEvent(false);
            _queue = new Queue<string>();

            string filepath = Path.Combine(
                ActGlobals.oFormActMain.AppDataFolder.FullName,
                @"Config\Triggerx2Plugin.config.xml"
            );

            this.LogWindow = new Log();
            this.Triggers = new Triggers(filepath);
            this.Triggers.Load();
            maintab = new MainTab(this);
            lblStatus = pluginStatusText;
            pluginScreenSpace.Controls.Add(maintab);
            maintab.Dock = DockStyle.Fill;

            ActGlobals.oFormActMain.BeforeLogLineRead += new LogLineEventDelegate(BeforeLogLineRead);

            queueCheckAsync();

            lblStatus.Text = "Plugin Started";
            pluginScreenSpace.Text = "Trigger x2";
        }
        void IActPluginV1.DeInitPlugin()
        {
            _active = false;
            _signal.Set();
            ActGlobals.oFormActMain.BeforeLogLineRead -= new LogLineEventDelegate(BeforeLogLineRead);
            Triggers.Save();
            lblStatus.Text = "Plugin Exited";
        }

        private void BeforeLogLineRead(bool isImport, LogLineEventArgs logInfo)
        {
            if (!isImport)
            {
                lock (_queue)
                {
                    _queue.Enqueue(logInfo.logLine);
                    _signal.Set();
                }
            }
        }
        private async void queueCheckAsync()
        {
            _active = true;
            await Task.Run(() =>
            {
                var logs = new List<string>();
                var tlist = new List<Trigger>();
                while (_active)
                {
                    _signal.WaitOne();
                    lock (_queue)
                    {
                        if (_queue.Count > 0)
                        {
                            logs.AddRange(_queue.ToArray());
                            _queue.Clear();
                        }
                        _signal.Reset();
                    }
                    if (logs.Count < 1)
                        continue;
                    bool _flagC = false;
                    bool _flagS = false;
                    lock (Triggers.TList)
                    {
                        foreach(var str in logs)
                        {
                            foreach(var trigger in Triggers.TList)
                            {
                                if (trigger[0].Check(str))
                                {
                                    var regex = trigger[0].GetRegex();
                                    var match = regex.Match(str);
                                    var group = match.Groups;
                                    if (group["Name"].Value == string.Empty)
                                    {
                                        tlist.AddRange( trigger.Where(e=> e.Match == group["Match"].Value ) );
                                    }
                                    else
                                    {
                                        tlist.AddRange(trigger.Where(e => e.Match.Equals(group["Match"].Value) && e.Name.Equals(group["Name"].Value)));
                                    }
                                    foreach (var tmp in tlist)
                                    {
                                        foreach (var t in ActGlobals.oFormActMain.CustomTriggers.Values)
                                        {
                                            if (tmp.EnableCustomTriggers.Contains(t.Category))
                                                _flagC = t.Active = true;
                                            else if (tmp.DisableCustomTriggers.Contains(t.Category))
                                                _flagC = !(t.Active = false);
                                        }

                                        foreach (var t in ActGlobals.oFormSpellTimers.TimerDefs.Values)
                                        {
                                            if (tmp.EnableSpellTimers.Contains(t.Category))
                                                _flagS = t.ActiveInList = true;
                                            else if (tmp.DisableSpellTimers.Contains(t.Category))
                                                _flagS = !(t.ActiveInList = false);
                                        }
                                    }
                                    tlist.Clear();
                                }
                            }
                            //ログウィンドウに追加
                            LogWindow.Add(str);
                        }
                    }
                    logs.Clear();
                    if (_flagC)
                        ActGlobals.oFormActMain.RebuildActiveCustomTriggers();
                    if (_flagS)
                    {
                        if (ActGlobals.oFormSpellTimers.InvokeRequired)
                        {
                            ActGlobals.oFormSpellTimers.Invoke((MethodInvoker)delegate
                            {
                                ActGlobals.oFormSpellTimers.RebuildSpellTreeView();
                            });
                        }
                        else
                        {
                            ActGlobals.oFormSpellTimers.RebuildSpellTreeView();
                        }
                    }
                }
            });
        }

    }
}
