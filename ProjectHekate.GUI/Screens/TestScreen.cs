using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProjectHekate.Core;
using ProjectHekate.GUI.Interfaces;
using SFML.Graphics;
using SFML.Window;

namespace ProjectHekate.GUI.Screens
{
    class Player
    {
        public float X { get; set; }
        public float Y { get; set; }
    }
    class TestScreen : GameScreen
    {
        private IBulletSystem _bulletSystem;
        private Player _player = new Player();
        private Sprite _playerSprite;

        public TestScreen()
        {
            _bulletSystem = new BulletSystem();
        }

        public override void LoadContent()
        {
            Game.TextureManager.LoadTexture("tilemap", @"Resources/Textures/tilemap.png");

            _playerSprite = new Sprite(Game.TextureManager.GetTexture("tilemap"), new IntRect(0,0,32,32));
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

            if (input.Keyboard.IsKeyDown(Keyboard.Key.Z)) {
                _bulletSystem.FireBullet(_player.X, _player.Y, 0);
            }

            _player.X += dx;
            _player.Y += dy;
        }

        public override void Update(TimeSpan gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

            _playerSprite.Position = new Vector2f(_player.X, _player.Y);
        }

        public override void Draw(TimeSpan gameTime)
        {
            Game.Window.Clear();

            Game.Window.Draw(_playerSprite);
        }
    }
}
