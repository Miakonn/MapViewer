using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MapViewer {

    public interface ISymbol {
        Point Position { get; set; }
        string Uid { get; set; }
        string Text { get; set; }
        Color Color { get; set; }
        int Layer { get; set; }
        double Size { get; set; }

        void CreateElements();
    }



    public class PlayerSymbol : ISymbol {
        public Point Position { get; set; }
        public string Uid { get; set; }
        public string Text { get; set; }
        public Color Color { get; set; }
        public int Layer { get; set; }
        public double Size { get; set; }

        public void CreateElements()
        {
            throw new System.NotImplementedException();
        }
    }
}
