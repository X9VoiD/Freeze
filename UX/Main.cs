using Eto.Drawing;
using Eto.Forms;
using Freeze.Library;

namespace Freeze.UX
{
    public class Main : Form
    {
        public Main()
        {
            this.Title = "Freeze";
            this.Size = new Size(1000, 600);

            this.status = new Label
            {
                Text = FreezeTools.ConvertToStatus(UWF.Active),
                VerticalAlignment = VerticalAlignment.Center,
                Size = new Size(-1, 20)
            };

            this.current = new ListBox
            {
                DataStore = UWF.CurrentSettings,
                Size = new Size(500 - 15, -1)
            };

            this.after = new ListBox
            {
                DataStore = UWF.NextSettings
            };

            var filterCmd = new Command();
            filterCmd.Executed += async (_, _) =>
            {
                if (UWF.Enabled)
                    await UWF.Disable();
                else
                    await UWF.Enable();

                await UWF.RefreshStatus();
            };

            var protectCmd = new Command();
            protectCmd.Executed += async (_, _) =>
            {
                await UWF.Protect();
                await UWF.RefreshStatus();
            };

            var excludeCmd = new Command();
            excludeCmd.Executed += (_, _) => new Exclusion().Show();

            this.filterButton = new Button
            {
                Command = filterCmd,
                Text = UWF.Active ? "Disable" : "Enable"
            };

            this.protectButton = new Button
            {
                Command = protectCmd,
                Text = "Protect"
            };

            this.excludeButton = new Button
            {
                Command = excludeCmd,
                Text = "Exclusions"
            };

            this.verticalSplit = new Splitter
            {
                Panel1 = current,
                Panel2 = after,
                Orientation = Orientation.Horizontal
            };

            var buttons = new StackLayout
            {
                Items =
                {
                    new StackLayoutItem(new Panel { Content = excludeButton, Padding = new Padding(5, 5, 0, 0) }),
                    new StackLayoutItem(new Panel { Content = protectButton, Padding = new Padding(5, 5, 0, 0) }),
                    new StackLayoutItem(new Panel { Content = filterButton, Padding = new Padding(5, 5, 0, 0) }),
                },
                Orientation = Orientation.Horizontal
            };

            this.split = new StackLayout
            {
                Items =
                {
                    new StackLayoutItem(status, HorizontalAlignment.Center),
                    new StackLayoutItem(verticalSplit, HorizontalAlignment.Stretch, true),
                    new StackLayoutItem(buttons, HorizontalAlignment.Right)
                }
            };

            this.main = new Panel
            {
                Content = split,
                Padding = 5
            };

            this.Content = main;

            UWF.PropertyChanged += (_, p) =>
            {
                this.status.Text = FreezeTools.ConvertToStatus(UWF.Active);
                this.filterButton.Text = UWF.Enabled ? "Disable" : "Enable";
            };
        }

        public static readonly UWF UWF = new UWF();
        private readonly Panel main;
        private readonly StackLayout split;
        private readonly Splitter verticalSplit;
        private readonly Label status;
        private readonly ListBox current, after;
        private readonly Button filterButton, protectButton, excludeButton;
    }
}