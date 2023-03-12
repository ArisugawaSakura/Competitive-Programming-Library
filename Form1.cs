using Microsoft.VisualBasic;
using System.Collections;
using System.Diagnostics;

namespace CPCM
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            LoadConfig();
        }

        private void treeView1_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                this.New.Visible = true;
                if (e.Node.Nodes.Count != 0)
                {
                    this.Dependence.Visible = true;
                    this.Edit.Visible = true;
                }
                else
                {
                    this.Dependence.Visible = false;
                    this.Edit.Visible = false;
                }
                this.Rename.Visible = true;
                this.Delete.Visible = true;
                if (e.Node.Parent != null)
                {
                    this.Up.Visible = true;
                    this.Down.Visible = true;
                }
                else
                {
                    this.Up.Visible = false;
                    this.Down.Visible = false;
                }
                this.treeView1.SelectedNode = e.Node;
            }
        }

        private void treeView1_AfterLabelEdit(object sender, NodeLabelEditEventArgs e)
        {
            if (e.Label == null)
            {
                e.CancelEdit = true;
                return;
            }
            if (names.Contains(e.Label))
            {
                MessageBox.Show("This name has already in use.");
                e.CancelEdit = true;
                return;
            }
            else
            {
                names.Remove(this.treeView1.SelectedNode.Text);
                names.Add(e.Label);
            }
            RenameDependences(this.treeView1.Nodes[0], this.treeView1.SelectedNode.Text, e.Label);
            this.treeView1.SelectedNode.Text = e.Label;
            Export();
        }

        private void RenameDependences(TreeNode t, String d, String nd)
        {
            for (Int32 i = 0; i < ((Node)t.Tag).dependences.Count; i++)
            {
                String s = (String)(((Node)t.Tag).dependences[i]);
                if (s.Equals(d)) ((Node)t.Tag).dependences[i] = nd;
            }
            foreach (TreeNode node in t.Nodes)
            {
                RenameDependences(node, d, nd);
            }
        }

        private void New_Click(object sender, EventArgs e)
        {
            TreeNode node = new TreeNode();
            node.Name = "New Node";
            node.Text = "New Node";
            node.Tag = new Node();
            this.treeView1.SelectedNode.Nodes.Add(node);
        }

        private void Dependence_Click(object sender, EventArgs e)
        {
            String placeholder = "";
            foreach (String d in ((Node)this.treeView1.SelectedNode.Tag).dependences)
            {
                placeholder += ";" + d;
            }
            String[] dependences = Interaction.InputBox("Please input name of the dependences, split with \';\':", "dependences", placeholder, -1, -1).Split(new Char[] { ';' });
            ((Node)this.treeView1.SelectedNode.Tag).dependences = ArrayList.Adapter(dependences);
            Export();
        }

        private void Edit_Click(object sender, EventArgs e)
        {
            String file = Environment.CurrentDirectory + @"\" + this.treeView1.SelectedNode.Name + @".cpp";
            if (!File.Exists(file)) File.Create(file);
            Process.Start(@"notepad.exe ", file);
        }

        private void Rename_Click(object sender, EventArgs e)
        {
            this.treeView1.SelectedNode.BeginEdit();
        }

        private void Delete_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to delete this node?", "confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.No) return;
            this.treeView1.Nodes.Remove(this.treeView1.SelectedNode);
            Export();
        }
        private Node[] nodes;
        private ArrayList copied = new ArrayList();
        private ArrayList names = new ArrayList();
        private Int32 n, m;
        private String code, cfg;
        private void LoadConfig()
        {
            try
            {
                StreamReader streamReader = new StreamReader(@"config.txt");
                String line;
                n = 0;
                nodes = new Node[65536];
                nodes[0] = new Node();
                Int32 i;
                while ((line = streamReader.ReadLine()) != null)
                {
                    n++;
                    nodes[n] = new Node();

                    Int32 f = Convert.ToInt32((String)line);
                    nodes[n].f = f;
                    nodes[f].subs.Add(n);

                    nodes[n].name = streamReader.ReadLine();

                    line = streamReader.ReadLine();
                    Int32 m = Convert.ToInt32((String)line);
                    for (i = 0; i < m; i++)
                    {
                        line = streamReader.ReadLine();
                        nodes[n].dependences.Add(line);
                    }
                }
                streamReader.Close();
                this.treeView1.Nodes[0].Tag = nodes[0];
                names.Add("Root");
                PreloadTreeNode(this.treeView1.Nodes[0], 0);
            }
            catch
            {
                return;
            }
        }
        private void PreloadTreeNode(TreeNode t, Int32 u)
        {
            foreach (Int32 i in nodes[u].subs)
            {
                TreeNode node = new TreeNode();
                node.Name = nodes[i].name;
                node.Text = nodes[i].name;
                node.Tag = nodes[i];
                names.Add(node.Name);
                Int32 v = t.Nodes.Add(node);
                PreloadTreeNode(t.Nodes[v], i);
            }
        }

        private void Export(TreeNode u, Int32 f)
        {
            Int32 k = m;
            if (k != 0)
            {
                cfg += Convert.ToString(f) + '\n';
                cfg += u.Text + '\n';
                cfg += Convert.ToString(((Node)u.Tag).dependences.Count) + '\n';
                foreach (String d in ((Node)u.Tag).dependences) cfg += d + '\n';
            }
            m++;
            foreach (TreeNode v in u.Nodes) Export(v, k);
        }

        private void Export() 
        {
            m = 0;
            cfg = "";
            Export(this.treeView1.Nodes[0], -1);
            File.WriteAllText(@"config.txt", cfg);
        }

        private TreeNode GetTreeNodeByText(String text)
        {
            Queue<TreeNode> queue = new Queue<TreeNode>();
            queue.Enqueue(this.treeView1.Nodes[0]);
            while (queue.Count > 0)
            {
                TreeNode u = queue.Dequeue();
                if (u.Text == text) return u;
                foreach (TreeNode v in u.Nodes) queue.Enqueue(v);
            }
            return null;
        }

        private String GetCodeByTreeNode(TreeNode u)
        {
            String ret = "";
            try
            {
                StreamReader streamReader = new StreamReader(u.Text + @".cpp");
                if (streamReader == null) return ret;
                ret = streamReader.ReadToEnd();
                streamReader.Close();
            }
            catch
            {
                return "";
            }
            return ret;
        }

        private void RecursivelyAdd(String text)
        {
            if (copied.Contains(text)) return;
            copied.Add(text);
            TreeNode u = GetTreeNodeByText(text);
            if (u != null)
            {
                foreach (String d in ((Node)u.Tag).dependences) RecursivelyAdd(d);
                code += GetCodeByTreeNode(u) + "\n";
            }
        }

        private void RecursivelyAdd(TreeNode u)
        {
            if (u.Nodes.Count > 0)
            {
                foreach (TreeNode v in u.Nodes)
                {
                    RecursivelyAdd(v);
                }
            }
            if (u.Checked) RecursivelyAdd(u.Text);
        }

        private void RecursivelyUncheck(TreeNode u)
        {
            u.Checked = false;
            foreach (TreeNode v in u.Nodes) RecursivelyUncheck(v);
        }

        private void Up_Click(object sender, EventArgs e)
        {
            TreeNode u = this.treeView1.SelectedNode.PrevNode;
            TreeNode v = this.treeView1.SelectedNode;
            TreeNode nu = (TreeNode)v.Clone();
            TreeNode nv = (TreeNode)u.Clone();
            TreeNode f = this.treeView1.SelectedNode.Parent;
            if (u != null)
            {
                f.Nodes.Insert(u.Index, nu);
                f.Nodes.Insert(v.Index, nv);
                f.Nodes.Remove(u);
                f.Nodes.Remove(v);
                this.treeView1.SelectedNode = nu;
            }
            Export();
        }

        private void Down_Click(object sender, EventArgs e)
        {
            TreeNode u = this.treeView1.SelectedNode;
            TreeNode v = this.treeView1.SelectedNode.NextNode;
            TreeNode nu = (TreeNode)v.Clone();
            TreeNode nv = (TreeNode)u.Clone();
            TreeNode f = this.treeView1.SelectedNode.Parent;
            if (v != null)
            {
                f.Nodes.Insert(u.Index, nu);
                f.Nodes.Insert(v.Index, nv);
                f.Nodes.Remove(u);
                f.Nodes.Remove(v);
                this.treeView1.SelectedNode = nu;
            }
            Export();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            copied = new ArrayList();
            code = "";
            foreach (TreeNode t in treeView1.Nodes[0].Nodes) RecursivelyAdd(t);
            if (code != "") Clipboard.SetText(code);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            foreach (TreeNode t in treeView1.Nodes[0].Nodes) RecursivelyUncheck(t);
        }
    }
    class Node
    {
        public Int32 f = 0;
        public String name = "";
        public ArrayList subs = new ArrayList();
        public ArrayList dependences = new ArrayList();
    }
}