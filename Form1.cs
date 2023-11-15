using Microsoft.VisualBasic.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Transactions;
using System.Windows.Forms;
using WinForms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static System.ComponentModel.Design.ObjectSelectorEditor;
using System.Globalization;
using System.Reflection;
using System.Xml.Linq;
using System.Net.Sockets;
using Newtonsoft.Json;

namespace LocManager
{
    public partial class Form1 : Form
    {
        private ImageList iconList = new ImageList();
        List<TreeNode> entry_nodes = new List<TreeNode>();
        ToolStripProgressBar progressBar = new ToolStripProgressBar();
        ToolStripDropDownButton dropDownButton = new ToolStripDropDownButton();
        ToolStripMenuItem clickedItem = new ToolStripMenuItem();
        private List<LocEntry> updated_locentries= new List<LocEntry>();
        private TreeNode selected_Node;
        string translated_text;
        ZipArchive ziparch;
        public Form1()
        {
            InitializeComponent();
            
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.tabControl1.SelectedTab = this.tabPage2;
            // Add the icons to the image list
            iconList.Images.Add("folder", PigeL2.Properties.Resources.folder.ToBitmap());
            iconList.Images.Add("file", PigeL2.Properties.Resources.file.ToBitmap());

            // Set the tree view's image list
            treeView1.ImageList = iconList;

            // Create a root node
            TreeNode rootNode = new TreeNode("<ROOT>");
            rootNode.ImageKey = "folder";
            rootNode.SelectedImageKey = "folder";


            
            treeView1.ImageList = iconList;
            treeView1.Nodes.Add(rootNode);

            StatusStrip statusStrip = new StatusStrip();
            ToolStripButton button = new ToolStripButton();
            button.Text = "Translate";
            button.Click += Button_Click;
            statusStrip.Items.Add(button);
            
            dropDownButton.DropDownItems.Add("English");
            dropDownButton.DropDownItems.Add("Polish");       //https://stackoverflow.com/questions/37593357/c-sharp-openfiledialog-open-zip-folder-containing-single-file
            dropDownButton.DropDownItems.Add("Spanish");
            dropDownButton.DropDownItems.Add("Portuguese");
            dropDownButton.DropDownItems.Add("Chinese");
            statusStrip.Items.Add(dropDownButton);

            
            ToolStripStatusLabel spring = new ToolStripStatusLabel();
            spring.Spring = true;
            statusStrip.Items.Add(spring);

            dropDownButton.DropDownItemClicked += (sender, e) =>
            {
                foreach (ToolStripMenuItem item in dropDownButton.DropDownItems)
                {
                    item.Checked = false;
                }

                clickedItem = (ToolStripMenuItem)e.ClickedItem;
                clickedItem.Checked = true;
            };

            
            progressBar.Alignment = ToolStripItemAlignment.Right;
            progressBar.Maximum = 100;
            progressBar.Value = 0;
            statusStrip.Items.Add(progressBar);

            
            this.Controls.Add(statusStrip);

        }
        private void Button_Click(object sender, EventArgs e)
        {

            if (selected_Node != null && entry_nodes.Contains(selected_Node))
            {
                backgroundWorker1.WorkerReportsProgress = true;
                backgroundWorker1.RunWorkerAsync();
                progressBar.Value = 0;
            }

        }
        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)   //https://stackoverflow.com/questions/13874122/running-a-method-in-backgroundworker-and-showing-progressbar
        {

            
            FieldInfo fieldInfo = typeof(Language).GetField(clickedItem.ToString());
            string description="";
            if (fieldInfo != null)
            {
                DescriptionAttribute[] attributes = (DescriptionAttribute[])fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);
                if (attributes != null && attributes.Length > 0)
                {
                    description = attributes[0].Description;
                }
            }
            string languageFrom = "EN";
            string languageTo = description;
            LocEntry loc = (LocEntry)selected_Node.Tag;
            string text=" ";
            int p = 0;
            foreach (KeyValuePair<WinForms.Language, string> translation in loc.Translations)
            {
                text=translation.Value;
                int progressPercentage = (int)((float)(p+1)/ loc.Translations.Count*100);
                backgroundWorker1.ReportProgress(progressPercentage);
                p++;
            }
           
