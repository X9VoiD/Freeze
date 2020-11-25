using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Eto.Drawing;
using Eto.Forms;

namespace Freeze.UX
{
    public class Exclusion : Form
    {
        public Exclusion()
        {
            this.Title = "Exclusion";
            this.Size = new Size(600, 400);

            this.current = new ListBox
            {
                DataStore = Main.UWF.CurrentExclusion,
                Size = new Size(300 - 15, -1),
            };

            this.next = new ListBox
            {
                DataStore = Main.UWF.NextExclusion
            };
            next.SelectedIndexChanged += ValidateMenu;

            this.verticalSplit = new Splitter
            {
                Panel1 = current,
                Panel2 = next,
                Orientation = Orientation.Horizontal
            };

            var addFileCmd = new Command();
            addFileCmd.Executed += async (_, _) => await AddFileExclusion();
            this.addFile = new Button
            {
                Command = addFileCmd,
                Text = "Add File"
            };

            var addFolderCmd = new Command();
            addFolderCmd.Executed += async (_, _) => await AddFolderExclusion();
            this.addFolder = new Button
            {
                Command = addFolderCmd,
                Text = "Add Folder"
            };

            var removeCmd = new Command { MenuText = "Remove..." };
            removeCmd.Executed += async (_, _) => await RemoveExclusion();

            this.contextMenu = new ContextMenu(removeCmd);


            var buttonStack = new StackLayout
            {
                Items =
                {
                    new StackLayoutItem(new Panel { Content = addFile, Padding = new Padding(5, 5, 0, 0) }),
                    new StackLayoutItem(new Panel { Content = addFolder, Padding = new Padding(5, 5, 0, 0) })
                },
                Orientation = Orientation.Horizontal
            };

            this.split = new StackLayout
            {
                Items =
                {
                    new StackLayoutItem(verticalSplit, HorizontalAlignment.Stretch, true),
                    new StackLayoutItem(buttonStack, HorizontalAlignment.Right)
                }
            };

            this.main = new Panel
            {
                Content = split,
                AllowDrop = true,
                Padding = 5,
            };

            this.Content = main;
        }

        private async Task AddFolderExclusion()
        {
            var dialog = new SelectFolderDialog
            {
                Title = "Select Folder"
            };
            var result = dialog.ShowDialog(this);
            if (result == DialogResult.Yes || result == DialogResult.Ok)
            {
                await Main.UWF.AddExclusion(dialog.Directory);
            }
            await Main.UWF.RefreshStatus();
        }

        private async Task AddFileExclusion()
        {
            var dialog = new OpenFileDialog
            {
                MultiSelect = true,
                Title = "Select File/s"
            };
            var result = dialog.ShowDialog(this);
            if (result == DialogResult.Yes || result == DialogResult.Ok)
            {
                foreach (var filename in dialog.Filenames)
                    await Main.UWF.AddExclusion(filename);
            }
            await Main.UWF.RefreshStatus();
        }

        private void ValidateMenu(object _, EventArgs args)
        {
            var index = this.next.SelectedIndex;
            if (index < 0)
            {
                this.next.ContextMenu = null;
                return;
            }
            var target = Regex.Match(Main.UWF.NextExclusion[index], "^\\s*(.+)$").Groups[1].Value;
            if (File.Exists(target) || Directory.Exists(target))
                this.next.ContextMenu = contextMenu;
            else
                this.next.ContextMenu = null;
        }

        private async Task RemoveExclusion()
        {
            var index = this.next.SelectedIndex;
            var target = Regex.Match(Main.UWF.NextExclusion[index], "^\\s*(.+)$").Groups[1].Value;
            await Main.UWF.RemoveExclusion(target);
            await Main.UWF.RefreshStatus();
        }

        private readonly Panel main;
        private readonly StackLayout split;
        private readonly Splitter verticalSplit;
        private readonly ListBox current, next;
        private readonly Button addFolder, addFile;
        private readonly ContextMenu contextMenu;
    }
}