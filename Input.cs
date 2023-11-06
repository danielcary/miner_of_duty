using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Input;

namespace Miner_Of_Duty
{
    public static class Input
    {
        public static int ControllingPlayer { get; set; }
        public static GamePadState[] NewGamePadState { get; private set; }
        public static GamePadState[] OldGamePadState { get; private set; }
        public static GamePadState ControllingPlayerNewGamePadState { get { return NewGamePadState[ControllingPlayer]; } }
        public static GamePadState ControllingPlayerOldGamePadState { get { return OldGamePadState[ControllingPlayer]; } }

        static Input()
        {
            NewGamePadState = new GamePadState[4];
            OldGamePadState = new GamePadState[4];
            backUpNewGamePadState = new GamePadState[4];
            backUpOldGamePadState = new GamePadState[4];
            ControllingPlayer = 0;
        }

        public static GamePadState Empty = new GamePadState();
        public static void Update()
        {
            for (int i = 0; i < 4; i++)
            {
                OldGamePadState[i] = NewGamePadState[i];
                NewGamePadState[i] = GamePad.GetState((Microsoft.Xna.Framework.PlayerIndex)i);
            }
        }


        private static GamePadState[] backUpNewGamePadState;
        private static GamePadState[] backUpOldGamePadState;
        public static void Flush()
        {
            for (int i = 0; i < 4; i++)
            {
                backUpOldGamePadState[i] = OldGamePadState[i];
                OldGamePadState[i] = new GamePadState();
                backUpNewGamePadState[i] = NewGamePadState[i];
                NewGamePadState[i] = new GamePadState();
            }
        }

        public static void ReFill()
        {
            for (int i = 0; i < 4; i++)
            {
                OldGamePadState[i] = backUpOldGamePadState[i];
                NewGamePadState[i] = backUpNewGamePadState[i];
            }
        }

        public enum Direction { Up, Left, Down, Right }

        public static bool IsThumbstickOrDPad(Direction direction, GamePadState state)
        {
            switch (direction)
            {
                case Direction.Up:
                    return state.ThumbSticks.Left.Y > .5f || state.DPad.Up == ButtonState.Pressed;
                case Direction.Down:
                    return state.ThumbSticks.Left.Y < -.5f || state.DPad.Down == ButtonState.Pressed;
                case Direction.Left:
                    return state.ThumbSticks.Left.X < -.5f || state.DPad.Left == ButtonState.Pressed;
                case Direction.Right:
                    return state.ThumbSticks.Left.X > .5f || state.DPad.Right == ButtonState.Pressed;
                default:
                    return false;
            }
        }

        public static bool IsThumbstickOrDPad(Direction direction, ref GamePadState state)
        {
            switch (direction)
            {
                case Direction.Up:
                    return state.ThumbSticks.Left.Y > .5f || state.DPad.Up == ButtonState.Pressed;
                case Direction.Down:
                    return state.ThumbSticks.Left.Y < -.5f || state.DPad.Down == ButtonState.Pressed;
                case Direction.Left:
                    return state.ThumbSticks.Left.X < -.5f || state.DPad.Left == ButtonState.Pressed;
                case Direction.Right:
                    return state.ThumbSticks.Left.X > .5f || state.DPad.Right == ButtonState.Pressed;
                default:
                    return false;
            }
        }

        public static bool IsDPad(Direction direction)
        {
            switch (direction)
            {
                case Direction.Up:
                    return ControllingPlayerNewGamePadState.DPad.Up == ButtonState.Pressed;
                case Direction.Down:
                    return ControllingPlayerNewGamePadState.DPad.Down == ButtonState.Pressed;
                case Direction.Left:
                    return ControllingPlayerNewGamePadState.DPad.Left == ButtonState.Pressed;
                case Direction.Right:
                    return ControllingPlayerNewGamePadState.DPad.Right == ButtonState.Pressed;
                default:
                    return false;
            }
        }

        public static bool IsDPad(Direction direction, ref GamePadState state)
        {
            switch (direction)
            {
                case Direction.Up:
                    return state.DPad.Up == ButtonState.Pressed;
                case Direction.Down:
                    return state.DPad.Down == ButtonState.Pressed;
                case Direction.Left:
                    return state.DPad.Left == ButtonState.Pressed;
                case Direction.Right:
                    return state.DPad.Right == ButtonState.Pressed;
                default:
                    return false;
            }
        }

        public static bool IsThumbstickOrDPad(Direction direction)
        {
            return IsThumbstickOrDPad(direction, ControllingPlayerNewGamePadState);
        }

        public static bool WasButtonPressed(Buttons button)
        {
            return ControllingPlayerNewGamePadState.IsButtonDown(button) && ControllingPlayerOldGamePadState.IsButtonUp(button);
        }

        public static bool WasButtonReleased(Buttons button)
        {
            return ControllingPlayerNewGamePadState.IsButtonUp(button) && ControllingPlayerOldGamePadState.IsButtonDown(button);
        }

        public static bool IsThumbstickOrDPad(Direction direction, int player)
        {
            switch (direction)
            {
                case Direction.Up:
                    return NewGamePadState[player].ThumbSticks.Left.Y > .5f || NewGamePadState[player].DPad.Up == ButtonState.Pressed;
                case Direction.Down:
                    return NewGamePadState[player].ThumbSticks.Left.Y < -.5f || NewGamePadState[player].DPad.Down == ButtonState.Pressed;
                case Direction.Left:
                    return NewGamePadState[player].ThumbSticks.Left.X < -.5f || NewGamePadState[player].DPad.Left == ButtonState.Pressed;
                case Direction.Right:
                    return NewGamePadState[player].ThumbSticks.Left.X > .5f || NewGamePadState[player].DPad.Right == ButtonState.Pressed;
                default:
                    return false;
            }
        }

        public static bool WasButtonPressed(Buttons button, int player)
        {
            return NewGamePadState[player].IsButtonDown(button) && OldGamePadState[player].IsButtonUp(button);
        }

        public static bool WasButtonPressed(Buttons button, ref GamePadState oldState, ref GamePadState newState)
        {
            return newState.IsButtonDown(button) && oldState.IsButtonUp(button);
        }

        public static bool WasButtonReleased(Buttons button, int player)
        {
            return NewGamePadState[player].IsButtonUp(button) && OldGamePadState[player].IsButtonDown(button);
        }

        public static bool WasButtonReleased(Buttons button, ref GamePadState oldState, ref GamePadState newState)
        {
            return newState.IsButtonUp(button) && oldState.IsButtonDown(button);
        }

    }
}
