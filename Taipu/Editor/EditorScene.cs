using ManagedBass;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using NativeFileDialogSharp;
using System;
using System.IO;
using System.Text.Json;
using Taipu.Scenes.MainMenu;

namespace Taipu.Editor
{
    public class EditorScene : Scene
    {
        public enum EditorTabs
        {
            None,
            MetaEditor = 1,
            Main = 2,
            Audio = 3,
            Export = 4,
        }
        public EditorTabs currentTab = EditorTabs.None;
        public Editor.Tabs.Main mainTab;
        public Editor.Tabs.MetaEditor metaTab;
        public Editor.Tabs.Export exportTab;
        public Editor.Tabs.Audio audioTab;

        public JukeboxSynced music;
        public TaipuMap level;
        public bool paused;
        public string mapPath;
        public MapLoader loader;
        public double time => music.streamPosition;

        public Sprite background;
        public Texture2D bgTex;
        public bool beatSnapping = true;
        public int beatSnapDivisor = 2;

        public float scrollFactor = 1.0f;

        public BadMetronomeTest metronome;
        public EditorScene(string mapPath)
        {
            while (mapPath == null)
            {
                var openResult = Dialog.FileOpen("taipu");
                if (openResult.IsOk)
                {
                    mapPath = openResult.Path;
                    break;
                }
            }

            mainTab = new(this);
            metaTab = new(this);
            exportTab = new(this);
            audioTab = new(this);
            this.mapPath = mapPath;
            loader = new();
            music = new();
            level = loader.Load(mapPath);
            LoadAudio();
            LoadBackground();


            currentTab = EditorTabs.Main;
            metronome = new(level.bpm, level.beatOffset);
        }
        public void LoadAudio()
        {
            if (File.Exists(Path.Combine(loader.mapFolder, level.audioFile)))
            {
                music.Stop();
                music.LoadStream(Path.Combine(loader.mapFolder, level.audioFile));
            }
        }
        public void LoadBackground()
        {
            if (File.Exists(Path.Combine(loader.mapFolder, level.imageBg)))
            {
                bgTex = ExtContent.getTexture(Path.Combine(loader.mapFolder, level.imageBg));
                background = new(bgTex, Vector2.Zero);
                background.color.A = (byte)90f;
                background.origin = background.centerOrigin;
                background.position = MatrixUpscaler.virtualResolution / 2f;
                background.scale = new Vector2(MatrixUpscaler.virtualResolution.X / background.size.X);
            }
        }
        public void Update()
        {
            if (KeyboardMan.JustPressed(Keys.F5))
            {
                Bass.ChannelSetAttribute(music.tempoStream, ChannelAttribute.Tempo, -90);
            }
            if (KeyboardMan.JustPressed(Keys.F6))
            {
                Bass.ChannelSetAttribute(music.tempoStream, ChannelAttribute.Tempo, -75);
            }
            if (KeyboardMan.JustPressed(Keys.F7))
            {
                Bass.ChannelSetAttribute(music.tempoStream, ChannelAttribute.Tempo, 0);
            }
            if (SceneManager.currentScene != this) {  return; }
            if (currentTab == EditorTabs.Audio)
            {
                metronome.bpm = level.bpm;
                metronome.offset = level.beatOffset;
                metronome.Update(time);
            }
            metaTab.Update(Global.gameTime);
            if (KeyboardMan.JustPressed(Keys.F1))
            {
                currentTab = EditorTabs.MetaEditor;
            }
            if (KeyboardMan.JustPressed(Keys.F2))
            {
                currentTab = EditorTabs.Main;
            }
            if (KeyboardMan.Down(Keys.LeftShift) && KeyboardMan.Down(Keys.Escape))
            {
                if (music.tempoStream != 0)
                {
                    ManagedBass.Bass.ChannelStop(music.tempoStream);
                }

                music.Free();
                music = null;

                SceneManager.LoadScene(new Scenes.MainMenu.TestMainMenu());
                return;
            }
            if (KeyboardMan.JustPressed(Keys.F3))
            {
                currentTab = EditorTabs.Audio;
            }
            if (currentTab != EditorTabs.MetaEditor)
            {
                if (KeyboardMan.JustPressed(Keys.Space) || mainTab.bottomBar.pauseBtn.JustToggled())
                {
                    if (music.state == ManagedBass.PlaybackState.Playing)
                    {
                        music.Stop();
                    }
                    else
                    {
                        music.Start(false);
                    }
                }

                if (KeyboardMan.JustPressed(Keys.F10))
                {
                    var options = new JsonSerializerOptions { IncludeFields = true };
                    string json = JsonSerializer.Serialize(level, typeof(TaipuMap), options);
                    File.Copy(mapPath, mapPath + ".bak", true);
                    File.WriteAllText(mapPath, json);
                }

                if (KeyboardMan.JustPressed(Keys.Home))
                {
                    music.Start(true);
                }
                if (KeyboardMan.Down(Keys.LeftShift) && KeyboardMan.Down(Keys.LeftControl))
                {
                    scrollFactor = 16.0f;
                }
                else if (KeyboardMan.Down(Keys.LeftShift))
                {
                    scrollFactor = 8.0f;
                }
                else if (KeyboardMan.Down(Keys.LeftControl))
                {
                    scrollFactor = 0.5f;
                }
                else
                {
                    scrollFactor = 1.0f;
                }
                if (currentTab == EditorTabs.Main) {
                    if (MouseMan.MWheelUp() || MouseMan.MWheelDown())
                    {
                        scrollFactor = scrollFactor * 2;
                    }
                    if (KeyboardMan.Down(Keys.Left) || MouseMan.MWheelUp())
                    {
                        music.Seek(music.streamPosition - (0.05 * scrollFactor));
                    }
                    if (KeyboardMan.Down(Keys.Right) || MouseMan.MWheelDown())
                    {
                        music.Seek(music.streamPosition + (0.05 * scrollFactor));
                    }
                }
            }
            switch (currentTab)
            {

                case EditorTabs.Main:
                    mainTab.Update(Global.gameTime); return;
                case EditorTabs.Audio:
                    audioTab.Update(Global.gameTime); return;
                case EditorTabs.Export:
                    exportTab.Update(Global.gameTime); return;
                case EditorTabs.MetaEditor:
                    metaTab.Update(Global.gameTime); return;
            }

        }
        public double CreateKeyTime(double time)
        {
            if (beatSnapping)
            {
                return Audio.BeatSnap.toDivisor(time, level.bpm, beatSnapDivisor);
            }
            else
            {
                return time;
            }
        }
        public void ResnapAll(int divisor)
        {
            foreach (string[] key in level.keys)
            {
                key[0] = Audio.BeatSnap.toDivisor(Convert.ToDouble(key[0]), level.bpm, divisor).ToString();
            }
        }
        public void CreateKey(char keypressed)
        {
            string[] arrtemp = [Math.Round(CreateKeyTime(time), 3).ToString(), keypressed.ToString()];
            int insPos = 0;
            while (insPos < level.keys.Count && double.Parse(level.keys[insPos][0]) < time)
            {
                insPos++;
            }
            level.keys.Insert(insPos, arrtemp);
        }
        public void Draw()
        {
            background?.Draw();
            switch (currentTab)
            {
                case EditorTabs.Main:
                    mainTab.Draw(Global.spriteBatch); return;
                case EditorTabs.Audio:
                    audioTab.Draw(Global.spriteBatch); return;
                case EditorTabs.Export:
                    exportTab.Draw(Global.spriteBatch); return;
                case EditorTabs.MetaEditor:
                    metaTab.Draw(Global.spriteBatch); return;
            }
        }
    }
}
