using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using System;
namespace Taipu.UI
{
    public class Slider : Element
    {
        public Color sliderBgColor = Color.Gray;
        public Color sliderOutColor = Color.Transparent;
        public Texture2D sliderPointTex;
        public Sprite sliderPoint;
        public float pointScaleDrag = 0.095f;
        public float pointScaleUndrag = 0.075f;
        public float pointScaleCurrent = 0f;
        public bool dragging = false;
        public double bottomRange = 0;
        public double upperRange = 1;
        public double value = 0;
        public double prevValue = 0;
        public double valueLinear = 0;
        public double prevValueLinear = 0;

        public Slider(Vector2 position, Vector2 size)
        {
            
            this.size = size;
            sliderPointTex = SkinLoader.getTexture("keysq_main.png");
            sliderPoint = new(sliderPointTex,new Vector2(localPosition.X, localPosition.Y+size.Y / 2f));
            sliderPoint.origin = sliderPoint.centerOrigin;
            pointScaleCurrent = pointScaleUndrag;
            this.localPosition = position;
            sliderPoint.scale = new Vector2(pointScaleCurrent);
        }
        protected override void OnUpdate(GameTime gameTime)
        {
            if ((valueLinear != prevValueLinear) && (value==prevValue))
            {
                value = bottomRange + (valueLinear * (upperRange - bottomRange));
            }
            if ((value != prevValue) && (valueLinear == prevValueLinear))
            {
                valueLinear = (value - bottomRange) / (upperRange-bottomRange);
            }
                if (MouseMan.LeftJustPressed() && !dragging)
            {
                if (sliderPoint.rect.Contains(MouseMan.mousePos) || absoluteRect.Contains(MouseMan.mousePos))
                {
                    dragging = true;
                    pointScaleCurrent = pointScaleDrag;
                }
            }
            if(MouseMan.LeftReleased() && dragging)
            {
                dragging = false;
                pointScaleCurrent = pointScaleUndrag;
            }
            if(dragging)
            {
                
                valueLinear = Math.Clamp(((MouseMan.mousePos.X - absolutePosition.X) / scaledSize.X), 0, 1);
                value = bottomRange + (valueLinear * (upperRange-bottomRange));
            }
            sliderPoint.scale = Vector2.Lerp(sliderPoint.scale,new Vector2(pointScaleCurrent) * absoluteScale, 16f * (float)Global.deltaTime);
            sliderPoint.position.X = float.Lerp(sliderPoint.position.X, (float)(absolutePosition.X+(valueLinear * scaledSize.X)), 32f * (float)Global.deltaTime);
            sliderPoint.position.Y = absolutePosition.Y + (size.Y / 2f);
            prevValue = value;
            prevValueLinear = valueLinear;
        }
        protected override void OnDraw(SpriteBatch spriteBatch)
        {
            Global.spriteBatch.FillRectangle(absoluteRect, sliderBgColor);
            Global.spriteBatch.DrawRectangle(absoluteRect, sliderOutColor);
            sliderPoint.Draw();
        }
    }
}
