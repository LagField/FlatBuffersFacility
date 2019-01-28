using System;
using Eto.Drawing;
using Eto.Forms;

namespace FlatBuffersFacility
{
    public class MainForm : Form
    {
        public MainForm()
        {
            Title = "FlatBuffersFacilityGenerator";
            ClientSize = new Size(800, 400);
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            Content = ConstructLayout();
        }

        private DynamicLayout ConstructLayout()
        {
            DynamicLayout layout = new DynamicLayout();

            layout.BeginVertical (); // create a fields section
            layout.BeginHorizontal ();
            layout.Add (new Label { Text = "Field 1" });
            layout.Add (new TextBox (), true); // true == scale horizontally
            layout.EndHorizontal ();

            layout.BeginHorizontal ();
            layout.Add (new Label { Text = "Field 2" });
            layout.Add (new ComboBox (), true);
            layout.EndHorizontal ();
            layout.EndVertical ();

            layout.BeginVertical (); // buttons section
            layout.BeginHorizontal ();
            layout.Add (null, true); // add a blank space scaled horizontally to fill space
            layout.Add (new Button { Text = "Cancel" });
            layout.Add (new Button { Text = "Ok" });
            layout.EndHorizontal ();
            layout.EndVertical ();

            return layout;
        }
    }
}