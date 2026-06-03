using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Taipu.VFX
{
    public class BgScroll
    {
        public Texture2D tileTex;
        public Vector2 scale;
        public Vector2 offset;
        public int hSize;
        public int vSize;
        public Color color;
        public Sprite tile;
        public Vector2 position;
        public float currentYOff = 0;
        public float speed = 0;
        public BgScroll(Texture2D texture, Vector2 position, Vector2 scale, Vector2 offset, int hsize, int vsize)
        {
            this.position = position;
            this.hSize = hsize;
            this.vSize = vsize;
            this.tileTex = texture;
            this.scale = scale;
            this.offset = offset;
            tile = new(tileTex, Vector2.Zero);
            tile.scale = scale;
            color = Color.White;
            Update();
        }
        
        public void Update()
        {
            currentYOff += speed*(float)Global.deltaTime;
            if (currentYOff >= offset.Y)
            {
                currentYOff %= offset.Y;
            }
            tile.scale = this.scale;
            tile.color = this.color;
        }
        public void Draw(SpriteBatch spriteBatch)
        {
            for (int i = 0; i < hSize; i++)
            {
                for (int j = 0; j<vSize;  j++)
                {
                    tile.DrawAt(new Vector2(offset.X*i,((2*(i%2)-1)*currentYOff) + (j*offset.Y)) + position);
                }
            }
        }
    }
}
