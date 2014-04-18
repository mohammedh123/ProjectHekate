using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SFML.Graphics;
using SFML.Window;

namespace ProjectHekate.GUI.DrawingHelpers
{
    static class LineDrawer
    {
        private static readonly VertexArray VertexArray = new VertexArray(PrimitiveType.Lines, 2);

        public static void Draw(float x1, float y1, float x2, float y2, RenderWindow window)
        {
            VertexArray[0] = new Vertex(new Vector2f(x1, y1));
            VertexArray[1] = new Vertex(new Vector2f(x2, y2));

            window.Draw(VertexArray);
        }
    }
}
