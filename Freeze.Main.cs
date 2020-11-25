using System;
using Eto.Forms;
using Freeze.Library;
using Freeze.UX;

namespace Freeze
{
    class FreezeMain
    {
        [STAThread]
        static void Main(string[] _)
        {
            new Application(Eto.Platforms.Wpf).Run(new Main());
        }
    }
}
