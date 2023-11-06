using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Miner_Of_Duty.Game.Editor;

namespace Miner_Of_Duty.Game.Networking
{
    public struct MovementPacketState
    {
        public Vector3 Position;
        /// <summary>
        /// It is converted to Local time
        /// </summary>
        public float TimeSent;
        public float TimeAdded;
        public float UpDownRotation;
        public float LeftRightRotation;
        public float MovementSpeed;
        public uint PacketNumber;

        public MovementPacketState(ref Vector3 position, float bodyRot, float timeSent, float movementSpeed, uint PacketID)
        {
            Position = position;
            UpDownRotation = 0;
            LeftRightRotation = bodyRot;
            TimeSent = timeSent;
            TimeAdded = TimeSent;
            MovementSpeed = movementSpeed;
            this.PacketNumber = PacketID;
        }

        public MovementPacketState(Vector3 position, float bodyRot, float timeSent, float movementSpeed, uint PacketID)
        {
            Position = position;
            UpDownRotation = 0;
            LeftRightRotation = bodyRot;
            TimeSent = timeSent;
            TimeAdded = TimeSent;
            MovementSpeed = movementSpeed;
            this.PacketNumber = PacketID;
        }

        public MovementPacketState(ref Vector3 position, ref Vector2 rots, float timeSent, float movementSpeed, uint PacketID)
        {
            Position = position;
            UpDownRotation = rots.X;
            LeftRightRotation = rots.Y;
            TimeSent = timeSent;
            TimeAdded = TimeSent;
            MovementSpeed = movementSpeed;
            this.PacketNumber = PacketID;
        }

        public MovementPacketState(Vector3 position, Vector2 rots, float timeSent, float movementSpeed, uint PacketID)
        {
            Position = position;
            UpDownRotation = rots.X;
            LeftRightRotation = rots.Y;
            TimeSent = timeSent;
            TimeAdded = TimeSent;
            MovementSpeed = movementSpeed;
            this.PacketNumber = PacketID;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="gameTime">The current gameTime</param>
        /// <param name="a">The older packet of data</param>
        /// <param name="b">The newer packet of data</param>
        /// <param name="toUpdate">The player to update</param>
        public static void InterpolatePlayer(GameTime gameTime, ref MovementPacketState a, ref MovementPacketState b, Player toUpdate)
        {
            //since we run off of packets that are older than us we must find that diff
            a.TimeAdded += (float)gameTime.ElapsedGameTime.TotalMilliseconds;

            float ratio = (a.TimeAdded - a.TimeSent) / (b.TimeSent - a.TimeSent);

            if (ratio <= 1)
            {
                Vector3.Lerp(ref a.Position, ref b.Position, ratio, out toUpdate.position);//might need to update bounding box
                toUpdate.SetPosition();

                toUpdate.upDownRot = MathHelper.Lerp(a.UpDownRotation, b.UpDownRotation, ratio);
                toUpdate.leftRightRot = MathHelper.Lerp(a.LeftRightRotation, b.LeftRightRotation, ratio);
                toUpdate.movingSpeed = MathHelper.Lerp(a.MovementSpeed, b.MovementSpeed, ratio);
            }
            else
            {
                ExtrapolateVector3(ref a.Position, ref b.Position, ratio, out toUpdate.position);
                toUpdate.SetPosition();

                toUpdate.upDownRot = ExtrapolateFloat(a.UpDownRotation, b.UpDownRotation, ratio);
                toUpdate.leftRightRot = ExtrapolateFloat(a.LeftRightRotation, b.LeftRightRotation, ratio);
                toUpdate.movingSpeed = ExtrapolateFloat(a.MovementSpeed, b.MovementSpeed, ratio);
            }


        }

        public static void InterpolateFooSwarmie(GameTime gameTime, ref MovementPacketState a, ref MovementPacketState b, FooSwarmie toUpdate)
        {
            //since we run off of packets that are older than us we must find that diff
            a.TimeAdded += (float)gameTime.ElapsedGameTime.TotalMilliseconds;

            float ratio = (a.TimeAdded - a.TimeSent) / (b.TimeSent - a.TimeSent);


            if (ratio <= 1)
            {
                Vector3.Lerp(ref a.Position, ref b.Position, ratio, out toUpdate.Position);//might need to update bounding box
                
                toUpdate.bodyRot = MathHelper.Lerp(a.LeftRightRotation, b.LeftRightRotation, ratio);
                toUpdate.movementSpeed = MathHelper.Lerp(a.MovementSpeed, b.MovementSpeed, ratio);
            }
            else
            {
                ExtrapolateVector3(ref a.Position, ref b.Position, ratio, out toUpdate.Position);

                toUpdate.bodyRot = ExtrapolateFloat(a.LeftRightRotation, b.LeftRightRotation, ratio);
                toUpdate.movementSpeed = ExtrapolateFloat(a.MovementSpeed, b.MovementSpeed, ratio);
            }


        }

        public static void InterpolatePlayer(GameTime gameTime, ref MovementPacketState a, ref MovementPacketState b, PlayerEditor toUpdate)
        {
            //since we run off of packets that are older than us we must find that diff
            a.TimeAdded += (float)gameTime.ElapsedGameTime.TotalMilliseconds;

            float ratio = (a.TimeAdded - a.TimeSent) / (b.TimeSent - a.TimeSent);

            if (ratio <= 1)
            {
                Vector3.Lerp(ref a.Position, ref b.Position, ratio, out toUpdate.position);//might need to update bounding box
                toUpdate.SetPosition();

                toUpdate.upDownRot = MathHelper.Lerp(a.UpDownRotation, b.UpDownRotation, ratio);
                toUpdate.leftRightRot = MathHelper.Lerp(a.LeftRightRotation, b.LeftRightRotation, ratio);
                toUpdate.movingSpeed = MathHelper.Lerp(a.MovementSpeed, b.MovementSpeed, ratio);
            }
            else
            {
                ExtrapolateVector3(ref a.Position, ref b.Position, ratio, out toUpdate.position);
                toUpdate.SetPosition();

                toUpdate.upDownRot = ExtrapolateFloat(a.UpDownRotation, b.UpDownRotation, ratio);
                toUpdate.leftRightRot = ExtrapolateFloat(a.LeftRightRotation, b.LeftRightRotation, ratio);
                toUpdate.movingSpeed = ExtrapolateFloat(a.MovementSpeed, b.MovementSpeed, ratio);
            }


        }

        private static void ExtrapolateVector3(ref Vector3 a, ref Vector3 b, float amount, out Vector3 result)
        {
            result = new Vector3(a.X + (amount * (b.X - a.X)),
            result.Y = a.Y + (amount * (b.Y - a.Y)),
            result.Z = a.Z + (amount * (b.Z - a.Z)));
        }

        private static float ExtrapolateFloat(float a, float b, float amount)
        {
            return a + (amount * (b - a));
        }
    }
}
