using System.Windows.Input;

namespace MapViewer.Tools {

    

    public abstract class  CanvasTool {
        public abstract void MouseDown(object sender, MouseButtonEventArgs e);

        public abstract void MouseMove(object sender, MouseEventArgs e);

        public virtual void MouseUp(object sender, MouseButtonEventArgs e) { }

        public virtual void KeyDown(object sender, KeyEventArgs e) { }

        public abstract void Deactivate();

        public virtual bool ShowPublicCursor() {  return false;  }

        protected const double MinimumMoveScreenPixel = 10;

    }



}

