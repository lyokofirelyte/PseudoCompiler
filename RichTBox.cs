using System.Windows.Forms;
using System.Drawing;

namespace PCGUI
{
    public partial class RichTBox : RichTextBox
    {
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams CP = base.CreateParams;
                CP.ExStyle |= 0x20;
                return CP;
            }
        }
    }

    public partial class TranslucentPanel : Panel
    {
        public TranslucentPanel()
        {
            SetStyle(ControlStyles.SupportsTransparentBackColor |
                 ControlStyles.OptimizedDoubleBuffer |
                 ControlStyles.AllPaintingInWmPaint |
                 ControlStyles.ResizeRedraw |
                 ControlStyles.UserPaint, true);
            BackColor = Color.Transparent;
        }
    }
}