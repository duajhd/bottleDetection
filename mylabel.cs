using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
namespace bottleDetection
{
    public class myLabel:Label


    {
        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            base.BackColor = Color.FromArgb(255, 219, 73);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            base.BackColor =Color.Transparent;
        }



    }
}
