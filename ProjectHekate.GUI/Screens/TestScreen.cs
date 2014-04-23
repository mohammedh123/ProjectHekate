using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using ProjectHekate.Core;
using ProjectHekate.GUI.DrawingHelpers;
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

            _engine.CreateScriptedController(512, 120, Math.PiOver2, true, ScriptedController1)
                .Build();
        }

        private IEnumerator<WaitInFrames> ScriptedController1(IController controller, IEngine engine)
        {
            if (controller.FramesAlive == 60) {
                var baseEmitter = engine.CreateScriptedEmitter(controller.X, controller.Y, Math.PiOver2, true, MoveDown);
                var numShots = 3;
                var diffAngle = Math.TwoPi/numShots;
                for (int i = 0; i < numShots; i++) {
                    baseEmitter = baseEmitter.WithOrbittingEmitter(200, diffAngle*i, true, SomeCrap1);
                }
                var finalEmitter = baseEmitter.Build();

                for (int i = 0; i < numShots; i++)
                {
                    engine.BulletSystem.FireOrbitingCurvedLaser(finalEmitter, 200, diffAngle * i, 8, 50, 0, 0, 3, null);
                    engine.BulletSystem.FireOrbitingCurvedLaser(finalEmitter, 100, diffAngle * i, 8, 50, 0, 0, 2, null);
                }
            }

            yield return new WaitInFrames(0);
        }

        public IEnumerator<WaitInFrames> MoveDown(Emitter e, IEngine engine)
        {
            e.Y += 0.5f;
            e.Angle += 2*Math.Pi/180.0f;

            yield return new WaitInFrames(0);
        }

        public IEnumerator<WaitInFrames> EmitterTestFunc(Emitter e, IEngine engine)
        {
            engine.BulletSystem.FireBasicBullet(e.X - 5, e.Y, Math.ToRadians(-90), 5, 0);
            engine.BulletSystem.FireBasicBullet(e.X + 5, e.Y, Math.ToRadians(-90), 5, 0);
            yield return new WaitInFrames(5);
        }
        
        public IEnumerator<WaitInFrames> SomeCrap1(Emitter e, IEngine engine)
        {
            const int numBullets = 9;
            const float angleDiff = Math.TwoPi / numBullets;
            const int delay = 30;

            if (e.FramesAlive == delay) {
                for (var i = 0; i < numBullets; i++) {
                    //bs.FireOrbitingBeam(e, 100, (angleDiff*i), 0, 32, 512, 60, 240, 0, 4);
                    engine.BulletSystem.FireOrbitingScriptedBullet(e, 32, angleDiff * i, 0, 3, 6, OrbitDistanceIncreaseToMax);
                }
                engine.BulletSystem.FireOrbitingBasicBullet(e, 0, 0, 0, 0, 5);
            }

            e.X += (float)System.Math.Cos(e.Angle) * 2.0f;
            e.Y += (float)System.Math.Sin(e.Angle) * 2.0f;

            yield return new WaitInFrames(0);
        }

        public IEnumerator<WaitInFrames> OrbitDistanceIncreaseToMax<T>(T proj, IInterpolationSystem ins) where T : AbstractProjectile
        {
            proj.OrbitDistance = 64 + 64*(float)System.Math.Sin(proj.FramesAlive*Math.Pi/30.0f);

            //if (proj.OrbitDistance < 128) {
                yield return new WaitInFrames(0);
            //}
            //else {
            //    yield return new WaitInFrames(1000);
            //}
        }

        public override void LoadContent()
        {
            Game.TextureManager.LoadTexture("tilemap", @"Resources/Textures/tilemap.png");
            Game.TextureManager.LoadSubTexture("laser", @"Resources/Textures/tilemap.png", 0, 32, 256, 32, true, true);

            _playerSprite = new Sprite(Game.TextureManager.GetTexture("tilemap"), new IntRect(0,0,32,32));

            //0
            _bulletSprites.Add(new Sprite(Game.TextureManager.GetTexture("tilemap"), new IntRect(32, 0, 16, 16)){ Origin = new Vector2f(8,8) });
            _bulletSprites.Add(new Sprite(Game.TextureManager.GetTexture("tilemap"), new IntRect(48, 0, 32, 16)){ Origin = new Vector2f(16,8) });
            _bulletSprites.Add(new Sprite(Game.TextureManager.GetTexture("tilemap"), new IntRect(48, 16, 32, 16)){ Origin = new Vector2f(16,8) });
            _bulletSprites.Add(new Sprite(Game.TextureManager.GetTexture("tilemap"), new IntRect(80, 0, 32, 16)){ Origin = new Vector2f(16,8) });

            //4
            _bulletSprites.Add(new Sprite(Game.TextureManager.GetTexture("laser")));

            //5
            _bulletSprites.Add(new Sprite(Game.TextureManager.GetTexture("tilemap"), new IntRect(0, 64, 64, 64)) { Origin = new Vector2f(32, 32) });

            //6
            _bulletSprites.Add(new Sprite(Game.TextureManager.GetTexture("tilemap"), new IntRect(112, 0, 32, 32)) { Origin = new Vector2f(16, 16) });
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

            _player.Controller.IsEnabled = input.Keyboard.IsKeyDown(Keyboard.Key.Z);
            //_engine.BulletSystem.FireScriptedBullet(_player.X, _player.Y, 0, 2, 0, TestFunc);

            _player.SetPosition(_player.X + dx, _player.Y + dy);
        }

        public IEnumerator<WaitInFrames> TestFunc(AbstractProjectile b, IInterpolationSystem ins)
        {
            const int delay = 30;
            ins.InterpolateProjectileAngle(b, b.Angle, b.Angle + Math.PiOver2, delay);
            yield return new WaitInFrames(delay);
            ins.InterpolateProjectileAngle(b, b.Angle, b.Angle - Math.PiOver2, delay);
            yield return new WaitInFrames(delay);
        }

        public IEnumerator<WaitInFrames> TestLaserFunc(CurvedLaser cv)
        {
            for (int i = 0; i < 30; i++) {
                cv.Angle += Math.Pi/50;
                yield return new WaitInFrames(0);
            }

            for (int i = 0; i < 30; i++)
            {
                cv.Angle -= Math.Pi / 180;
                yield return new WaitInFrames(0);
            }
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
            DrawBeams();
            DrawLasers();

            DrawRenderFrameTime();
        }

        private void DrawRenderFrameTime()
        {
            var textStr = String.Format("Avg render time: {0}\nRender time: {1}\nAvg update time: {2}", Game.AverageRenderTimeForLastSecond.ToString("##.##"), Game.LastRenderTime.ToString("##.##"), Game.AverageUpdateTime.ToString("##.##"));
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
                b = _engine.BulletSystem.Bullets[i];

                if (b.IsActive)
                {
                    sprite = _bulletSprites[b.SpriteIndex];
                    sprite.Position = new Vector2f(b.X, b.Y);
                    sprite.Rotation = Math.ToDegrees(b.Angle);

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
                cv = _engine.BulletSystem.CurvedLasers[i];

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

        private void DrawBeams()
        {
            IBeam b;
            Sprite sprite;
            _vertexArray.Resize(4);
            int texLeft, texTop, texWidth, texHeight;
 
            for (int i = 0; i < _engine.BulletSystem.Beams.Count; i++)
            {
                b = _engine.BulletSystem.Beams[i];

                if (b.IsActive)
                {
                    sprite = _bulletSprites[b.SpriteIndex];
                    texLeft = sprite.TextureRect.Left;
                    texTop = sprite.TextureRect.Top;
                    texWidth = sprite.TextureRect.Width;
                    texHeight = sprite.TextureRect.Height;

                    var texSkip = 0U;
                    var renderStates = new RenderStates(sprite.Texture);

                    // draw warning line
                    if (b.FramesAlive < b.DelayInFrames) {
                        var x2 = b.X + (float) System.Math.Cos(b.Angle)*b.Length;
                        var y2 = b.Y + (float) System.Math.Sin(b.Angle)*b.Length;

                        LineDrawer.Draw(b.X, b.Y, x2, y2, Game.Window);
                    }

                    // draw beam
                    // once the 'warning period' is 85% done, start drawing the beam
                    // at a lower alpha

                    var ratio = (float)b.FramesAlive/b.DelayInFrames;
                    var realRatio = 0.0f;
                    const float lowerLimit = 0.85f;
                    if (ratio < lowerLimit) {
                        realRatio = 0.0f;
                    }
                    else {
                        realRatio = (ratio - lowerLimit) / (1.0f-lowerLimit);
                    }

                    var radiusToUse = b.Radius;
                    var alpha = Math.SmoothStep(0.0f, 1.0f, realRatio);
                    var color = new Color(255, 255, 255, (byte) (alpha*255));

                    texSkip = b.FramesAlive * 16;
                    _vertexArray[0] = new Vertex(
                        new Vector2f(
                            b.X + (float)System.Math.Cos(b.Angle - Math.PiOver2) * radiusToUse,
                            b.Y + (float)System.Math.Sin(b.Angle - Math.PiOver2) * radiusToUse
                        ),
                        color,
                        new Vector2f(texSkip + texLeft, texTop)
                    );

                    _vertexArray[1] = new Vertex(
                        new Vector2f(
                            b.X + (float)System.Math.Cos(b.Angle + Math.PiOver2) * radiusToUse,
                            b.Y + (float)System.Math.Sin(b.Angle + Math.PiOver2) * radiusToUse
                        ),
                        color,
                        new Vector2f(texSkip + texLeft, texTop + texHeight)
                    );

                    _vertexArray[2] = new Vertex(
                        new Vector2f(
                            b.X + (float)System.Math.Cos(b.Angle) * b.Length + (float)System.Math.Cos(b.Angle - Math.PiOver2) * radiusToUse,
                            b.Y + (float)System.Math.Sin(b.Angle) * b.Length + (float)System.Math.Sin(b.Angle - Math.PiOver2) * radiusToUse
                        ),
                        color,
                        new Vector2f(texSkip + texLeft + texWidth * 1, texTop)
                    );

                    _vertexArray[3] = new Vertex(
                        new Vector2f(
                            b.X + (float)System.Math.Cos(b.Angle) * b.Length + (float)System.Math.Cos(b.Angle + Math.PiOver2) * radiusToUse,
                            b.Y + (float)System.Math.Sin(b.Angle) * b.Length + (float)System.Math.Sin(b.Angle + Math.PiOver2) * radiusToUse
                        ),
                        color,
                        new Vector2f(texSkip + texLeft + texWidth * 1, texTop + texHeight)
                    );
                    Game.Window.Draw(_vertexArray, renderStates);
                }
            }
        }

        private void DrawLasers()
        {
            ILaser l;
            Sprite sprite;
            _vertexArray.Resize(4);
            int texLeft, texTop, texWidth, texRight, texHeight;

            for (int i = 0; i < _engine.BulletSystem.Lasers.Count; i++)
            {
                l = _engine.BulletSystem.Lasers[i];

                if (l.IsActive)
                {
                    sprite = _bulletSprites[l.SpriteIndex];
                    texLeft = sprite.TextureRect.Left;
                    texTop = sprite.TextureRect.Top;
                    texWidth = sprite.TextureRect.Width;
                    texRight = texLeft + texWidth;
                    texHeight = sprite.TextureRect.Height;

                    // texLeft needs to be calculated
                    // for example, if the lasers CurrentLength is half of its Length, it should look like:
                    // ====>
                    // while if the lasers CurrentLength is 100% of its Length, it should look like:
                    // <========>
                    // texLeft = texRight - (int)((l.CurrentLength/l.Length)*texWidth);
                    // on second thought...maybe lets not do that

                    _vertexArray[0] = new Vertex(
                        new Vector2f(
                            l.X + (float)System.Math.Cos(l.Angle - Math.PiOver2) * l.Radius,
                            l.Y + (float)System.Math.Sin(l.Angle - Math.PiOver2) * l.Radius
                        ),
                        Color.White,
                        new Vector2f(texLeft, texTop)
                    );

                    _vertexArray[1] = new Vertex(
                        new Vector2f(
                            l.X + (float)System.Math.Cos(l.Angle + Math.PiOver2) * l.Radius,
                            l.Y + (float)System.Math.Sin(l.Angle + Math.PiOver2) * l.Radius
                        ),
                        Color.White,
                        new Vector2f(texLeft, texTop + texHeight)
                    );

                    _vertexArray[2] = new Vertex(
                        new Vector2f(
                            l.X - (float)System.Math.Cos(l.Angle) * l.CurrentLength + (float)System.Math.Cos(l.Angle - Math.PiOver2) * l.Radius,
                            l.Y - (float)System.Math.Sin(l.Angle) * l.CurrentLength + (float)System.Math.Sin(l.Angle - Math.PiOver2) * l.Radius
                        ),
                        Color.White,
                        new Vector2f(texRight, texTop)
                    );

                    _vertexArray[3] = new Vertex(
                        new Vector2f(
                            l.X - (float)System.Math.Cos(l.Angle) * l.CurrentLength + (float)System.Math.Cos(l.Angle + Math.PiOver2) * l.Radius,
                            l.Y - (float)System.Math.Sin(l.Angle) * l.CurrentLength + (float)System.Math.Sin(l.Angle + Math.PiOver2) * l.Radius
                        ),
                        Color.White,
                        new Vector2f(texRight, texTop + texHeight)
                    );

                    var renderStates = new RenderStates(sprite.Texture) { BlendMode = BlendMode.Add};

                    Game.Window.Draw(_vertexArray, renderStates);
                }
            }
        }
    }
}