            string translatedText = Translator.Translate(languageFrom, languageTo, text);
            translated_text = translatedText;

            
        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar.Value = e.ProgressPercentage;     //https://www.codeproject.com/Tips/83317/BackgroundWorker-and-ProgressBar-demo

        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {

            progressBar.Value =100;
            ListViewItem item = new ListViewItem(new string[] { clickedItem.Text.ToString(), translated_text });
            listView1.Items.Add(item);
            
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();             //https://stackoverflow.com/questions/37593357/c-sharp-openfiledialog-open-zip-folder-containing-single-file
            fileDialog.Filter = "Zip Files(*.ZIP)|*.ZIP|All Files(*.*)|*.*";
            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                string filepath = fileDialog.FileName;
                ziparch = ZipFile.OpenRead(filepath);
                var nodeDict = new Dictionary<string, TreeNode>();
                treeView1.Nodes.Clear();    
                foreach (ZipArchiveEntry entry in ziparch.Entries)
                {
                    using (StreamReader reader = new StreamReader(entry.Open()))
                    {
                        string value = reader.ReadToEnd();
                        LocEntry jsonvalue = System.Text.Json.JsonSerializer.Deserialize<LocEntry>(value);
                        updated_locentries.Add(jsonvalue);
                        var path = jsonvalue.HierarchyPath.Split("-");
                        var parentNode = new TreeNode();

                        for (int i = 0; i < path.Length; i++)
                        {
                            var nodeName = path[i];
                            if (!nodeDict.TryGetValue(nodeName, out var node))
                            {
                                node = new TreeNode(nodeName);
                                nodeDict.Add(nodeName, node);
                                if (i == 0)
                                {
                                    treeView1.Nodes.Add(node);
                                }
                                else
                                {
                                    parentNode.Nodes.Add(node);
                                }
                            }
                            parentNode = node;
                            if (i == path.Length - 1)
                            {
                                var entryNode = new TreeNode(jsonvalue.EntryName);
                                entry_nodes.Add(entryNode);
                                entryNode.Tag = jsonvalue;
                                parentNode.Nodes.Add(entryNode);
                                entryNode.ImageKey = "file";
                                entryNode.SelectedImageKey = "file";
                            }
                        }
                    }
                }
                //ziparch.Dispose();
            }

        }

       
        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            LocEntry loc = e.Node.Tag as LocEntry; 
            if (loc == null) //if loc is null then we move on and dont display anything
            {
                return;
            }
            textBox1.Text = e.Node.FullPath;

            selected_Node = e.Node;

            listView1.Items.Clear();


            foreach (KeyValuePair<WinForms.Language, string> translation in loc.Translations)
            {
                ListViewItem item = new ListViewItem(new string[] { translation.Key.ToString(), translation.Value });
                textBox2.Text = translation.Value;
                listView1.Items.Add(item);
            }

            
        }

        private void tabPage1_Click(object sender, EventArgs e)
        {

        }

        private void listView2_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            listView2.Items.Clear();

            foreach (var entry in entry_nodes)
            {
                LocEntry loc = (LocEntry)entry.Tag;
                if (entry.Text.Contains(textBox3.Text, StringComparison.InvariantCultureIgnoreCase))
                {
                    foreach (KeyValuePair<WinForms.Language, string> translation in loc.Translations)
                    {
                        ListViewItem listviewitem = new ListViewItem(new string[] { loc.ToString(), entry.FullPath, translation.Value });
                        listView2.Items.Add(listviewitem);
                    }
                }

            }
        }
            
        private void button1_KeyPress(object sender, KeyPressEventArgs e)
        {

        }

        private void textBox3_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                button1.PerformClick();        //https://learn.microsoft.com/en-us/dotnet/api/system.windows.forms.keypresseventargs.keychar?redirectedfrom=MSDN&view=windowsdesktop-7.0#System_Windows_Forms_KeyPressEventArgs_KeyChar
                e.Handled = true;
            }
            
            
        }
        

        private void treeView1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
               
                TreeNode clickedNode = treeView1.GetNodeAt(e.X, e.Y);
                if (clickedNode != null)
                {
                    
                    ContextMenuStrip menu = new ContextMenuStrip();
                    string[] itemNames = { "New Group", "New Subgroup", "Delete Node" };
                    Action<TreeNode>[] itemActions = { CreateNewGroup, CreateNewSubgroup, DeleteNode };

                    for (int i = 0; i < itemNames.Length; i++)
                    {
                      
                        int index = i;

                        ToolStripMenuItem menuItem = new ToolStripMenuItem(itemNames[i]);
                        menuItem.Click += (s, args) => itemActions[index](clickedNode);
                        menu.Items.Add(menuItem);
                    }

                    // Show the context menu
                    menu.Show(treeView1, e.Location);
                }
            }
        }

        private void CreateNewGroup(TreeNode node)
        {
            TreeNode newNode = new TreeNode("<New Group>");
            if (node.Parent == null)
            {
                treeView1.Nodes.Insert(node.Index + 1, newNode);
            }
            else
            {
                node.Parent.Nodes.Insert(node.Index + 1, newNode);
            }
            treeView1.SelectedNode = newNode;
            treeView1.LabelEdit = true;
            newNode.BeginEdit();
        }

        private void CreateNewSubgroup(TreeNode node)
        {
            TreeNode newSubNode = new TreeNode("<New Subgroup>");
            node.Nodes.Add(newSubNode);
            treeView1.SelectedNode = newSubNode;
            treeView1.LabelEdit = true;
            newSubNode.BeginEdit();
        }

        private void DeleteNode(TreeNode node)
        {
            node.Remove();
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Zip Files(.ZIP)|.ZIP;";
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                using (ZipArchive outputZip = ZipFile.Open(saveFileDialog.FileName, ZipArchiveMode.Create))
                {

                    foreach (var locEntry in updated_locentries)
                    {
                        var jsonFile = JsonConvert.SerializeObject(locEntry);
                        ZipArchiveEntry outputEntry = outputZip.CreateEntry($"Lockey#{locEntry.LocKey}.json");
                        using (StreamWriter writer = new StreamWriter(outputEntry.Open()))
                        {
                            writer.Write(jsonFile);
                        }
                    }
                }
            }

        }

        TreeNode new_entry = new TreeNode("<New Entry>");
        private void newEntryToolStripMenuItem_Click(object sender, EventArgs e)
        {

            if (treeView1.SelectedNode == null) return;

           
            new_entry.ImageKey = "file";
            new_entry.SelectedImageKey = "file";
            //LocEntry lnew = new LocEntry(new_entry.FullPath,new_entry.Text);

        
        
            treeView1.SelectedNode.Nodes.Add(new_entry);
            treeView1.SelectedNode = new_entry;
  
            textBox1.ReadOnly = false;
            textBox1.Text = new_entry.FullPath.Replace('\\', '-'); ;
            textBox2.Clear();
            string hierarchyPath = treeView1.SelectedNode.FullPath.Replace('\\', '-');
            string entryName = new_entry.Text;
            LocEntry newLocEntry = new LocEntry(hierarchyPath, entryName);
            updated_locentries.Add(newLocEntry);

        }

        private void deleteEntryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            treeView1.SelectedNode.Remove();
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            var locEntry = selected_Node.Tag as LocEntry;
            if (locEntry != null)
            {
                foreach (var l in updated_locentries)
                {
                    if (l == locEntry)
                    {
                        if (listView1.Items.Count > 0)
                        {
                            var language = (Language)Enum.Parse(typeof(Language), listView1.Items[0].SubItems[0].Text);
                            if (locEntry.Translations.ContainsKey(language))
                            {
                                var newValue = textBox2.Text;
                                var oldValue = locEntry.Translations[language];
                                if (newValue != oldValue)
                                {
                                    l.Translations[language] = newValue;
                                    listView1.Items[0].SubItems[1].Text = newValue;
                                }
                            }
                        }
                    }
                }
            }
        }

       
        private void textBox1_Leave(object sender, EventArgs e)
        {
            string[] fullpath = textBox1.Text.Split("-");
            new_entry.Text = fullpath[fullpath.Length - 1];
            textBox1.ReadOnly = true;
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
