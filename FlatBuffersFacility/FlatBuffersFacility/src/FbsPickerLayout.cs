using Eto.Drawing;
using Eto.Forms;

namespace FlatBuffersFacility
{
    public class FbsPickerLayout
    {
        public DynamicLayout ConstructLayout()
        {
            DynamicLayout layout = new DynamicLayout();

            layout.BeginVertical(new Padding(0, 10, 0, 10));


            layout.EndVertical();

            return layout;
        }

        private TableLayout ConstructScrollLayout()
        {
            TableLayout scrollLayout = new TableLayout();

            Scrollable scrollable = new Scrollable {Height = 300};

            DynamicLayout scrollContentLayout = new DynamicLayout();
            

            return scrollLayout;
        }
    }
}