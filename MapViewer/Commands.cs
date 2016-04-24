using System.Windows.Input;

namespace MapViewer {
	public static class CustomCommands {

		public static readonly RoutedUICommand Fireball = new RoutedUICommand("Fireball", "Fireball", typeof(CustomCommands), null);
		public static readonly RoutedUICommand Moonbeam = new RoutedUICommand("Moonbeam", "Moonbeam", typeof(CustomCommands), null);
		public static readonly RoutedUICommand Wall = new RoutedUICommand("Wall", "Wall", typeof(CustomCommands), null);
		public static readonly RoutedUICommand Measure = new RoutedUICommand("Measure", "Measure", typeof(CustomCommands), null);

	}
}
