using MonoGame.Extended.BitmapFonts;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
namespace Taipu.Editor.Tabs
{
    public class Main
    {
        Editor.EditorScene root = null;
        Editor.EditorScene.EditorTabs me = Editor.EditorScene.EditorTabs.Main;
        public List<KeyObject> renderKeys;
        public Sprite background;
        public Texture2D bgTex;
        KeyboardBg keyboard;
        public BitmapFont font;
        public UI.Label bpmLabel;
        public Editor.bottomBar bottomBar = new();
        public Editor.beatsnapBar beatsnapBar = new();
        public float scrollFactor = 1.0f;
        public Main(Editor.EditorScene root)
        {
            this.root = root;
            font = SkinLoader.getFont("fonts/main/main.fnt");
            keyboard = new();
            //keyboard.editor = this;
            bpmLabel = new(new Vector2(120,110),"!!! Set the BPM in Audio menu to edit",font);
            bpmLabel.textScale = Vector2.One / 4f;
            renderKeys = new();
            bottomBar.localPosition = new Vector2(0, 300);
        }
        public void Update(GameTime gameTime) 
        {
            if (root?.currentTab == me)
            bpmLabel.Update(gameTime);
            beatsnapBar.Update(gameTime);
            root.beatSnapDivisor = beatsnapBar.beatSnap;
            bottomBar.localPosition = Vector2.Lerp(bottomBar.localPosition, Vector2.Zero, 8f * (float)Global.deltaTime);
            bottomBar.Update(Global.gameTime);
            if (bottomBar.timeSlider.upperRange != root.music.streamLength)
            {
                bottomBar.timeSlider.upperRange = root.music.streamLength;
            }
            bottomBar.timeSlider.bottomRange = 0;
            if (!bottomBar.timeSlider.dragging)
            {
                bottomBar.timeSlider.value = root.time;
            }
            else
            {
                root.music.Seek(bottomBar.timeSlider.value);
            }
            keyboard.Update();
            foreach (String[] key in root.level.keys)
            {
                if (root.time+10 < double.Parse(key[0]) - root.level.preRingTime - root.level.ringTime)
                {
                    break;
                }
                if ((root.time > double.Parse(key[0]) - root.level.preRingTime - root.level.ringTime) && (root.time < double.Parse(key[0]) + root.level.hitTimeframe + root.level.disappearTime))
                {
                    bool foundFlag = false;
                    foreach (KeyObject renderKey in renderKeys)
                    {
                        if (renderKey.keyLink == key)
                        {
                            foundFlag = true;
                            break;
                        }
                    }
                    if (!foundFlag)
                    {
                        KeyObject tempKey = new(keyboard.getPosition(char.Parse(key[1])), keyboard.scale);
                        tempKey.keyLink = key;
                        tempKey.keyText = key[1].ToString();
                        tempKey.spawnStamp = double.Parse(key[0]) - root.level.preRingTime - root.level.ringTime;
                        renderKeys.Add(tempKey);
                    }
                }
            }
            for (int i = renderKeys.Count - 1; i >= 0; i--)
            {
                KeyObject key = renderKeys[i];
                
                if (!root.level.keys.Contains(key.keyLink))
                {
                    renderKeys.RemoveAt(i);
                    continue;
                }
                
                if (((key.keyTime > root.level.preRingTime + root.level.ringTime + root.level.hitTimeframe + root.level.disappearTime) || (key.keyTime < 0)) && !key.visible)
                {
                    renderKeys.RemoveAt(i);
                }
                else
                {
                    key?.Update();
                }
            }
            if (root.music.state == ManagedBass.PlaybackState.Playing)
            {
                root.paused = false;
                if (!bottomBar.pauseBtn.JustToggled())
                {
                    bottomBar.pauseBtn.SetToggle(false);
                }

            }
            else
            {
                root.paused = true;
                if (!bottomBar.pauseBtn.JustToggled())
                {
                    bottomBar.pauseBtn.SetToggle(true);
                }
            }


            if (MouseMan.RightJustPressed())
            {
                foreach (KeyObject key in renderKeys)
                {
                    if (key != null)
                    {
                        if (key.visible && key.collRect.Contains(MouseMan.mousePos))
                        {
                            root.level.keys.Remove(key.keyLink);
                            break;

                        }
                    }
                }
            }

            

            if (KeyboardMan.Down(Keys.LeftShift) && KeyboardMan.JustPressed(Keys.Delete))
            {
                root.level.keys.Clear();
                renderKeys.Clear();
            }

            bottomBar.timerText = TimeSpan.FromSeconds(root.time).ToString(@"mm\:ss\.ff");
            foreach (Keys key in KeyboardMan.currentKeyboard.GetPressedKeys())
            {
                char keypressed = '\0';
                if (KeyboardMan.JustPressed(key))
                {
                    if (key >= Keys.A && key <= Keys.Z)
                    {
                        keypressed = (char)key;
                    }
                    if (key >= Keys.D0 && key <= Keys.D9)
                    {
                        keypressed = (char)key;
                    }
                    if (keypressed != '\0')
                    {
                        if (root.level.bpm > 0)
                        {
                            root.CreateKey(keypressed);
                        }

                    }
                }

            }

        }
        public void Draw(SpriteBatch spriteBatch) 
        {
            background?.Draw();
            beatsnapBar.Draw(Global.spriteBatch);
            bottomBar.Draw(Global.spriteBatch);
            keyboard.Draw();
            foreach (KeyObject key in renderKeys)
            {
                if (key != null)
                {
                    if (key.visible)
                    {
                        key.Blur.Draw();
                    }
                }

            }
            foreach (KeyObject key in renderKeys)
            {
                if (key != null)
                {
                    if (key.visible)
                    {
                        key.KeyMain.Draw();
                        key.DrawText();
                    }
                }

            }
            foreach (KeyObject key in renderKeys)
            {
                if (key != null)
                {
                    if (key.visible)
                    {
                        key.Outline.Draw();
                    }
                }

            }
            foreach (KeyObject key in renderKeys)
            {
                if (key != null)
                {
                    if (key.visible)
                    {
                        key.HitRank.Draw();
                    }
                }

            }
            if (root.level.bpm==0)
            {
                bpmLabel.Draw(Global.spriteBatch);
            }
            //timel.Draw(level.keys);
            //textbox.Draw();
        }
    }
}
