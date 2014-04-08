using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProjectHekate.Core;
using ProjectHekate.GUI.Interfaces;
using SFML.Graphics;
using SFML.Window;
using Math = ProjectHekate.Core.Helpers.Math;

namespace ProjectHekate.GUI.Screens
{
    class Player
    {
        public float X { get; set; }
        public float Y { get; set; }

        public void SetPosition(float x, float y)
        {
            X = x;
            Y = y;

            if (Controller != null)
            {
                Controller.X = x;
                Controller.Y = y;
            }
        }

        public IController Controller { get; set; }
    }

    class TestScreen : GameScreen
    {
        private IEngine _engine;
        private Player _player = new Player();
        private Sprite _playerSprite;
        private List<Sprite> _bulletSprites = new List<Sprite>();
        private Font _textFont = new Font(@"Resources/Fonts/arial.ttf");

        public TestScreen()
        {
            _engine = new Engine();

            _player.Controller = _engine
                .CreateController(_player.X, _player.Y, 0, true)
                .WithEmitter(0, 0, 0, true, EmitterTestFunc)
                .Build();

            //_engine.CreateController(512, 300, 0, true)
            //    .WithEmitter(0, 0, 0, true, SomeCrap1)
            //    .Build();
        }

        public IEnumerator<WaitInFrames> EmitterTestFunc(Emitter e, IBulletSystem bs)
        {
            bs.FireBasicBullet(e.X - 5, e.Y, Math.ToRadians(-90), 5, 0);
            bs.FireBasicBullet(e.X + 5, e.Y, Math.ToRadians(-90), 5, 0);
            yield return new WaitInFrames(5);
        }

        public IEnumerator<WaitInFrames> SomeCrap1(Emitter e, IBulletSystem bs)
        {
            e.Angle += Math.TwoPi/90;
            var angles = 5;
            var angleDiff = Math.TwoPi/angles;
            for (int i = 0; i < angles; i++)
            {
                bs.FireScriptedBullet(e.X, e.Y, (e.Angle + angleDiff * i), 2, 0, TestFunc);
                bs.FireScriptedBullet(e.X, e.Y, (-e.Angle + angleDiff * i), 2, 0, TestFunc);
                bs.FireScriptedBullet(e.X, e.Y, (e.Angle + angleDiff * i), 3, 0, TestFunc);
                bs.FireScriptedBullet(e.X, e.Y, (-e.Angle + angleDiff * i), 3, 0, TestFunc);
            }
            yield return new WaitInFrames(5);
        }

        public override void LoadContent()
        {
            Game.TextureManager.LoadTexture("tilemap", @"Resources/Textures/tilemap.png");

            _playerSprite = new Sprite(Game.TextureManager.GetTexture("tilemap"), new IntRect(0,0,32,32));

            _bulletSprites.Add(new Sprite(Game.TextureManager.GetTexture("tilemap"), new IntRect(32, 0, 16, 16)));
            _bulletSprites.Add(new Sprite(Game.TextureManager.GetTexture("tilemap"), new IntRect(48, 0, 16, 16)));
        }

        public override void HandleInput(IInputManager<Mouse.Button, Vector2i, Window, Keyboard.Key> input, TimeSpan gameTime)
        {
            base.HandleInput(input, gameTime);

            float dx=0, dy=0;

            var speed = 120;

            if (input.Keyboard.IsKeyDown(Keyboard.Key.Left))
            {
                dx -= speed;
            }
            if (input.Keyboard.IsKeyDown(Keyboard.Key.Right))
            {
                dx += speed;
            }
            if (input.Keyboard.IsKeyDown(Keyboard.Key.Up))
            {
                dy -= speed;
            }
            if (input.Keyboard.IsKeyDown(Keyboard.Key.Down))
            {
                dy += speed;
            }
            
            dx *= (float)gameTime.TotalSeconds;
            dy *= (float)gameTime.TotalSeconds;

            _player.Controller.Enabled = input.Keyboard.IsKeyDown(Keyboard.Key.Z);
            //_engine.BulletSystem.FireScriptedBullet(_player.X, _player.Y, 0, 2, 0, TestFunc);

            _player.SetPosition(_player.X + dx, _player.Y + dy);
        }

        public IEnumerator<WaitInFrames> TestFunc(Bullet b)
        {
            b.Angle += Math.Pi/180;
            yield return new WaitInFrames(1);
        }

        public override void Update(TimeSpan gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

            _playerSprite.Position = new Vector2f(_player.X, _player.Y);
            _engine.Update((float) gameTime.TotalSeconds);
        }

        public override void Draw(TimeSpan gameTime)
        {
            Game.Window.Clear();

            Game.Window.Draw(_playerSprite);

            DrawBullets();

            DrawRenderFrameTime();
        }

        private void DrawRenderFrameTime()
        {
            var textStr = String.Format("Avg render time: {0}\nRender time: {1}", Game.AverageRenderTimeForLastSecond.ToString("##.##"), Game.LastRenderTime.ToString("##.##"));
            var text = new Text(textStr, _textFont);
            text.CharacterSize = 16;

            Game.Window.Draw(text);
        }

        private void DrawBullets()
        {
            IBullet b;
            Sprite sprite;
            for (int i = 0; i < _engine.BulletSystem.Bullets.Count; i++) {
                b = _engine.BulletSystem.Bullets.ElementAt(i);
                
                if (b.IsActive) {
                    sprite = _bulletSprites[b.SpriteIndex];
                    sprite.Position = new Vector2f(b.X, b.Y);

                    Game.Window.Draw(sprite);
                }
            }
        }
    }
}
