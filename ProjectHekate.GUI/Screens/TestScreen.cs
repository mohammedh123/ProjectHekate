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
        private VertexArray _vertexArray = new VertexArray(PrimitiveType.TrianglesStrip);

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
            _bulletSprites.Add(new Sprite(Game.TextureManager.GetTexture("tilemap"), new IntRect(48, 0, 32, 16)));
            _bulletSprites.Add(new Sprite(Game.TextureManager.GetTexture("tilemap"), new IntRect(48, 16, 32, 16)));
            _bulletSprites.Add(new Sprite(Game.TextureManager.GetTexture("tilemap"), new IntRect(48, 32, 32, 16)));
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
            DrawCurvedLasers();

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
            for (int i = 0; i < _engine.BulletSystem.Bullets.Count; i++)
            {
                b = _engine.BulletSystem.Bullets.ElementAt(i);

                if (b.IsActive)
                {
                    sprite = _bulletSprites[b.SpriteIndex];
                    sprite.Position = new Vector2f(b.X, b.Y);

                    Game.Window.Draw(sprite);
                }
            }
        }

        private void DrawCurvedLasers()
        {
            ICurvedLaser cv;
            Sprite sprite;

            int texLeft, texTop, texWidth, texHeight;
            for (int i = 0; i < _engine.BulletSystem.CurvedLasers.Count; i++)
            {
                cv = _engine.BulletSystem.CurvedLasers.ElementAt(i);

                // needs at least 2 points to draw [at least 2 coordinates]
                if (cv.IsActive && cv.FramesAlive >= 2) {
                    sprite = _bulletSprites[cv.SpriteIndex];
                    texLeft = sprite.TextureRect.Left;
                    texTop = sprite.TextureRect.Top;
                    texWidth = sprite.TextureRect.Width;
                    texHeight = sprite.TextureRect.Height;

                    uint limit = System.Math.Min(cv.FramesAlive, cv.Lifetime);
                    _vertexArray.Resize(limit * 2);

                    // 1st coordinate is always aimed at the 2nd coordinate
                    var firstToSecondAngle = System.Math.Atan2(
                        cv.Coordinates.ElementAt(1).Y - cv.Coordinates.ElementAt(0).Y,
                        cv.Coordinates.ElementAt(1).X - cv.Coordinates.ElementAt(0).X
                    );

                    _vertexArray[0] = new Vertex(
                        new Vector2f(
                            cv.Coordinates.ElementAt(0).X + (float)System.Math.Cos(firstToSecondAngle - Math.PiOver2) * cv.Radius,
                            cv.Coordinates.ElementAt(0).Y + (float)System.Math.Sin(firstToSecondAngle - Math.PiOver2) * cv.Radius
                        ), 
                        Color.White,
                        new Vector2f(texLeft,texTop)
                    );
                    _vertexArray[1] = new Vertex(
                        new Vector2f(
                            cv.Coordinates.ElementAt(0).X + (float)System.Math.Cos(firstToSecondAngle + Math.PiOver2) * cv.Radius,
                            cv.Coordinates.ElementAt(0).Y + (float)System.Math.Sin(firstToSecondAngle + Math.PiOver2) * cv.Radius
                        ),
                        Color.White,
                        new Vector2f(texLeft, texTop+texHeight)
                    );

                    // the 'middle' coordinates are aimed slightly differently
                    // to calculate where the offsets are, use the previous point's "3rd-angle"
                    // "3rd-angle" refers to a point's angle to the point after the next point
                    // i.e. point 1 to point 3
                    for (var x = 1; x <= limit - 2; x++)
                    {
                        var xCoord = (float)x / (limit - 1);
                        var previousThirdAngle = System.Math.Atan2(
                            cv.Coordinates.ElementAt(x + 1).Y - cv.Coordinates.ElementAt(x - 1).Y,
                            cv.Coordinates.ElementAt(x + 1).X - cv.Coordinates.ElementAt(x - 1).X
                        );
                        
                        _vertexArray[(uint)x*2] = new Vertex(
                            new Vector2f(
                                cv.Coordinates.ElementAt(x).X + (float)System.Math.Cos(previousThirdAngle - Math.PiOver2) * cv.Radius,
                                cv.Coordinates.ElementAt(x).Y + (float)System.Math.Sin(previousThirdAngle - Math.PiOver2) * cv.Radius
                            ),
                            Color.White,
                            new Vector2f(texLeft+xCoord*texWidth,texTop)
                        );
                        _vertexArray[(uint)x*2+1] = new Vertex(
                            new Vector2f(
                                cv.Coordinates.ElementAt(x).X + (float)System.Math.Cos(previousThirdAngle + Math.PiOver2) * cv.Radius,
                                cv.Coordinates.ElementAt(x).Y + (float)System.Math.Sin(previousThirdAngle + Math.PiOver2) * cv.Radius
                            ),
                            Color.White,
                            new Vector2f(texLeft + xCoord * texWidth, texTop+texHeight)
                        );
                    }

                    // penultimate coordinate is always aimed at last coordinate
                    var penultimateToLastAngle = System.Math.Atan2(
                        cv.Coordinates.ElementAt((int)limit-1).Y - cv.Coordinates.ElementAt((int)limit-2).Y,
                        cv.Coordinates.ElementAt((int)limit-1).X - cv.Coordinates.ElementAt((int)limit-2).X
                    );

                    _vertexArray[limit*2-2] = new Vertex(
                        new Vector2f(
                            cv.Coordinates.ElementAt((int)limit-1).X + (float)System.Math.Cos(penultimateToLastAngle - Math.PiOver2) * cv.Radius,
                            cv.Coordinates.ElementAt((int)limit-1).Y + (float)System.Math.Sin(penultimateToLastAngle - Math.PiOver2) * cv.Radius
                        ),
                        Color.White,
                        new Vector2f(texLeft + texWidth, texTop)
                    );
                    _vertexArray[limit*2-1] = new Vertex(
                        new Vector2f(
                            cv.Coordinates.ElementAt((int)limit-1).X + (float)System.Math.Cos(penultimateToLastAngle + Math.PiOver2) * cv.Radius,
                            cv.Coordinates.ElementAt((int)limit-1).Y + (float)System.Math.Sin(penultimateToLastAngle + Math.PiOver2) * cv.Radius
                        ),
                        Color.White,
                        new Vector2f(texLeft + texWidth, texTop + texHeight)
                    );

                    
                    var renderStates = new RenderStates(sprite.Texture);

                    Game.Window.Draw(_vertexArray, renderStates);
                }
            }
        }

                    sprite = _bulletSprites[b.SpriteIndex];
                    sprite.Position = new Vector2f(b.X, b.Y);

                    Game.Window.Draw(sprite);
                }
            }
        }
    }
}
