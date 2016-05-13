using System.Windows.Input;

namespace MapViewer {
	public interface ICanvasTool {
		void Activate();
		void MouseDown(object sender, MouseButtonEventArgs e);
		void MouseUp(object sender, MouseButtonEventArgs e);
		void MouseMove(object sender, MouseEventArgs e);
		void KeyDown(object sender, KeyEventArgs e);
		void Deactivate();
	}
}
