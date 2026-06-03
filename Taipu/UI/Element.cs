using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
namespace Taipu.UI
{
    public class Element
    {
        public UI.Element parent;
        public List<UI.Element> children = new();

        public Vector2 localPosition = Vector2.Zero;
        public Vector2 absolutePosition = Vector2.Zero;

        public Vector2 localScale = Vector2.One;
        public Vector2 absoluteScale = Vector2.One;

        public Vector2 size = Vector2.One;
        public Vector2 origin = Vector2.Zero;
        public float rotation = 0f;

        public Vector2 centerOrigin => size / 2f;
        public Vector2 scaledSize => size * absoluteScale;
        public Vector2 scaledOrigin => origin * absoluteScale;

        public bool visible = true;

        public RectangleF absoluteRect => new RectangleF(absolutePosition - scaledOrigin, scaledSize);
        public RectangleF localRect => new RectangleF(localPosition - (origin * localScale), size * localScale);

        public void AddChild(UI.Element child)
        {
            if (child.parent != null)
            {
                child.parent.RemoveChild(child);
            }
            child.parent = this;
            children.Add(child);
            child.RecalculateTransform();
        }
        public void AddChildren(UI.Element[] children)
        {
            foreach (UI.Element child in children) {
                if (child.parent != null)
                {
                    child.parent.RemoveChild(child);
                }
                child.parent = this;
                this.children.Add(child);
            }
        }
        public void RemoveChild(UI.Element child)
        {
            children.Remove(child);
            child.parent = null;
        }
        public void RecalculateTransform()
        {
            if (parent != null)
            {
                absoluteScale = parent.absoluteScale * localScale;
                absolutePosition = parent.absolutePosition + (localPosition * parent.absoluteScale);
            }
            else
            {
                absolutePosition = localPosition;
                absoluteScale = localScale;
            }
            foreach (UI.Element child in children)
            {
                child.RecalculateTransform();
            }
        }

        public virtual void Update(GameTime gameTime)
        {
            RecalculateTransform();
            OnUpdate(gameTime);
            foreach (UI.Element child in children)
            {
                child.Update(gameTime);
            }
        }
        protected virtual void OnUpdate(GameTime gameTime) { }
        public virtual void Draw(SpriteBatch spriteBatch)
        {
            if (visible)
            {
                OnDraw(spriteBatch);
                foreach (UI.Element child in children)
                {
                    child.Draw(spriteBatch);
                }
            }
        }
        protected virtual void OnDraw(SpriteBatch spriteBatch) { }
    }
}
