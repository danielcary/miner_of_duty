using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Miner_Of_Duty.Game
{
    public abstract class Camera
    {
        public Matrix ViewMatrix = Matrix.Identity;
        public Matrix ProjMatrix = Matrix.Identity;

        public Vector3 Position;
        public Vector3 Target;

        protected Vector3 cameraRef;
        protected static Vector3 UnitY = new Vector3(0, 1, 0);

        public Camera(GraphicsDevice gd, Vector3 pos, Vector3 target)
        {
            cameraRef = target;
            Target = target;
            Position = pos;
            ProjMatrix = Matrix.CreatePerspectiveFieldOfView(
                MathHelper.PiOver4, gd.Viewport.AspectRatio, .1f, 1000);
        }

    }

    public class FPSCamera : Camera
    {
        public FPSCamera(GraphicsDevice gd, Vector3 pos, Vector3 target)
            : base(gd, pos, target) { }

        public void Update(float totalLeftRight, float totalUpDown, ref Vector3 position)
        {
            Target = Vector3.Transform(Vector3.Forward, Matrix.CreateRotationX(totalUpDown) * Matrix.CreateRotationY(totalLeftRight));

            Position = position;
            Vector3.Add(ref Target, ref position, out Target);

            Matrix.CreateLookAt(ref Position, ref Target, ref UnitY, out ViewMatrix);
        }
    }

    public class FreeRoamCamera : Camera
    {
        public FreeRoamCamera(GraphicsDevice gd, Vector3 pos, Vector3 target)
            : base(gd, pos, target) { }

        protected float leftRightRot = 0;
        protected float upDownRot = 0;
        protected const float rotationSpeed = .7f;

        public void Update(GameTime gameTime, float leftRight, float upDown, Vector3 moveDist)
        {
            leftRightRot += -leftRight * (float)gameTime.ElapsedGameTime.TotalSeconds;
            upDownRot += upDown * (float)gameTime.ElapsedGameTime.TotalSeconds;

            Matrix rotMat = Matrix.CreateRotationX(upDownRot) * Matrix.CreateRotationY(leftRightRot);
            Vector3 transformedRef = Vector3.Transform(cameraRef, rotMat);
            Position += Vector3.Transform(moveDist, rotMat) * 25 * (float)gameTime.ElapsedGameTime.TotalSeconds;

            Target = Vector3.Transform(Vector3.Forward, Matrix.CreateRotationX(upDownRot) * Matrix.CreateRotationY(leftRightRot));

            Vector3.Add(ref Target, ref Position, out Target);

            Matrix.CreateLookAt(ref Position, ref Target, ref UnitY, out ViewMatrix);
        }
    }

    public class DeathCamera : Camera
    {
        public DeathCamera(GraphicsDevice gd, Vector3 pos, Vector3 target)
            : base(gd, pos, target)
        {
            SetLook(pos, target);
        }

        public void SetLook(Vector3 position, Vector3 target)
        {
            Matrix.CreateLookAt(ref position, ref target, ref UnitY, out ViewMatrix);
            Position = position;
        }

    }
}
