using Advanced_Combat_Tracker;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TriggerTrigger
{
    public partial class MainTab : UserControl
    {
        static readonly string CustomTriggerSuffix = @" (Custom)";
        static readonly string SpellTimerSuffix = @" (Spell)";
        TriggerX2Plugin _plugin;
        string _editKey=string.Empty;
        public MainTab(TriggerX2Plugin parent)
        {
            InitializeComponent();
            _plugin = parent;
        }

        private void MainTab_Load(object sender, EventArgs e)
        {
            RefreshTriggers();
            buttonEditSave.Enabled = false;
        }

        //最新のTriggerのTrigger一覧を表示
        void RefreshTriggers()
        {
            treeViewTriggers.Nodes.Clear();
            comboBoxRegex.Items.Clear();
            string lastRegex = String.Empty;
            TreeNode regex_node = null;
            foreach(var key in _plugin.Triggers.Keys())
            {
                var trigger = _plugin.Triggers.Get(key);
                if(trigger.RegEx != lastRegex || regex_node == null)
                {
                    regex_node = new TreeNode(trigger.RegEx);
                    treeViewTriggers.Nodes.Add(regex_node);
                    lastRegex = trigger.RegEx;
                    comboBoxRegex.Items.Add(lastRegex);
                }
                var node = new TreeNode(trigger.Name);
                node.Name = key;
                if (trigger.Match != string.Empty)
                    node.Text += ":" + trigger.Match;
                regex_node.Nodes.Add(node);
              
            }
            treeViewTriggers.ExpandAll();
        }


        //表示されるたびにカスタムトリガーの一覧を更新
        private void MainTab_VisibleChanged(object sender, EventArgs e)
        {
            updateCustomTriggersListBox();
        }
        void updateCustomTriggersListBox()
        {
            List<string> checkEnable = new List<string>();
            foreach (var item in checkedListBoxEnable.CheckedItems)
                checkEnable.Add(item.ToString());
            checkedListBoxEnable.Items.Clear();

            List<string> checkDisable = new List<string>();
            foreach (var item in checkedListBoxDisable.CheckedItems)
                checkDisable.Add(item.ToString());
            checkedListBoxDisable.Items.Clear();


            List<string> customlist = new List<string>();
            foreach (var ct in ActGlobals.oFormActMain.CustomTriggers)
            {
                var category = ct.Value.Category + CustomTriggerSuffix;
                if (!customlist.Contains(category))
                    customlist.Add(category);
            }
            foreach (var timer in ActGlobals.oFormSpellTimers.TimerDefs)
            {
                var category = timer.Value.Category + SpellTimerSuffix;
                if (!customlist.Contains(category))
                    customlist.Add(category);
            }

            foreach (var category in customlist)
            {
                checkedListBoxEnable.Items.Add(category, checkEnable.Contains(category));
                checkedListBoxDisable.Items.Add(category, checkDisable.Contains(category));
            }
        }

        //入力内容を追加
        private void buttonAdd_Click(object sender, EventArgs e)
        {
            Trigger newitem = getTrigger();
            if (newitem == null)
                return;
            if (_plugin.Triggers.Add(newitem)){
                RefreshTriggers();
            }else{
                MessageBox.Show("fail", "Trigger", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        //入力内容で既存のデータを更新
        private void buttonEditSave_Click(object sender, EventArgs e)
        {
            Trigger newitem = getTrigger();
            if (newitem == null || _editKey == string.Empty)
                return;
            if ( _plugin.Triggers.Edit(_editKey,newitem))
            {
                RefreshTriggers();
            }
            else
            {
                MessageBox.Show("fail", "Trigger", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }
        //入力内容からTriggerクラスを作成
        Trigger getTrigger()
        {
            Trigger item = new Trigger
            {
                Name = this.textBoxName.Text,
                RegEx = this.comboBoxRegex.Text,
                Match = this.textBoxMatch.Text
            };
            if (item.Name == string.Empty)
            {
                MessageBox.Show("Please enter a Name", "Trigger", MessageBoxButtons.OK, MessageBoxIcon.Error);
                textBoxName.Focus();
                return null;
            }
            if (item.RegEx == string.Empty)
            {
                MessageBox.Show("Please enter a Regex", "Trigger", MessageBoxButtons.OK, MessageBoxIcon.Error);
                comboBoxRegex.Focus();
                return null;
            }
            if (!item.EnableRegex())
            {
                MessageBox.Show("bad Regex", "Trigger", MessageBoxButtons.OK, MessageBoxIcon.Error);
                comboBoxRegex.Focus();
                return null;
            }
            foreach (var i in checkedListBoxEnable.CheckedItems)
            {
                string check = i.ToString();
                if (check.EndsWith(CustomTriggerSuffix))
                {
                    item.EnableCustomTriggers.Add(
                        check.Substring(0, check.Length - CustomTriggerSuffix.Length)
                        );
                }
                else if (check.EndsWith(SpellTimerSuffix))
                {
                    item.EnableSpellTimers.Add(
                        check.Substring(0, check.Length - SpellTimerSuffix.Length)
                        );
                }
            }

            foreach (var i in checkedListBoxDisable.CheckedItems)
            {
                string check = i.ToString();
                if (check.EndsWith(CustomTriggerSuffix))
                {
                    item.DisableCustomTriggers.Add(
                        check.Substring(0, check.Length - CustomTriggerSuffix.Length)
                        );
                }
                else if (check.EndsWith(SpellTimerSuffix))
                {
                    item.DisableSpellTimers.Add(
                        check.Substring(0, check.Length - SpellTimerSuffix.Length)
                        );
                }
            }
            if (item.EnableCustomTriggers.Count == 0
                && item.EnableSpellTimers.Count == 0
                && item.DisableCustomTriggers.Count == 0
                && item.DisableSpellTimers.Count == 0)
            {
                MessageBox.Show("Please check", "Trigger", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }

            return item;

        }

        //"New"ボタンがクリアされたらフォームを綺麗にする。
        private void buttonClear_Click(object sender, EventArgs e)
        {
            buttonAdd.Enabled = true;
            buttonEditSave.Enabled = false;
            buttonRemove.Enabled = false;

            _editKey = string.Empty;
            textBoxName.Text = "";
            comboBoxRegex.Text = "";
            textBoxMatch.Text = "";
            ClearCheckdList();
        }
        //チェックボックスを解除
        void ClearCheckdList()
        {
            foreach (int i in checkedListBoxEnable.CheckedIndices)
            {
                checkedListBoxEnable.SetItemChecked(i, false);
            }
            foreach (int i in checkedListBoxDisable.CheckedIndices)
            {
                checkedListBoxDisable.SetItemChecked(i, false);
            }
        }


        //Removeがクリックされたら、編集中データ削除
        private void buttonRemove_Click(object sender, EventArgs e)
        {
            if (_editKey == string.Empty)
                return;
            if (MessageBox.Show("Aer you sure wan to delete?", "Trigger", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                _plugin.Triggers.Remove(_editKey);
                RefreshTriggers();
                buttonClear_Click(this,null);
            }
        }


        //左一覧を選択した時、右フォームにデータを表示
        private void treeViewTriggers_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Node.Name == string.Empty)
                return;

            Trigger item = _plugin.Triggers.Get(e.Node.Name);
            if (item == null)
                return;

            ClearCheckdList();

            buttonAdd.Enabled = false;
            buttonEditSave.Enabled = true;
            buttonRemove.Enabled = true;

            //編集中のキーを記憶
            _editKey = item.Key;

            textBoxName.Text = item.Name;
            comboBoxRegex.Text = item.RegEx;
            textBoxMatch.Text = item.Match;

            foreach (var str in item.EnableCustomTriggers)
            {
                string name = str + CustomTriggerSuffix;
                int ret = checkedListBoxEnable.FindStringExact(name);
                if (ret != ListBox.NoMatches)
                    checkedListBoxEnable.SetItemCheckState(ret, CheckState.Checked);
            }

            foreach (var str in item.DisableCustomTriggers)
            {
                string name = str + CustomTriggerSuffix;
                int ret = checkedListBoxDisable.FindStringExact(name);
                if (ret != ListBox.NoMatches)
                    checkedListBoxDisable.SetItemCheckState(ret, CheckState.Checked);
            }

            foreach (var str in item.EnableSpellTimers)
            {
                string name = str + SpellTimerSuffix;
                int ret = checkedListBoxEnable.FindStringExact(name);
                if (ret != ListBox.NoMatches)
                    checkedListBoxEnable.SetItemCheckState(ret, CheckState.Checked);
            }

            foreach (var str in item.DisableSpellTimers)
            {
                string name = str + SpellTimerSuffix;
                int ret = checkedListBoxDisable.FindStringExact(name);
                if (ret != ListBox.NoMatches)
                    checkedListBoxDisable.SetItemCheckState(ret, CheckState.Checked);
            }

        }

        //ログウィンドウを表示
        private void buttonShowLog_Click(object sender, EventArgs e)
        {
            _plugin.LogWindow.Show();
        }
        //ログウィンドウの流れを止める
        private void buttonStop_Click(object sender, EventArgs e)
        {
            _plugin.LogWindow.Off();
        }
    }
}
